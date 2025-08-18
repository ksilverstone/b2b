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

        // Alıcı siparişleri
        [Authorize(Roles="Buyer")] // Buyer rolü
        public async Task<IActionResult> MyOrders()
        {
            // Alıcı şirketine göre filtre
            int? companyId = GetCurrentCompanyId();
            if (!companyId.HasValue) return RedirectToAction("AccessDenied", "Auth");

            var orders = await _context.CustomerOrders
                .Where(o => o.BuyerCompanyId == companyId.Value)
                .OrderByDescending(o => o.OrderDate)
                .ToListAsync();

            var vms = orders.Select(o => new OrderHistoryViewModel
            {
                OrderNumber = o.OrderNumber,
                OrderDate = o.OrderDate,
                CustomerId = o.CustomerId,
                CustomerName = string.Empty,
                ItemCount = o.ItemCount,
                TotalAmount = o.TotalAmount,
                Status = o.Status,
                Description = o.Description ?? string.Empty
            }).ToList();

            return View(vms);
        }

        private int? GetCurrentCompanyId()
        {
            var claim = User.FindFirst("CompanyId")?.Value;
            if (int.TryParse(claim, out int companyId)) return companyId;
            return null;
        }

        private int GetCurrentCustomerId()
        {
            var userEmail = User.Identity?.Name;
            if (string.IsNullOrEmpty(userEmail))
                return 0;

            var user = _context.Users
                .Include(u => u.Company)
                .FirstOrDefault(u => u.Email == userEmail);

            return user?.CompanyId ?? 0;
        }

        private int GetCurrentUserId()
        {
            var userEmail = User.Identity?.Name;
            if (string.IsNullOrEmpty(userEmail))
                return 0;

            var user = _context.Users.FirstOrDefault(u => u.Email == userEmail);
            return user?.Id ?? 0;
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