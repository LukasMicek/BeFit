using System.Net;
using BeFit.Data;
using BeFit.Models;
using BeFit.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace BeFit.IntegrationTests;

public class TrainingEntryOwnershipTests : IClassFixture<BeFitWebApplicationFactory>
{
    private readonly BeFitWebApplicationFactory _factory;

    public TrainingEntryOwnershipTests(BeFitWebApplicationFactory factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task CreateEntry_WithSessionNotOwnedByUser_FailsAndEntryNotCreated()
    {
        // Arrange
        using var scope = _factory.Services.CreateScope();
        var entryService = scope.ServiceProvider.GetRequiredService<ITrainingEntryService>();
        var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

        // Create exercise type
        var exerciseType = new ExerciseType { Name = "Bench Press" };
        context.ExerciseTypes.Add(exerciseType);

        // Create session owned by different user
        var otherUserSession = new TrainingSession
        {
            UserId = "other-user-id",
            StartTime = DateTime.Now,
            EndTime = DateTime.Now.AddHours(1)
        };
        context.TrainingSessions.Add(otherUserSession);
        await context.SaveChangesAsync();

        var dto = new TrainingEntryCreateDto
        {
            TrainingSessionId = otherUserSession.Id,
            ExerciseTypeId = exerciseType.Id,
            Weight = 100,
            Sets = 3,
            Repetitions = 10
        };

        // Act
        var result = await entryService.CreateAsync(dto, BeFitWebApplicationFactory.TestUserId);

        // Assert
        Assert.False(result.Success);
        Assert.Equal(EntryError.SessionNotOwned, result.Error);

        // Verify no entry was created
        var entriesCount = await context.TrainingEntries
            .Where(e => e.UserId == BeFitWebApplicationFactory.TestUserId)
            .CountAsync();
        Assert.Equal(0, entriesCount);
    }

    [Fact]
    public async Task HttpCreateEntry_WithSessionNotOwnedByUser_ReturnsNotFound()
    {
        // Arrange
        using var scope = _factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

        // Create exercise type
        var exerciseType = new ExerciseType { Name = "Deadlift" };
        context.ExerciseTypes.Add(exerciseType);

        // Create session owned by different user
        var otherUserSession = new TrainingSession
        {
            UserId = "other-user-id",
            StartTime = DateTime.Now,
            EndTime = DateTime.Now.AddHours(1)
        };
        context.TrainingSessions.Add(otherUserSession);
        await context.SaveChangesAsync();

        var client = _factory.CreateClient(new Microsoft.AspNetCore.Mvc.Testing.WebApplicationFactoryClientOptions
        {
            AllowAutoRedirect = false
        });

        var formData = new FormUrlEncodedContent(new[]
        {
            new KeyValuePair<string, string>("TrainingSessionId", otherUserSession.Id.ToString()),
            new KeyValuePair<string, string>("ExerciseTypeId", exerciseType.Id.ToString()),
            new KeyValuePair<string, string>("Weight", "100"),
            new KeyValuePair<string, string>("Sets", "3"),
            new KeyValuePair<string, string>("Repetitions", "10")
        });

        // Get antiforgery token first
        var getResponse = await client.GetAsync("/TrainingEntries/Create");
        var getContent = await getResponse.Content.ReadAsStringAsync();

        // Extract antiforgery token from the form
        var tokenMatch = System.Text.RegularExpressions.Regex.Match(
            getContent,
            @"name=""__RequestVerificationToken""\s+type=""hidden""\s+value=""([^""]+)""");

        if (!tokenMatch.Success)
        {
            // Try alternate pattern
            tokenMatch = System.Text.RegularExpressions.Regex.Match(
                getContent,
                @"value=""([^""]+)""\s+name=""__RequestVerificationToken""");
        }

        Assert.True(tokenMatch.Success, "Could not find antiforgery token");
        var token = tokenMatch.Groups[1].Value;

        var formDataWithToken = new FormUrlEncodedContent(new[]
        {
            new KeyValuePair<string, string>("TrainingSessionId", otherUserSession.Id.ToString()),
            new KeyValuePair<string, string>("ExerciseTypeId", exerciseType.Id.ToString()),
            new KeyValuePair<string, string>("Weight", "100"),
            new KeyValuePair<string, string>("Sets", "3"),
            new KeyValuePair<string, string>("Repetitions", "10"),
            new KeyValuePair<string, string>("__RequestVerificationToken", token)
        });

        // Copy cookies from GET request
        var cookies = getResponse.Headers.GetValues("Set-Cookie");
        var request = new HttpRequestMessage(HttpMethod.Post, "/TrainingEntries/Create")
        {
            Content = formDataWithToken
        };
        foreach (var cookie in cookies)
        {
            request.Headers.Add("Cookie", cookie.Split(';')[0]);
        }

        // Act
        var response = await client.SendAsync(request);

        // Assert - should return NotFound (404) because session is not owned by user
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }
}
