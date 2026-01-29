using BeFit.Data;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

namespace BeFit.Tests;

public class TestDbFactory : IDisposable
{
    private readonly SqliteConnection _connection;

    public TestDbFactory()
    {
        // Keep connection open to preserve in-memory database
        _connection = new SqliteConnection("DataSource=:memory:");
        _connection.Open();

        // Disable foreign key enforcement for tests
        using var cmd = _connection.CreateCommand();
        cmd.CommandText = "PRAGMA foreign_keys = OFF;";
        cmd.ExecuteNonQuery();
    }

    public ApplicationDbContext CreateContext()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseSqlite(_connection)
            .Options;

        var context = new ApplicationDbContext(options);
        context.Database.EnsureCreated();
        return context;
    }

    public void Dispose()
    {
        _connection.Dispose();
    }
}
