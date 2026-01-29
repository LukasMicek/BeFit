using BeFit.Data;
using BeFit.Models;
using BeFit.Services;
using Microsoft.EntityFrameworkCore;

namespace BeFit.Tests.Services;

public class StatsServiceTests : IDisposable
{
    private readonly TestDbFactory _dbFactory;
    private readonly ApplicationDbContext _context;

    public StatsServiceTests()
    {
        _dbFactory = new TestDbFactory();
        _context = _dbFactory.CreateContext();
    }

    public void Dispose()
    {
        _context.Dispose();
        _dbFactory.Dispose();
    }

    [Fact]
    public async Task GetUserStatsAsync_ReturnsOnlyUserStats()
    {
        // Arrange
        var service = new StatsService(_context);
        var userId = "user-1";
        var otherUserId = "user-2";

        _context.ExerciseTypes.Add(new ExerciseType { Id = 1, Name = "Bench Press" });
        _context.TrainingSessions.AddRange(
            new TrainingSession { Id = 1, UserId = userId, StartTime = DateTime.Now.AddDays(-1), EndTime = DateTime.Now.AddDays(-1).AddHours(1) },
            new TrainingSession { Id = 2, UserId = otherUserId, StartTime = DateTime.Now.AddDays(-1), EndTime = DateTime.Now.AddDays(-1).AddHours(1) }
        );
        _context.TrainingEntries.AddRange(
            new TrainingEntry { UserId = userId, TrainingSessionId = 1, ExerciseTypeId = 1, Weight = 100, Sets = 3, Repetitions = 10 },
            new TrainingEntry { UserId = otherUserId, TrainingSessionId = 2, ExerciseTypeId = 1, Weight = 80, Sets = 4, Repetitions = 12 }
        );
        await _context.SaveChangesAsync();

        // Act
        var result = await service.GetUserStatsAsync(userId);

        // Assert
        Assert.Single(result);
        Assert.Equal("Bench Press", result.First().ExerciseTypeName);
    }

    [Fact]
    public async Task GetUserStatsAsync_FiltersEntriesOlderThanDaysBack()
    {
        // Arrange
        var service = new StatsService(_context);
        var userId = "user-1";

        _context.ExerciseTypes.Add(new ExerciseType { Id = 1, Name = "Bench Press" });
        _context.TrainingSessions.AddRange(
            new TrainingSession { Id = 1, UserId = userId, StartTime = DateTime.Now.AddDays(-10), EndTime = DateTime.Now.AddDays(-10).AddHours(1) },
            new TrainingSession { Id = 2, UserId = userId, StartTime = DateTime.Now.AddDays(-40), EndTime = DateTime.Now.AddDays(-40).AddHours(1) }
        );
        _context.TrainingEntries.AddRange(
            new TrainingEntry { UserId = userId, TrainingSessionId = 1, ExerciseTypeId = 1, Weight = 100, Sets = 3, Repetitions = 10 },
            new TrainingEntry { UserId = userId, TrainingSessionId = 2, ExerciseTypeId = 1, Weight = 80, Sets = 3, Repetitions = 10 }
        );
        await _context.SaveChangesAsync();

        // Act
        var result = await service.GetUserStatsAsync(userId, daysBack: 28);

        // Assert
        var stats = result.First();
        Assert.Equal(1, stats.TimesPerformed);
        Assert.Equal(100, stats.MaxWeight);
    }

