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

        [Required(AllowEmptyStrings = false)]
        [Display(Name = "帐号")]
        public string UserName { get; set; }

        [Required(AllowEmptyStrings = false)]
        [Display(Name = "新帐号")]
        public string NewUserName { get; set; }

    }
}
