using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace IdentityServerSystem.Models.GetUserInfoFromWebApiViewModels
{
    public class HumanResourceIdentityOption
    {


        public string Authority { get; set; }
        public string ClientId { get; set; }
        public string ClientSecret { get; set; }
        public string Scope { get; set; }
        public string PersonInfoListUri { get; set; }
        public string PersonInfosUriNoAuthorize { get; set; }


    }
}
