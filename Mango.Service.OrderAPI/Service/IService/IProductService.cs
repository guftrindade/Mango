using Mango.Service.OrderAPI.Models.Dto;

namespace Mango.Service.OrderAPI.Service.IService;

public interface IProductService
{
    Task<IEnumerable<ProductDto>> GetProducts();
}
