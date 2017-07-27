using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;
using IdentityServer4.EntityFramework.Entities;

namespace IdentityServerSystem.Models.ManageClientViewModels
{
    public class EditClientViewModel
    {
        public int id { get; set; }
        [Required(AllowEmptyStrings = false)]
        public string ClientId { get; set; }

        [Required(AllowEmptyStrings = false)]
        public string ClientName { get; set; }

        public List<string> AllowedGrantTypes { get; set; }


        public List<string> RedirectUris { get; set; }

        public List<string> PostLogoutRedirectUris { get; set; }


        public List<string> AllowedScopes { get; set; }

        /// <summary>
        /// 如果GrantTypesEnum选中HybridAndClientCredentials模式，请置为True，并且在AllowedScopes中添加
        /// </summary>
        public bool AllowOfflineAccess { get; set; }
    }
}
