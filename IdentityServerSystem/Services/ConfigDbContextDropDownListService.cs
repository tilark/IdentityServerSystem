using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using IdentityServer4.EntityFramework.DbContexts;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace IdentityServerSystem.Services
{
    public class ConfigDbContextDropDownListService
    {
        private readonly ConfigurationDbContext _context;
        private IMemoryCache _cache;
        public ConfigDbContextDropDownListService(ConfigurationDbContext context, IMemoryCache memoryCache)
        {
            this._context = context;
            this._cache = memoryCache;
        }
        /// <summary>
        /// IdentityResource的DisplayName和Id
        /// </summary>
        /// <returns></returns>
        public List<SelectListItem> GetIdentityResourceIDDropDownList()
        {
            var identityResourceSelect = new List<SelectListItem>();

            // Look for cache key.
            if (!_cache.TryGetValue(CacheKeys.IdentityResourceIDSelectEntry, out identityResourceSelect))
            {

                // Key not in cache, so get data.
                identityResourceSelect = _context.IdentityResources.Select(a => new SelectListItem { Text = a.DisplayName, Value = a.Id.ToString() }).ToList();

                // Set cache options.
                var cacheEntryOptions = new MemoryCacheEntryOptions()
                    // Keep in cache for this time, reset time if accessed.
                    .SetSlidingExpiration(TimeSpan.FromMinutes(2));

                // Save data in cache.
                _cache.Set(CacheKeys.IdentityResourceIDSelectEntry, identityResourceSelect, cacheEntryOptions);
            }
            return identityResourceSelect;
        }

        /// <summary>
        /// IdentityResource的Name和DisplayName
        /// </summary>
        /// <returns></returns>
        public List<SelectListItem> GetIdentityResourceDisplayNameDropDownList()
        {
            var identityResourceSelect = new List<SelectListItem>();

            // Look for cache key.
            if (!_cache.TryGetValue(CacheKeys.IdentityResourceDisplayNameSelectEntry, out identityResourceSelect))
            {

                // Key not in cache, so get data.
                identityResourceSelect = _context.IdentityResources.Select(a => new SelectListItem { Text = a.DisplayName, Value = a.Name }).ToList();

                // Set cache options.
                var cacheEntryOptions = new MemoryCacheEntryOptions()
                    // Keep in cache for this time, reset time if accessed.
                    .SetSlidingExpiration(TimeSpan.FromMinutes(2));

                // Save data in cache.
                _cache.Set(CacheKeys.IdentityResourceDisplayNameSelectEntry, identityResourceSelect, cacheEntryOptions);
            }
            return identityResourceSelect;
        }

        /// <summary>
        /// IdentityResource的细则
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public List<SelectListItem> GetIdentityResourceClaimTypeDropDownList(string key = null)
        {
            var identityResourceSelect = new List<SelectListItem>();

            // Look for cache key.
            if (!_cache.TryGetValue(CacheKeys.IdentityResourceDisplayNameSelectEntry, out identityResourceSelect))
            {

                // Key not in cache, so get data.
                if (String.IsNullOrEmpty(key))
                {
                    identityResourceSelect = _context.IdentityResources.SelectMany(a => a.UserClaims).Select(a => new SelectListItem { Text = a.Type, Value = a.Type }).ToList();
                }
                else
                {
                    identityResourceSelect = _context.IdentityResources.SelectMany(a => a.UserClaims).Select(a => new SelectListItem { Text = a.Type, Value = a.Type, Selected = a.Type == key }).ToList();
                }

                // Set cache options.
                var cacheEntryOptions = new MemoryCacheEntryOptions()
                    // Keep in cache for this time, reset time if accessed.
                    .SetSlidingExpiration(TimeSpan.FromMinutes(2));

                // Save data in cache.
                _cache.Set(CacheKeys.IdentityResourceDisplayNameSelectEntry, identityResourceSelect, cacheEntryOptions);
            }
            return identityResourceSelect;
        }
        public List<SelectListItem> GetIdentityResourceClaimTypeDropDownList()
        {
            var identityResourceUserClaimsSelect = new List<SelectListItem>();

            // Look for cache key.
            if (!_cache.TryGetValue(CacheKeys.IdentityResourceUserClaimSelectEntry, out identityResourceUserClaimsSelect))
            {

                // Key not in cache, so get data.
                identityResourceUserClaimsSelect = _context.IdentityResources.SelectMany(a => a.UserClaims).Select(a => new SelectListItem { Text = a.Type, Value = a.Type }).ToList();

                // Set cache options.
                var cacheEntryOptions = new MemoryCacheEntryOptions()
                    // Keep in cache for this time, reset time if accessed.
                    .SetSlidingExpiration(TimeSpan.FromMinutes(2));

                // Save data in cache.
                _cache.Set(CacheKeys.IdentityResourceUserClaimSelectEntry, identityResourceUserClaimsSelect, cacheEntryOptions);
            }
            return identityResourceUserClaimsSelect;
        }
    }
}
