using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Extensions.Caching.Memory;
using IdentityServerSystem.Data;

namespace IdentityServerSystem.Services
{
    public class DropDownListService 
    {
        private readonly ApplicationDbContext _context;
        private IMemoryCache _cache;
        public DropDownListService(ApplicationDbContext context, IMemoryCache memoryCache)
        {
            this._context = context;
            this._cache = memoryCache;
        }
        public List<SelectListItem> GetDepartmentDropDownList()
        {
            var departmentSelect = new List<SelectListItem>();

            // Look for cache key.
            if (!_cache.TryGetValue(CacheKeys.DepartmentSelectEntry, out departmentSelect))
            {
               
                // Key not in cache, so get data.
                departmentSelect = _context.ApplicationDepartments.Select(a => new SelectListItem { Text = a.DepartmentName, Value = a.DepartmentID.ToString() }).ToList();

                // Set cache options.
                var cacheEntryOptions = new MemoryCacheEntryOptions()
                    // Keep in cache for this time, reset time if accessed.
                    .SetSlidingExpiration(TimeSpan.FromMinutes(30));

                // Save data in cache.
                _cache.Set(CacheKeys.DepartmentSelectEntry, departmentSelect, cacheEntryOptions);
            }
            return departmentSelect;
        }

    }
}
