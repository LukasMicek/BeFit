using BeFit.Data;
using BeFit.Models;
using BeFit.Services;
using Microsoft.EntityFrameworkCore;

namespace BeFit.Tests.Services;

public class TrainingEntryServiceTests : IDisposable
{
    private readonly TestDbFactory _dbFactory;
    private readonly ApplicationDbContext _context;

    public TrainingEntryServiceTests()
    {
        _dbFactory = new TestDbFactory();
        _context = _dbFactory.CreateContext();
    }

    public void Dispose()
    {
        _context.Dispose();
        _dbFactory.Dispose();
    }

    private async Task SeedTestData(string userId)
    {
        _context.ExerciseTypes.Add(new ExerciseType { Id = 1, Name = "Bench Press" });
        _context.TrainingSessions.Add(new TrainingSession
        {
            Id = 1,
            UserId = userId,
            StartTime = DateTime.Now,
            EndTime = DateTime.Now.AddHours(1)
        });
        await _context.SaveChangesAsync();
    }

    [Fact]
    public async Task GetUserEntriesAsync_ReturnsOnlyUserEntries()
    {
        // Arrange
        var service = new TrainingEntryService(_context);
        var userId = "user-1";
        var otherUserId = "user-2";

        await SeedTestData(userId);
        _context.TrainingSessions.Add(new TrainingSession
        {
            Id = 2,
            UserId = otherUserId,
            StartTime = DateTime.Now,
            EndTime = DateTime.Now.AddHours(1)
        });

        _context.TrainingEntries.AddRange(
            new TrainingEntry { UserId = userId, TrainingSessionId = 1, ExerciseTypeId = 1, Weight = 100, Sets = 3, Repetitions = 10 },
            new TrainingEntry { UserId = userId, TrainingSessionId = 1, ExerciseTypeId = 1, Weight = 110, Sets = 3, Repetitions = 8 },
            new TrainingEntry { UserId = otherUserId, TrainingSessionId = 2, ExerciseTypeId = 1, Weight = 80, Sets = 4, Repetitions = 12 }
        );
        await _context.SaveChangesAsync();

        // Act
        var result = await service.GetUserEntriesAsync(userId);

        // Assert
        Assert.Equal(2, result.Count());
        Assert.All(result, e => Assert.Equal(userId, e.UserId));
    }

    [Fact]
    public async Task GetUserEntriesAsync_IncludesExerciseTypeAndSession()
    {
        // Arrange
        var service = new TrainingEntryService(_context);
        var userId = "user-1";

        await SeedTestData(userId);
        _context.TrainingEntries.Add(
            new TrainingEntry { UserId = userId, TrainingSessionId = 1, ExerciseTypeId = 1, Weight = 100, Sets = 3, Repetitions = 10 }
        );
        await _context.SaveChangesAsync();

        // Act
        var result = (await service.GetUserEntriesAsync(userId)).First();

        // Assert
        Assert.NotNull(result.ExerciseType);
        Assert.NotNull(result.TrainingSession);
        Assert.Equal("Bench Press", result.ExerciseType.Name);
    }

    [Fact]
    public async Task GetByIdAsync_ReturnsEntry_WhenBelongsToUser()
    {
        // Arrange
        var service = new TrainingEntryService(_context);
        var userId = "user-1";

        await SeedTestData(userId);
        var entry = new TrainingEntry { UserId = userId, TrainingSessionId = 1, ExerciseTypeId = 1, Weight = 100, Sets = 3, Repetitions = 10 };
        _context.TrainingEntries.Add(entry);
        await _context.SaveChangesAsync();

        // Act
        var result = await service.GetByIdAsync(entry.Id, userId);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(100, result.Weight);
    }

