using Mango.Service.OrderAPI.Models;
using Microsoft.EntityFrameworkCore;

namespace Mango.Service.OrderAPI.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<OrderHeader> OrderHeaders { get; set; }
    public DbSet<OrderDetails> OrderDetails { get; set; }
}
