﻿using IdentityServerSystem.Models;
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
                var adminUser = new CreateUserViewModel { Id = Guid.Parse("1F128859-C245-425F-AA73-904CE4078887"), UserName = "Administrator", Password = "52166057", ConfirmPassword = "52166057", FamilyName = "Administrator", FirstName = "Administrator" };
                var user = adminUser.CreateUserAsync(userManager).Result;

            //添加 Claims
            if(user != null)
            {
                var adminUserClaimDict = new Dictionary<string, string>();
                adminUserClaimDict.Add("Administrator", "Administrator");
                var adminUserClaim = new Models.ManageUserClaimViewModels.AddUserClaimViewModel
                {
                    Id = adminUser.Id,
                    FullName = adminUser.FirstName + adminUser.FirstName,
                    UserName = adminUser.UserName,
                    PlanAddUserClaimDict = adminUserClaimDict
                };
                var user2 = adminUserClaim.AddUserClaims(userManager).Result;
            }
            
            //}
        }
    }
}
