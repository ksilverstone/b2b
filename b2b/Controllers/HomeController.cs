using b2b.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using System.Diagnostics;

namespace b2b.Controllers
{
    // Ana dashboard controller - Satıcı ve alıcı için farklı sayfalar
    [Authorize]
    public class HomeController : Controller
    {
        // Dependency injection
        // Logger ve Context
        // Logging için
        private readonly ILogger<HomeController> _logger;
        // Veritabanı erişimi için
        private readonly B2BContext _context;

        public HomeController(ILogger<HomeController> logger, B2BContext context)
        {
            _logger = logger;
            _context = context;
        }

        // Ana dashboard - Rol bazlı veri yükleme
        public async Task<IActionResult> Index()
        {
            // Kullanıcının şirket ID'sini al
            var companyId = GetCurrentCompanyId();

            if (companyId == null)
            {
                return RedirectToAction("AccessDenied", "Auth");
            }

            // Kullanıcının şirket bilgisini al
            var company = await _context.Companies
                .AsNoTracking()
                .FirstOrDefaultAsync(c => c.Id == companyId.Value);

            if (company == null)
            {
                return RedirectToAction("AccessDenied", "Auth");
            }

            var viewModel = new DashboardViewModel();

            // Debug bilgisi
            _logger.LogInformation($"Company: {company.Name}, CompanyId: {companyId.Value}");
            var userEmail = User.Identity?.Name ?? "Unknown";
            _logger.LogInformation($"User: {userEmail}");

            // Kullanıcının rolünü UserRoles tablosundan al
            var currentUser = await _context.Users
                .FirstOrDefaultAsync(u => u.Email == userEmail);

            if (currentUser == null)
            {
                return RedirectToAction("AccessDenied", "Auth");
            }

            var userRole = await _context.UserRoles
                .Where(ur => ur.UserId == currentUser.Id)
                .Include(ur => ur.Role)
                .Select(ur => ur.Role.RoleName)
                .FirstOrDefaultAsync();

            _logger.LogInformation($"User Role: {userRole}");

            if (userRole == "Seller" || userRole == "Admin")
            {
                // Satıcı dashboard
                _logger.LogInformation("Loading Seller Dashboard");
                await LoadSellerDashboard(viewModel, companyId.Value);
            }
            else
            {
                // Alıcı dashboard'ı yükle
                _logger.LogInformation("Loading Buyer Dashboard");
                await LoadBuyerDashboard(viewModel, companyId.Value);
            }

            return View(viewModel);
        }

        public IActionResult Privacy()
        {
            return View();
        }

        // Veritabanı bağlantı testi
        public IActionResult TestDb()
        {
            var userCount = _context.Users.Count();
            return Content("User count: " + userCount);
        }

