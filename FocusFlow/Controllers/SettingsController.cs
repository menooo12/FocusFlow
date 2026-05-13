using FocusFlow.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace FocusFlow.Controllers
{
    [Authorize]
    public class SettingsController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;

        public SettingsController(UserManager<ApplicationUser> userManager)
        {
            _userManager = userManager;
        }

        // GET: Settings
        public async Task<IActionResult> Index()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return NotFound();

            ViewBag.WeeklyTargetHours = user.WeeklyTargetHours;
            return View();
        }

        // POST: Settings
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Index(int weeklyTargetHours)
        {
            if (weeklyTargetHours < 1 || weeklyTargetHours > 168)
            {
                ModelState.AddModelError("", "Target must be between 1 and 168 hours.");
                ViewBag.WeeklyTargetHours = weeklyTargetHours;
                return View();
            }

            var user = await _userManager.GetUserAsync(User);
            if (user == null) return NotFound();

            user.WeeklyTargetHours = weeklyTargetHours;
            await _userManager.UpdateAsync(user);

            TempData["Success"] = "Settings saved successfully!";
            return RedirectToAction(nameof(Index));
        }
    }
}