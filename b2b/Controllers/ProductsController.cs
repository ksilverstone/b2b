using System.Security.Claims;
using b2b.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Http;
using System.IO;

namespace b2b.Controllers
{
            // Ürün ekleme, düzenleme, listeleme
    [Authorize]
    public class ProductsController : Controller
    {
        // Veritabanı bağlantısı
        private readonly B2BContext _context;

        public ProductsController(B2BContext context)
        {
            _context = context;
        }

        // Ürün listesi - filtreleme ve arama ile
        [HttpGet]
        public async Task<IActionResult> List(
            string? searchTerm,
            string? category,
            string? brand,
            string? status,
            bool? lowStock)
        {
            int? sellerCompanyId = GetCurrentCompanyId();
            if (sellerCompanyId == null)
            {
                return RedirectToAction("AccessDenied", "Auth");
            }

            // Ürünleri filtreleme için query oluştur
            IQueryable<Product> query = _context.Products
                .AsNoTracking()
                .Include(p => p.ProductImages)
                .Where(p => p.SellerCompanyId == sellerCompanyId.Value);

            // Filtreler
            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                string term = searchTerm.Trim();
                query = query.Where(p =>
                    p.Name.Contains(term) ||
                    (p.Description != null && p.Description.Contains(term)) ||
                    (p.SKU != null && p.SKU.Contains(term))
                );
            }

            if (!string.IsNullOrWhiteSpace(category))
            {
                query = query.Where(p => p.Category == category);
            }

            if (!string.IsNullOrWhiteSpace(brand))
            {
                query = query.Where(p => p.Brand == brand);
            }

            if (!string.IsNullOrWhiteSpace(status))
            {
                query = query.Where(p => p.Status == status);
            }

            if (lowStock.HasValue)
            {
                if (lowStock.Value)
                {
                    query = query.Where(p => p.Stock <= p.MinStock);
                }
                else
                {
                    query = query.Where(p => p.Stock > p.MinStock);
                }
            }

            var products = await query
                .OrderBy(p => p.Name)
                .ToListAsync();

            var viewModel = new ProductListViewModel
            {
                Products = products.Select(MapToViewModel).ToList(),
                SearchTerm = searchTerm,
                CategoryFilter = category,
                BrandFilter = brand,
                StatusFilter = status,
                StockFilter = lowStock,
                Categories = await _context.Products
                    .Where(p => p.SellerCompanyId == sellerCompanyId.Value && p.Category != null)
                    .Select(p => p.Category!)
                    .Distinct()
                    .OrderBy(c => c)
                    .ToListAsync(),
                Brands = await _context.Products
                    .Where(p => p.SellerCompanyId == sellerCompanyId.Value && p.Brand != null)
                    .Select(p => p.Brand!)
                    .Distinct()
                    .OrderBy(b => b)
                    .ToListAsync()
            };

            return View(viewModel);
        }

        // Ürün oluşturma sayfası
        [HttpGet]
        public IActionResult Create()
        {
            return View(new CreateProductRequest());
        }

