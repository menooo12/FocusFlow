using System.Diagnostics;
using System.Security.Claims;
using FocusFlow.Data;
using FocusFlow.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FocusFlow.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly FocusFlowContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public HomeController(
            ILogger<HomeController> logger,
            FocusFlowContext context,
            UserManager<ApplicationUser> userManager)
        {
            _logger = logger;
            _context = context;
            _userManager = userManager;
        }

        public async Task<IActionResult> Index()
        {
            if (!User.Identity!.IsAuthenticated)
            {
                ViewBag.IsLoggedIn = false;
                return View();
            }

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var userName = User.Identity.Name?.Split('@')[0] ?? "Student";

            // ── Weekly target من الـ DB ───────────────────────────────────
            var appUser = await _userManager.FindByIdAsync(userId!);
            double weeklyTarget = appUser?.WeeklyTargetHours ?? 20;

            var sessions = await _context.StudySessions
                .Where(s => s.UserId == userId)
                .OrderByDescending(s => s.Date)
                .ToListAsync();

            var subjects = await _context.Subjects
                .Where(s => s.UserId == userId)
                .ToListAsync();

            double totalHours = sessions.Sum(s => s.Hours);
            int sessionCount = sessions.Count;

            var weekStart = DateTime.Today.AddDays(-(int)DateTime.Today.DayOfWeek + 1);
            double weekHours = sessions.Where(s => s.Date.Date >= weekStart).Sum(s => s.Hours);

            double focusScore = weeklyTarget > 0
                ? Math.Min(Math.Round(weekHours / weeklyTarget * 100, 1), 100)
                : 0;

            int streak = 0;
            var checkDate = DateTime.Today;
            while (sessions.Any(s => s.Date.Date == checkDate))
            { streak++; checkDate = checkDate.AddDays(-1); }

            var mostStudied = sessions
                .GroupBy(s => s.SubjectName)
                .OrderByDescending(g => g.Sum(s => s.Hours))
                .FirstOrDefault()?.Key ?? "—";

            var hoursPerSubject = sessions
                .GroupBy(s => s.SubjectId)
                .ToDictionary(g => g.Key, g => g.Sum(s => s.Hours));

            var weakestSubject = subjects
                .OrderBy(s => hoursPerSubject.TryGetValue(s.Id, out var h) ? h : 0)
                .FirstOrDefault()?.Name ?? "—";

            var lastSession = sessions.FirstOrDefault();

            var reminder = sessions
                .GroupBy(s => s.SubjectName)
                .ToDictionary(g => g.Key, g => (DateTime.Today - g.Max(s => s.Date.Date)).Days)
                .Where(kv => kv.Value >= 3)
                .OrderByDescending(kv => kv.Value)
                .Select(kv => $"You haven't studied {kv.Key} in {kv.Value} days 📚")
                .FirstOrDefault();

            var hour = DateTime.Now.Hour;
            string greeting = hour < 12 ? $"Good Morning, {userName} ☀️"
                            : hour < 17 ? $"Good Afternoon, {userName} 🌤️"
                            : hour < 21 ? $"Good Evening, {userName} 🌆"
                            : $"Keep grinding tonight, {userName} 🌙";

            var heatmapData = Enumerable.Range(0, 49).Select(i =>
            {
                var date = DateTime.Today.AddDays(-48 + i);
                var hours = sessions.Where(s => s.Date.Date == date).Sum(s => s.Hours);
                return new { date = date.ToString("yyyy-MM-dd"), hours };
            }).ToList();

            ViewBag.IsLoggedIn = true;
            ViewBag.Greeting = greeting;
            ViewBag.TotalHours = totalHours;
            ViewBag.SessionCount = sessionCount;
            ViewBag.WeekHours = weekHours;
            ViewBag.FocusScore = focusScore;
            ViewBag.TotalTargetHours = weeklyTarget;
            ViewBag.Streak = streak;
            ViewBag.MostStudied = mostStudied;
            ViewBag.WeakestSubject = weakestSubject;
            ViewBag.SubjectCount = subjects.Count;
            ViewBag.Reminder = reminder;
            ViewBag.LastSubject = lastSession?.SubjectName ?? "—";
            ViewBag.LastDate = lastSession?.Date.ToString("dd MMM") ?? "—";
            ViewBag.HeatmapJson = System.Text.Json.JsonSerializer.Serialize(heatmapData);
            ViewBag.RecentSessions = sessions.Take(5).ToList();

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