    [Fact]
    public async Task GetByIdAsync_ReturnsNull_WhenBelongsToDifferentUser()
    {
        // Arrange
        var service = new TrainingEntryService(_context);
        var userId = "user-1";

        await SeedTestData(userId);
        var entry = new TrainingEntry { UserId = userId, TrainingSessionId = 1, ExerciseTypeId = 1, Weight = 100, Sets = 3, Repetitions = 10 };
        _context.TrainingEntries.Add(entry);
        await _context.SaveChangesAsync();

        // Act
        var result = await service.GetByIdAsync(entry.Id, "user-2");

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task CreateAsync_AddsEntryWithCorrectData()
    {
        // Arrange
        var service = new TrainingEntryService(_context);
        var userId = "user-1";

        await SeedTestData(userId);

        var dto = new TrainingEntryCreateDto
        {
            TrainingSessionId = 1,
            ExerciseTypeId = 1,
            Weight = 120,
            Sets = 4,
            Repetitions = 8
        };

        // Act
        var result = await service.CreateAsync(dto, userId);

        // Assert
        Assert.Equal(userId, result.UserId);
        Assert.Equal(120, result.Weight);
        Assert.Equal(4, result.Sets);
        Assert.Equal(8, result.Repetitions);
        Assert.Equal(1, await _context.TrainingEntries.CountAsync());
    }

    [Fact]
    public async Task UpdateAsync_ReturnsTrue_AndUpdatesEntry()
    {
        // Arrange
        var service = new TrainingEntryService(_context);
        var userId = "user-1";

        await SeedTestData(userId);
        var entry = new TrainingEntry { UserId = userId, TrainingSessionId = 1, ExerciseTypeId = 1, Weight = 100, Sets = 3, Repetitions = 10 };
        _context.TrainingEntries.Add(entry);
        await _context.SaveChangesAsync();

        var dto = new TrainingEntryCreateDto
        {
            TrainingSessionId = 1,
            ExerciseTypeId = 1,
            Weight = 120,
            Sets = 4,
            Repetitions = 6
        };

        // Act
        var result = await service.UpdateAsync(entry.Id, dto, userId);

        // Assert
        Assert.True(result);
        var updated = await _context.TrainingEntries.FindAsync(entry.Id);
        Assert.Equal(120, updated!.Weight);
        Assert.Equal(4, updated.Sets);
        Assert.Equal(6, updated.Repetitions);
    }

    [Fact]
    public async Task UpdateAsync_ReturnsFalse_WhenEntryNotFound()
    {
        // Arrange
        var service = new TrainingEntryService(_context);

        var dto = new TrainingEntryCreateDto
        {
            TrainingSessionId = 1,
            ExerciseTypeId = 1,
            Weight = 100,
            Sets = 3,
            Repetitions = 10
        };

        // Act
        var result = await service.UpdateAsync(999, dto, "user-1");

        // Assert
        Assert.False(result);
    }

    [Fact]
    public async Task UpdateAsync_ReturnsFalse_WhenEntryBelongsToDifferentUser()
    {
        // Arrange
        var service = new TrainingEntryService(_context);
        var userId = "user-1";

        await SeedTestData(userId);
        var entry = new TrainingEntry { UserId = userId, TrainingSessionId = 1, ExerciseTypeId = 1, Weight = 100, Sets = 3, Repetitions = 10 };
        _context.TrainingEntries.Add(entry);
        await _context.SaveChangesAsync();

        var dto = new TrainingEntryCreateDto
        {
            TrainingSessionId = 1,
            ExerciseTypeId = 1,
            Weight = 120,
            Sets = 4,
            Repetitions = 6
        };

        // Act
        var result = await service.UpdateAsync(entry.Id, dto, "user-2");

        // Assert
        Assert.False(result);
    }

    [Fact]
    public async Task DeleteAsync_ReturnsTrue_AndRemovesEntry()
    {
        // Arrange
        var service = new TrainingEntryService(_context);
        var userId = "user-1";

        await SeedTestData(userId);
        var entry = new TrainingEntry { UserId = userId, TrainingSessionId = 1, ExerciseTypeId = 1, Weight = 100, Sets = 3, Repetitions = 10 };
        _context.TrainingEntries.Add(entry);
        await _context.SaveChangesAsync();

        // Act
        var result = await service.DeleteAsync(entry.Id, userId);

        // Assert
        Assert.True(result);
        Assert.Equal(0, await _context.TrainingEntries.CountAsync());
    }

    [Fact]
    public async Task DeleteAsync_ReturnsFalse_WhenEntryBelongsToDifferentUser()
    {
        // Arrange
        var service = new TrainingEntryService(_context);
        var userId = "user-1";

        await SeedTestData(userId);
        var entry = new TrainingEntry { UserId = userId, TrainingSessionId = 1, ExerciseTypeId = 1, Weight = 100, Sets = 3, Repetitions = 10 };
        _context.TrainingEntries.Add(entry);
        await _context.SaveChangesAsync();

        // Act
        var result = await service.DeleteAsync(entry.Id, "user-2");

        // Assert
        Assert.False(result);
        Assert.Equal(1, await _context.TrainingEntries.CountAsync());
    }
}
