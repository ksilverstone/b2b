using System.ComponentModel.DataAnnotations;

namespace b2b.Models
{
    // Dashboard'da gösterilecek tüm veriler
    public class DashboardViewModel
    {
        // Dashboard istatistikleri
        public DashboardStats Stats { get; set; } = new();
        public List<RecentOrder> RecentOrders { get; set; } = new();
        public List<RecentQuote> RecentQuotes { get; set; } = new();
        public List<LowStockProduct> LowStockProducts { get; set; } = new();
        public List<CustomerSummary> TopCustomers { get; set; } = new();
        public List<MonthlyRevenue> MonthlyRevenues { get; set; } = new();
        public List<OrderStatusCount> OrderStatusCounts { get; set; } = new();
        public List<CategorySales> CategorySales { get; set; } = new();
        public List<QuickAction> QuickActions { get; set; } = new();
        public List<StockAlert> StockAlerts { get; set; } = new();
        public List<RevenueTrend> RevenueTrends { get; set; } = new();
    }

            // Tüm istatistikler
    public class DashboardStats
    {
        // Satıcı istatistikleri
        public int TotalOrders { get; set; }
        public int PendingOrders { get; set; }
        public int ApprovedOrders { get; set; }
        public int CompletedOrders { get; set; }
        public int CancelledOrders { get; set; }
        public decimal TotalRevenue { get; set; }
        public decimal MonthlyRevenue { get; set; }
        public decimal WeeklyRevenue { get; set; }
        public int TotalCustomers { get; set; }
        public int ActiveCustomers { get; set; }
        public int LowStockCount { get; set; }
        public int TotalProducts { get; set; }
        public int OutOfStockCount { get; set; }
        public decimal AverageOrderValue { get; set; }
        
        // Alıcı istatistikleri
        public decimal CustomerBalance { get; set; }
        public int TotalPurchases { get; set; }
        public decimal TotalSpent { get; set; }
        public int CartItemsCount { get; set; }
        public int ActiveQuotes { get; set; }
    }

    public class RecentOrder
    {
        public int Id { get; set; }
        public string OrderNumber { get; set; } = string.Empty;
        public string CustomerName { get; set; } = string.Empty;
        public decimal TotalAmount { get; set; }
        public string Status { get; set; } = string.Empty;
        public DateTime OrderDate { get; set; }
        public int ItemCount { get; set; }
        public string StatusColor => Status switch
        {
            "Beklemede" => "warning",
            "Tamamlandı" => "success",
            "İptal" => "danger",
            "Hazırlanıyor" => "info",
            _ => "secondary"
        };
    }

    public class RecentQuote
    {
        public int Id { get; set; }
        public string RequestType { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public DateTime CreatedDate { get; set; }
        public string ProductName { get; set; } = string.Empty;
        public string StatusColor => Status switch
        {
            "Beklemede" => "warning",
            "Yanıtlandı" => "success",
            "İptal" => "danger",
            _ => "secondary"
        };
    }

    public class LowStockProduct
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public int Stock { get; set; }
        public int MinStock { get; set; }
        public string Category { get; set; } = string.Empty;
        public string Brand { get; set; } = string.Empty;
        public string ImageUrl { get; set; } = string.Empty;
        public bool IsCritical => Stock <= MinStock;
    }

    public class CustomerSummary
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public decimal Balance { get; set; }
        public int OrderCount { get; set; }
        public decimal TotalSpent { get; set; }
        public DateTime LastOrderDate { get; set; }
        public string Status { get; set; } = string.Empty;
    }

    public class MonthlyRevenue
    {
        public string Month { get; set; } = string.Empty;
        public decimal Revenue { get; set; }
        public int OrderCount { get; set; }
    }

    public class OrderStatusCount
    {
        public string Status { get; set; } = string.Empty;
        public int Count { get; set; }
        public string Color => Status switch
        {
            "Beklemede" => "#ffc107",
            "Tamamlandı" => "#28a745",
            "İptal" => "#dc3545",
            "Hazırlanıyor" => "#17a2b8",
            _ => "#6c757d"
        };
    }

    public class CategorySales
    {
        public string Category { get; set; } = string.Empty;
        public decimal TotalSales { get; set; }
        public int OrderCount { get; set; }
        public string Color { get; set; } = string.Empty;
    }

    public class QuickAction
    {
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string Icon { get; set; } = string.Empty;
        public string Url { get; set; } = string.Empty;
        public string Color { get; set; } = string.Empty;
    }

    public class StockAlert
    {
        public int ProductId { get; set; }
        public string ProductName { get; set; } = string.Empty;
        public int CurrentStock { get; set; }
        public int MinStock { get; set; }
        public string AlertType { get; set; } = string.Empty;
        public string Color { get; set; } = string.Empty;
    }

    public class RevenueTrend
    {
        public string Period { get; set; } = string.Empty;
        public decimal Revenue { get; set; }
        public decimal ChangePercent { get; set; }
        public bool IsPositive => ChangePercent >= 0;
    }
}