    [Fact]
    public async Task GetUserStatsAsync_CalculatesCorrectAggregates()
    {
        // Arrange
        var service = new StatsService(_context);
        var userId = "user-1";

        _context.ExerciseTypes.Add(new ExerciseType { Id = 1, Name = "Squat" });
        _context.TrainingSessions.Add(
            new TrainingSession { Id = 1, UserId = userId, StartTime = DateTime.Now.AddDays(-1), EndTime = DateTime.Now.AddDays(-1).AddHours(1) }
        );
        _context.TrainingEntries.AddRange(
            new TrainingEntry { UserId = userId, TrainingSessionId = 1, ExerciseTypeId = 1, Weight = 100, Sets = 3, Repetitions = 10 },
            new TrainingEntry { UserId = userId, TrainingSessionId = 1, ExerciseTypeId = 1, Weight = 120, Sets = 2, Repetitions = 8 }
        );
        await _context.SaveChangesAsync();

        // Act
        var result = (await service.GetUserStatsAsync(userId)).First();

        // Assert
        Assert.Equal("Squat", result.ExerciseTypeName);
        Assert.Equal(2, result.TimesPerformed);
        Assert.Equal(46, result.TotalRepetitions); // (3*10) + (2*8) = 30 + 16 = 46
        Assert.Equal(110, result.AverageWeight); // (100 + 120) / 2 = 110
        Assert.Equal(120, result.MaxWeight);
    }

    [Fact]
    public async Task GetUserStatsAsync_GroupsByExerciseType()
    {
        // Arrange
        var service = new StatsService(_context);
        var userId = "user-1";

        _context.ExerciseTypes.AddRange(
            new ExerciseType { Id = 1, Name = "Bench Press" },
            new ExerciseType { Id = 2, Name = "Deadlift" }
        );
        _context.TrainingSessions.Add(
            new TrainingSession { Id = 1, UserId = userId, StartTime = DateTime.Now.AddDays(-1), EndTime = DateTime.Now.AddDays(-1).AddHours(1) }
        );
        _context.TrainingEntries.AddRange(
            new TrainingEntry { UserId = userId, TrainingSessionId = 1, ExerciseTypeId = 1, Weight = 100, Sets = 3, Repetitions = 10 },
            new TrainingEntry { UserId = userId, TrainingSessionId = 1, ExerciseTypeId = 2, Weight = 150, Sets = 5, Repetitions = 5 }
        );
        await _context.SaveChangesAsync();

        // Act
        var result = (await service.GetUserStatsAsync(userId)).ToList();

        // Assert
        Assert.Equal(2, result.Count);
        Assert.Contains(result, s => s.ExerciseTypeName == "Bench Press");
        Assert.Contains(result, s => s.ExerciseTypeName == "Deadlift");
    }

    [Fact]
    public async Task GetUserStatsAsync_ReturnsOrderedByExerciseName()
    {
        // Arrange
        var service = new StatsService(_context);
        var userId = "user-1";

        _context.ExerciseTypes.AddRange(
            new ExerciseType { Id = 1, Name = "Squat" },
            new ExerciseType { Id = 2, Name = "Bench Press" }
        );
        _context.TrainingSessions.Add(
            new TrainingSession { Id = 1, UserId = userId, StartTime = DateTime.Now.AddDays(-1), EndTime = DateTime.Now.AddDays(-1).AddHours(1) }
        );
        _context.TrainingEntries.AddRange(
            new TrainingEntry { UserId = userId, TrainingSessionId = 1, ExerciseTypeId = 1, Weight = 100, Sets = 3, Repetitions = 10 },
            new TrainingEntry { UserId = userId, TrainingSessionId = 1, ExerciseTypeId = 2, Weight = 80, Sets = 3, Repetitions = 10 }
        );
        await _context.SaveChangesAsync();

        // Act
        var result = (await service.GetUserStatsAsync(userId)).ToList();

        // Assert
        Assert.Equal("Bench Press", result[0].ExerciseTypeName);
        Assert.Equal("Squat", result[1].ExerciseTypeName);
    }

    [Fact]
    public async Task GetUserStatsAsync_ReturnsEmptyList_WhenNoEntries()
    {
        // Arrange
        var service = new StatsService(_context);

        // Act
        var result = await service.GetUserStatsAsync("user-1");

        // Assert
        Assert.Empty(result);
    }
}
