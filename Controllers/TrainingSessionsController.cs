using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using BeFit.Data;
using BeFit.Models;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;



namespace BeFit.Controllers
{
    [Authorize]
    public class TrainingSessionsController : Controller
    {
        private readonly ApplicationDbContext _context;

        public TrainingSessionsController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: TrainingSessions
        public async Task<IActionResult> Index()
        {
            var userId = GetUserId();
            if (userId == null)
                return Challenge();

            var sessions = await _context.TrainingSessions
                .Where(s => s.UserId == userId)
                .OrderByDescending(s => s.StartTime)
                .ToListAsync();

            return View(sessions);
        }


        // GET: TrainingSessions/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
                return NotFound();

            var userId = GetUserId();
            if (userId == null)
                return Challenge();

            var trainingSession = await _context.TrainingSessions
                .FirstOrDefaultAsync(s => s.Id == id && s.UserId == userId);

            if (trainingSession == null)
                return NotFound(); 

            return View(trainingSession);
        }


        // GET: TrainingSessions/Create
        public IActionResult Create()
        {
            var model = new TrainingSessionCreateDto
            {
                StartTime = DateTime.Now,
                EndTime = DateTime.Now
            };

            return View(model);
        }


        // POST: TrainingSessions/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(TrainingSessionCreateDto dto)
        {
            if (!ModelState.IsValid)
            {
                return View(dto);
            }

            var userId = GetUserId();
            if (userId == null)
            {
                return Challenge();
            }

            var session = new TrainingSession
            {
                StartTime = dto.StartTime,
                EndTime = dto.EndTime,
                UserId = userId
            };

            _context.Add(session);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        // GET: TrainingSessions/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
                return NotFound();

            var userId = GetUserId();
            if (userId == null)
                return Challenge();

            var trainingSession = await _context.TrainingSessions
                .FirstOrDefaultAsync(s => s.Id == id && s.UserId == userId);

            if (trainingSession == null)
                return NotFound(); 

            return View(trainingSession);
        }


        // POST: TrainingSessions/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, TrainingSessionCreateDto dto)
        {
            if (!ModelState.IsValid)
                return View(dto);

            var userId = GetUserId();
            if (userId == null)
                return Challenge();

            var session = await _context.TrainingSessions
                .FirstOrDefaultAsync(s => s.Id == id && s.UserId == userId);

            if (session == null)
                return NotFound();

            session.StartTime = dto.StartTime;
            session.EndTime = dto.EndTime;

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }


        // GET: TrainingSessions/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
                return NotFound();

            var userId = GetUserId();
            if (userId == null)
                return Challenge();

            var trainingSession = await _context.TrainingSessions
                .FirstOrDefaultAsync(s => s.Id == id && s.UserId == userId);

            if (trainingSession == null)
                return NotFound();

            return View(trainingSession);
        }


        // POST: TrainingSessions/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var userId = GetUserId();
            if (userId == null)
                return Challenge();

            var session = await _context.TrainingSessions
                .FirstOrDefaultAsync(s => s.Id == id && s.UserId == userId);

            if (session == null)
                return NotFound();

            _context.TrainingSessions.Remove(session);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private string? GetUserId()
        {
            return User.FindFirstValue(ClaimTypes.NameIdentifier);
        }

    }
}
