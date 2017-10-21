using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using IdentityServer4.EntityFramework.DbContexts;
using IdentityServer4.Models;
using IdentityServer4.EntityFramework.Mappers;

namespace IdentityServerSystem.Models.ManageClientViewModels
{
    public class CreateImplicitViewModel
    {
        [Required(AllowEmptyStrings = false)]
        public string ClientId { get; set; }

        [Required(AllowEmptyStrings = false)]
        public string ClientName { get; set; }

        public List<string> RedirectUris { get; set; }

        public List<string> PostLogoutRedirectUris { get; set; }

        public List<string> AllowedScopes { get; set; }

        internal async Task<Client> CreateImplicitClientAsync(ConfigurationDbContext _configurationContext)
        {
            var newClient = new Client
            {
                ClientId = ClientId,
                ClientName = ClientName,
                AllowedGrantTypes = GrantTypes.Implicit,
                RedirectUris = RedirectUris,
                PostLogoutRedirectUris = PostLogoutRedirectUris,
                AllowedScopes = AllowedScopes
            };
            try
            {
                _configurationContext.Clients.Add(newClient.ToEntity());
           
                await _configurationContext.SaveChangesAsync();
                return newClient;
            }
            catch (Exception)
            {

                return null;
            }
        }
    }
}
