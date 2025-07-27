using b2b.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System.Diagnostics;

namespace b2b.Controllers
{
    [Authorize]
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly B2BContext _context;

        public HomeController(ILogger<HomeController> logger, B2BContext context)
        {
            _logger = logger;
            _context = context;
        }

        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }

        // Veritabaný baðlantý test action'ý
        public IActionResult TestDb()
        {
            var userCount = _context.Users.Count();
            return Content("User count: " + userCount);
        }
    }
}