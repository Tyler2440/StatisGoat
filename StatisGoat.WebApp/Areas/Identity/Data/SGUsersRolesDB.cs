﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using StatisGoat.Areas.Identity.Data;

namespace StatisGoat.Data
{
    public class SGUsersRolesDB : IdentityDbContext<SGUser>
    {
        public SGUsersRolesDB(DbContextOptions<SGUsersRolesDB> options)
            : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
        }
    }
}
