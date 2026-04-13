using FocusFlow.Models;
using FocusFlow.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims; 
using Microsoft.AspNetCore.Authorization;
namespace FocusFlow.Controllers
{
    [Authorize]
    public class StudySessionController : Controller
    {
        private readonly FocusFlowContext _context;

        public StudySessionController(FocusFlowContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            var sessions = await _context.StudySessions
                .Where(s => s.UserId == userId) // فلترة المستخدم
                .OrderByDescending(s => s.Date)
                .ToListAsync();

            return View(sessions);
        }

        [HttpGet]
        public async Task<IActionResult> Log()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            var subjects = await _context.Subjects
                .Where(s => s.UserId == userId)
                .ToListAsync();

            if (!subjects.Any())
            {
                TempData["Warning"] = "Please add a subject first.";
                return RedirectToAction("Create", "Subject");
            }

            ViewBag.SubjectList = subjects
                .Select(s => new SelectListItem
                {
                    Value = s.Id.ToString(),
                    Text = s.Name
                }).ToList();

            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Log(StudySession session)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (ModelState.IsValid)
            {
                var subject = await _context.Subjects
                    .FirstOrDefaultAsync(s => s.Id == session.SubjectId && s.UserId == userId);

                if (subject != null)
                {
                    session.SubjectName = subject.Name;
                    session.UserId = userId; 

                    _context.StudySessions.Add(session);
                    await _context.SaveChangesAsync();

                    return RedirectToAction(nameof(Index));
                }
            }

            var userSubjects = await _context.Subjects.Where(s => s.UserId == userId).ToListAsync();
            ViewBag.SubjectList = userSubjects
                .Select(s => new SelectListItem
                {
                    Value = s.Id.ToString(),
                    Text = s.Name
                }).ToList();

            return View(session);
        }
    }
}