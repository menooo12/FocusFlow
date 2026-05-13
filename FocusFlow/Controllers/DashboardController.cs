using FocusFlow.Models;
using FocusFlow.Services;
using FocusFlow.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;

namespace FocusFlow.Controllers
{
    [Authorize]
    public class DashboardViewModel
    {
        public double TotalHours { get; set; }
        public double WeeklyHours { get; set; }
        public double TargetHours { get; set; }
        public double ScorePercentage { get; set; }
        public string FeedbackMessage { get; set; } = string.Empty;
        public string FeedbackLevel { get; set; } = string.Empty;
        public string BadgeColor { get; set; } = string.Empty;
        public int SubjectCount { get; set; }
        public string StrongestSubject { get; set; } = "—";
        public double StrongestHours { get; set; }
        public string WeakestSubject { get; set; } = "—";
        public double WeakestHours { get; set; }
        public Dictionary<string, double> HoursPerSubject { get; set; } = new Dictionary<string, double>();
        public List<StudySession> RecentSessions { get; set; } = new List<StudySession>();
    }

    public class DashboardController : Controller
    {
        private readonly FocusScoreService _scoreService;
        private readonly FocusFlowContext _context;

        public DashboardController(FocusScoreService scoreService, FocusFlowContext context)
        {
            _scoreService = scoreService;
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            var sessions = await _context.StudySessions
                .Where(s => s.UserId == userId)
                .ToListAsync();

            var subjects = await _context.Subjects
                .Where(s => s.UserId == userId)
                .ToListAsync();


            const double targetWeeklyHours = 20.0;


            int daysSinceSaturday = ((int)DateTime.Today.DayOfWeek + 1) % 7;
            var startOfWeek = DateTime.Today.AddDays(-daysSinceSaturday);

            var weeklySessions = sessions
                .Where(s => s.Date.Date >= startOfWeek)
                .ToList();

            double weeklyHours = weeklySessions.Sum(s => s.Hours);

            double calculatedScore = targetWeeklyHours > 0
                ? Math.Min(Math.Round((weeklyHours / targetWeeklyHours) * 100, 1), 100)
                : 0;

            var scoreResult = _scoreService.GetFullResult(weeklySessions, targetWeeklyHours);
            var hoursPerSubject = _scoreService.CalculateHoursPerSubject(sessions);

            string strongestName = "—";
            double strongestHours = 0;
            if (hoursPerSubject.Any())
            {
                var strongest = hoursPerSubject.MaxBy(kv => kv.Value);
                strongestName = strongest.Key;
                strongestHours = strongest.Value;
            }

            string weakestName = "—";
            double weakestHours = 0;
            if (subjects.Any())
            {
                var allSubjectHours = subjects.ToDictionary(
                    s => s.Name,
                    s => hoursPerSubject.TryGetValue(s.Name, out var h) ? h : 0.0
                );
                var weakest = allSubjectHours.MinBy(kv => kv.Value);
                weakestName = weakest.Key;
                weakestHours = weakest.Value;
            }

            var vm = new DashboardViewModel
            {
                TotalHours = sessions.Sum(s => s.Hours),
                WeeklyHours = weeklyHours,
                TargetHours = targetWeeklyHours,
                ScorePercentage = calculatedScore,
                FeedbackMessage = scoreResult.FeedbackMessage,
                FeedbackLevel = scoreResult.FeedbackLevel,
                BadgeColor = scoreResult.BadgeColor,
                SubjectCount = subjects.Count,
                StrongestSubject = strongestName,
                StrongestHours = strongestHours,
                WeakestSubject = weakestName,
                WeakestHours = weakestHours,
                HoursPerSubject = hoursPerSubject,
                RecentSessions = sessions
                    .OrderByDescending(s => s.Date)
                    .Take(5)
                    .ToList()
            };

            return View(vm);
        }
    }
}