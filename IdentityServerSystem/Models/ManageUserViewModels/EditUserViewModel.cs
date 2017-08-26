using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using IdentityServerSystem.Models.ManageUserClaimViewModels;

namespace IdentityServerSystem.Models.ManageUserViewModels
{

    public class EditUserViewModel
    {
        public Guid Id { get; set; }
        [Display(Name = "帐号")]
        public string UserName { get; set; }
        [Display(Name = "姓")]
        public string FamilyName { get; set; }

        [Display(Name = "名")]
        public string FirstName { get; set; }


        [Display(Name = "电话号码")]
        public string Telephone { get; set; }


        #region Public Method
        /// <summary>
        /// 更新人员信息，此处只更新人员的名称，在工号相同的情况下
        /// </summary>
        /// <param name="_userManager"></param>
        /// <returns></returns>
        public async Task<ApplicationUser> UpdateUserInfo(UserManager<ApplicationUser> _userManager)
        {

            var userInfo = await _userManager.Users.FirstOrDefaultAsync(a => a.Id == Id);
            if (userInfo != null)
            {
                //如果人员的名称相同，则不处理
                if(userInfo.FamilyName == FamilyName && userInfo.FirstName == FirstName)
                {
                    return userInfo;
                }
                userInfo.FamilyName = FamilyName;
                userInfo.FirstName = FirstName;
                userInfo.Telephone = Telephone;
                var result = await _userManager.UpdateAsync(userInfo);
                if (result.Succeeded)
                {
                    //更改UserClaim
                    var userClaims = new Dictionary<string, string>();
                    userClaims.Add("family_name", FamilyName);
                    userClaims.Add("given_name", FirstName);
                    userClaims.Add("preferred_username", FamilyName + FirstName);
                    EditUserClaimViewModel updateUserClaim = new EditUserClaimViewModel
                    {
                        Id = Id,
                        UserClaims = userClaims

                    };
                    var user = await updateUserClaim.UpdateUserClaims(_userManager);

                    return user;
                }               
            }
            return null;
        }
        #endregion


    }
}
