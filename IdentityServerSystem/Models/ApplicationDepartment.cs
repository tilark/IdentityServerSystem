using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore.Design;
namespace IdentityServerSystem.Models
{
    public class ApplicationDepartment
    {
        [Key]
        public Guid DepartmentID { get; set; }

        [Required(AllowEmptyStrings = false)]
        [Display(Name ="科室名称")]
        public string DepartmentName { get; set; }

        [Display(Name = "备注")]

        public string Remarks { get; set; }
        [ScaffoldColumn(false)]
        [Timestamp]
        public byte[] TimesStamp { get; set; }
    }
}
