using IdentityServerSystem.Models;
using IdentityServerSystem.Models.ManageUserViewModels;
using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace IdentityServerSystem.Data
{
    public static class ApplicationDbContextExtensions
    {
        public static void EnsureSeedData(this ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            //if (!context.Users.Any())
            //{
                var adminUser = new CreateUserViewModel { Id = Guid.NewGuid(), UserName = "Administrator", Password = "52166057", ConfirmPassword = "52166057", FamilyName = "Administrator", FirstName = "Administrator" };
                var user = adminUser.CreateUserAsync(userManager).Result;
            //}
        }
    }
}
