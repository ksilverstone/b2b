using System.Collections.Generic;
using b2b.Models;

public class DashboardViewModel
{
    public List<Offer> Offers { get; set; } = new List<Offer>();
    public List<Order> Orders { get; set; } = new List<Order>();
    public List<Campaign> Campaigns { get; set; } = new List<Campaign>();
    public List<Notification> Notifications { get; set; } = new List<Notification>();
    public List<string> Roles { get; set; } = new List<string>();
} 