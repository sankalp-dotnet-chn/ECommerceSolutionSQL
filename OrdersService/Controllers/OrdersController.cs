using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OrdersService.Data;
using OrdersService.Models;
using System.Text.Json;

namespace OrdersService.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class OrdersController : ControllerBase
    {
        private readonly OrderContext _context;
        private readonly HttpClient _httpClient;

        public OrdersController(OrderContext context, IHttpClientFactory httpClientFactory)
        {
            _context = context;
            _httpClient = httpClientFactory.CreateClient();
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<Order>>> GetOrders()
            => await _context.Orders.ToListAsync();

        [HttpGet("{id}")]
        public async Task<ActionResult<Order>> GetOrder(int id)
        {
            var order = await _context.Orders.FindAsync(id);
            if (order == null) return NotFound();
            return order;
        }

        [HttpPost]
        public async Task<ActionResult<Order>> CreateOrder(Order order)
        {
            var productResponse = await _httpClient.GetAsync($"http://localhost:5259/api/products/{order.ProductId}");
            if (!productResponse.IsSuccessStatusCode)
                return BadRequest("Invalid Product ID");

            var productJson = await productResponse.Content.ReadAsStringAsync();
            var product = JsonSerializer.Deserialize<ProductDto>(productJson, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
            if (product == null)
                return BadRequest("Product not found");

            if (product.Stock == 0)
                return BadRequest("Product is Out of stock…!!!");

            if (order.Quantity > product.Stock)
                return BadRequest("The given quantity for the given product is not available…!!");

            order.Total = order.Quantity * product.Price;

            _context.Orders.Add(order);
            await _context.SaveChangesAsync();

            var newStock = product.Stock - order.Quantity;
            var stockUpdate = new { Stock = newStock };

            var updateContent = new StringContent(
                JsonSerializer.Serialize(stockUpdate),
                System.Text.Encoding.UTF8,
                "application/json"
            );

            await _httpClient.PatchAsync($"http://localhost:5259/api/products/{product.Id}/stock", updateContent);

            return CreatedAtAction(nameof(GetOrder), new { id = order.Id }, order);
        }

        [HttpPost("multiple")]
        public async Task<ActionResult> CreateMultipleOrders([FromBody] List<OrderRequest> orders)
        {
            if (orders == null || !orders.Any())
                return BadRequest("No orders provided.");

            var failedOrders = new List<string>();
            var validOrders = new List<Order>();

            foreach (var req in orders)
            {
                var productResponse = await _httpClient.GetAsync($"http://localhost:5259/api/products/{req.ProductId}");
                if (!productResponse.IsSuccessStatusCode)
                {
                    failedOrders.Add($"ProductId {req.ProductId} - Invalid Product ID.");
                    continue;
                }

                var productJson = await productResponse.Content.ReadAsStringAsync();
                var product = JsonSerializer.Deserialize<ProductDto>(productJson, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

                if (product == null)
                {
                    failedOrders.Add($"ProductId {req.ProductId} - Product not found.");
                    continue;
                }

                if (product.Stock == 0)
                {
                    failedOrders.Add($"ProductId {req.ProductId} - Product is Out of stock…!!!");
                    continue;
                }

                if (req.Quantity > product.Stock)
                {
                    failedOrders.Add($"ProductId {req.ProductId} - The given quantity is not available…!!");
                    continue;
                }

                var order = new Order
                {
                    ProductId = req.ProductId,
                    Quantity = req.Quantity,
                    Total = req.Quantity * product.Price
                };

                validOrders.Add(order);
            }

            if (failedOrders.Any())
            {
                return BadRequest(new
                {
                    Message = "Some orders could not be processed.",
                    Errors = failedOrders
                });
            }

            _context.Orders.AddRange(validOrders);
            await _context.SaveChangesAsync();

            foreach (var order in validOrders)
            {
                var productResponse = await _httpClient.GetAsync($"http://localhost:5259/api/products/{order.ProductId}");
                var productJson = await productResponse.Content.ReadAsStringAsync();
                var product = JsonSerializer.Deserialize<ProductDto>(productJson, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

                var newStock = product.Stock - order.Quantity;
                var stockUpdate = new { Stock = newStock };

                var updateContent = new StringContent(
                    JsonSerializer.Serialize(stockUpdate),
                    System.Text.Encoding.UTF8,
                    "application/json"
                );

                await _httpClient.PatchAsync($"http://localhost:5259/api/products/{product.Id}/stock", updateContent);
            }

            return Ok(new
            {
                Message = "All orders placed successfully.",
                Orders = validOrders
            });
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateOrder(int id, Order order)
        {
            if (id != order.Id)
                return BadRequest();

            _context.Entry(order).State = EntityState.Modified;
            await _context.SaveChangesAsync();
            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteOrder(int id)
        {
            var order = await _context.Orders.FindAsync(id);
            if (order == null)
                return NotFound();

            _context.Orders.Remove(order);
            await _context.SaveChangesAsync();
            return NoContent();
        }
    }

    public class ProductDto
    {
        public int Id { get; set; }
        public decimal Price { get; set; }
        public int Stock { get; set; }
    }

    public class OrderRequest
    {
        public int ProductId { get; set; }
        public int Quantity { get; set; }
    }
}