        // Ürün oluşturma işlemi
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CreateProductRequest request)
        {
            if (!ModelState.IsValid)
            {
                return View(request);
            }

            int? sellerCompanyId = GetCurrentCompanyId();
            if (sellerCompanyId == null)
            {
                return RedirectToAction("AccessDenied", "Auth");
            }

            string? uploadedImageUrl = await SaveProductImageIfProvided(request.ImageFile);

            var product = new Product
            {
                SellerCompanyId = sellerCompanyId.Value,
                Name = request.Name,
                Description = request.Description,
                SKU = request.SKU,
                Price = request.Price,
                Stock = request.Stock,
                ImageUrl = !string.IsNullOrWhiteSpace(uploadedImageUrl) ? uploadedImageUrl : request.ImageUrl,
                Category = request.Category,
                Brand = request.Brand,
                Unit = request.Unit,
                MinStock = request.MinStock,
                IsActive = request.IsActive,
                Status = request.IsActive ? "Aktif" : "Pasif",
                CreatedDate = DateTime.Now
            };

            _context.Products.Add(product);
            await _context.SaveChangesAsync();

            // Ana görseli galeriye kaydet
            if (!string.IsNullOrWhiteSpace(product.ImageUrl))
            {
                _context.ProductImages.Add(new ProductImage
                {
                    ProductId = product.Id,
                    ImageUrl = product.ImageUrl,
                    IsPrimary = true,
                    DisplayOrder = 0
                });
            }

            // Galeri dosyalarını kaydet
            if (request.GalleryFiles != null && request.GalleryFiles.Any())
            {
                int order = (await _context.ProductImages.CountAsync(pi => pi.ProductId == product.Id)) > 0 ?
                    await _context.ProductImages.Where(pi => pi.ProductId == product.Id).MaxAsync(pi => pi.DisplayOrder) + 1 : 0;
                int startOrder = order;
                foreach (var file in request.GalleryFiles)
                {
                    var url = await SaveProductImageIfProvided(file);
                    if (!string.IsNullOrWhiteSpace(url))
                    {
                        _context.ProductImages.Add(new ProductImage
                        {
                            ProductId = product.Id,
                            ImageUrl = url,
                            IsPrimary = false,
                            DisplayOrder = order++
                        });
                    }
                }
                await _context.SaveChangesAsync();

                // Kapak görselini belirle
                var allImages = await _context.ProductImages.Where(pi => pi.ProductId == product.Id)
                    .OrderBy(pi => pi.DisplayOrder).ToListAsync();
                foreach (var img in allImages) img.IsPrimary = false;

                ProductImage? cover = null;
                if (request.CoverIndex.HasValue)
                {
                    var newlyAdded = await _context.ProductImages.Where(pi => pi.ProductId == product.Id && pi.DisplayOrder >= startOrder)
                        .OrderBy(pi => pi.DisplayOrder).ToListAsync();
                    if (request.CoverIndex.Value >= 0 && request.CoverIndex.Value < newlyAdded.Count)
                    {
                        cover = newlyAdded[request.CoverIndex.Value];
                    }
                }
                if (cover == null)
                {
                    // İlk görseli kullan
                    cover = allImages.FirstOrDefault();
                }
                if (cover != null)
                {
                    cover.IsPrimary = true;
                    product.ImageUrl = cover.ImageUrl;
                }
            }

            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(List));
        }

        // Ürün düzenleme sayfası
        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            int? sellerCompanyId = GetCurrentCompanyId();
            if (sellerCompanyId == null)
            {
                return RedirectToAction("AccessDenied", "Auth");
            }

            var product = await _context.Products.FirstOrDefaultAsync(p => p.Id == id && p.SellerCompanyId == sellerCompanyId.Value);
            if (product == null)
            {
                return NotFound();
            }

            var model = new UpdateProductRequest
            {
                Id = product.Id,
                Name = product.Name,
                Description = product.Description,
                SKU = product.SKU,
                Price = product.Price,
                Stock = product.Stock,
                ImageUrl = product.ImageUrl,
                Category = product.Category,
                Brand = product.Brand,
                Unit = product.Unit,
                MinStock = product.MinStock,
                IsActive = product.IsActive,
                Status = product.Status
            };

            return View(model);
        }

        // Ürün düzenleme işlemi
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(UpdateProductRequest request)
        {
            if (!ModelState.IsValid)
            {
                return View(request);
            }

            int? sellerCompanyId = GetCurrentCompanyId();
            if (sellerCompanyId == null)
            {
                return RedirectToAction("AccessDenied", "Auth");
            }

            var product = await _context.Products.FirstOrDefaultAsync(p => p.Id == request.Id && p.SellerCompanyId == sellerCompanyId.Value);
            if (product == null)
            {
                return NotFound();
            }

            product.Name = request.Name;
            product.Description = request.Description;
            product.SKU = request.SKU;
            product.Price = request.Price;
            product.Stock = request.Stock;
            var updatedImageUrl = await SaveProductImageIfProvided(request.ImageFile);
            if (!string.IsNullOrWhiteSpace(updatedImageUrl))
            {
                product.ImageUrl = updatedImageUrl;
            }
            else
            {
                product.ImageUrl = request.ImageUrl;
            }
            product.Category = request.Category;
            product.Brand = request.Brand;
            product.Unit = request.Unit;
            product.MinStock = request.MinStock;
            // IsActive alanı formda yok, durum "Aktif" ise aktif kabul et
            product.Status = request.Status;
            product.IsActive = string.Equals(request.Status, "Aktif", StringComparison.OrdinalIgnoreCase);
            product.UpdatedDate = DateTime.Now;

            await _context.SaveChangesAsync();

            // Düzenleme sırasında yeni galeri yüklemelerini işle
            if (request.GalleryFiles != null && request.GalleryFiles.Any())
            {
                int maxOrder = await _context.ProductImages.Where(pi => pi.ProductId == product.Id)
                    .Select(pi => (int?)pi.DisplayOrder).MaxAsync() ?? 0;

                for (int i = 0; i < request.GalleryFiles.Count; i++)
                {
                    var file = request.GalleryFiles[i];
                    var url = await SaveProductImageIfProvided(file);
                    if (!string.IsNullOrWhiteSpace(url))
                    {
                        _context.ProductImages.Add(new ProductImage
                        {
                            ProductId = product.Id,
                            ImageUrl = url,
                            IsPrimary = false,
                            DisplayOrder = maxOrder + 1 + i
                        });
                    }
                }
                await _context.SaveChangesAsync();

                if (request.CoverIndex.HasValue)
                {
                    // CoverIndex yeni yüklenen dosyaların sırasını belirtir
                    var newlyAdded = await _context.ProductImages
                        .Where(pi => pi.ProductId == product.Id && pi.DisplayOrder > maxOrder)
                        .OrderBy(pi => pi.DisplayOrder)
                        .ToListAsync();
                    if (request.CoverIndex.Value >= 0 && request.CoverIndex.Value < newlyAdded.Count)
                    {
                        var chosen = newlyAdded[request.CoverIndex.Value];
                        var all = await _context.ProductImages.Where(pi => pi.ProductId == product.Id).ToListAsync();
                        foreach (var img in all) img.IsPrimary = false;
                        chosen.IsPrimary = true;
                        product.ImageUrl = chosen.ImageUrl;
                        await _context.SaveChangesAsync();
                    }
                }
            }

            // Ana görselin galeride olduğundan emin ol
            var primary = await _context.ProductImages.FirstOrDefaultAsync(pi => pi.ProductId == product.Id && pi.IsPrimary);
            if (primary == null && !string.IsNullOrWhiteSpace(product.ImageUrl))
            {
                _context.ProductImages.Add(new ProductImage
                {
                    ProductId = product.Id,
                    ImageUrl = product.ImageUrl,
                    IsPrimary = true,
                    DisplayOrder = 0
                });
            }
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(List));
        }

        // Ürün detay sayfası
        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> Detail(int id)
        {
            var product = await _context.Products.AsNoTracking().FirstOrDefaultAsync(p => p.Id == id && p.IsActive);
            if (product == null) return NotFound();

            var vm = MapToViewModel(product);
            // Fiyat kademelerini getir
            try
            {
                var tiers = await _context.ProductPriceTiers.AsNoTracking()
                    .Where(t => t.ProductId == product.Id)
                    .OrderBy(t => t.MinQuantity)
                    .ToListAsync();
                ViewBag.PriceTiers = tiers;
            }
            catch
            {
                ViewBag.PriceTiers = new List<ProductPriceTier>();
            }
            vm.Gallery = await _context.ProductImages.Where(pi => pi.ProductId == product.Id)
                .OrderBy(pi => pi.DisplayOrder).Select(pi => pi.ImageUrl).ToListAsync();
            if ((vm.Gallery == null || vm.Gallery.Count == 0) && !string.IsNullOrWhiteSpace(vm.ImageUrl))
            {
                vm.Gallery = new List<string> { vm.ImageUrl };
            }
            if (string.IsNullOrWhiteSpace(vm.ImageUrl) && vm.Gallery != null && vm.Gallery.Count > 0)
            {
                vm.ImageUrl = vm.Gallery[0];
            }
            return View(vm);
        }

        private async Task<string?> SaveProductImageIfProvided(IFormFile? imageFile)
        {
            if (imageFile == null || imageFile.Length == 0) return null;
            var allowed = new[] { ".jpg", ".jpeg", ".png", ".gif", ".webp" };
            var ext = Path.GetExtension(imageFile.FileName).ToLowerInvariant();
            if (!allowed.Contains(ext)) return null;

            var uploadsRoot = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads", "products");
            if (!Directory.Exists(uploadsRoot)) Directory.CreateDirectory(uploadsRoot);

            var fileName = Guid.NewGuid().ToString("N") + ext;
            var fullPath = Path.Combine(uploadsRoot, fileName);
            using (var stream = new FileStream(fullPath, FileMode.Create))
            {
                await imageFile.CopyToAsync(stream);
            }
            var publicPath = $"/uploads/products/{fileName}";
            return publicPath;
        }

        // Galeriye tek görsel yükle
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UploadImage(int id, IFormFile ImageFile)
        {
            int? sellerCompanyId = GetCurrentCompanyId();
            if (sellerCompanyId == null) return Json(new { success = false, message = "Yetki yok" });

            var product = await _context.Products.FirstOrDefaultAsync(p => p.Id == id && p.SellerCompanyId == sellerCompanyId.Value);
            if (product == null) return Json(new { success = false, message = "Ürün bulunamadı" });

            var url = await SaveProductImageIfProvided(ImageFile);
            if (string.IsNullOrWhiteSpace(url)) return Json(new { success = false, message = "Geçersiz dosya" });

            int nextOrder = await _context.ProductImages.Where(pi => pi.ProductId == product.Id)
                .Select(pi => (int?)pi.DisplayOrder).MaxAsync() ?? -1;

            var img = new ProductImage
            {
                ProductId = product.Id,
                ImageUrl = url,
                IsPrimary = false,
                DisplayOrder = nextOrder + 1
            };
            _context.ProductImages.Add(img);
            await _context.SaveChangesAsync();
            return Json(new { success = true, image = new { Id = img.Id, ImageUrl = img.ImageUrl, IsPrimary = img.IsPrimary, DisplayOrder = img.DisplayOrder } });
        }

                    // Sepete ekle
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddToCart(int productId, int quantity)
        {
            if (quantity <= 0) return Json(new { success = false, message = "Geçersiz miktar" });

            int customerId = GetCurrentCustomerId();
            if (customerId == 0) return Json(new { success = false, message = "Müşteri bulunamadı" });

            var product = await _context.Products.AsNoTracking().FirstOrDefaultAsync(p => p.Id == productId && p.IsActive);
            if (product == null) return Json(new { success = false, message = "Ürün bulunamadı" });

            // Satıcıya göre aktif sepet bul veya oluştur
            var cart = await _context.Carts.FirstOrDefaultAsync(c => c.CustomerId == customerId && c.Status == "Active" && c.SellerCompanyId == product.SellerCompanyId);
            if (cart == null)
            {
                // Null satıcılı aktif sepet varsa kullan, yoksa yeni oluştur
                var neutralCart = await _context.Carts.FirstOrDefaultAsync(c => c.CustomerId == customerId && c.Status == "Active" && c.SellerCompanyId == null);
                if (neutralCart != null)
                {
                    neutralCart.SellerCompanyId = product.SellerCompanyId;
                    neutralCart.UpdatedDate = DateTime.Now;
                    cart = neutralCart;
                    await _context.SaveChangesAsync();
                }
                else
                {
                    cart = new Cart { CustomerId = customerId, Status = "Active", CreatedDate = DateTime.Now, SellerCompanyId = product.SellerCompanyId };
                    _context.Carts.Add(cart);
                    await _context.SaveChangesAsync();
                }
            }

            var item = await _context.CartItems.FirstOrDefaultAsync(ci => ci.CartId == cart.Id && ci.ProductId == productId);

            // Fiyat kademelerini kullanarak birim fiyatı belirle
            decimal basePrice = product.Price;
            try
            {
                int targetQty = item == null ? quantity : (item.Quantity + quantity);
                var tier = await _context.ProductPriceTiers.AsNoTracking()
                    .Where(t => t.ProductId == product.Id && t.MinQuantity <= targetQty && (t.MaxQuantity == null || targetQty <= t.MaxQuantity))
                    .OrderByDescending(t => t.MinQuantity)
                    .FirstOrDefaultAsync();
                if (tier != null) basePrice = tier.UnitPrice;
            }
            catch { }

            if (item == null)
            {
                item = new CartItem
                {
                    CartId = cart.Id,
                    ProductId = productId,
                    Quantity = quantity,
                    UnitPrice = basePrice,
                    CreatedDate = DateTime.Now
                };
                _context.CartItems.Add(item);
            }
            else
            {
                item.Quantity += quantity;
                item.UnitPrice = basePrice;
            }

            await _context.SaveChangesAsync();

            // AJAX ise JSON döndür, değilse yönlendir
            if (Request.Headers.ContainsKey("X-Requested-With") &&
                string.Equals(Request.Headers["X-Requested-With"], "XMLHttpRequest", StringComparison.OrdinalIgnoreCase))
            {
                return Json(new { success = true, redirectUrl = Url.Action("Index","Cart") });
            }
            return RedirectToAction("Index", "Cart");
        }
        // Ürün silme işlemi
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            int? sellerCompanyId = GetCurrentCompanyId();
            if (sellerCompanyId == null)
            {
                return Json(new { success = false, message = "Yetki yok" });
            }

            var product = await _context.Products.FirstOrDefaultAsync(p => p.Id == id && p.SellerCompanyId == sellerCompanyId.Value);
            if (product == null)
            {
                return Json(new { success = false, message = "Ürün bulunamadı" });
            }

            _context.Products.Remove(product);
            await _context.SaveChangesAsync();
            return Json(new { success = true });
        }

        // Ürün kataloğu
        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> Catalog(string? searchTerm, string? category, string? brand)
        {
            IQueryable<Product> query = _context.Products.AsNoTracking()
                .Include(p => p.ProductImages)
                .Where(p => p.IsActive);

            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                string term = searchTerm.Trim();
                query = query.Where(p =>
                    p.Name.Contains(term) ||
                    (p.Description != null && p.Description.Contains(term)) ||
                    (p.SKU != null && p.SKU.Contains(term)));
            }

            if (!string.IsNullOrWhiteSpace(category))
            {
                query = query.Where(p => p.Category == category);
            }

            if (!string.IsNullOrWhiteSpace(brand))
            {
                query = query.Where(p => p.Brand == brand);
            }

            var products = await query.OrderBy(p => p.Name).ToListAsync();

            var model = new ProductListViewModel
            {
                Products = products.Select(MapToViewModel).ToList(),
                SearchTerm = searchTerm,
                CategoryFilter = category,
                BrandFilter = brand,
                Categories = await _context.Products.Where(p => p.Category != null).Select(p => p.Category!).Distinct().OrderBy(c => c).ToListAsync(),
                Brands = await _context.Products.Where(p => p.Brand != null).Select(p => p.Brand!).Distinct().OrderBy(b => b).ToListAsync()
            };

            return View(model);
        }

        // Stok yönetimi
        [HttpGet]
        public async Task<IActionResult> Stock()
        {
            int? sellerCompanyId = GetCurrentCompanyId();
            if (sellerCompanyId == null)
            {
                return RedirectToAction("AccessDenied", "Auth");
            }

            var products = await _context.Products.AsNoTracking()
                .Where(p => p.SellerCompanyId == sellerCompanyId.Value)
                .OrderBy(p => p.Stock)
                .ToListAsync();

            var model = new ProductListViewModel
            {
                Products = products.Select(MapToViewModel).ToList()
            };

            return View(model);
        }

        // Stok ekleme işlemi
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddStock(int productId, int quantity, string? description)
        {
            if (quantity <= 0)
            {
                return Json(new { success = false, message = "Geçersiz miktar" });
            }

            int? sellerCompanyId = GetCurrentCompanyId();
            if (sellerCompanyId == null)
            {
                return Json(new { success = false, message = "Yetki yok" });
            }

            var product = await _context.Products.FirstOrDefaultAsync(p => p.Id == productId && p.SellerCompanyId == sellerCompanyId.Value);
            if (product == null)
            {
                return Json(new { success = false, message = "Ürün bulunamadı" });
            }

            product.Stock += quantity;
            product.UpdatedDate = DateTime.Now;

            // StockMovement kaldırıldı, sadece stok güncelleniyor
            await _context.SaveChangesAsync();

            return Json(new { success = true });
        }

        // Teklif isteme işlemi
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RequestQuote(int productId, int quantity, string? description)
        {
            int customerId = GetCurrentCustomerId();
            if (customerId == 0)
            {
                return Json(new { success = false, message = "Müşteri bilgisi bulunamadı." });
            }

            var product = await _context.Products
                .AsNoTracking()
                .FirstOrDefaultAsync(p => p.Id == productId && p.IsActive && p.Status == "Aktif");

            if (product == null)
            {
                return Json(new { success = false, message = "Ürün bulunamadı veya aktif değil." });
            }

            if (quantity <= 0)
            {
                return Json(new { success = false, message = "Geçersiz miktar." });
            }

            var productRequest = new ProductRequest
            {
                CustomerId = customerId,
                ProductId = productId,
                RequestType = "Price",
                Description = description ?? $"{product.Name} ürünü için {quantity} {product.Unit} teklif isteği",
                Status = "Beklemede",
                CreatedDate = DateTime.Now
            };

            _context.ProductRequests.Add(productRequest);
            await _context.SaveChangesAsync();

            return Json(new { success = true, message = "Teklif isteğiniz başarıyla gönderildi." });
        }

        // Satıcı tarafı teklif listesi
        [HttpGet]
        public async Task<IActionResult> Requests(string? status)
        {
            int? sellerCompanyId = GetCurrentCompanyId();
            if (sellerCompanyId == null)
            {
                return RedirectToAction("AccessDenied", "Auth");
            }

            var query = from pr in _context.ProductRequests.AsNoTracking()
                        join p in _context.Products.AsNoTracking() on pr.ProductId equals p.Id into pp
                        from p in pp.DefaultIfEmpty()
                        join cu in _context.Customers.AsNoTracking() on pr.CustomerId equals cu.Id
                        where p != null && p.SellerCompanyId == sellerCompanyId.Value
                        select new b2b.Models.ProductRequestListItem
                        {
                            Id = pr.Id,
                            ProductId = pr.ProductId,
                            ProductName = p.Name,
                            CustomerName = cu.Name,
                            RequestType = pr.RequestType,
                            Description = pr.Description,
                            Status = pr.Status,
                            CreatedDate = pr.CreatedDate
                        };

            if (!string.IsNullOrWhiteSpace(status))
            {
                query = query.Where(x => x.Status == status);
            }

            var list = await query.OrderByDescending(x => x.CreatedDate).ToListAsync();
            ViewBag.StatusFilter = status;
            return View(list);
        }

        // Alıcı tarafı teklif listesi
        [HttpGet]
        public async Task<IActionResult> MyRequests(string? status)
        {
            int customerId = GetCurrentCustomerId();
            if (customerId == 0)
            {
                return RedirectToAction("AccessDenied", "Auth");
            }

            var query = from pr in _context.ProductRequests.AsNoTracking()
                        join p in _context.Products.AsNoTracking() on pr.ProductId equals p.Id into pp
                        from p in pp.DefaultIfEmpty()
                        where pr.CustomerId == customerId
                        select new b2b.Models.ProductRequestListItem
                        {
                            Id = pr.Id,
                            ProductId = pr.ProductId,
                            ProductName = p != null ? p.Name : "-",
                            CustomerName = string.Empty,
                            RequestType = pr.RequestType,
                            Description = pr.Description,
                            Status = pr.Status,
                            CreatedDate = pr.CreatedDate
                        };

            if (!string.IsNullOrWhiteSpace(status))
            {
                query = query.Where(x => x.Status == status);
            }

            var list = await query.OrderByDescending(x => x.CreatedDate).ToListAsync();
            ViewBag.StatusFilter = status;
            return View(list);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateRequestStatus(int id, string status)
        {
            int? sellerCompanyId = GetCurrentCompanyId();
            if (sellerCompanyId == null)
            {
                return Json(new { success = false, message = "Yetki yok" });
            }

            var pr = await _context.ProductRequests.Include(x => x.Product).FirstOrDefaultAsync(x => x.Id == id);
            if (pr == null || pr.Product == null || pr.Product.SellerCompanyId != sellerCompanyId.Value)
            {
                return Json(new { success = false, message = "Kayıt bulunamadı" });
            }

            pr.Status = status;
            pr.UpdatedDate = DateTime.Now;
            await _context.SaveChangesAsync();
            return Json(new { success = true });
        }

        // Toplu ürün güncelleme
        [HttpGet]
        public IActionResult BulkUpdate()
        {
            return RedirectToAction(nameof(List));
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
            if (string.IsNullOrEmpty(userEmail)) return 0;

            var user = _context.Users.FirstOrDefault(u => u.Email == userEmail);
            if (user == null) return 0;

            var company = _context.Companies.FirstOrDefault(c => c.Id == user.CompanyId);
            if (company == null || !company.IsBuyer) return 0;

            var customer = _context.Customers.FirstOrDefault(c => c.Email == user.Email);
            return customer?.Id ?? 0;
        }

        private static ProductViewModel MapToViewModel(Product p)
        {
            // ImageUrl boşsa, birincil görseli veya ilk görseli kullan
            string? imageUrl = p.ImageUrl;
            if (string.IsNullOrWhiteSpace(imageUrl) && p.ProductImages != null)
            {
                var primary = p.ProductImages.FirstOrDefault(i => i.IsPrimary);
                imageUrl = primary?.ImageUrl ?? p.ProductImages.OrderBy(i => i.DisplayOrder).FirstOrDefault()?.ImageUrl;
            }

            return new ProductViewModel
            {
                Id = p.Id,
                SellerCompanyId = p.SellerCompanyId,
                Name = p.Name,
                Description = p.Description,
                SKU = p.SKU,
                Price = p.Price,
                Stock = p.Stock,
                ImageUrl = imageUrl,
                Category = p.Category,
                Brand = p.Brand,
                Unit = p.Unit,
                MinStock = p.MinStock,
                IsActive = p.IsActive,
                Status = p.Status,
                CreatedDate = p.CreatedDate,
                UpdatedDate = p.UpdatedDate
            };
        }

        // --- Image management endpoints for Edit UI ---

        [HttpGet]
        public async Task<IActionResult> Images(int productId)
        {
            int? sellerCompanyId = GetCurrentCompanyId();
            if (sellerCompanyId == null) return Unauthorized();

            var product = await _context.Products.AsNoTracking().FirstOrDefaultAsync(p => p.Id == productId && p.SellerCompanyId == sellerCompanyId.Value);
            if (product == null) return NotFound();

            var images = await _context.ProductImages
                .Where(pi => pi.ProductId == productId)
                .OrderBy(pi => pi.DisplayOrder)
                .Select(pi => new { pi.Id, imageUrl = pi.ImageUrl, pi.IsPrimary, pi.DisplayOrder })
                .ToListAsync();
            return Json(new { success = true, images });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteImage(int id)
        {
            int? sellerCompanyId = GetCurrentCompanyId();
            if (sellerCompanyId == null) return Json(new { success = false, message = "Yetki yok" });

            var img = await _context.ProductImages.Include(pi => pi.Product).FirstOrDefaultAsync(pi => pi.Id == id);
            if (img == null || img.Product == null || img.Product.SellerCompanyId != sellerCompanyId.Value)
                return Json(new { success = false, message = "Görsel bulunamadı" });

            _context.ProductImages.Remove(img);
            await _context.SaveChangesAsync();

            // Silinen görsel kapak görseli ise yenisini ayarla
            if (img.IsPrimary)
            {
                var next = await _context.ProductImages.Where(pi => pi.ProductId == img.ProductId)
                    .OrderBy(pi => pi.DisplayOrder).FirstOrDefaultAsync();
                if (next != null)
                {
                    next.IsPrimary = true;
                    img.Product.ImageUrl = next.ImageUrl;
                }
                else
                {
                    img.Product.ImageUrl = null;
                }
                await _context.SaveChangesAsync();
            }

            return Json(new { success = true });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ReorderImages(int productId, [FromBody] int[] orderedIds)
        {
            int? sellerCompanyId = GetCurrentCompanyId();
            if (sellerCompanyId == null) return Json(new { success = false, message = "Yetki yok" });

            var product = await _context.Products.FirstOrDefaultAsync(p => p.Id == productId && p.SellerCompanyId == sellerCompanyId.Value);
            if (product == null) return Json(new { success = false, message = "Ürün bulunamadı" });

            var images = await _context.ProductImages.Where(pi => pi.ProductId == productId).ToListAsync();
            int order = 0;
            foreach (var id in orderedIds)
            {
                var img = images.FirstOrDefault(i => i.Id == id);
                if (img != null) img.DisplayOrder = order++;
            }
            await _context.SaveChangesAsync();
            return Json(new { success = true });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SetCoverImage(int id)
        {
            int? sellerCompanyId = GetCurrentCompanyId();
            if (sellerCompanyId == null) return Json(new { success = false, message = "Yetki yok" });

            var img = await _context.ProductImages.Include(i => i.Product).FirstOrDefaultAsync(i => i.Id == id);
            if (img == null || img.Product == null || img.Product.SellerCompanyId != sellerCompanyId.Value)
                return Json(new { success = false, message = "Görsel bulunamadı" });

            var list = await _context.ProductImages.Where(i => i.ProductId == img.ProductId).ToListAsync();
            foreach (var i in list) i.IsPrimary = false;
            img.IsPrimary = true;
            img.Product.ImageUrl = img.ImageUrl;
            await _context.SaveChangesAsync();
            return Json(new { success = true });
        }
    }
}


