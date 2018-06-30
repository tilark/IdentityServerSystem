using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore.Extensions;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Identity;

namespace IdentityServerSystem.Models
{
    // Add profile data for application users by adding properties to the ApplicationUser class
    public class ApplicationUser : IdentityUser<Guid>
    {
        [Display(Name = "姓")]
        public string FamilyName { get; set; }

        [Display(Name = "名")]
        public string FirstName { get; set; }

        
        [Display(Name = "电话号码")]
        public string Telephone { get; set; }
       

        [Display(Name = "姓名")]
        public string FullName
        {
            get
            {
                return FamilyName + " " + FirstName;
            }
        }

        /// <summary>
        /// Navigation property for the roles this user belongs to.
        /// </summary>
        public virtual ICollection<IdentityUserRole<int>> Roles { get; } = new List<IdentityUserRole<int>>();

        /// <summary>
        /// Navigation property for the claims this user possesses.
        /// </summary>
        public virtual ICollection<IdentityUserClaim<Guid>> Claims { get; } = new List<IdentityUserClaim<Guid>>();

        /// <summary>
        /// Navigation property for this users login accounts.
        /// </summary>
        public virtual ICollection<IdentityUserLogin<Guid>> Logins { get; } = new List<IdentityUserLogin<Guid>>();
    }
}
