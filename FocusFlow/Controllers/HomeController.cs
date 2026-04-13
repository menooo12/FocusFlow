using System.Diagnostics;
using System.Security.Claims;
using FocusFlow.Data;
using FocusFlow.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FocusFlow.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly FocusFlowContext _context;

        public HomeController(ILogger<HomeController> logger, FocusFlowContext context)
        {
            _logger = logger;
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            if (!User.Identity!.IsAuthenticated)
            {
                ViewBag.TotalHours = 0.0;
                ViewBag.SessionCount = 0;
                ViewBag.FocusScore = 0.0;
                ViewBag.Message = "";
                ViewBag.AlertType = "secondary";
                ViewBag.IsLoggedIn = false;
                return View();
            }

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            var sessions = await _context.StudySessions
                .Where(s => s.UserId == userId)
                .ToListAsync();

            double totalHours = sessions.Sum(s => s.Hours);
            int sessionCount = sessions.Count;

            string message, alertType;
            if (totalHours < 20)
            {
                message = "⚡ You need to focus more — keep pushing!";
                alertType = "danger";
            }
            else if (totalHours <= 40)
            {
                message = "👍 Good progress — stay consistent!";
                alertType = "warning";
            }
            else
            {
                message = "🏆 Excellent work — you're crushing it!";
                alertType = "success";
            }

            ViewBag.TotalHours = totalHours;
            ViewBag.SessionCount = sessionCount;
            ViewBag.Message = message;
            ViewBag.AlertType = alertType;
            ViewBag.FocusScore = Math.Min(Math.Round(totalHours / 80.0 * 100, 1), 100);
            ViewBag.IsLoggedIn = true;

            return View();
        }

        public IActionResult Privacy() => View();

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel
            {
                RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier
            });
        }
    }
}