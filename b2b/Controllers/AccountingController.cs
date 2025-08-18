using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using b2b.Models;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics;
using Microsoft.AspNetCore.Mvc.ViewEngines;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.IO;

namespace b2b.Controllers
{
    [Authorize]
    public class AccountingController : Controller
    {
        private readonly B2BContext _context;

        public AccountingController(B2BContext context)
        {
            _context = context;
        }

        // Cari listesi
        [Authorize]
        public async Task<IActionResult> Accounts()
        {
            ViewBag.ActivePage = "Accounts";
            
            // Cache engelle
            Response.Headers["Cache-Control"] = "no-cache, no-store, must-revalidate";
            Response.Headers["Pragma"] = "no-cache";
            Response.Headers["Expires"] = "0";
            
            // Rol kontrolü
            if (!User.IsInRole("Seller") && !User.IsInRole("Admin"))
            {
                return RedirectToAction("AccessDenied", "Auth");
            }

            // Veritabanından tüm carileri getir
            var customers = await _context.Customers
                .Include(c => c.Transactions)
                .OrderBy(c => c.Name)
                .ToListAsync();

            // ViewModel'e dönüştür
            var accountViewModels = customers.Select(c => new AccountViewModel
            {
                Id = c.Id,
                Name = c.Name,
                Email = c.Email ?? "",
                Phone = c.Phone ?? "",
                Address = c.Address ?? "",
                TaxNumber = c.TaxNumber ?? "",
                TaxOffice = c.TaxOffice ?? "",
                Balance = c.Balance,
                CustomerGroup = c.CustomerGroup,
                Status = c.Status,
                CreatedDate = c.CreatedDate,
                LastTransactionDate = c.Transactions.Any() ? c.Transactions.Max(t => t.TransactionDate) : null
            }).ToList();

            return View(accountViewModels);
        }

        // Cari ekstresi
        [Authorize]
        public async Task<IActionResult> Statement(int id)
        {
            ViewBag.ActivePage = "Statement";
            
            // Rol kontrolü
            if (!User.IsInRole("Seller") && !User.IsInRole("Admin"))
            {
                return RedirectToAction("AccessDenied", "Auth");
            }

            // Cari bilgilerini getir
            var customer = await _context.Customers
                .Include(c => c.Transactions.OrderByDescending(t => t.TransactionDate))
                .FirstOrDefaultAsync(c => c.Id == id);

            if (customer == null)
            {
                return RedirectToAction("Accounts");
            }

            // ViewModel'e dönüştür
            var account = new AccountViewModel
            {
                Id = customer.Id,
                Name = customer.Name,
                Email = customer.Email ?? "",
                Phone = customer.Phone ?? "",
                Address = customer.Address ?? "",
                TaxNumber = customer.TaxNumber ?? "",
                TaxOffice = customer.TaxOffice ?? "",
                Balance = customer.Balance,
                CustomerGroup = customer.CustomerGroup,
                Status = customer.Status,
                CreatedDate = customer.CreatedDate
            };

            // İşlemleri TransactionViewModel'e dönüştür
            var transactions = customer.Transactions.Select(t => new TransactionViewModel
            {
                Date = t.TransactionDate,
                DocumentNo = t.DocumentNo ?? "",
                Description = t.Description ?? "",
                Debit = t.Debit,
                Credit = t.Credit,
                Balance = t.Balance ?? 0
            }).ToList();

            var statementViewModel = new StatementViewModel
            {
                Account = account,
                Transactions = transactions
            };

            return View(statementViewModel);
        }



