using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace IdentityServerSystem.Models.ManageUserClaimViewModels
{
    public class EditUserClaimViewModel
    {
        public Guid Id { get; set; }
        public string UserName { get; set; }

        public string FullName { get; set; }

        public IDictionary<string, string> UserClaims { get; set; }

        #region 更新用户的UserClaims
        public async Task<ApplicationUser> UpdateUserClaims(UserManager<ApplicationUser> userManager)
        {
            bool success = false;
            var user = await userManager.Users.Include(a => a.Claims).Where(a => a.Id == Id).FirstOrDefaultAsync();
            if (user != null)
            {
                foreach (var userClaim in UserClaims)
                {
                    var findClaim = user.Claims.AsParallel().FirstOrDefault(a => a.ClaimType == userClaim.Key);
                    if (findClaim != null)
                    {
                        if (findClaim.ClaimValue != userClaim.Value)
                        {
                            //更改user的UserClaim

                            var newClaim = new Claim(userClaim.Key, userClaim.Value);
                            try
                            {
                                var result = await userManager.ReplaceClaimAsync(user, findClaim.ToClaim(), newClaim);
                                success = result.Succeeded ? true : false;
                            }
                            catch (Exception)
                            {
                                success = false;
                            }
                        }
                    }
                }
            }
            return success ? user : null;
        }
        #endregion
    }
}
