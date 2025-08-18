using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using b2b.Models;

namespace b2b.Controllers
{
    [Authorize]
    public class CartController : Controller
    {
        private readonly B2BContext _context;

        public CartController(B2BContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            int customerId = GetCurrentCustomerId();
            if (customerId == 0) return RedirectToAction("AccessDenied", "Auth");

            var cart = await _context.Carts
                .Include(c => c.Items)
                .ThenInclude(i => i.Product)
                .FirstOrDefaultAsync(c => c.CustomerId == customerId && c.Status == "Active");

            if (cart == null)
            {
                cart = new Cart { CustomerId = customerId, Status = "Active", CreatedDate = DateTime.Now };
                _context.Carts.Add(cart);
                await _context.SaveChangesAsync();
                cart.Items = new List<CartItem>();
            }

            var model = cart;
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateItem(int itemId, int quantity)
        {
            if (quantity <= 0) return Json(new { success = false, message = "Geçersiz miktar" });
            int customerId = GetCurrentCustomerId();
            if (customerId == 0) return Json(new { success = false, message = "Yetki yok" });

            var item = await _context.CartItems
                .Include(i => i.Cart)
                .Include(i => i.Product)
                .FirstOrDefaultAsync(i => i.Id == itemId);
            if (item == null || item.Cart == null || item.Cart.CustomerId != customerId || item.Cart.Status != "Active")
                return Json(new { success = false, message = "Öğe bulunamadı" });

            // Stok kontrolü
            if (item.Product != null && item.Product.Stock < quantity)
                return Json(new { success = false, message = "Stok yetersiz" });

            item.Quantity = quantity;
            await _context.SaveChangesAsync();
            return Json(new { success = true });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RemoveItem(int itemId)
        {
            int customerId = GetCurrentCustomerId();
            if (customerId == 0) return Json(new { success = false, message = "Yetki yok" });

            var item = await _context.CartItems.Include(i => i.Cart).FirstOrDefaultAsync(i => i.Id == itemId);
            if (item == null || item.Cart == null || item.Cart.CustomerId != customerId || item.Cart.Status != "Active")
                return Json(new { success = false, message = "Öğe bulunamadı" });

            _context.CartItems.Remove(item);
            await _context.SaveChangesAsync();
            if (Request.Headers.ContainsKey("X-Requested-With") && string.Equals(Request.Headers["X-Requested-With"], "XMLHttpRequest", StringComparison.OrdinalIgnoreCase))
                return Json(new { success = true });
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Checkout()
        {
            int customerId = GetCurrentCustomerId();
            if (customerId == 0) return Json(new { success = false, message = "Yetki yok" });

            var cart = await _context.Carts.Include(c => c.Items).ThenInclude(i => i.Product)
                .FirstOrDefaultAsync(c => c.CustomerId == customerId && c.Status == "Active");
            if (cart == null || cart.Items.Count == 0) return Json(new { success = false, message = "Sepet boş" });

            // Toplam hesapla
            decimal total = 0m;
            foreach (var it in cart.Items)
            {
                total += it.UnitPrice * it.Quantity;
                if (it.Product != null)
                {
                    if (it.Product.Stock < it.Quantity)
                        return Json(new { success = false, message = $"{it.Product.Name} için stok yetersiz" });
                    it.Product.Stock -= it.Quantity;
                    it.Product.UpdatedDate = DateTime.Now;
                }
            }

            // Sipariş oluştur
            var userEmail = User.Identity?.Name;
            var user = await _context.Users.AsNoTracking().FirstOrDefaultAsync(u => u.Email == userEmail);
            if (user == null) return Json(new { success = false, message = "Kullanıcı bulunamadı" });
            var buyerCompany = await _context.Companies.AsNoTracking().FirstOrDefaultAsync(c => c.Id == user.CompanyId);
            if (buyerCompany == null) return Json(new { success = false, message = "Alıcı şirket bulunamadı" });
            var sellerCompanyId = cart.SellerCompanyId ?? 0;
            if (sellerCompanyId == 0) return Json(new { success = false, message = "Satıcı şirket bilgisi eksik" });

            // Alıcı şirketi için cari kaydı bul veya oluştur
            var buyerAccount = await _context.Customers.FirstOrDefaultAsync(c => c.Email == user.Email || c.Name == buyerCompany.Name);
            if (buyerAccount == null)
            {
                buyerAccount = new Customer
                {
                    Name = buyerCompany.Name,
                    Email = user.Email,
                    Phone = null,
                    Address = null,
                    TaxNumber = null,
                    TaxOffice = null,
                    CustomerGroup = "Müşteri",
                    IsActive = true,
                    Status = "Aktif",
                    CreatedDate = DateTime.Now
                };
                _context.Customers.Add(buyerAccount);
                await _context.SaveChangesAsync();
            }

            var order = new CustomerOrder
            {
                OrderNumber = $"SO-{DateTime.Now:yyyyMMddHHmmss}",
                CustomerId = buyerAccount.Id, // Alıcı cari kaydı
                // CompanyId kaldırıldı
                BuyerCompanyId = buyerCompany.Id,
                SellerCompanyId = cart.SellerCompanyId,
                DocumentType = "Sipariş",
                OrderDate = DateTime.Now,
                TotalAmount = total,
                TotalGross = total,
                Status = "Beklemede",
                ItemCount = cart.Items.Count,
                CreatedDate = DateTime.Now
            };
            _context.CustomerOrders.Add(order);

            // Sepet kalemlerini siparişe taşı
            foreach (var it in cart.Items)
            {
                var line = new CustomerOrderItem
                {
                    Order = order,
                    LineNo = 1,
                    ProductId = it.ProductId,
                    ProductName = it.Product?.Name ?? "-",
                    SKU = it.Product?.SKU,
                    Unit = it.Product?.Unit ?? "Adet",
                    Quantity = it.Quantity,
                    UnitPrice = it.UnitPrice,
                    DiscountRate = 0,
                    TaxRate = 20,
                    NetAmount = it.UnitPrice * it.Quantity,
                    TaxAmount = Math.Round((it.UnitPrice * it.Quantity) * 0.20m, 2),
                    TotalAmount = Math.Round((it.UnitPrice * it.Quantity) * 1.20m, 2),
                    CreatedDate = DateTime.Now
                };
                _context.CustomerOrderItems.Add(line);
            }

            // Sepeti kapat
            cart.Status = "Ordered";
            cart.UpdatedDate = DateTime.Now;

            // Muhasebe hareketi oluştur
            var customerForBalance = await _context.Customers.FirstOrDefaultAsync(c => c.Id == buyerAccount.Id);
            if (customerForBalance != null)
            {
                customerForBalance.Balance += total;
                var tr = new CustomerTransaction
                {
                    CustomerId = customerForBalance.Id,
                    TransactionDate = DateTime.Now,
                    DocumentNo = order.OrderNumber,
                    Description = "Sipariş",
                    Debit = total,
                    Credit = 0,
                    TransactionType = "Order",
                    Balance = customerForBalance.Balance,
                    CreatedDate = DateTime.Now
                };
                _context.CustomerTransactions.Add(tr);
            }

            await _context.SaveChangesAsync();
            if (Request.Headers.ContainsKey("X-Requested-With") && string.Equals(Request.Headers["X-Requested-With"], "XMLHttpRequest", StringComparison.OrdinalIgnoreCase))
                return Json(new { success = true, redirectUrl = Url.Action("MyOrders", "Orders") });
            return RedirectToAction("MyOrders", "Orders");
        }

        private int GetCurrentCustomerId()
        {
            var userEmail = User.Identity?.Name;
            if (string.IsNullOrEmpty(userEmail)) return 0;

            var user = _context.Users.FirstOrDefault(u => u.Email == userEmail);
            if (user == null) return 0;

            var company = _context.Companies.FirstOrDefault(c => c.Id == user.CompanyId);
            if (company == null || !company.IsBuyer) return 0;

            var customer = _context.Customers.FirstOrDefault(c => c.Email == user.Email);
            return customer?.Id ?? 0;
        }
    }
}


