using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Identity;
using System.Security.Claims;

namespace IdentityServerSystem.Models.ManageUserViewModels
{
    public class CreateUserViewModel
    {
        [Display(Name = "Id")]
        public Guid Id { get; set; }

        [Required(AllowEmptyStrings = false)]
        [Display(Name = "帐号")]
        public string UserName { get; set; }

        [Required]
        [StringLength(100, ErrorMessage = "The {0} must be at least {2} and at max {1} characters long.", MinimumLength = 4)]
        [DataType(DataType.Password)]
        [Display(Name = "密码")]
        public string Password { get; set; }

        [DataType(DataType.Password)]
        [Display(Name = "确认密码")]
        [Compare("Password", ErrorMessage = "The password and confirmation password do not match.")]
        public string ConfirmPassword { get; set; }

        [Display(Name = "姓氏")]
        public string FamilyName { get; set; }

        [Display(Name = "名字")]
        public string FirstName { get; set; }


        [Display(Name = "电话号码")]
        public string Telephone { get; set; }

        #region public Method

        public async Task<ApplicationUser> CreateUserAsync(UserManager<ApplicationUser> _userManager)
        {
            //先创建一个新的ApplicationUser，看是否成功，成功返回该用户信息
            var user = new ApplicationUser { Id = Id, UserName = UserName, FamilyName = FamilyName, FirstName = FirstName, Telephone = Telephone };
            var query = await _userManager.FindByNameAsync(user.UserName);
            if(query != null)
            {
                //表示已经存在
                return null;
            }
            var result = await _userManager.CreateAsync(user, Password);

            if (result.Succeeded)
            {
                await _userManager.AddClaimsAsync(user, new Claim[]
                {
                    new Claim("family_name", FamilyName),
                    new Claim("given_name", FirstName),
                    new Claim("preferred_username", user.FullName)
                });

                //如果是UserName是“Administrator"，添加"Administrator"的UserClaim
                if (String.Equals(UserName, "Administrator"))
                {
                    await _userManager.AddClaimsAsync(user, new Claim[]
                    {
                    new Claim("Administrator", "Administrator")
                    });
                }
                return user;
            }
            else
            {               
                //不成功，返回null
                return null;
            }
        }
        #endregion
    }
}
