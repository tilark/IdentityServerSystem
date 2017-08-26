using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace IdentityServerSystem.Models.ManageUserViewModels
{
    /// <summary>
    /// 删除用户信息的模型
    /// </summary>
    public class DeleteUserViewModel
    {
        [Display(Name = "Id")]
        public Guid Id { get; set; }

        [Display(Name = "帐号")]
        public string UserName { get; set; }

        [Display(Name = "姓氏")]
        public string FamilyName { get; set; }

        [Display(Name = "名称")]
        public string FirstName { get; set; }

        public string FullName { get { return FamilyName + FirstName; } }

        #region Public Method
        public async Task<IdentityResult> DeleteUserAsync(UserManager<ApplicationUser> _userManager)
        {
            //删除
            var user = await _userManager.Users.FirstOrDefaultAsync(a => a.Id == Id);
            if(user != null)
            {
                //不能删除"Administrator"
                if (user.UserName != "Administrator")
                {
                    var result = await _userManager.DeleteAsync(user);
                    return result;
                }               
            }
            return null;
        }
        #endregion
    }
}
