using AutoMapper;
using Mango.Service.OrderAPI.Data;
using Mango.Service.OrderAPI.Models;
using Mango.Service.OrderAPI.Models.Dto;
using Mango.Service.OrderAPI.Service.IService;
using Mango.Service.OrderAPI.Utility;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Stripe.Checkout;
using Stripe;
using Mango.MessageBus;

namespace Mango.Service.OrderAPI.Controllers;

[Route("api/order")]
[ApiController]
public class OrderAPIController : ControllerBase
{
    protected ResponseDto _response;
    private readonly IMapper _mapper;
    private readonly AppDbContext _db;
    private readonly IProductService _productService;
    private readonly IMessageBus _messageBus;
    private readonly IConfiguration _configuration;

    public OrderAPIController(AppDbContext db, IProductService productService, IMapper mapper, IMessageBus messageBus, IConfiguration configuration)
    {
        _db = db;
        _response = new ResponseDto();
        _productService = productService;
        _mapper = mapper;
        _messageBus = messageBus;
        _configuration = configuration;
    }

    [Authorize]
    [HttpPost("CreateOrder")]
    public async Task<ResponseDto> CreateOrder([FromBody] CartDto cartDto)
    {
        try
        {
            OrderHeaderDto orderHeaderDto = _mapper.Map<OrderHeaderDto>(cartDto.CartHeader);
            orderHeaderDto.OrderTime = DateTime.Now;
            orderHeaderDto.Status = SD.Status_Pending;
            orderHeaderDto.OrderDetails = _mapper.Map<IEnumerable<OrderDetailsDto>>(cartDto.CartDetails);

            OrderHeader orderCreated = _db.OrderHeaders.Add(_mapper.Map<OrderHeader>(orderHeaderDto)).Entity;
            await _db.SaveChangesAsync();

            orderHeaderDto.OrderHeaderId = orderCreated.OrderHeaderId;
            _response.Result = orderHeaderDto;
        }
        catch (Exception ex)
        {
            _response.IsSuccess = false;
            _response.Message = ex.Message;
        }

        return _response;
    }


    [Authorize]
    [HttpPost("CreateStripeSession")]
    public async Task<ResponseDto> CreateStripeSession([FromBody] StripeRequestDto stripeRequestDto)
    {
        try
        {
            var options = new SessionCreateOptions
            {
                SuccessUrl = stripeRequestDto.ApprovedUrl,
                CancelUrl = stripeRequestDto.CancelUrl,
                LineItems = new List<SessionLineItemOptions>(),
                Mode = "payment",
            };

            var DiscountsObj = new List<SessionDiscountOptions>()
            {
                new SessionDiscountOptions
                {
                    Coupon = stripeRequestDto.OrderHeader.CouponCode
                }
            };

            foreach (var item in stripeRequestDto.OrderHeader.OrderDetails)
            {
                var sessionLineItem = new SessionLineItemOptions
                {
                    PriceData = new SessionLineItemPriceDataOptions
                    {
                        UnitAmount = (long)(item.Price * 100),
                        Currency = "usd",
                        ProductData = new SessionLineItemPriceDataProductDataOptions
                        {
                            Name = item.Product.Name
                        }
                    },
                    Quantity = item.Count
                };

                options.LineItems.Add(sessionLineItem);
            }

            if(stripeRequestDto.OrderHeader.Discount > 0)
            {
                options.Discounts = DiscountsObj;
            }

            var service = new SessionService();
            Session session = service.Create(options);
            stripeRequestDto.StripeSessionUrl = session.Url;

            OrderHeader orderHeader = _db.OrderHeaders.First(x => x.OrderHeaderId == stripeRequestDto.OrderHeader.OrderHeaderId);
            orderHeader.StripeSessionId = session.Id;

            _db.SaveChanges();
            _response.Result = stripeRequestDto;
        }
        catch (Exception ex)
        {
            _response.IsSuccess = false;
            _response.Message = ex.Message;
        }

        return _response;
    }

    [Authorize]
    [HttpPost("ValidateStripeSession")]
    public async Task<ResponseDto> ValidateStripeSession([FromBody] int orderHeaderId)
    {
        try
        {
            OrderHeader orderHeader = _db.OrderHeaders.First(x => x.OrderHeaderId == orderHeaderId);

            var service = new SessionService();
            Session session = service.Get(orderHeader.StripeSessionId);

            var paymentIntentService = new PaymentIntentService();
            PaymentIntent paymentIntent = paymentIntentService.Get(session.PaymentIntentId);

            if(paymentIntent.Status == "succeeded")
            {
                orderHeader.PaymentIntentId = paymentIntent.Id;
                orderHeader.Status = SD.Status_Approved;
                _db.SaveChanges();

                RewardsDto rewardsDto = new()
                {
                    OrderId = orderHeader.OrderHeaderId,
                    RewardsActivity = Convert.ToInt32(orderHeader.OrderTotal),
                    UserId = orderHeader.UserId
                };

                string topicName = _configuration.GetValue<string>("TopicAndQueueNames:OrderCreatedTopic");
                await _messageBus.PublishMessage(rewardsDto, topicName);

                _response.Result = _mapper.Map<OrderHeaderDto>(orderHeader);
            }
        }
        catch (Exception ex)
        {
            _response.IsSuccess = false;
            _response.Message = ex.Message;
        }

        return _response;
    }
}
