using AutoMapper;
using Mango.Services.ShoppingCartAPI.Data;
using Mango.Services.ShoppingCartAPI.Models;
using Mango.Services.ShoppingCartAPI.Models.Dto;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Mango.Services.ShoppingCartAPI.Controllers;

[Route("api/cart")]
[ApiController]
public class CartAPIController : ControllerBase
{
    private readonly AppDbContext _db;
    private readonly IMapper _mapper;
    private ResponseDto _response;

    public CartAPIController(AppDbContext db, IMapper mapper)
    {
        _db = db;
        _response = new ResponseDto();
        _mapper = mapper;
    }

    [HttpPost("CartUpsert")]
    public async Task<ResponseDto> CartUpsert (CartDto cartDto)
    {
        try
        {
            var cartHeaderFromDb = await _db.CartHeaders.AsNoTracking().FirstOrDefaultAsync(u => u.UserId == cartDto.CartHeader.UserId);

            if(cartHeaderFromDb == null)
            {
                CartHeader cartHeader = _mapper.Map<CartHeader>(cartDto.CartHeader);
                _db.CartHeaders.Add(cartHeader);
                await _db.SaveChangesAsync();

                cartDto.CartDetails.First().CartHeaderId = cartHeader.CartHeaderId;
                var cartDetails = _mapper.Map<CartDetails>(cartDto.CartDetails.First());
                _db.CartDetails.Add(cartDetails);
                await _db.SaveChangesAsync();
            }
            else
            {
                var cartDetailsFromDb = await _db.CartDetails.AsNoTracking().FirstOrDefaultAsync(
                    u => u.ProductId == cartDto.CartDetails.First().ProductId &&
                    u.CartHeaderId == cartHeaderFromDb.CartHeaderId);

                if(cartDetailsFromDb == null)
                {
                    cartDto.CartDetails.First().CartHeaderId = cartHeaderFromDb.CartHeaderId;
                    var cartDetails = _mapper.Map<CartDetails>(cartDto.CartDetails.First());
                    _db.CartDetails.Add(cartDetails);
                    await _db.SaveChangesAsync();
                }
                else
                {
                    cartDto.CartDetails.First().Count += cartDetailsFromDb.Count;
                    cartDto.CartDetails.First().CartHeaderId = cartDetailsFromDb.CartHeaderId;
                    cartDto.CartDetails.First().CartDetailsId = cartDetailsFromDb.CartDetailsId;

                    var cartDetails = _mapper.Map<CartDetails>(cartDto.CartDetails.First());
                    _db.CartDetails.Update(cartDetails);
                    await _db.SaveChangesAsync();
                }
            }

            _response.Result = cartDto;

        }
        catch (Exception ex)
        {
            _response.Message = ex.Message.ToString();
            _response.IsSuccess = false;
        }

        return _response;
    }
}
