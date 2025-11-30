using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using BeFit.Models;
using Microsoft.Identity.Client;

namespace BeFit.Data
{
    public class ApplicationDbContext : IdentityDbContext<AppUser>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }
        public DbSet<ExerciseType> ExerciseTypes { get; set; }
        public DbSet<TrainingSession> TrainingSessions { get; set; }
        public DbSet<TrainingEntry> TrainingEntries { get; set; }

    }
}
