using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using IdentityServerSystem.Models;
using IdentityServerSystem.Models.ManageViewModels;
using IdentityServerSystem.Services;
using IdentityServerSystem.Data;
using Microsoft.EntityFrameworkCore;
using IdentityServerSystem.Models.ManageUserViewModels;
using static IdentityServerSystem.Controllers.ManageController;
using System.Security.Claims;

namespace IdentityServerSystem.Controllers
{
    /// <summary>
    /// 管理用户信息，新增用户、更改用户信息、更改或重置用户密码、删除用户信息
    /// </summary>
    /// 
   
    public class ManageUsersController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly string _externalCookieScheme;
        private readonly IEmailSender _emailSender;
        private readonly ISmsSender _smsSender;
        private readonly ILogger _logger;
        public ManageUsersController(
           UserManager<ApplicationUser> userManager,
           SignInManager<ApplicationUser> signInManager,
           IOptions<IdentityCookieOptions> identityCookieOptions,
           IEmailSender emailSender,
           ISmsSender smsSender,
           ILoggerFactory loggerFactory)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _externalCookieScheme = identityCookieOptions.Value.ExternalCookieAuthenticationScheme;
            _emailSender = emailSender;
            _smsSender = smsSender;
            _logger = loggerFactory.CreateLogger<ManageController>();
        }

        public IActionResult Index()
        {
            return View();
        }

        #region 编辑用户
        /// <summary>
        /// 编辑用户信息
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public async Task<IActionResult> Edit(Guid? id)
        {
            if (!id.HasValue || id.Value.Equals(Guid.Empty))
            {
                return BadRequest();
            }
            var viewModel = await _userManager.Users.Where(a => a.Id == id.Value).Select(a => new EditUserViewModel { id = a.Id, DepartmentID = a.DepartmentID, FamilyName = a.FamilyName, FirstName = a.FirstName, Telephone = a.Telephone, UserName = a.UserName }).FirstOrDefaultAsync();
            return View(viewModel);
        }

        [HttpPost]
        [AutoValidateAntiforgeryToken]
        public async Task<IActionResult> Edit(EditUserViewModel editUserViewModel)
        {
            if (ModelState.IsValid)
            {
                var userInfo = await UpdateUserInfo(editUserViewModel.id, editUserViewModel.DepartmentID, editUserViewModel.FamilyName, editUserViewModel.FirstName, editUserViewModel.Telephone);
                if (userInfo != null)
                {
                    return RedirectToAction(nameof(ListUsers));
                }
            }
            return View(editUserViewModel);
        }

        private async Task<ApplicationUser> UpdateUserInfo(Guid id, Guid? departmentID, string familyName, string firstName, string telephone)
        {

            var userInfo = await _userManager.Users.FirstOrDefaultAsync(a => a.Id == id);
            if (userInfo != null)
            {
                userInfo.DepartmentID = departmentID;
                userInfo.FamilyName = familyName;
                userInfo.FirstName = firstName;
                userInfo.Telephone = telephone;
                var result = await _userManager.UpdateAsync(userInfo);
                if (result.Succeeded)
                {
                    return userInfo;
                }
                else
                {
                    AddErrors(result);
                }
            }
            return null;
        }
        #endregion

        #region 新建用户
        /// <summary>
        /// 新增用户
        /// </summary>
        /// <returns></returns>
        public IActionResult AddUser()
        {
            return View();
        }

        [HttpPost]
        [AutoValidateAntiforgeryToken]
        public async Task<IActionResult> AddUser(CreateUserViewModel createUserViewModel)
        {
            if (ModelState.IsValid)
            {
                var newUserInfo = await CreateNewUser(createUserViewModel.UserName, createUserViewModel.Password, createUserViewModel.DepartmentID, createUserViewModel.FamilyName, createUserViewModel.FirstName, createUserViewModel.Telephone);

                if (newUserInfo != null)
                {
                    //创建未成功，返回创建页面，需重新创建
                    return RedirectToAction("ListUsers");
                }
            }
            return View(createUserViewModel);
        }

        /// <summary>
        /// 创建新用户
        /// </summary>
        /// <param name="userName">登陆帐号</param>
        /// <param name="password">登陆密码</param>
        /// <param name="departmentID">人员所在科室</param>
        /// <param name="familyName">姓氏</param>
        /// <param name="firstName">名字</param>
        /// <param name="telephone">电话号码</param>
        /// <returns></returns>
        private async Task<ApplicationUser> CreateNewUser(string userName, string password, Guid? departmentID, string familyName, string firstName, string telephone)
        {
            //先创建一个新的ApplicationUser，看是否成功，成功返回该用户信息
            var user = new ApplicationUser { UserName = userName, DepartmentID = departmentID, FamilyName = familyName, FirstName = firstName, Telephone = telephone };
            var result = await _userManager.CreateAsync(user, password);
           
            if (result.Succeeded)
            {
                return user;
            }
            else
            {
                AddErrors(result);
                //不成功，返回null
                return null;
            }
        }

        private void AddErrors(IdentityResult result)
        {
            foreach (var error in result.Errors)
            {
                ModelState.AddModelError(string.Empty, error.Description);
            }
        }
        #endregion

        #region 删除用户
        public async Task<IActionResult> DeleteUser(Guid? id)
        {
            if (!id.HasValue && id.Equals(Guid.Empty))
            {
                return BadRequest();
            }
            var user = await _userManager.Users.FirstOrDefaultAsync(a => a.Id == id.Value);
            return View(user);
        }

        [HttpPost, ActionName("DeleteUser")]
        [AutoValidateAntiforgeryToken]
        public async Task<IActionResult> DeleteUserConfirmed(Guid id)
        {
            if (id.Equals(Guid.Empty))
            {
                return BadRequest();
            }
            //删除
            var user = await _userManager.Users.FirstOrDefaultAsync(a => a.Id == id);
            var result = await _userManager.DeleteAsync(user);
            if (result.Succeeded)
            {
                return RedirectToAction("ListUsers");
            }
            else
            {
                AddErrors(result);
                return View(user);
            }
        }
        #endregion
        #region 用户列表
        /// <summary>
        /// 用户列表
        /// </summary>
        /// <returns></returns>
        public async Task<IActionResult> ListUsers()
        {
            var viewModel = await _userManager.Users.Include(a => a.ApplicationDepartment).ToListAsync();
            return View(viewModel);
        }
        #endregion

        #region 用户详情
        public async Task<IActionResult> Details(Guid? id)
        {
            if (!id.HasValue && id.Equals(Guid.Empty))
            {
                return BadRequest();
            }

            var viewModel = await _userManager.Users.FirstOrDefaultAsync(a => a.Id == id.Value);
            return View(viewModel);
        }
        #endregion

        #region 更改用户密码
        public async Task<IActionResult> ChangeUserPassword(Guid? id)
        {
            if(!id.HasValue && id.Value.Equals(Guid.Empty))
            {
                return BadRequest();
            }
            var viewModel = await _userManager.Users.Where(a => a.Id == id.Value).Select(a => new ChangeUserPasswordViewModel {id = a.Id, UserName = a.UserName }).FirstOrDefaultAsync();

            return View(viewModel);
        }

        [HttpPost]
        [AutoValidateAntiforgeryToken]
        public async Task<IActionResult> ChangeUserPassword(ChangeUserPasswordViewModel changeUserPasswordViewModel)
        {
            if (ModelState.IsValid)
            {
                var user = await _userManager.Users.FirstOrDefaultAsync(a => a.Id == changeUserPasswordViewModel.id);
                if (user != null)
                {
                    var result = await _userManager.ChangePasswordAsync(user, changeUserPasswordViewModel.OldPassword, changeUserPasswordViewModel.NewPassword);
                    if (result.Succeeded)
                    {
                        return RedirectToAction(nameof(ListUsers), new { Message = ManageMessageId.ChangePasswordSuccess });
                    }
                    AddErrors(result);
                    return View(changeUserPasswordViewModel);
                }
                return RedirectToAction(nameof(ListUsers), new { Message = ManageMessageId.Error });
            }
            return View(changeUserPasswordViewModel);
        }
        #endregion

        #region 重置用户密码
        public async Task<IActionResult> ResetUserPassword(Guid? id)
        {
            if (!id.HasValue && id.Value.Equals(Guid.Empty))
            {
                return BadRequest();
            }
            var viewModel = await _userManager.Users.Where(a => a.Id == id.Value).Select(a => new ResetUserPasswordViewModel { id = a.Id, UserName = a.UserName }).FirstOrDefaultAsync();

            return View(viewModel);
        }

        [HttpPost]
        [AutoValidateAntiforgeryToken]
        public async Task<IActionResult> ResetUserPassword(ResetUserPasswordViewModel resetUserPasswordViewModel)
        {
            if (ModelState.IsValid)
            {
                var user = await _userManager.Users.FirstOrDefaultAsync(a => a.Id == resetUserPasswordViewModel.id);
                if (user != null)
                {
                    var removePasswordResult = await _userManager.RemovePasswordAsync(user);
                    if (removePasswordResult.Succeeded)
                    {
                        var addPasswordResult = await _userManager.AddPasswordAsync(user, resetUserPasswordViewModel.NewPassword);
                        if (addPasswordResult.Succeeded)
                        {
                            return RedirectToAction(nameof(ListUsers), new { Message = ManageMessageId.SetPasswordSuccess });

                        }
                    }
                    AddErrors(removePasswordResult);
                    return View(resetUserPasswordViewModel);
                }
                return RedirectToAction(nameof(ListUsers), new { Message = ManageMessageId.Error });
            }
            return View(resetUserPasswordViewModel);
        }
        #endregion
    }
}