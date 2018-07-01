using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.EntityFrameworkCore.Diagnostics;
namespace IdentityServerSystem.Models
{
    public class ApplicationDepartment
    {
        [Key]
        public Guid DepartmentID { get; set; }
        public string DepartmentName { get; set; }
        public string Remarks { get; set; }

        [Timestamp]
        public byte[] TimesStamp { get; set; }
    }
}