        // Satıcı dashboard verilerini yükle
        private async Task LoadSellerDashboard(DashboardViewModel viewModel, int sellerCompanyId)
        {
            // Sipariş istatistikleri
            viewModel.Stats.TotalOrders = await _context.CustomerOrders
                .Where(o => o.SellerCompanyId == sellerCompanyId)
                .CountAsync();

            viewModel.Stats.PendingOrders = await _context.CustomerOrders
                .Where(o => o.SellerCompanyId == sellerCompanyId && o.Status == "Beklemede")
                .CountAsync();

            viewModel.Stats.ApprovedOrders = await _context.CustomerOrders
                .Where(o => o.SellerCompanyId == sellerCompanyId && o.Status == "Onaylandı")
                .CountAsync();

            viewModel.Stats.CompletedOrders = await _context.CustomerOrders
                .Where(o => o.SellerCompanyId == sellerCompanyId && o.Status == "Tamamlandı")
                .CountAsync();

            viewModel.Stats.CancelledOrders = await _context.CustomerOrders
                .Where(o => o.SellerCompanyId == sellerCompanyId && o.Status == "İptal")
                .CountAsync();

            viewModel.Stats.TotalRevenue = await _context.CustomerOrders
                .Where(o => o.SellerCompanyId == sellerCompanyId && o.Status == "Tamamlandı")
                .SumAsync(o => o.TotalAmount);

            viewModel.Stats.MonthlyRevenue = await _context.CustomerOrders
                .Where(o => o.SellerCompanyId == sellerCompanyId && 
                           o.Status == "Tamamlandı" && 
                           o.OrderDate.Month == DateTime.Now.Month)
                .SumAsync(o => o.TotalAmount);

            // Haftalık gelir hesapla
            var weekStart = DateTime.Now.AddDays(-(int)DateTime.Now.DayOfWeek);
            viewModel.Stats.WeeklyRevenue = await _context.CustomerOrders
                .Where(o => o.SellerCompanyId == sellerCompanyId && 
                           o.Status == "Tamamlandı" && 
                           o.OrderDate >= weekStart)
                .SumAsync(o => o.TotalAmount);

            viewModel.Stats.TotalProducts = await _context.Products
                .Where(p => p.SellerCompanyId == sellerCompanyId)
                .CountAsync();

            viewModel.Stats.TotalCustomers = await _context.Customers
                .Where(c => c.CompanyId == sellerCompanyId)
                .CountAsync();

            viewModel.Stats.ActiveCustomers = await _context.Customers
                .Where(c => c.CompanyId == sellerCompanyId && c.Status == "Aktif")
                .CountAsync();

            viewModel.Stats.LowStockCount = await _context.Products
                .Where(p => p.SellerCompanyId == sellerCompanyId && p.Stock <= p.MinStock)
                .CountAsync();

            viewModel.Stats.OutOfStockCount = await _context.Products
                .Where(p => p.SellerCompanyId == sellerCompanyId && p.Stock == 0)
                .CountAsync();

            // Ortalama sipariş değeri hesapla
            var totalOrders = await _context.CustomerOrders
                .Where(o => o.SellerCompanyId == sellerCompanyId && o.Status == "Tamamlandı")
                .CountAsync();
            
            if (totalOrders > 0)
            {
                viewModel.Stats.AverageOrderValue = viewModel.Stats.TotalRevenue / totalOrders;
            }

            // Son 5 siparişi getir
            viewModel.RecentOrders = await _context.CustomerOrders
                .Where(o => o.SellerCompanyId == sellerCompanyId)
                .Include(o => o.Customer)
                .OrderByDescending(o => o.OrderDate)
                .Take(5)
                .Select(o => new RecentOrder
                {
                    Id = o.Id,
                    OrderNumber = o.OrderNumber ?? "",
                    CustomerName = o.Customer!.Name,
                    TotalAmount = o.TotalAmount,
                    Status = o.Status ?? "Beklemede",
                    OrderDate = o.OrderDate,
                    ItemCount = o.ItemCount
                })
                .ToListAsync();

            // Düşük stok ürünleri
            viewModel.LowStockProducts = await _context.Products
                .Where(p => p.SellerCompanyId == sellerCompanyId && p.Stock <= p.MinStock)
                .OrderBy(p => p.Stock)
                .Take(5)
                .Select(p => new LowStockProduct
                {
                    Id = p.Id,
                    Name = p.Name,
                    Stock = p.Stock,
                    MinStock = p.MinStock,
                    Category = p.Category ?? "",
                    Brand = p.Brand ?? "",
                    ImageUrl = p.ImageUrl ?? ""
                })
                .ToListAsync();

            // Sipariş durumlarına göre sayıları hesapla
            viewModel.OrderStatusCounts = await _context.CustomerOrders
                .Where(o => o.SellerCompanyId == sellerCompanyId)
                .GroupBy(o => o.Status)
                .Select(g => new OrderStatusCount
                {
                    Status = g.Key ?? "Bilinmiyor",
                    Count = g.Count()
                })
                .ToListAsync();

            // Kategori bazında satış istatistikleri
            viewModel.CategorySales = await _context.CustomerOrderItems
                .Where(oi => oi.Order!.SellerCompanyId == sellerCompanyId && 
                            oi.Order.Status == "Tamamlandı")
                .GroupBy(oi => oi.Product!.Category)
                .Select(g => new CategorySales
                {
                    Category = g.Key ?? "Kategorisiz",
                    TotalSales = g.Sum(oi => oi.TotalAmount) ?? 0m,
                    OrderCount = g.Count(),
                    Color = GetRandomColor()
                })
                .OrderByDescending(cs => cs.TotalSales)
                .Take(6)
                .ToListAsync();

            // Hızlı işlemler menüsü
            viewModel.QuickActions = new List<QuickAction>
            {
                new QuickAction { Title = "Yeni Ürün Ekle", Description = "Ürün kataloğuna yeni ürün ekle", Icon = "fas fa-plus", Url = "/Products/Create", Color = "primary" },
                new QuickAction { Title = "Stok Yönetimi", Description = "Ürün stoklarını yönet", Icon = "fas fa-boxes", Url = "/Products/Stock", Color = "warning" },
                new QuickAction { Title = "Teklifleri Görüntüle", Description = "Müşteri tekliflerini incele", Icon = "fas fa-file-contract", Url = "/Products/Requests", Color = "success" },
                new QuickAction { Title = "Ürün Listesi", Description = "Tüm ürünleri görüntüle", Icon = "fas fa-th-large", Url = "/Products/Catalog", Color = "info" }
            };

            // Stok uyarıları
            var stockAlerts = await _context.Products
                .Where(p => p.SellerCompanyId == sellerCompanyId && (p.Stock <= p.MinStock || p.Stock == 0))
                .OrderBy(p => p.Stock)
                .Take(5)
                .Select(p => new StockAlert
                {
                    ProductId = p.Id,
                    ProductName = p.Name,
                    CurrentStock = p.Stock,
                    MinStock = p.MinStock,
                    AlertType = p.Stock == 0 ? "Stok Tükendi" : "Düşük Stok",
                    Color = p.Stock == 0 ? "danger" : "warning"
                })
                .ToListAsync();

            // Tekrarlanan ürünleri temizle
            viewModel.StockAlerts = stockAlerts
                .GroupBy(sa => sa.ProductId)
                .Select(g => g.First())
                .ToList();
        }

