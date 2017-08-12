using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using IdentityServer4.EntityFramework.DbContexts;
using Microsoft.EntityFrameworkCore;
using IdentityServerSystem.Models.ManageIdentityResourceViewModel;
using IdentityServer4.Models;
using IdentityServer4.EntityFramework.Mappers;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Authorization;

namespace IdentityServerSystem.Controllers
{
    /// <summary>
    /// 管理IdentityResources
    /// </summary>
    /// 
    [Authorize(Policy = "Administrator")]
    public class ManageIdentityResourcesController : Controller
    {
        private readonly ConfigurationDbContext _configurationContext;
        private IMemoryCache _cache;

        public ManageIdentityResourcesController(ConfigurationDbContext configurationContext, IMemoryCache memoryCache)
        {
            this._configurationContext = configurationContext;
            this._cache = memoryCache;
        }
        #region IdentityResources列表
        public async Task<IActionResult> Index()
        {
            var viewModel = await this._configurationContext.IdentityResources.Include(a => a.UserClaims).ToListAsync();
            return View(viewModel);
        }
        #endregion

        #region 创建
        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [AutoValidateAntiforgeryToken]
        public async Task<IActionResult> Create(IdentityResourcesCreateViewModel identityResourcesCreateViewModel)
        {
            if (ModelState.IsValid)
            {
                IdentityResource identityResource = await CreateIdentityResource(identityResourcesCreateViewModel.Name, identityResourcesCreateViewModel.DisplayName, identityResourcesCreateViewModel.Emphasize, identityResourcesCreateViewModel.ClaimTypes);
                if(identityResource != null)
                {
                    var identityResourceSelect = new List<SelectListItem>();
                    if (_cache.TryGetValue(CacheKeys.IdentityResourceDisplayNameSelectEntry, out  identityResourceSelect))
                    {
                        _cache.Remove(CacheKeys.IdentityResourceDisplayNameSelectEntry);

                    }
                    return RedirectToAction(nameof(Index));
                }
                else
                {
                    ModelState.AddModelError(String.Empty, "创建失败！请注意输入项是否完整！");
                }
            }
            return View(identityResourcesCreateViewModel);
        }

        private async Task<IdentityServer4.Models.IdentityResource> CreateIdentityResource(string name, string displayName,bool emphasize, List<string> claimTypes)
        {
            var identityResource = new IdentityServer4.Models.IdentityResource(name, displayName, claimTypes);
            identityResource.Emphasize = emphasize;
            _configurationContext.IdentityResources.Add(identityResource.ToEntity());

            try
            {
                await _configurationContext.SaveChangesAsync();
                return identityResource;
            }
            catch (Exception)
            {

                return null;
            }
        }
        #endregion

        #region 更改
        /// <summary>
        /// 更改IdentityResource的内容，包括name, displayName, ClaimType
        /// </summary>
        /// <param name="id">IdentityResource Id</param>
        /// <returns></returns>
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return BadRequest();
            }
            var identityResource = await _configurationContext.IdentityResources.Include(a => a.UserClaims).Where(a => a.Id == id).FirstOrDefaultAsync();
            if(identityResource != null)
            {
                var identityResourceModel = identityResource.ToModel();
                var viewModel = new IdentityResourcesEditViewModel
                {
                    Id = identityResource.Id,
                    Name = identityResourceModel.Name,
                    DisplayName = identityResourceModel.DisplayName,
                    ClaimTypes = identityResourceModel.UserClaims.ToList()
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
        public async Task<IActionResult>Edit(IdentityResourcesEditViewModel identityResourcesEditViewModel)
        {
            if (ModelState.IsValid)
            {
                var identityResource = await EditIdentityResource(identityResourcesEditViewModel.Id, identityResourcesEditViewModel.Name, identityResourcesEditViewModel.DisplayName, identityResourcesEditViewModel.Emphasize, identityResourcesEditViewModel.ClaimTypes);
                if (identityResource != null)
                {
                    var identityResourceSelect = new List<SelectListItem>();

                    if (_cache.TryGetValue(CacheKeys.IdentityResourceDisplayNameSelectEntry, out  identityResourceSelect))
                    {
                        _cache.Remove(CacheKeys.IdentityResourceDisplayNameSelectEntry);

                    }

                    return RedirectToAction(nameof(Index));
                }
                else
                {
                    ModelState.AddModelError(String.Empty, "更新失败！请注意输入项是否完整！");
                }
            }
            return View(identityResourcesEditViewModel);
        }

        /// <summary>
        /// 更新IdentityResource
        /// </summary>
        /// <param name="id">Identity Resource Id</param>
        /// <param name="name"></param>
        /// <param name="displayName"></param>
        /// <param name="claimTypes"></param>
        /// <returns></returns>
        private async Task<IdentityResource> EditIdentityResource(int id, string name, string displayName, bool emphasize, List<string> claimTypes)
        {
            var identityResource = await _configurationContext.IdentityResources.Include(a => a.UserClaims).SingleOrDefaultAsync(a => a.Id == id);
            if(identityResource != null)
            {
                var updateIdentityResource = new IdentityResource(name, displayName, claimTypes).ToEntity();

                identityResource.Name = updateIdentityResource.Name;
                identityResource.DisplayName = updateIdentityResource.DisplayName;
                identityResource.Emphasize = emphasize;
                identityResource.UserClaims.Clear();
                identityResource.UserClaims = updateIdentityResource.UserClaims;
            }
            _configurationContext.IdentityResources.Update(identityResource);
            try
            {
                await _configurationContext.SaveChangesAsync();
                return identityResource.ToModel();
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
            if (id == null)
            {
                return BadRequest();
            }
            var identityResource = await _configurationContext.IdentityResources.SingleOrDefaultAsync(a => a.Id == id);
            if (identityResource != null)
            {
                return View(identityResource);
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
            var identityResource = await _configurationContext.IdentityResources.SingleOrDefaultAsync(m => m.Id == id);
            _configurationContext.IdentityResources.Remove(identityResource);
            await _configurationContext.SaveChangesAsync();
            return RedirectToAction("Index");
        }

        #endregion

        #region 通过IdentityResourceID获得ClaimType
        /// <summary>
        /// 
        /// </summary>
        /// <param name="id">IdentityResource Id</param>
        /// <returns></returns>
        public async Task<IActionResult> GetClaimTypeByIdentityResourceID(int? id)
        {
            if(id == null)
            {
                return new EmptyResult();
            }
            var viewModel = await _configurationContext.IdentityResources.Where(a => a.Id == id).SelectMany(a => a.UserClaims).Select(a => a.Type ).ToListAsync();

            return PartialView(viewModel);
        }
        #endregion
    }
}