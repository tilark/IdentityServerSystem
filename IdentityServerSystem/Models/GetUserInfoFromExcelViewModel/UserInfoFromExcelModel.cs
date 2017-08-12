using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;
using IdentityServerSystemResource;
namespace IdentityServerSystem.Models.GetUserInfoFromExcelViewModel
{
    public class UserInfoFromExcelModel
    {
        [Display(ResourceType = typeof(Resource), Name = "Id")]
        public Guid Id { get; set; }

        [Display(ResourceType = typeof(Resource), Name = "UserName")]

        public string UserName { get; set; }

        [Display(ResourceType = typeof(Resource), Name = "EmployeeNo")]

        public string EmployeeNo { get; set; }

        [Display(ResourceType = typeof(Resource), Name = "Department")]
        public string Department { get; set; }

        [Display(ResourceType = typeof(Resource), Name = "Gender")]
        public string Gender { get; set; }

        [Display(ResourceType = typeof(Resource), Name = "PersonCategory")]
        public string PersonCategory { get; set; }

        [Display(ResourceType = typeof(Resource), Name = "DeleteFlag")]
        public string DeleteFlag { get; set; }
    }
}
