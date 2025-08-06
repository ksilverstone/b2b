using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using b2b.Models;
using Microsoft.EntityFrameworkCore;

namespace b2b.Controllers
{
    [Authorize]
    public class OrdersController : Controller
    {
        private readonly B2BContext _context;

        public OrdersController(B2BContext context)
        {
            _context = context;
        }

        // Sipariş Geçmişi - Sadece Satıcı ve Admin görebilir
        [Authorize]
        public async Task<IActionResult> History(int? customerId = null)
        {
            // Rol kontrolü
            if (!User.IsInRole("Seller") && !User.IsInRole("Admin"))
            {
                return RedirectToAction("AccessDenied", "Auth");
            }

            // Veritabanından siparişleri getir
            IQueryable<CustomerOrder> query = _context.CustomerOrders
                .Include(co => co.Customer);

            // Eğer customerId varsa, sadece o carinin siparişlerini getir
            if (customerId.HasValue)
            {
                query = query.Where(co => co.CustomerId == customerId.Value);
            }

            var orders = await query
                .OrderByDescending(co => co.OrderDate)
                .ToListAsync();

            // ViewModel'e dönüştür
            var orderViewModels = orders.Select(o => new OrderHistoryViewModel
            {
                OrderNumber = o.OrderNumber,
                OrderDate = o.OrderDate,
                CustomerId = o.CustomerId,
                CustomerName = o.Customer?.Name ?? "",
                ItemCount = o.ItemCount,
                TotalAmount = o.TotalAmount,
                Status = o.Status
            }).ToList();

            return View(orderViewModels);
        }
    }

    public class OrderHistoryViewModel
    {
        public string OrderNumber { get; set; } = string.Empty;
        public DateTime OrderDate { get; set; }
        public int CustomerId { get; set; }
        public string CustomerName { get; set; } = string.Empty;
        public int ItemCount { get; set; }
        public decimal TotalAmount { get; set; }
        public string Status { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
    }
} 