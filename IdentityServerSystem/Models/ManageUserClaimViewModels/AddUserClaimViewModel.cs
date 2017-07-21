using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace IdentityServerSystem.Models.ManageUserClaimViewModels
{
    public class AddUserClaimViewModel
    {
        public Guid Id { get; set; }
        public string UserName { get; set; }

        public string FullName { get; set; }

        public IDictionary<string, string> PlanAddUserClaimDict { get; set; }
        public IDictionary<string, string> HasExsistUserClaims { get; set; }
    }   
}
