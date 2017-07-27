using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;

namespace IdentityServerSystem.Models.ManageClientViewModels
{
    public class CreateClientViewModel
    {
        [Required(AllowEmptyStrings = false)]
        public string ClientId { get; set; }

        [Required(AllowEmptyStrings = false)]
        public string ClientName { get; set; }

        [Display(Name = "GrantType")]
        public GrantTypesEnum GrantTypesEnum { get; set; }

        /// <summary>
        /// 使用Hybrid模式，需要使用Secrets。
        /// </summary>
        public List<string> ClientSecrets { get; set; }
        public List<string> RedirectUris { get; set; }

        public List<string> PostLogoutRedirectUris { get; set; }


        public List<string> AllowedScopes { get; set; }

        /// <summary>
        /// 如果GrantTypesEnum选中HybridAndClientCredentials模式，请置为True，并且在AllowedScopes中添加
        /// </summary>
        public bool AllowOfflineAccess { get; set; }

    }
}
