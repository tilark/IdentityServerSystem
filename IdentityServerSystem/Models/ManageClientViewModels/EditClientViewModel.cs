using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;
using IdentityServer4.EntityFramework.DbContexts;
using Microsoft.EntityFrameworkCore;
using IdentityServer4.Models;
using IdentityServer4.EntityFramework.Mappers;

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

        #region Public Method
        public async Task<Client> UpdateClientAsync(ConfigurationDbContext _configurationContext)
        {
            var updateClient = await _configurationContext.Clients.Include(a => a.AllowedScopes).Include(a => a.RedirectUris).Include(a => a.PostLogoutRedirectUris).Include(a => a.AllowedGrantTypes).Where(a => a.Id == id).FirstOrDefaultAsync();
            var newClientModel = new Client
            {
                ClientName = ClientName,
                RedirectUris = RedirectUris,
                PostLogoutRedirectUris = PostLogoutRedirectUris,
                AllowedScopes = AllowedScopes
            }.ToEntity();
            updateClient.ClientName = newClientModel.ClientName;
            updateClient.AllowOfflineAccess = AllowOfflineAccess;
            updateClient.RedirectUris.Clear();
            updateClient.RedirectUris = newClientModel.RedirectUris;

            updateClient.PostLogoutRedirectUris.Clear();
            updateClient.PostLogoutRedirectUris = newClientModel.PostLogoutRedirectUris;

            updateClient.AllowedScopes.Clear();
            updateClient.AllowedScopes = newClientModel.AllowedScopes;

            _configurationContext.Clients.Update(updateClient);
            try
            {
                await _configurationContext.SaveChangesAsync();
                return updateClient.ToModel();
            }
            catch (Exception)
            {

                return null;
            }
        }
        #endregion
    }
}