        private async Task LoadBuyerDashboard(DashboardViewModel viewModel, int buyerCompanyId)
        {
            // Alıcı istatistikleri
            viewModel.Stats.CartItemsCount = await _context.Carts
                .Where(c => c.BuyerCompanyId == buyerCompanyId)
                .SelectMany(c => _context.CartItems.Where(ci => ci.CartId == c.Id))
                .CountAsync();

            // Son 5 teklif
            viewModel.RecentQuotes = await _context.ProductRequests
                .Where(pr => pr.BuyerCompanyId == buyerCompanyId)
                .Include(pr => pr.Product)
                .OrderByDescending(pr => pr.CreatedDate)
                .Take(5)
                .Select(pr => new RecentQuote
                {
                    Id = pr.Id,
                    RequestType = pr.RequestType,
                    Description = pr.Description ?? "",
                    Status = pr.Status,
                    CreatedDate = pr.CreatedDate,
                    ProductName = pr.Product != null ? pr.Product.Name : "Genel Talep"
                })
                .ToListAsync();

            // Ürün kataloğu için 6 ürün getir
            viewModel.LowStockProducts = await _context.Products
                .Where(p => p.IsActive == true) // Aktif ürünler
                .OrderBy(p => p.Category)
                .Take(6) // 6 ürün göster
                .Select(p => new LowStockProduct
                {
                    Id = p.Id,
                    Name = p.Name,
                    Stock = p.Stock,
                    MinStock = p.MinStock,
                    Category = p.Category ?? "Kategorisiz",
                    Brand = p.Brand ?? "",
                    ImageUrl = p.ImageUrl ?? ""
                })
                .ToListAsync();

            // Alıcı için hızlı işlemler
            viewModel.QuickActions = new List<QuickAction>
            {
                new QuickAction { Title = "Ürün Kataloğu", Description = "Tüm ürünleri görüntüle ve sipariş ver", Icon = "fas fa-th-large", Url = "/Products/Catalog", Color = "primary" },
                new QuickAction { Title = "Sepetim", Description = "Sepet içeriğini görüntüle ve düzenle", Icon = "fas fa-shopping-cart", Url = "/Cart", Color = "warning" },
                new QuickAction { Title = "Siparişlerim", Description = "Sipariş geçmişini ve durumlarını takip et", Icon = "fas fa-list", Url = "/Orders/MyOrders", Color = "info" },
                new QuickAction { Title = "Teklif İste", Description = "Yeni ürün teklifi iste ve fiyat öğren", Icon = "fas fa-file-contract", Url = "/Products/RequestQuote", Color = "success" }
            };
        }



        // Kullanıcının şirket ID'sini al
        private int? GetCurrentCompanyId()
        {
            var companyIdClaim = User.FindFirst("CompanyId");
            if (companyIdClaim != null && int.TryParse(companyIdClaim.Value, out int companyId))
            {
                return companyId;
            }
            return null;
        }

        // Rastgele renk seç
        private string GetRandomColor()
        {
            var colors = new[] { "#007bff", "#28a745", "#ffc107", "#dc3545", "#17a2b8", "#6f42c1", "#fd7e14", "#20c997" };
            var random = new Random();
            return colors[random.Next(colors.Length)];
        }
    }
}