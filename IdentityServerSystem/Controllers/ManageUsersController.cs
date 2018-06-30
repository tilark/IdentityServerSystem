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
using IdentityServerSystem.Models.ManageUserClaimViewModels;

namespace IdentityServerSystem.Controllers
{
    /// <summary>
    /// 管理用户信息，新增用户、更改用户信息、更改或重置用户密码、删除用户信息
    /// </summary>
    /// 

    [Authorize(Policy = "AdminUser")]
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
           IEmailSender emailSender,
           ISmsSender smsSender,
           ILoggerFactory loggerFactory)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _emailSender = emailSender;
            _smsSender = smsSender;
            _logger = loggerFactory.CreateLogger<ManageController>();
        }

        public IActionResult Index()
        {
            return View();
        }

        #region 用户列表
        /// <summary>
        /// 用户列表
        /// </summary>
        /// <returns></returns>
        public async Task<IActionResult> ListUsers()
        {
            var viewModel = await _userManager.Users.ToListAsync();
            return View(viewModel);
        }
        #endregion

        #region 新建用户
        [Authorize(Policy = "Administrator")]
        /// <summary>
        /// 新增用户
        /// </summary>
        /// <returns></returns>
        public IActionResult AddUser()
        {
            return View();
        }

        [Authorize(Policy = "Administrator")]
        [HttpPost]
        [AutoValidateAntiforgeryToken]
        public async Task<IActionResult> AddUser(CreateUserViewModel createUserViewModel)
        {
            if (ModelState.IsValid)
            {
                
                var newUserInfo = await createUserViewModel.CreateUserAsync(this._userManager);

                if (newUserInfo != null)
                {
                    //创建未成功，返回创建页面，需重新创建
                    return RedirectToAction("ListUsers");
                }
            }
            return View(createUserViewModel);
        }       


        #endregion

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
            var viewModel = await _userManager.Users.Where(a => a.Id == id.Value).Select(a => new EditUserViewModel { Id = a.Id, FamilyName = a.FamilyName, FirstName = a.FirstName, Telephone = a.Telephone, UserName = a.UserName }).FirstOrDefaultAsync();
            return View(viewModel);
        }

        [HttpPost]
        [AutoValidateAntiforgeryToken]
        public async Task<IActionResult> Edit(EditUserViewModel editUserViewModel)
        {
            if (ModelState.IsValid)
            {
                //var userInfo = await UpdateUserInfo(editUserViewModel.id, editUserViewModel.FamilyName, editUserViewModel.FirstName, editUserViewModel.Telephone);
                var userInfo = await editUserViewModel.UpdateUserInfo(_userManager);
                if (userInfo != null)
                {
                    return RedirectToAction(nameof(ListUsers));
                }
            }
            return View(editUserViewModel);
        }

        private async Task<ApplicationUser> UpdateUserInfo(Guid id, string familyName, string firstName, string telephone)
        {

            var userInfo = await _userManager.Users.FirstOrDefaultAsync(a => a.Id == id);
            if (userInfo != null)
            {
                userInfo.FamilyName = familyName;
                userInfo.FirstName = firstName;
                userInfo.Telephone = telephone;
                var result = await _userManager.UpdateAsync(userInfo);
                if (result.Succeeded)
                {
                    //更改UserClaim
                    var userClaims = new Dictionary<string, string>();
                    userClaims.Add("family_name", familyName);
                    userClaims.Add("given_name", firstName);
                    userClaims.Add("preferred_username", familyName + firstName);
                    EditUserClaimViewModel updateUserClaim = new EditUserClaimViewModel
                    {
                        Id = id,
                        UserClaims = userClaims

                    };
                    var user = await updateUserClaim.UpdateUserClaims(_userManager);

                    return user;
                }
                else
                {
                    AddErrors(result);
                }
            }
            return null;
        }
        #endregion        

        #region 删除用户

        [Authorize(Policy = "Administrator")]
        public async Task<IActionResult> DeleteUser(Guid? id)
        {
            if (!id.HasValue && id.Equals(Guid.Empty))
            {
                return BadRequest();
            }
            var user = await _userManager.Users.FirstOrDefaultAsync(a => a.Id == id.Value);
            if(user != null)
            {
                var viewModel = new DeleteUserViewModel { Id = user.Id, UserName = user.UserName, FamilyName = user.FamilyName, FirstName = user.FirstName };
                return View(viewModel);
            }
            return BadRequest();
        }

        [Authorize(Policy = "Administrator")]        
        [AutoValidateAntiforgeryToken]
        public async Task<IActionResult> DeleteUser(DeleteUserViewModel deleteUserViewModel)
        {
            if (deleteUserViewModel.Id.Equals(Guid.Empty))
            {
                return BadRequest();
            }
            var userResult = await deleteUserViewModel.DeleteUserAsync(_userManager);
            if (userResult != null && userResult.Succeeded)
            {
                return RedirectToAction("ListUsers");
            }
            else
            {
                return View(deleteUserViewModel);
            }
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
            if (!id.HasValue && id.Value.Equals(Guid.Empty))
            {
                return BadRequest();
            }
            var viewModel = await _userManager.Users.Where(a => a.Id == id.Value).Select(a => new ChangeUserPasswordViewModel { id = a.Id, UserName = a.UserName }).FirstOrDefaultAsync();

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

        #region 更改用户登陆帐号

        [Authorize(Policy = "Administrator")]
        /// <summary>
        /// 更改用户的登陆帐号
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public async Task<IActionResult> ChangeUserAccount(Guid? id)
        {
            if (!id.HasValue || id.Value.Equals(Guid.Empty))
            {
                return BadRequest();
            }
            var viewModel = await _userManager.Users.Where(a => a.Id == id.Value).Select(a => new ChangeUserAccountViewModel { Id = a.Id, UserName = a.UserName }).FirstOrDefaultAsync();
            return View(viewModel);
        }

        [Authorize(Policy = "Administrator")]
        [HttpPost]
        [AutoValidateAntiforgeryToken]
        public async Task<IActionResult> ChangeUserAccount(ChangeUserAccountViewModel changeUserAccountViewModel)
        {
            if (ModelState.IsValid)
            {               
                var result = await changeUserAccountViewModel.ChangeUserAccountAsync(_userManager);
                if(result != null)
                {
                    if (result.Succeeded)
                    {
                        return RedirectToAction(nameof(ListUsers));

                    }
                    else
                    {
                        AddErrors(result);
                    }
                }
                else
                {
                    return RedirectToAction(nameof(ListUsers), new { Message = ManageMessageId.Error });
                }
            }
            return View(changeUserAccountViewModel);
        }
        #endregion

        #region Private method
        private void AddErrors(IdentityResult result)
        {
            foreach (var error in result.Errors)
            {
                ModelState.AddModelError(string.Empty, error.Description);
            }
        }
        #endregion
    }
}