        // Yeni cari ekle
        [HttpPost]
        public async Task<IActionResult> AddAccount([FromBody] AddAccountRequest request)
        {
            try
            {
                // Rol kontrolü
                if (!User.IsInRole("Seller") && !User.IsInRole("Admin"))
                {
                    return Json(new { success = false, message = "Yetkiniz bulunmamaktadır." });
                }

                // Yeni cari oluştur
                var newCustomer = new Customer
                {
                    Name = request.Name,
                    Email = request.Email,
                    Phone = request.Phone,
                    Address = request.Address,
                    TaxNumber = request.TaxNumber,
                    TaxOffice = request.TaxOffice,
                    Balance = request.Balance,
                    CustomerGroup = request.CustomerGroup,
                    Status = request.IsActive ? "Aktif" : "Pasif",
                    IsActive = request.IsActive,
                    CreatedDate = DateTime.Now
                };

                _context.Customers.Add(newCustomer);
                await _context.SaveChangesAsync();

                // ViewModel'e dönüştür
                var newAccount = new AccountViewModel
                {
                    Id = newCustomer.Id,
                    Name = newCustomer.Name,
                    Email = newCustomer.Email ?? "",
                    Phone = newCustomer.Phone ?? "",
                    Address = newCustomer.Address ?? "",
                    TaxNumber = newCustomer.TaxNumber ?? "",
                    TaxOffice = newCustomer.TaxOffice ?? "",
                    Balance = newCustomer.Balance,
                    CustomerGroup = newCustomer.CustomerGroup,
                    Status = newCustomer.Status,
                    CreatedDate = newCustomer.CreatedDate
                };

                return Json(new { success = true, account = newAccount, message = "Cari başarıyla eklendi." });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Hata oluştu: " + ex.Message });
            }
        }

        // Cari sil
        [HttpPost]
        public async Task<IActionResult> DeleteAccount(int id)
        {
            try
            {
                // Rol kontrolü
                if (!User.IsInRole("Seller") && !User.IsInRole("Admin"))
                {
                    return Json(new { success = false, message = "Yetkiniz bulunmamaktadır." });
                }

                var customer = await _context.Customers.FindAsync(id);
                if (customer == null)
                {
                    return Json(new { success = false, message = "Cari bulunamadı." });
                }

                _context.Customers.Remove(customer);
                await _context.SaveChangesAsync();

                return Json(new { success = true, message = "Cari başarıyla silindi." });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Hata oluştu: " + ex.Message });
            }
        }

        // Cari bilgilerini getir
        [HttpGet]
        public async Task<IActionResult> GetAccount(int id)
        {
            try
            {
                // Rol kontrolü
                if (!User.IsInRole("Seller") && !User.IsInRole("Admin"))
                {
                    return Json(new { success = false, message = "Yetkiniz bulunmamaktadır." });
                }

                var customer = await _context.Customers.FindAsync(id);
                if (customer == null)
                {
                    return Json(new { success = false, message = "Cari bulunamadı." });
                }

                var accountViewModel = new AccountViewModel
                {
                    Id = customer.Id,
                    Name = customer.Name,
                    Email = customer.Email ?? "",
                    Phone = customer.Phone ?? "",
                    Address = customer.Address ?? "",
                    TaxNumber = customer.TaxNumber ?? "",
                    TaxOffice = customer.TaxOffice ?? "",
                    Balance = customer.Balance,
                    CustomerGroup = customer.CustomerGroup,
                    Status = customer.Status,
                    CreatedDate = customer.CreatedDate
                };

                // JavaScript için küçük harfli alanlar
                var accountData = new
                {
                    id = customer.Id,
                    name = customer.Name,
                    email = customer.Email ?? "",
                    phone = customer.Phone ?? "",
                    address = customer.Address ?? "",
                    taxNumber = customer.TaxNumber ?? "",
                    taxOffice = customer.TaxOffice ?? "",
                    balance = customer.Balance,
                    group = customer.CustomerGroup,
                    status = customer.Status,
                    isActive = customer.IsActive,
                    createdDate = customer.CreatedDate
                };

                return Json(new { success = true, account = accountData });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Hata oluştu: " + ex.Message });
            }
        }

