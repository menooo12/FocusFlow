using FocusFlow.Models;
using FocusFlow.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
namespace FocusFlow.Controllers
{
    [Authorize]
    public class SubjectController : Controller
    {
        private readonly FocusFlowContext _context;

        public SubjectController(FocusFlowContext context)
        {
            _context = context;
        }

        // GET: Subject/Index
        public async Task<IActionResult> Index()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var subjects = await _context.Subjects
                .Where(s => s.UserId == userId)
                .ToListAsync();

            var sessions = await _context.StudySessions
                .Where(s => s.UserId == userId)
                .ToListAsync();
            var hoursPerSubject = sessions
                .GroupBy(s => s.SubjectId)
                .ToDictionary(g => g.Key, g => g.Sum(s => s.Hours));
            var viewModel = subjects.Select(s => new SubjectProgressViewModel
            {
                Subject = s,
                LoggedHours = hoursPerSubject.TryGetValue(s.Id, out var h) ? h : 0.0,
                ProgressPct = s.TargetHours > 0
                    ? Math.Min(Math.Round((hoursPerSubject.TryGetValue(s.Id, out var h2) ? h2 : 0.0) / s.TargetHours * 100, 1), 100)
                    : 0.0
            }).ToList();

            return View(viewModel);
        }

        // GET: Subject/Create
        public IActionResult Create() => View();

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Subject subject)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            subject.UserId = userId;

            if (ModelState.IsValid)
            {
                _context.Add(subject);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(subject);
        }

        // GET: Subject/Edit/5
        public async Task<IActionResult> Edit(int id)
        {
            var subject = await _context.Subjects.FindAsync(id);
            if (subject == null) return NotFound();
            return View(subject);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Subject subject)
        {
            if (id != subject.Id) return NotFound();

            if (ModelState.IsValid)
            {
                _context.Update(subject);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(subject);
        }

        // GET: Subject/Delete/5 (Direct Delete logic)
        public async Task<IActionResult> Delete(int id)
        {
            var subject = await _context.Subjects.FindAsync(id);
            if (subject != null)
            {
                var relatedSessions = _context.StudySessions.Where(s => s.SubjectId == id);
                _context.StudySessions.RemoveRange(relatedSessions);

                _context.Subjects.Remove(subject);

                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Index));
        }
    }
}