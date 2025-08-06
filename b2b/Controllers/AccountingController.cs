using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using b2b.Models;
using Microsoft.EntityFrameworkCore;

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

        // Cari Listesi - Sadece Satıcı ve Admin görebilir
        [Authorize]
        public async Task<IActionResult> Accounts()
        {
            ViewBag.ActivePage = "Accounts";
            
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
                Group = c.Group,
                Status = c.Status,
                CreatedDate = c.CreatedDate,
                LastTransactionDate = c.Transactions.Any() ? c.Transactions.Max(t => t.TransactionDate) : null
            }).ToList();

            return View(accountViewModels);
        }

        // Cari Ekstresi - Sadece Satıcı ve Admin görebilir
        [Authorize]
        public async Task<IActionResult> Statement(int id)
        {
            ViewBag.ActivePage = "Statement";
            
            // Rol kontrolü
            if (!User.IsInRole("Seller") && !User.IsInRole("Admin"))
            {
                return RedirectToAction("AccessDenied", "Auth");
            }

            // Cari bilgilerini veritabanından bul
            var customer = await _context.Customers
                .Include(c => c.Transactions.OrderByDescending(t => t.TransactionDate))
                .FirstOrDefaultAsync(c => c.Id == id);

            if (customer == null)
            {
                return RedirectToAction("Accounts");
            }

            // AccountViewModel'e dönüştür
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
                Group = customer.Group,
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



        // Yeni cari ekleme
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
                    CompanyId = 1, // Tek firma olduğu için sabit
                    Name = request.Name,
                    Email = request.Email,
                    Phone = request.Phone,
                    Address = request.Address,
                    TaxNumber = request.TaxNumber,
                    TaxOffice = request.TaxOffice,
                    Balance = request.Balance,
                    Group = request.Group,
                    Status = request.IsActive ? "Aktif" : "Pasif",
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
                    Group = newCustomer.Group,
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

        // Cari silme
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
                    Group = customer.CustomerGroup,
                    Status = customer.Status,
                    CreatedDate = customer.CreatedDate
                };

                // JavaScript için küçük harfli property'ler
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
                    group = customer.Group,
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

        // Cari güncelleme
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
                    Group = customer.CustomerGroup,
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

        // Yeni işlem ekleme
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

                var customer = await _context.Customers.FindAsync(request.CustomerId);
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

                await _context.SaveChangesAsync();

                return Json(new { success = true, message = "İşlem başarıyla eklendi." });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Hata oluştu: " + ex.Message });
            }
        }

        // Static liste artık kullanılmıyor, veritabanından geliyor

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
                    .Select(o => new b2b.Controllers.OrderHistoryViewModel
                    {
                        OrderNumber = o.OrderNumber,
                        OrderDate = o.OrderDate,
                        CustomerId = o.CustomerId,
                        CustomerName = o.Customer.Name,
                        ItemCount = o.ItemCount,
                        TotalAmount = o.TotalAmount,
                        Status = o.Status,
                        Description = o.Description ?? ""
                    })
                    .ToListAsync();

                return View("~/Views/Orders/History.cshtml", orders);
            }
            catch (Exception ex)
            {
                return View("~/Views/Orders/History.cshtml", new List<b2b.Controllers.OrderHistoryViewModel>());
            }
        }

        // Belirli bir carinin sipariş geçmişi
        [Authorize]
        public async Task<IActionResult> CustomerOrderHistory(int customerId)
        {
            try
            {
                // Debug için başlangıç logu
                System.Diagnostics.Debug.WriteLine($"CustomerOrderHistory başlatıldı: CustomerId={customerId}");

                // Rol kontrolü
                if (!User.IsInRole("Seller") && !User.IsInRole("Admin"))
                {
                    System.Diagnostics.Debug.WriteLine("Rol kontrolü başarısız");
                    return RedirectToAction("AccessDenied", "Auth");
                }

                var customer = await _context.Customers.FindAsync(customerId);
                if (customer == null)
                {
                    System.Diagnostics.Debug.WriteLine($"Customer bulunamadı: CustomerId={customerId}");
                    return RedirectToAction("Accounts");
                }

                System.Diagnostics.Debug.WriteLine($"Customer bulundu: {customer.Name}");

                var orders = await _context.CustomerOrders
                    .Where(o => o.CustomerId == customerId)
                    .OrderByDescending(o => o.OrderDate)
                    .Select(o => new b2b.Controllers.OrderHistoryViewModel
                    {
                        OrderNumber = o.OrderNumber,
                        OrderDate = o.OrderDate,
                        CustomerId = o.CustomerId,
                        CustomerName = customer.Name,
                        ItemCount = o.ItemCount,
                        TotalAmount = o.TotalAmount,
                        Status = o.Status,
                        Description = o.Description ?? ""
                    })
                    .ToListAsync();

                ViewBag.CustomerName = customer.Name;
                ViewBag.CustomerId = customerId;

                // Debug için sipariş sayısını logla
                System.Diagnostics.Debug.WriteLine($"CustomerOrderHistory: CustomerId={customerId}, CustomerName={customer.Name}, OrderCount={orders.Count}");

                return View("~/Views/Orders/History.cshtml", orders);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"CustomerOrderHistory Error: {ex.Message}");
                System.Diagnostics.Debug.WriteLine($"CustomerOrderHistory StackTrace: {ex.StackTrace}");
                return RedirectToAction("Accounts");
            }
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
        public string Group { get; set; } = string.Empty;
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
        public string Group { get; set; } = string.Empty;
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
} 