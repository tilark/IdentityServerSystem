using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace IdentityServerSystem.Models.AccountViewModels
{
    public class LoginViewModel
    {

        [Required(AllowEmptyStrings = false)]
        [Display(Name ="帐号")]
        public string UserName { get; set; }
        //[Required]
        [EmailAddress]
        public string Email { get; set; }

        [Required]
        [DataType(DataType.Password)]
        [Display(Name = "密码")]
        public string Password { get; set; }

        [Display(Name = "记住登陆信息?")]
        public bool RememberMe { get; set; }
    }
}
