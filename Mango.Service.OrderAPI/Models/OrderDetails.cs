using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Mango.Service.OrderAPI.Models.Dto;

namespace Mango.Service.OrderAPI.Models;

public class OrderDetails
{
    [Key]
    public int OrderDetailsId { get; set; }

    public int OrderHeaderId { get; set; }
    [ForeignKey("OrderHeaderId")]
    public OrderHeader? CartHeader { get; set; }

    public int ProductId { get; set; }
    [NotMapped]
    public ProductDto? Product { get; set; }

    public int Count { get; set; }
    public string ProductName { get; set; }
    public double Price { get; set; }
}
