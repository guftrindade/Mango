using AutoMapper;
using Mango.Service.OrderAPI.Data;
using Mango.Service.OrderAPI.Models;
using Mango.Service.OrderAPI.Models.Dto;
using Mango.Service.OrderAPI.Service.IService;
using Mango.Service.OrderAPI.Utility;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Mango.Service.OrderAPI.Controllers;

[Route("api/order")]
[ApiController]
public class OrderAPIController : ControllerBase
{
    protected ResponseDto _response;
    private IMapper _mapper;
    private readonly AppDbContext _db;
    private IProductService _productService;

    public OrderAPIController(AppDbContext db, IProductService productService, IMapper mapper)
    {
        _db = db;
        _response = new ResponseDto();
        _productService = productService;
        _mapper = mapper;
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
}
