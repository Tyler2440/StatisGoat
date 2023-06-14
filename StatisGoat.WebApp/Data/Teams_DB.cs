using Microsoft.EntityFrameworkCore;
using StatisGoat.WebApp.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace StatisGoat.WebApp.Data
{
    public class Teams_DB : DbContext
    {
        public Teams_DB(DbContextOptions<Teams_DB> context) : base(context)
        {
        }

        // Represents the database table for all the applications
        public DbSet<Team> Teams { get; set; }

        // Builds the Applications DBSet to the Application table
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Team>().ToTable("Team");
        }
    }
}
