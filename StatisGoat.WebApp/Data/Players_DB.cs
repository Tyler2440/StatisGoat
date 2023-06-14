using Microsoft.EntityFrameworkCore;
using StatisGoat.WebApp.Models;

namespace StatisGoat.WebApp.Data
{
    public class Players_DB : DbContext
    {
        public Players_DB(DbContextOptions<Players_DB> context) : base(context)
        {
        }

        // Represents the database table for all the applications
        public DbSet<Player> Players { get; set; }

        // Builds the Applications DBSet to the Application table
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Player>().ToTable("Player");
        }
    }
}
