using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;

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

       
    }
}
