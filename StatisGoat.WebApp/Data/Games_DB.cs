using Microsoft.EntityFrameworkCore;
using StatisGoat.WebApp.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace StatisGoat.WebApp.Data
{
    public class Games_DB : DbContext
    {
        public Games_DB(DbContextOptions<Games_DB> context) : base(context)
        {
        }

        // Represents the database table for all the applications
        public DbSet<Match> Games { get; set; }

        // Builds the Applications DBSet to the Application table
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Match>().ToTable("Game");
        }
    }
}
