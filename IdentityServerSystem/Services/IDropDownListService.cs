using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace IdentityServerSystem.Services
{
    public interface IDropDownListService
    {
        List<SelectListItem> GetDepartmentDropDownList();
    }
}
