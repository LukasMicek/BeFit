using BeFit.Data;
using BeFit.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

[Authorize]
public class StatsController : Controller
{
    private readonly ApplicationDbContext _context;

    public StatsController(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<IActionResult> Index()
    {
        var userId = GetUserId();
        if (userId == null)
            return Challenge();

        var fourWeeksAgo = DateTime.Now.AddDays(-28);

        var statsQuery = _context.TrainingEntries
            .Include(te => te.TrainingSession)
            .Include(te => te.ExerciseType)
            .Where(te => te.UserId == userId
                         && te.TrainingSession != null
                         && te.TrainingSession.StartTime >= fourWeeksAgo)
            .GroupBy(te => new { te.ExerciseTypeId, te.ExerciseType!.Name })
            .Select(g => new ExerciseStatsViewModel
            {
                ExerciseTypeName = g.Key.Name,
                TimesPerformed = g.Count(),
                TotalRepetitions = g.Sum(te => te.Sets * te.Repetitions),
                AverageWeight = g.Average(te => te.Weight),
                MaxWeight = g.Max(te => te.Weight)
            })
            .OrderBy(s => s.ExerciseTypeName);

        var stats = await statsQuery.ToListAsync();

        return View(stats);
    }

    private string? GetUserId()
    {
        return User.FindFirstValue(ClaimTypes.NameIdentifier);
    }
}
