using Microsoft.EntityFrameworkCore;
using OrdersService.Models;
using System.Collections.Generic;

namespace OrdersService.Data
{
    public class OrderContext : DbContext
    {
        public OrderContext(DbContextOptions<OrderContext> options) : base(options) { }
        public DbSet<Order> Orders { get; set; }
    }
}
