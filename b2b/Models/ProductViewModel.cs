using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;

namespace b2b.Models
{
    public class ProductViewModel
    {
        public int Id { get; set; }
        public int SellerCompanyId { get; set; }
        public string Name { get; set; } = null!;
        public string? Description { get; set; }
        public string? SKU { get; set; }
        public decimal Price { get; set; }
        public int Stock { get; set; }
        public string? ImageUrl { get; set; }
        public string? Category { get; set; }
        public string? Brand { get; set; }
        public string Unit { get; set; } = "Adet";
        public int MinStock { get; set; }
        public bool IsActive { get; set; }
        public string Status { get; set; } = "Aktif";
        public DateTime CreatedDate { get; set; }
        public DateTime? UpdatedDate { get; set; }
        public string? SellerCompanyName { get; set; }
        public bool IsLowStock => Stock <= MinStock;
        public List<string> Gallery { get; set; } = new();
    }

    public class CreateProductRequest
    {
        [Required(ErrorMessage = "Ürün adı zorunludur")]
        [StringLength(200, ErrorMessage = "Ürün adı en fazla 200 karakter olabilir")]
        public string Name { get; set; } = null!;
        
        [StringLength(1000, ErrorMessage = "Açıklama en fazla 1000 karakter olabilir")]
        public string? Description { get; set; }
        
        [StringLength(50, ErrorMessage = "SKU en fazla 50 karakter olabilir")]
        public string? SKU { get; set; }
        
        [Required(ErrorMessage = "Fiyat zorunludur")]
        [Range(0, double.MaxValue, ErrorMessage = "Fiyat 0'dan büyük olmalıdır")]
        public decimal Price { get; set; }
        
        [Required(ErrorMessage = "Stok miktarı zorunludur")]
        [Range(0, int.MaxValue, ErrorMessage = "Stok miktarı 0'dan büyük olmalıdır")]
        public int Stock { get; set; }
        
        [StringLength(500)]
        public string? ImageUrl { get; set; }
        
        [StringLength(100)]
        public string? Category { get; set; }
        
        [StringLength(100)]
        public string? Brand { get; set; }
        
        [StringLength(20)]
        public string Unit { get; set; } = "Adet";
        
        [Range(0, int.MaxValue)]
        public int MinStock { get; set; } = 0;
        
        public bool IsActive { get; set; } = true;

        // Dosya yükleme için
        public IFormFile? ImageFile { get; set; }
        public List<IFormFile>? GalleryFiles { get; set; }
        public int? CoverIndex { get; set; }
    }

    public class UpdateProductRequest : CreateProductRequest
    {
        public int Id { get; set; }
        public string Status { get; set; } = "Aktif";
    }

    public class ProductListViewModel
    {
        public List<ProductViewModel> Products { get; set; } = new();
        public string? SearchTerm { get; set; }
        public string? CategoryFilter { get; set; }
        public string? BrandFilter { get; set; }
        public string? StatusFilter { get; set; }
        public bool? StockFilter { get; set; } // true = düşük stok, false = normal stok
        public List<string> Categories { get; set; } = new();
        public List<string> Brands { get; set; } = new();
    }
} 