using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using FocusFlow.Data;
using FocusFlow.Models;

namespace FocusFlow.Controllers
{
    public class FocusTasksController : Controller
    {
        private readonly FocusFlowContext _context;

        public FocusTasksController(FocusFlowContext context)
        {
            _context = context;
        }

        // GET: FocusTasks
        public async Task<IActionResult> Index()
        {
            return View(await _context.FocusTask.ToListAsync());
        }

        // GET: FocusTasks/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var focusTask = await _context.FocusTask
                .FirstOrDefaultAsync(m => m.Id == id);
            if (focusTask == null)
            {
                return NotFound();
            }

            return View(focusTask);
        }

        // GET: FocusTasks/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: FocusTasks/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,Title,IsComplete,DueDate,Notes")] FocusTask focusTask)
        {
            if (ModelState.IsValid)
            {
                _context.Add(focusTask);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(focusTask);
        }

        // GET: FocusTasks/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var focusTask = await _context.FocusTask.FindAsync(id);
            if (focusTask == null)
            {
                return NotFound();
            }
            return View(focusTask);
        }

        // POST: FocusTasks/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Title,IsComplete,DueDate,Notes")] FocusTask focusTask)
        {
            if (id != focusTask.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(focusTask);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!FocusTaskExists(focusTask.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            return View(focusTask);
        }

        // GET: FocusTasks/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var focusTask = await _context.FocusTask
                .FirstOrDefaultAsync(m => m.Id == id);
            if (focusTask == null)
            {
                return NotFound();
            }

            return View(focusTask);
        }

        // POST: FocusTasks/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var focusTask = await _context.FocusTask.FindAsync(id);
            if (focusTask != null)
            {
                _context.FocusTask.Remove(focusTask);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool FocusTaskExists(int id)
        {
            return _context.FocusTask.Any(e => e.Id == id);
        }
    }
}
