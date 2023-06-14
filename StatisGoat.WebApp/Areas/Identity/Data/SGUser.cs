using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;

namespace StatisGoat.Areas.Identity.Data
{
    // Add profile data for application users by adding properties to the SGUser class
    public class SGUser : IdentityUser
    {
        public string Name { get; set; }
        public string Email { get; set; }
        public string FavoriteTeam { get; set; }

        // Any fields we have on the account creation page/want users to have will go here
    }
}
