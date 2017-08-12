using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace IdentityServerSystem.Models.ManageUserClaimViewModels
{
    public class AddUserClaimViewModel
    {
        public Guid Id { get; set; }
        public string UserName { get; set; }

        public string FullName { get; set; }

        public IDictionary<string, string> PlanAddUserClaimDict { get; set; }
        public IDictionary<string, string> HasExsistUserClaims { get; set; }


        #region Public Method
        public async Task<ApplicationUser> AddUserClaims(UserManager<ApplicationUser> _userManager)
        {
            var user = await _userManager.Users.Include(a => a.Claims).Where(a => a.Id == Id).FirstOrDefaultAsync();
            if (user != null)
            {
                foreach (var planUserClaim in PlanAddUserClaimDict)
                {
                    if (user.Claims.All(a => a.ClaimType != planUserClaim.Key))
                    {
                        //UserClaim中不包含计划添加的，且Value不能为null，则需添加
                        if (planUserClaim.Value != null)
                        {
                            var newClaim = new Claim(planUserClaim.Key, planUserClaim.Value);
                            await _userManager.AddClaimAsync(user, newClaim);
                        }

                    }                   
                }
                return user;
            }
            else
            {
                return null;
            }
        }
        #endregion
    }   
}