        // Cari güncelle
        [HttpPost]
        public async Task<IActionResult> UpdateAccount([FromBody] UpdateAccountRequest request)
        {
            try
            {
                // Rol kontrolü
                if (!User.IsInRole("Seller") && !User.IsInRole("Admin"))
                {
                    return Json(new { success = false, message = "Yetkiniz bulunmamaktadır." });
                }

                var customer = await _context.Customers.FindAsync(request.Id);
                if (customer == null)
                {
                    return Json(new { success = false, message = "Cari bulunamadı." });
                }

                customer.Name = request.Name;
                customer.Email = request.Email;
                customer.Phone = request.Phone;
                customer.Address = request.Address;
                customer.TaxNumber = request.TaxNumber;
                customer.TaxOffice = request.TaxOffice;
                customer.Balance = request.Balance;
                customer.IsActive = request.IsActive;
                customer.Status = request.IsActive ? "Aktif" : "Pasif";
                customer.UpdatedDate = DateTime.Now;

                await _context.SaveChangesAsync();

                var accountViewModel = new AccountViewModel
                {
                    Id = customer.Id,
                    Name = customer.Name,
                    Email = customer.Email ?? "",
                    Phone = customer.Phone ?? "",
                    Address = customer.Address ?? "",
                    TaxNumber = customer.TaxNumber ?? "",
                    TaxOffice = customer.TaxOffice ?? "",
                    Balance = customer.Balance,
                    CustomerGroup = customer.CustomerGroup,
                    Status = customer.Status,
                    CreatedDate = customer.CreatedDate
                };

                return Json(new { success = true, account = accountViewModel, message = "Cari başarıyla güncellendi." });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Hata oluştu: " + ex.Message });
            }
        }

        // Yeni işlem ekle
        [HttpPost]
        public async Task<IActionResult> AddTransaction([FromBody] AddTransactionRequest request)
        {
            try
            {
                // Rol kontrolü
                if (!User.IsInRole("Seller") && !User.IsInRole("Admin"))
                {
                    return Json(new { success = false, message = "Yetkiniz bulunmamaktadır." });
                }

                var customer = await _context.Customers.Include(x=>x.Transactions).FirstOrDefaultAsync(x=>x.Id==request.CustomerId);
                if (customer == null)
                {
                    return Json(new { success = false, message = "Cari bulunamadı." });
                }

                // Yeni işlem oluştur
                var newTransaction = new CustomerTransaction
                {
                    CustomerId = request.CustomerId,
                    TransactionDate = DateTime.Now,
                    DocumentNo = request.DocumentNo,
                    Description = request.Description,
                    Debit = request.TransactionType == "Debit" ? request.Amount : 0,
                    Credit = request.TransactionType == "Credit" ? request.Amount : 0,
                    TransactionType = request.TransactionType,
                    CreatedDate = DateTime.Now
                };

                _context.CustomerTransactions.Add(newTransaction);

                // Müşteri bakiyesini güncelle
                if (request.TransactionType == "Debit")
                {
                    customer.Balance += request.Amount;
                }
                else
                {
                    customer.Balance -= request.Amount;
                }

                // Satırın bakiye alanını doldur
                newTransaction.Balance = customer.Balance;
                await _context.SaveChangesAsync();

                return Json(new { success = true, message = "İşlem başarıyla eklendi." });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Hata oluştu: " + ex.Message });
            }
        }

        // Statik liste kullanılmıyor

        // Genel sipariş geçmişi
        [Authorize]
        public async Task<IActionResult> OrderHistory()
        {
            try
            {
                // Rol kontrolü
                if (!User.IsInRole("Seller") && !User.IsInRole("Admin"))
                {
                    return RedirectToAction("AccessDenied", "Auth");
                }

                var orders = await _context.CustomerOrders
                    .Include(o => o.Customer)
                    .OrderByDescending(o => o.OrderDate)
                    .ToListAsync();

                var orderViewModels = orders.Select(o => new b2b.Controllers.OrderHistoryViewModel
                {
                    OrderNumber = o.OrderNumber,
                    OrderDate = o.OrderDate,
                    CustomerId = o.CustomerId,
                    CustomerName = o.Customer?.Name ?? "",
                    ItemCount = o.ItemCount,
                    TotalAmount = o.TotalAmount,
                    Status = o.Status ?? "",
                    Description = o.Description ?? ""
                }).ToList();

                return View("OrderHistory", orderViewModels);

                return View("OrderHistory", orders);
            }
            catch
            {
                return View("OrderHistory", new List<b2b.Controllers.OrderHistoryViewModel>());
            }
        }

