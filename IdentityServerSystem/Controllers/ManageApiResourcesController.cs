using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using IdentityServer4.EntityFramework.DbContexts;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.EntityFrameworkCore;
using IdentityServerSystem.Models.ManageApiResourceViewModels;
using IdentityServer4.Models;
using IdentityServer4.EntityFramework.Mappers;
// For more information on enabling MVC for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace IdentityServerSystem.Controllers
{
    /// <summary>
    /// 管理Api Resource
    /// </summary>
    public class ManageApiResourcesController : Controller
    {
        private readonly ConfigurationDbContext _configurationContext;
        private IMemoryCache _cache;

        public ManageApiResourcesController(ConfigurationDbContext configurationContext, IMemoryCache memoryCache)
        {
            this._configurationContext = configurationContext;
            this._cache = memoryCache;
        }
        #region Api Resource列表
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        // GET: /<controller>/
        public async Task<IActionResult> Index()
        {
            var viewModel = await _configurationContext.ApiResources.Include(a => a.UserClaims).ToListAsync();
            return View(viewModel);
        }
        #endregion

        #region 创建
        public IActionResult Create()
        {
            return View();
        }

        /// <summary>
        /// 从IdentityResource选择对应的值填写，不做自行添加处理
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        [AutoValidateAntiforgeryToken]
        public async Task<IActionResult> Create(ApiResourceCreateViewModel apiResourceCreateViewModel)
        {
            if (ModelState.IsValid)
            {
                var apiResource = await CreateApiResource(apiResourceCreateViewModel.Name, apiResourceCreateViewModel.DisplayName);
                if(apiResource != null)
                {
                    return RedirectToAction(nameof(Index));
                }
            }
            return View(apiResourceCreateViewModel);
        }

        /// <summary>
        /// 创建新的Api Resource
        /// </summary>
        /// <param name="apiResourceCreateViewModel"></param>
        /// <returns></returns>
        private async Task<ApiResource> CreateApiResource(string name, string displayName)
        {
            var query = _configurationContext.ApiResources.Where(a => a.Name == name).FirstOrDefaultAsync();
            if(query == null)
            {
                return null;
            }
            var apiResource = new ApiResource(name, displayName);
            _configurationContext.ApiResources.Add(apiResource.ToEntity());
            try
            {
                await _configurationContext.SaveChangesAsync();
                return apiResource;
            }
            catch (Exception)
            {

                return null;
            }
        }
        #endregion

        #region 编辑
        public async Task<IActionResult> Edit(int? id)
        {
            if(id == null)
            {
                return BadRequest();
            }
            var viewModel = await _configurationContext.ApiResources.Where(a => a.Id == id).Select(a => new ApiResourceEditViewModel { Id = a.Id, Name = a.Name, DisplayName = a.DisplayName }).FirstOrDefaultAsync();
            return View(viewModel);
        }
        [HttpPost]
        [AutoValidateAntiforgeryToken]
        public async Task<IActionResult> Edit(ApiResourceEditViewModel apiResourceEditViewModel)
        {
            if (ModelState.IsValid)
            {
                var apiResource = await EditApiResource(apiResourceEditViewModel.Id, apiResourceEditViewModel.DisplayName);
                if(apiResource != null)
                {
                    return RedirectToAction(nameof(Index));
                }
                else
                {
                    ModelState.AddModelError(String.Empty, "该ApiResource已经被删除!");
                }
            }
            return View(apiResourceEditViewModel);
        }

        private async Task<ApiResource> EditApiResource(int id, string displayName)
        {
            try
            {
                var aipResource = await _configurationContext.ApiResources.FindAsync(id);
                aipResource.DisplayName = displayName;
                _configurationContext.ApiResources.Update(aipResource);
                await _configurationContext.SaveChangesAsync();
                return aipResource.ToModel();
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
            try
            {
                var viewModel = await _configurationContext.ApiResources.Where(a => a.Id == id).Select(a => new ApiResourceEditViewModel { Id = a.Id, Name = a.Name, DisplayName = a.DisplayName }).FirstOrDefaultAsync();
                return View(viewModel);
            }
            catch (Exception)
            {
                return BadRequest();
            }           
        }
        [HttpPost, ActionName("Delete")]
        [AutoValidateAntiforgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int? id)
        {
            if(id == null)
            {
                return BadRequest();
            }
            try
            {
                var apiResource = await _configurationContext.ApiResources.FindAsync(id);
                _configurationContext.Remove(apiResource);
                await _configurationContext.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            catch (Exception)
            {
                throw;
            }
        }
        #endregion
    }
}
