using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using StatisGoat.Data;

namespace StatisGoat.Areas.Identity.Data
{
    public class SeedUsersRolesDB
    {
        public static async Task Initialize(RoleManager<IdentityRole> rm, UserManager<SGUser> um, SGUsersRolesDB context)
        {
            context.Database.EnsureCreated();

            // Look for any applications.
            if (rm.Roles.ToList().Any() && um.Users.Any())
            {
                return;   // DB has been seeded
            }

            await rm.CreateAsync(new IdentityRole("Administrator"));
            await rm.CreateAsync(new IdentityRole("User"));
            //await rm.CreateAsync(new IdentityRole("Professor"));
            //await rm.CreateAsync(new IdentityRole("Applicant"));

            SGUser admin = new SGUser { UserName = "admin@utah.edu", Name = "admin", Email = "admin@utah.edu", EmailConfirmed = true };
            IdentityResult r1 = um.CreateAsync(admin, "123ABC!@#def").Result;

            if (r1.Succeeded)
            {
                await um.AddToRoleAsync(admin, "Administrator");
            }

            //TAUser professor = new TAUser { UserName = "professor@utah.edu", Name = "professor", Email = "professor@utah.edu", EmailConfirmed = true, HasApplication = false };
            //IdentityResult r2 = um.CreateAsync(professor, "123ABC!@#def").Result;

            //if (r2.Succeeded)
            //{
            //    await um.AddToRoleAsync(professor, "Professor");
            //}

            //TAUser applicant1 = new TAUser { UserName = "u0000000@utah.edu", Name = "applicant1", uID = "u0000000", Email = "u0000000@utah.edu", EmailConfirmed = true, HasApplication = false};
            //IdentityResult r3 = um.CreateAsync(applicant1, "123ABC!@#def").Result;

            //if (r3.Succeeded)
            //{
            //    await um.AddToRoleAsync(applicant1, "Applicant");
            //}

            context.SaveChanges();
        }
    }
}