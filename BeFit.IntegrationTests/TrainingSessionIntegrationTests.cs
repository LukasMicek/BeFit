using BeFit.Data;
using BeFit.Models;
using BeFit.Services;
using Microsoft.Extensions.DependencyInjection;

namespace BeFit.IntegrationTests;

public class TrainingSessionIntegrationTests : IClassFixture<BeFitWebApplicationFactory>
{
    private readonly BeFitWebApplicationFactory _factory;

    public TrainingSessionIntegrationTests(BeFitWebApplicationFactory factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task TrainingSessionService_CreateAndRetrieve_WorksEndToEnd()
    {
        // Arrange
        using var scope = _factory.Services.CreateScope();
        var service = scope.ServiceProvider.GetRequiredService<ITrainingSessionService>();
        var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

        var dto = new TrainingSessionCreateDto
        {
            StartTime = DateTime.Now,
            EndTime = DateTime.Now.AddHours(1)
        };

        // Act - Create
        var created = await service.CreateAsync(dto, BeFitWebApplicationFactory.TestUserId);

        // Act - Retrieve
        var retrieved = await service.GetByIdAsync(created.Id, BeFitWebApplicationFactory.TestUserId);

        // Assert
        Assert.NotNull(retrieved);
        Assert.Equal(created.Id, retrieved.Id);
        Assert.Equal(BeFitWebApplicationFactory.TestUserId, retrieved.UserId);
        Assert.Equal(dto.StartTime, retrieved.StartTime);
        Assert.Equal(dto.EndTime, retrieved.EndTime);
    }

    [Fact]
    public async Task TrainingSessionService_GetUserSessions_ReturnsOnlyOwnSessions()
    {
        // Arrange
        using var scope = _factory.Services.CreateScope();
        var service = scope.ServiceProvider.GetRequiredService<ITrainingSessionService>();
        var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

        // Add a session for a different user directly to DB
        context.TrainingSessions.Add(new TrainingSession
        {
            UserId = "other-user",
            StartTime = DateTime.Now.AddDays(-5),
            EndTime = DateTime.Now.AddDays(-5).AddHours(1)
        });
        await context.SaveChangesAsync();

        // Create session for test user via service
        var dto = new TrainingSessionCreateDto
        {
            StartTime = DateTime.Now,
            EndTime = DateTime.Now.AddHours(1)
        };
        await service.CreateAsync(dto, BeFitWebApplicationFactory.TestUserId);

        // Act
        var userSessions = await service.GetUserSessionsAsync(BeFitWebApplicationFactory.TestUserId);

        // Assert
        Assert.All(userSessions, s => Assert.Equal(BeFitWebApplicationFactory.TestUserId, s.UserId));
    }
}
