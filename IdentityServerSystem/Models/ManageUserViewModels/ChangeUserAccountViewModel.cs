using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace IdentityServerSystem.Models.ManageUserViewModels
{
    public class ChangeUserAccountViewModel
    {
        public Guid Id { get; set; }

        [Display(Name = "帐号")]
        public string UserName { get; set; }

        [Required(AllowEmptyStrings = false)]
        [Display(Name = "新帐号")]
        public string NewUserName { get; set; }

        #region Public Method

        /// <summary>
        /// 更改用户的登录帐号，判断数据库中的帐号与给定的新帐号是否相同，如果不同则更改，如果相同则不需要处理
        /// </summary>
        /// <returns>如果更改了帐号，则返回处理结果IdentityResult，否则返回null</returns>
        public async Task<IdentityResult> ChangeUserAccountAsync(UserManager<ApplicationUser> _userManager)
        {
            var user = await _userManager.Users.FirstOrDefaultAsync(a => a.Id == Id);

            if (user != null)
            {
                if(user.UserName != NewUserName)
                {
                    var result = await _userManager.SetUserNameAsync(user, NewUserName);
                    return result;
                }                
            }
            return null;
        }
        #endregion

    }
}
