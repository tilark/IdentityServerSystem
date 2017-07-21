using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;

namespace IdentityServerSystem.Models.ManageUserViewModels
{

    public class EditUserViewModel
    {
        public Guid id { get; set; }
        [Display(Name = "帐号")]
        public string UserName { get; set; }
        [Display(Name = "姓")]
        public string FamilyName { get; set; }

        [Display(Name = "名")]
        public string FirstName { get; set; }


        [Display(Name = "电话号码")]
        public string Telephone { get; set; }

        [Display(Name = "所属科室")]
        public Guid? DepartmentID { get; set; }

    }
}