        // Belirli bir carinin sipariş geçmişi
        [Authorize]
        [Route("CustomerOrderHistory/{customerId}")]
        public async Task<IActionResult> CustomerOrderHistory(int customerId)
        {
            try
            {
                // Rol kontrolü
                if (!User.IsInRole("Seller") && !User.IsInRole("Admin"))
                {
                    return RedirectToAction("AccessDenied", "Auth");
                }

                // Cari bilgilerini bul
                var customer = await _context.Customers.FirstOrDefaultAsync(c => c.Id == customerId);
                if (customer == null)
                {
                    return RedirectToAction("Accounts");
                }

                // Sadece bu carinin siparişlerini getir
                var orders = await _context.CustomerOrders
                    .Where(o => o.CustomerId == customerId)
                    .OrderByDescending(o => o.OrderDate)
                    .ToListAsync();

                var orderViewModels = orders.Select(o => new b2b.Controllers.OrderHistoryViewModel
                {
                    OrderNumber = o.OrderNumber,
                    OrderDate = o.OrderDate,
                    CustomerId = o.CustomerId,
                    CustomerName = o.Customer?.Name ?? "",
                    ItemCount = o.ItemCount,
                    TotalAmount = o.TotalAmount,
                    Status = o.Status ?? "",
                    Description = o.Description ?? string.Empty
                }).ToList();

                ViewBag.CustomerName = customer.Name;
                ViewBag.CustomerId = customerId;
                return View("OrderHistory", orderViewModels);

                ViewBag.CustomerName = customer.Name;
                ViewBag.CustomerId = customerId;
                return View("OrderHistory", orders);
            }
            catch
            {
                return RedirectToAction("Accounts");
            }
        }

        // Fatura oluşturma
        [HttpGet]
        [Authorize(Roles = "Seller,Admin")]
        public async Task<IActionResult> CreateInvoice(string orderNumber)
        {
            try
            {
                var order = await _context.CustomerOrders
                    .Include(o => o.Customer)
                    .Include(o => o.Items)
                    .ThenInclude(oi => oi.Product)
                    .FirstOrDefaultAsync(o => o.OrderNumber == orderNumber);

                if (order == null)
                    return RedirectToAction("OrderHistory");

                // Fatura numarası oluştur
                var invoiceNumber = $"INV-{DateTime.Now:yyyyMMdd}-{orderNumber}";
                
                // Fatura oluştur
                var invoice = new b2b.Models.Invoice
                {
                    InvoiceNumber = invoiceNumber,
                    OrderId = order.Id,
                    CustomerId = order.CustomerId,
                    InvoiceDate = DateTime.Now,
                    DueDate = DateTime.Now.AddDays(30),
                    TotalAmount = order.TotalAmount,
                    Status = "Beklemede",
                    CreatedAt = DateTime.Now
                };

                _context.Invoices.Add(invoice);
                await _context.SaveChangesAsync();

                // Fatura detayları sayfasına yönlendir
                return RedirectToAction("InvoiceDetails", new { invoiceId = invoice.Id });
            }
            catch
            {
                return RedirectToAction("OrderHistory");
            }
        }

        // Fatura detayları
        [HttpGet]
        [Authorize(Roles = "Seller,Admin")]
        public async Task<IActionResult> InvoiceDetails(int invoiceId)
        {
            try
            {
                var invoice = await _context.Invoices
                    .Include(i => i.Customer)
                    .Include(i => i.Order)
                    .ThenInclude(oi => oi.Items)
                    .ThenInclude(oi => oi.Product)
                    .FirstOrDefaultAsync(i => i.Id == invoiceId);

                if (invoice == null)
                    return RedirectToAction("OrderHistory");

                return View("InvoiceDetails", invoice);
            }
            catch
            {
                return RedirectToAction("OrderHistory");
            }
        }







