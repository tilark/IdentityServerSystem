using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Identity;
using IdentityServerSystem.Models;
using IdentityServerSystem.Services.ReadFromExcelServices;
using IdentityServerSystem.Models.GetUserInfoFromExcelViewModel;
using Microsoft.AspNetCore.Http;

// For more information on enabling MVC for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace IdentityServerSystem.Controllers
{
    /// <summary>
    /// 从Excel中导入人员信息，并创建人员
    /// </summary>
    public class GetUserInfoFromExcelController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;

        public GetUserInfoFromExcelController(UserManager<ApplicationUser> userManager)
        {
            this._userManager = userManager;
        }
        // GET: /<controller>/
        public IActionResult Index()
        {
            return View();
        }

        /// <summary>
        /// 导入DepartmentType信息
        /// </summary>
        /// <returns></returns>
        public IActionResult ImportUserInfo()
        {

            return View();
        }
        [HttpPost]
        [AutoValidateAntiforgeryToken]
        public async Task<IActionResult> ImportUserInfo(IFormFile file)
        {
            if (!IsValidFile(file))
            {
                ViewBag.ErrorMessage = "上传文件为空！";
                return View();
            }
            var fileStream = file.OpenReadStream();
            var result = await new ReadPersonInfoFromExcelService(this._userManager, file.ContentType, fileStream).ExcuteAsync<UserInfoFromExcelModel>();
            if (String.IsNullOrEmpty(result))
            {
                return RedirectToAction("ListUsers", "ManageUsers");

            }
            ViewBag.ErrorMessage = result;
            return View();
        }

        private bool IsValidFile(IFormFile file)
        {
            if (file != null)
            {
                return true;
            }
            return false;
        }
    }
}
