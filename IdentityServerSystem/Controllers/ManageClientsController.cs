using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

using IdentityServer4.EntityFramework.DbContexts;
using IdentityServerSystem.Models.ManageClientViewModels;
using IdentityServer4.Models;
using IdentityServer4.EntityFramework.Mappers;

namespace IdentityServerSystem.Controllers
{
    /// <summary>
    /// 管理Clients
    /// </summary>
    public class ManageClientsController : Controller
    {
        private readonly ConfigurationDbContext _configurationContext;
        public ManageClientsController(ConfigurationDbContext configurationContext)
        {
            this._configurationContext = configurationContext;
        }
        public async Task<IActionResult> Index()
        {
            var viewModel = await _configurationContext.Clients.Include(a => a.AllowedScopes).Include(a => a.RedirectUris).Include(a => a.PostLogoutRedirectUris).Include(a => a.AllowedGrantTypes).ToListAsync();
            return View(viewModel);
        }

        #region 创建Client
        public IActionResult Create()
        {
            return View();
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="createClientViewModel"></param>
        /// <returns></returns>
        [HttpPost]
        [AutoValidateAntiforgeryToken]
        public async Task<IActionResult> Create(CreateClientViewModel createClientViewModel)
        {
            if (ModelState.IsValid)
            {
                var newClient = await CreateNewClient(createClientViewModel.ClientId, createClientViewModel.ClientName, createClientViewModel.GrantTypesEnum, createClientViewModel.RedirectUris, createClientViewModel.PostLogoutRedirectUris, createClientViewModel.AllowedScopes);
                if (newClient != null)
                {
                    return RedirectToAction(nameof(Index));
                }
                else
                {
                    ModelState.AddModelError(String.Empty, "创建Client失败！");
                }
            }
            return View(createClientViewModel);
        }

        private async Task<Client> CreateNewClient(string clientId, string clientName, GrantTypesEnum grantTypesEnum, List<string> redirectUris, List<string> postLogoutRedirectUris, List<string> allowedScopes)
        {
            var newClient = new Client
            {
                ClientId = clientId,
                ClientName = clientName,
                AllowedGrantTypes = GetGrantType(grantTypesEnum),
                RedirectUris = redirectUris,
                PostLogoutRedirectUris = postLogoutRedirectUris,
                AllowedScopes = allowedScopes
            };
            _configurationContext.Clients.Add(newClient.ToEntity());
            try
            {
                await _configurationContext.SaveChangesAsync();
                return newClient;
            }
            catch (Exception)
            {

                return null;
            }
        }

        private IEnumerable<string> GetGrantType(GrantTypesEnum grantTypesEnum)
        {
            switch (grantTypesEnum)
            {
                case GrantTypesEnum.Implicit:
                    return GrantTypes.Implicit;
                case GrantTypesEnum.ClientCredentials:
                    return GrantTypes.ClientCredentials;
                case GrantTypesEnum.Code:
                    return GrantTypes.Code;
                case GrantTypesEnum.CodeAndClientCredentials:
                    return GrantTypes.CodeAndClientCredentials;
                case GrantTypesEnum.Hybrid:
                    return GrantTypes.Hybrid;
                case GrantTypesEnum.HybridAndClientCredentials:
                    return GrantTypes.HybridAndClientCredentials;
                case GrantTypesEnum.ImplicitAndClientCredentials:
                    return GrantTypes.ImplicitAndClientCredentials;
                case GrantTypesEnum.ResourceOwnerPassword:
                    return GrantTypes.ResourceOwnerPassword;
                case GrantTypesEnum.ResourceOwnerPasswordAndClientCredentials:
                    return GrantTypes.ResourceOwnerPasswordAndClientCredentials;
            }
            return null;
        }
        #endregion

        #region 编辑
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return BadRequest();
            }
            var client = await _configurationContext.Clients.Include(a => a.AllowedScopes).Include(a => a.RedirectUris).Include(a => a.PostLogoutRedirectUris).Include(a => a.AllowedGrantTypes).Where(a => a.Id == id).FirstOrDefaultAsync();
            if (client != null)
            {
                var viewModel = new EditClientViewModel
                {
                    ClientId = client.ClientId,
                    id = client.Id,
                    ClientName = client.ClientName,
                    AllowedGrantTypes = client.AllowedGrantTypes.Select(a => a.GrantType).ToList(),
                    AllowedScopes = client.AllowedScopes.Select(a => a.Scope).ToList(),
                    RedirectUris = client.RedirectUris.Select(a => a.RedirectUri).ToList(),
                    PostLogoutRedirectUris = client.PostLogoutRedirectUris.Select(a => a.PostLogoutRedirectUri).ToList()
                };
                return View(viewModel);
            }
            else
            {
                return BadRequest();

            }
        }

        [HttpPost]
        [AutoValidateAntiforgeryToken]
        /// <summary>
        /// 编辑Client ，只能修改ClientName，RedirectUris，PostLogoutRedirectUris，AllowedScopes
        /// </summary>
        /// <param name="editClientViewModel"></param>
        /// <returns></returns>
        public async Task<IActionResult> Edit(EditClientViewModel editClientViewModel)
        {
            if (ModelState.IsValid)
            {
                var updateClient = await EditClient(editClientViewModel.id, editClientViewModel.ClientName, editClientViewModel.RedirectUris, editClientViewModel.PostLogoutRedirectUris, editClientViewModel.AllowedScopes);
                if(updateClient != null)
                {
                    return RedirectToAction(nameof(Index));
                }
                else
                {
                    ModelState.AddModelError(String.Empty, "更新Client失败！");
                }
            }
            return View(editClientViewModel);
        }

        /// <summary>
        /// 更新Client属性
        /// </summary>
        /// <param name="id"></param>
        /// <param name="clientName"></param>
        /// <param name="redirectUris"></param>
        /// <param name="postLogoutRedirectUris"></param>
        /// <param name="allowedScopes"></param>
        /// <returns></returns>
        private async Task<Client> EditClient(int id, string clientName, List<string> redirectUris, List<string> postLogoutRedirectUris, List<string> allowedScopes)
        {
            var updateClient = await _configurationContext.Clients.Include(a => a.AllowedScopes).Include(a => a.RedirectUris).Include(a => a.PostLogoutRedirectUris).Include(a => a.AllowedGrantTypes).Where(a => a.Id == id).FirstOrDefaultAsync();
            var newClientModel = new Client
            {
                ClientName = clientName,
                RedirectUris = redirectUris,
                PostLogoutRedirectUris = postLogoutRedirectUris,
                AllowedScopes = allowedScopes
            }.ToEntity();
            updateClient.ClientName = newClientModel.ClientName;
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

        #region 删除
        public async Task<IActionResult> Delete(int? id)
        {
            if(id == null)
            {
                return BadRequest();
            }
            var client = await _configurationContext.Clients.SingleOrDefaultAsync(a => a.Id == id);
            if(client != null)
            {
                return View(client);
            }
            else
            {
                return BadRequest();
            }
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var client = await _configurationContext.Clients.SingleOrDefaultAsync(m => m.Id == id);
            _configurationContext.Clients.Remove(client);
            await _configurationContext.SaveChangesAsync();
            return RedirectToAction("Index");
        }

        #endregion
    }
}