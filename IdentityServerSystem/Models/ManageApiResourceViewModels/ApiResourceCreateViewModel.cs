using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;

namespace IdentityServerSystem.Models.ManageApiResourceViewModels
{
    public class ApiResourceCreateViewModel
    {
        [Required(AllowEmptyStrings = false)]
        [Display(Name = "名称")]
        public string Name { get; set; }

        [Display(Name = "显示名称")]
        public string DisplayName { get; set; }
    }
}
