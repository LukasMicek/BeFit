using BeFit.Data;
using BeFit.Models;
using BeFit.Services;
using Microsoft.EntityFrameworkCore;

namespace BeFit.Tests.Services;

public class TrainingSessionServiceTests : IDisposable
{
    private readonly TestDbFactory _dbFactory;
    private readonly ApplicationDbContext _context;

    public TrainingSessionServiceTests()
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
    public async Task GetUserSessionsAsync_ReturnsOnlyUserSessions()
    {
        // Arrange
        var service = new TrainingSessionService(_context);
        var userId = "user-1";
        var otherUserId = "user-2";

        _context.TrainingSessions.AddRange(
            new TrainingSession { UserId = userId, StartTime = DateTime.Now, EndTime = DateTime.Now.AddHours(1) },
            new TrainingSession { UserId = userId, StartTime = DateTime.Now, EndTime = DateTime.Now.AddHours(1) },
            new TrainingSession { UserId = otherUserId, StartTime = DateTime.Now, EndTime = DateTime.Now.AddHours(1) }
        );
        await _context.SaveChangesAsync();

        // Act
        var result = await service.GetUserSessionsAsync(userId);

        // Assert
        Assert.Equal(2, result.Count());
        Assert.All(result, s => Assert.Equal(userId, s.UserId));
    }

    [Fact]
    public async Task GetUserSessionsAsync_ReturnsOrderedByStartTimeDescending()
    {
        // Arrange
        var service = new TrainingSessionService(_context);
        var userId = "user-1";

        var earlier = DateTime.Now.AddDays(-1);
        var later = DateTime.Now;

        _context.TrainingSessions.AddRange(
            new TrainingSession { UserId = userId, StartTime = earlier, EndTime = earlier.AddHours(1) },
            new TrainingSession { UserId = userId, StartTime = later, EndTime = later.AddHours(1) }
        );
        await _context.SaveChangesAsync();

        // Act
        var result = (await service.GetUserSessionsAsync(userId)).ToList();

        // Assert
        Assert.Equal(2, result.Count);
        Assert.True(result[0].StartTime > result[1].StartTime);
    }

    [Fact]
    public async Task GetByIdAsync_ReturnsSession_WhenBelongsToUser()
    {
        // Arrange
        var service = new TrainingSessionService(_context);
        var userId = "user-1";

        var session = new TrainingSession { UserId = userId, StartTime = DateTime.Now, EndTime = DateTime.Now.AddHours(1) };
        _context.TrainingSessions.Add(session);
        await _context.SaveChangesAsync();

        // Act
        var result = await service.GetByIdAsync(session.Id, userId);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(session.Id, result.Id);
    }

    [Fact]
    public async Task GetByIdAsync_ReturnsNull_WhenBelongsToDifferentUser()
    {
        // Arrange
        var service = new TrainingSessionService(_context);

        var session = new TrainingSession { UserId = "user-1", StartTime = DateTime.Now, EndTime = DateTime.Now.AddHours(1) };
        _context.TrainingSessions.Add(session);
        await _context.SaveChangesAsync();

        // Act
        var result = await service.GetByIdAsync(session.Id, "user-2");

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task CreateAsync_AddsSessionWithUserId()
    {
        // Arrange
        var service = new TrainingSessionService(_context);
        var userId = "user-1";
        var dto = new TrainingSessionCreateDto
        {
            StartTime = DateTime.Now,
            EndTime = DateTime.Now.AddHours(1)
        };

        // Act
        var result = await service.CreateAsync(dto, userId);

        // Assert
        Assert.Equal(userId, result.UserId);
        Assert.Equal(dto.StartTime, result.StartTime);
        Assert.Equal(dto.EndTime, result.EndTime);
        Assert.Equal(1, await _context.TrainingSessions.CountAsync());
    }

    [Fact]
    public async Task UpdateAsync_ReturnsTrue_AndUpdatesSession()
    {
        // Arrange
        var service = new TrainingSessionService(_context);
        var userId = "user-1";
        var originalTime = DateTime.Now;
        var newTime = DateTime.Now.AddDays(1);

        var session = new TrainingSession { UserId = userId, StartTime = originalTime, EndTime = originalTime.AddHours(1) };
        _context.TrainingSessions.Add(session);
        await _context.SaveChangesAsync();

        var dto = new TrainingSessionCreateDto { StartTime = newTime, EndTime = newTime.AddHours(2) };

        // Act
        var result = await service.UpdateAsync(session.Id, dto, userId);

        // Assert
        Assert.True(result);
        var updated = await _context.TrainingSessions.FindAsync(session.Id);
        Assert.Equal(newTime, updated!.StartTime);
    }

    [Fact]
    public async Task UpdateAsync_ReturnsFalse_WhenSessionNotFound()
    {
        // Arrange
        var service = new TrainingSessionService(_context);
        var dto = new TrainingSessionCreateDto { StartTime = DateTime.Now, EndTime = DateTime.Now.AddHours(1) };

        // Act
        var result = await service.UpdateAsync(999, dto, "user-1");

        // Assert
        Assert.False(result);
    }

    [Fact]
    public async Task UpdateAsync_ReturnsFalse_WhenSessionBelongsToDifferentUser()
    {
        // Arrange
        var service = new TrainingSessionService(_context);

        var session = new TrainingSession { UserId = "user-1", StartTime = DateTime.Now, EndTime = DateTime.Now.AddHours(1) };
        _context.TrainingSessions.Add(session);
        await _context.SaveChangesAsync();

        var dto = new TrainingSessionCreateDto { StartTime = DateTime.Now, EndTime = DateTime.Now.AddHours(1) };

        // Act
        var result = await service.UpdateAsync(session.Id, dto, "user-2");

        // Assert
        Assert.False(result);
    }

    [Fact]
    public async Task DeleteAsync_ReturnsTrue_AndRemovesSession()
    {
        // Arrange
        var service = new TrainingSessionService(_context);
        var userId = "user-1";

        var session = new TrainingSession { UserId = userId, StartTime = DateTime.Now, EndTime = DateTime.Now.AddHours(1) };
        _context.TrainingSessions.Add(session);
        await _context.SaveChangesAsync();

        // Act
        var result = await service.DeleteAsync(session.Id, userId);

        // Assert
        Assert.True(result);
        Assert.Equal(0, await _context.TrainingSessions.CountAsync());
    }

    [Fact]
    public async Task DeleteAsync_ReturnsFalse_WhenSessionBelongsToDifferentUser()
    {
        // Arrange
        var service = new TrainingSessionService(_context);

        var session = new TrainingSession { UserId = "user-1", StartTime = DateTime.Now, EndTime = DateTime.Now.AddHours(1) };
        _context.TrainingSessions.Add(session);
        await _context.SaveChangesAsync();

        // Act
        var result = await service.DeleteAsync(session.Id, "user-2");

        // Assert
        Assert.False(result);
        Assert.Equal(1, await _context.TrainingSessions.CountAsync());
    }
}