        // Sipariş durumu güncelleme
        [HttpPost]
        [Authorize(Roles = "Seller,Admin")]
        public async Task<IActionResult> UpdateStatus([FromBody] UpdateOrderStatusModel model)
        {
            try
            {
                if (string.IsNullOrEmpty(model.OrderNumber))
                {
                    return Json(new { success = false, message = "Sipariş numarası boş olamaz." });
                }
                
                var order = await _context.CustomerOrders
                    .FirstOrDefaultAsync(o => o.OrderNumber == model.OrderNumber);
                    
                if (order == null)
                {
                    return Json(new { success = false, message = "Sipariş bulunamadı." });
                }
                
                var oldStatus = order.Status;
                order.Status = model.Status;
                order.UpdatedDate = DateTime.Now;
                
                // Önce siparişi güncelle
                _context.CustomerOrders.Update(order);
                await _context.SaveChangesAsync();
                
                // Durum geçmişi şimdilik kaldırıldı (DbSet yok)
                
                return Json(new { success = true, message = "Durum başarıyla güncellendi." });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Durum güncellenirken hata oluştu." });
            }
        }



        // Teslimat takibi
        [HttpGet]
        [Authorize(Roles = "Seller,Admin")]
        public async Task<IActionResult> TrackDelivery(string orderNumber)
        {
            try
            {
                var order = await _context.CustomerOrders
                    .Include(o => o.Customer)
                    .FirstOrDefaultAsync(o => o.OrderNumber == orderNumber);

                if (order == null)
                    return RedirectToAction("OrderHistory");

                return View("TrackDelivery", order);
            }
            catch
            {
                return RedirectToAction("OrderHistory");
            }
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

    public class AccountViewModel
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Phone { get; set; } = string.Empty;
        public string Address { get; set; } = string.Empty;
        public string TaxNumber { get; set; } = string.Empty;
        public string TaxOffice { get; set; } = string.Empty;
        public decimal Balance { get; set; }
        public string CustomerGroup { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public DateTime CreatedDate { get; set; }
        public DateTime? LastTransactionDate { get; set; }
    }

    public class TransactionViewModel
    {
        public DateTime Date { get; set; }
        public string DocumentNo { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public decimal Debit { get; set; }
        public decimal Credit { get; set; }
        public decimal Balance { get; set; }
    }

    public class StatementViewModel
    {
        public AccountViewModel Account { get; set; } = new();
        public List<TransactionViewModel> Transactions { get; set; } = new();
    }

    public class AddAccountRequest
    {
        public string Name { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Phone { get; set; } = string.Empty;
        public string Address { get; set; } = string.Empty;
        public string TaxNumber { get; set; } = string.Empty;
        public string TaxOffice { get; set; } = string.Empty;
        public decimal Balance { get; set; }
        public string CustomerGroup { get; set; } = string.Empty;
        public bool IsActive { get; set; }
    }

    public class UpdateAccountRequest
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Phone { get; set; } = string.Empty;
        public string Address { get; set; } = string.Empty;
        public string TaxNumber { get; set; } = string.Empty;
        public string TaxOffice { get; set; } = string.Empty;
        public decimal Balance { get; set; }
        public bool IsActive { get; set; }
    }

    public class AddTransactionRequest
    {
        public int CustomerId { get; set; }
        public string DocumentNo { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public decimal Amount { get; set; }
        public string TransactionType { get; set; } = string.Empty; // "Debit" veya "Credit"
    }

    // Fatura modeli
    public class Invoice
    {
        public int Id { get; set; }
        public string InvoiceNumber { get; set; }
        public int OrderId { get; set; }
        public int CustomerId { get; set; }
        public DateTime InvoiceDate { get; set; }
        public DateTime DueDate { get; set; }
        public decimal TotalAmount { get; set; }
        public string Status { get; set; }
        public DateTime CreatedAt { get; set; }
        
        // İlişki alanları
        public Customer Customer { get; set; }
        public CustomerOrder Order { get; set; }
    }

    // Sipariş durumu güncelleme modeli
    public class UpdateOrderStatusModel
    {
        public string OrderNumber { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public string Note { get; set; } = string.Empty;
    }
} 