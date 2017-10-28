using IdentityServer4.EntityFramework.DbContexts;
using IdentityServer4.EntityFramework.Interfaces;
using IdentityServer4.EntityFramework.Mappers;
using IdentityServer4.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace IdentityServerSystem.Models.ManageClientViewModels
{
    public class EditImplicitViewModel
    {
        public int id { get; set; }
        [Required(AllowEmptyStrings = false)]
        public string ClientId { get; set; }

        [Required(AllowEmptyStrings = false)]
        public string ClientName { get; set; }


        public List<string> RedirectUris { get; set; }

        public List<string> PostLogoutRedirectUris { get; set; }


        public List<string> AllowedScopes { get; set; }

        #region Public Method
        public async Task<Client> UpdateClientAsync(IConfigurationDbContext _configurationContext)
        {
            var updateClient = await _configurationContext.Clients.Include(a => a.AllowedScopes).Include(a => a.RedirectUris).Include(a => a.PostLogoutRedirectUris).Include(a => a.AllowedGrantTypes).Where(a => a.Id == id).FirstOrDefaultAsync();
            var newClientModel = new Client
            {
                ClientName = ClientName,
                RedirectUris = RedirectUris.Select(a => a.Trim()).ToList(),
                PostLogoutRedirectUris = PostLogoutRedirectUris.Select(a => a.Trim()).ToList(),
                AllowedScopes = AllowedScopes
            }.ToEntity();
            updateClient.ClientName = newClientModel.ClientName;
            updateClient.RedirectUris.Clear();
            updateClient.RedirectUris = newClientModel.RedirectUris;

            updateClient.PostLogoutRedirectUris.Clear();
            updateClient.PostLogoutRedirectUris = newClientModel.PostLogoutRedirectUris;

            updateClient.AllowedScopes.Clear();
            updateClient.AllowedScopes = newClientModel.AllowedScopes;

            try
            {
                _configurationContext.Clients.Update(updateClient);

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
