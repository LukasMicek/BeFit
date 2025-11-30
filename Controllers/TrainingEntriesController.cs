using BeFit.Data;
using BeFit.Models;
using Humanizer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;



namespace BeFit.Controllers
{
    [Authorize]
    public class TrainingEntriesController : Controller
    {
        private readonly ApplicationDbContext _context;

        public TrainingEntriesController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: TrainingEntries
        public async Task<IActionResult> Index()
        {
            var userId = GetUserId();
            if (userId == null)
                return Challenge();

            var entries = await _context.TrainingEntries
                .Include(e => e.ExerciseType)
                .Include(e => e.TrainingSession)
                .Where(e => e.UserId == userId)
                .OrderByDescending(e => e.TrainingSession!.StartTime)
                .ToListAsync();

            return View(entries);
        }


        // GET: TrainingEntries/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
                return NotFound();

            var userId = GetUserId();
            if (userId == null)
                return Challenge();

            var entry = await _context.TrainingEntries
                .Include(e => e.ExerciseType)
                .Include(e => e.TrainingSession)
                .FirstOrDefaultAsync(e => e.Id == id && e.UserId == userId);

            if (entry == null)
                return NotFound();

            return View(entry);
        }

        // GET: TrainingEntries/Create
        public IActionResult Create()
        {
            var userId = GetUserId();
            if (userId == null)
                return Challenge();

            PopulateDropdowns(userId);
            var model = new TrainingEntryCreateDto();
            return View(model);
        }



        // POST: TrainingEntries/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(TrainingEntryCreateDto dto)
        {
            var userId = GetUserId();
            if (userId == null)
                return Challenge();

            if (!ModelState.IsValid)
            {
                PopulateDropdowns(userId, dto.ExerciseTypeId, dto.TrainingSessionId);
                return View(dto);
            }

            var entry = new TrainingEntry
            {
                TrainingSessionId = dto.TrainingSessionId,
                ExerciseTypeId = dto.ExerciseTypeId,
                Weight = dto.Weight,
                Sets = dto.Sets,
                Repetitions = dto.Repetitions,
                UserId = userId
            };

            _context.Add(entry);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }



        // GET: TrainingEntries/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
                return NotFound();

            var userId = GetUserId();
            if (userId == null)
                return Challenge();

            var entry = await _context.TrainingEntries
                .FirstOrDefaultAsync(e => e.Id == id && e.UserId == userId);

            if (entry == null)
                return NotFound();

            var dto = new TrainingEntryCreateDto
            {
                TrainingSessionId = entry.TrainingSessionId,
                ExerciseTypeId = entry.ExerciseTypeId,
                Weight = entry.Weight,
                Sets = entry.Sets,
                Repetitions = entry.Repetitions
            };

            PopulateDropdowns(userId, dto.ExerciseTypeId, dto.TrainingSessionId);
            return View(dto);
        }


        // POST: TrainingEntries/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(TrainingEntryCreateDto dto)
        {
            var userId = GetUserId();
            if (userId == null)
                return Challenge();

            if (!ModelState.IsValid)
            {
                PopulateDropdowns(userId, dto.ExerciseTypeId, dto.TrainingSessionId);
                return View(dto);
            }

            var entry = new TrainingEntry
            {
                TrainingSessionId = dto.TrainingSessionId,
                ExerciseTypeId = dto.ExerciseTypeId,
                Weight = dto.Weight,
                Sets = dto.Sets,
                Repetitions = dto.Repetitions,
                UserId = userId
            };

            _context.Add(entry);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        // GET: TrainingEntries/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
                return NotFound();

            var userId = GetUserId();
            if (userId == null)
                return Challenge();

            var entry = await _context.TrainingEntries
                .Include(e => e.ExerciseType)
                .Include(e => e.TrainingSession)
                .FirstOrDefaultAsync(e => e.Id == id && e.UserId == userId);

            if (entry == null)
                return NotFound();

            return View(entry);
        }

        // POST: TrainingEntries/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var trainingEntry = await _context.TrainingEntries.FindAsync(id);
            if (trainingEntry != null)
            {
                _context.TrainingEntries.Remove(trainingEntry);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool TrainingEntryExists(int id)
        {
            return _context.TrainingEntries.Any(e => e.Id == id);
        }

        private string? GetUserId()
        {
            return User.FindFirstValue(ClaimTypes.NameIdentifier);
        }

        private void PopulateDropdowns(string userId, int? selectedExerciseTypeId = null, int? selectedSessionId = null)
        {
            ViewData["ExerciseTypeId"] = new SelectList(
                _context.ExerciseTypes,
                "Id",
                "Name",
                selectedExerciseTypeId
            );

            ViewData["TrainingSessionId"] = new SelectList(
                _context.TrainingSessions
                    .Where(s => s.UserId == userId),
                "Id",
                "StartTime",
                selectedSessionId
            );
        }

    }
}
