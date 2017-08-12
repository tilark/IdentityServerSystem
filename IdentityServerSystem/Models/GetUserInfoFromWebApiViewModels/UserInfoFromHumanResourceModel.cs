using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace IdentityServerSystem.Models.GetUserInfoFromWebApiViewModels
{
    public class UserInfoFromHumanResourceModel
    {


        public Guid userId { get; set; }
        public string employeeNo { get; set; }
        public string userName { get; set; }
        public Guid department { get; set; }

    }
}
