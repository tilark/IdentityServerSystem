using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;

namespace IdentityServerSystem.Models.ManageIdentityResourceViewModel
{
    public class IdentityResourcesEditViewModel
    {
        public int Id { get; set; }

        [Required(AllowEmptyStrings = false)]
        [Display(Name = "名称")]
        public string Name { get; set; }

        [Display(Name = "显示名称")]
        public string DisplayName { get; set; }

        [Display(Name = "Emphasize")]
        public bool Emphasize { get; set; }

        [Display(Name = "ClaimTypes")]

        public List<string> ClaimTypes { get; set; }
    }
}
