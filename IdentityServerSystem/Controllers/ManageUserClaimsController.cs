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
using System.Security.Claims;
using IdentityServerSystem.Models.ManageUserClaimViewModels;

namespace IdentityServerSystem.Controllers
{
    public class ManageUserClaimsController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly string _externalCookieScheme;
        private readonly IEmailSender _emailSender;
        private readonly ISmsSender _smsSender;
        private readonly ILogger _logger;
        public ManageUserClaimsController(
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
            _logger = loggerFactory.CreateLogger<ManageUserClaimsController>();
        }

        #region UserClaims列表
        /// <summary>
        /// 
        /// </summary>
        /// <param name="id">ApplicationUser Id</param>
        /// <returns></returns>
        public async Task<IActionResult> Index(Guid? id)
        {
            if (!id.HasValue || id.Value.Equals(Guid.Empty))
            {
                return BadRequest();
            }
            var viewModel = await _userManager.Users.Include(a => a.Claims).Where(a => a.Id == id).FirstOrDefaultAsync();
            return View(viewModel);
        }
        #endregion

        #region 添加UserClaim及值
        /// <summary>
        /// 
        /// </summary>
        /// <param name="id">Application User Id</param>
        /// <returns></returns>
        public async Task<IActionResult> AddUserClaim(Guid? id)
        {
            if (!id.HasValue || id.Value.Equals(Guid.Empty))
            {
                return BadRequest();
            }
            var userModel = await _userManager.Users.Include(a => a.Claims).Where(a => a.Id == id).FirstOrDefaultAsync();
            if (userModel != null)
            {
                var viewModel = new AddUserClaimViewModel
                {
                    Id = userModel.Id,
                    FullName = userModel.FullName,
                    UserName = userModel.UserName,
                    HasExsistUserClaims = userModel.Claims.Select(a => new { Key = a.ClaimType, Value = a.ClaimValue }).ToDictionary(a => a.Key, a => a.Value)
                };
                return View(viewModel);
            }
            return BadRequest();

        }

        /// <summary>
        /// 更新User的UserClaims
        /// </summary>
        /// <param name="addUserClaimViewModel"></param>
        /// <returns></returns>
        [HttpPost]
        [AutoValidateAntiforgeryToken]
        public async Task<IActionResult> AddUserClaim(AddUserClaimViewModel addUserClaimViewModel)
        {
            if (ModelState.IsValid)
            {
                ApplicationUser user = await AddUserClaimToUser(addUserClaimViewModel.Id, addUserClaimViewModel.PlanAddUserClaimDict);
                if (user != null)
                {
                    return RedirectToAction(nameof(Index), new { id = user.Id });
                }
            }
            return View(addUserClaimViewModel);
        }

        /// <summary>
        /// 如果计划增加的UserClaim未在原User的UserClaim中，则增加，否则忽略，保留原值。
        /// </summary>
        /// <param name="id">User Id</param>
        /// <param name="planAddUserClaimDict">计划增加的UserClaim</param>
        /// <returns></returns>
        private async Task<ApplicationUser> AddUserClaimToUser(Guid id, IDictionary<string, string> planAddUserClaimDict)
        {
            var userModel = await _userManager.Users.Include(a => a.Claims).Where(a => a.Id == id).FirstOrDefaultAsync();
            if (userModel != null)
            {
                foreach (var planUserClaim in planAddUserClaimDict)
                {
                    if (userModel.Claims.All(a => a.ClaimType != planUserClaim.Key))
                    {
                        //UserClaim中不包含计划添加的，则需添加
                        var newClaim = new Claim(planUserClaim.Key, planUserClaim.Value);
                        await _userManager.AddClaimAsync(userModel, newClaim);
                    }
                    else
                    {
                        ModelState.AddModelError("PlanAddUserClaimDict", $"UserClaimType:{planUserClaim.Key}已经存在！无法再添加");
                    }
                }
                return userModel;
            }
            else
            {
                return null;
            }
        }
        #endregion

        #region 更改UserClaim
        /// <summary>
        /// 
        /// </summary>
        /// <param name="id">Application User Id</param>
        /// <returns></returns>
        public async Task<IActionResult> EditUserClaim(Guid? id)
        {
            if (!id.HasValue || id.Value.Equals(Guid.Empty))
            {
                return BadRequest();
            }
            var userModel = await _userManager.Users.Include(a => a.Claims).Where(a => a.Id == id).FirstOrDefaultAsync();
            if (userModel != null)
            {
                var viewModel = new EditUserClaimViewModel
                {
                    Id = userModel.Id,
                    FullName = userModel.FullName,
                    UserName = userModel.UserName,
                    UserClaims = userModel.Claims.OrderBy(a => a.ClaimType).Select(a => new { ClaimType = a.ClaimType, ClaimValue = a.ClaimValue }).ToDictionary(a => a.ClaimType, a => a.ClaimValue)
                };
                return View(viewModel);
            }
            return BadRequest();

        }

        /// <summary>
        /// 更新User的UserClaims
        /// </summary>
        /// <param name="editUserClaimViewModel"></param>
        /// <returns></returns>
        [HttpPost]
        [AutoValidateAntiforgeryToken]
        public async Task<IActionResult> EditUserClaim(EditUserClaimViewModel editUserClaimViewModel)
        {
            if (ModelState.IsValid)
            {
                ApplicationUser user = await EditUserClaimToUser(editUserClaimViewModel.Id, editUserClaimViewModel.UserClaims);
                if (user != null)
                {
                    return RedirectToAction(nameof(Index), new { id = user.Id });
                }
            }
            return View(editUserClaimViewModel);
        }
        /// <summary>
        /// 更新UserClaim，如果值未变，则不做处理
        /// </summary>
        /// <param name="id">Application User</param>
        /// <param name="userClaims">User拥有的UserClaim</param>
        /// <returns></returns>
        private async Task<ApplicationUser> EditUserClaimToUser(Guid id, IDictionary<string, string> userClaims)
        {
            bool success = false;
            var user = await _userManager.Users.Include(a => a.Claims).Where(a => a.Id == id).FirstOrDefaultAsync();
            if (user != null)
            {
                foreach (var userClaim in userClaims)
                {
                    var findClaim = user.Claims.AsParallel().FirstOrDefault(a => a.ClaimType == userClaim.Key);
                    if (findClaim != null)
                    {
                        if (findClaim.ClaimValue != userClaim.Value)
                        {
                            //更改user的UserClaim

                            var newClaim = new Claim(userClaim.Key, userClaim.Value);
                            try
                            {
                               var result = await _userManager.ReplaceClaimAsync(user, findClaim.ToClaim(), newClaim);
                                success = result.Succeeded ? true : false;
                            }
                            catch (Exception)
                            {
                                success = false;
                            }
                        }
                    }
                }
            }
            return success ? user : null;
        }

        #endregion

        #region 删除UserClaim
        /// <summary>
        /// 
        /// </summary>
        /// <param name="id">Application User Id</param>
        /// <returns></returns>
        public async Task<IActionResult> DeleteUserClaim(Guid? id)
        {
            if (!id.HasValue || id.Value.Equals(Guid.Empty))
            {
                return BadRequest();
            }
            var userModel = await _userManager.Users.Include(a => a.Claims).Where(a => a.Id == id).FirstOrDefaultAsync();
            if (userModel != null)
            {
                var viewModel = new DeleteUserClaimViewModel
                {
                    Id = userModel.Id,
                    FullName = userModel.FullName,
                    UserName = userModel.UserName,
                    UserClaims = userModel.Claims.OrderBy(a => a.ClaimType).Select(a => new { ClaimType = a.ClaimType, ClaimValue = a.ClaimValue }).ToDictionary(a => a.ClaimType, a => a.ClaimValue)
                };
                return View(viewModel);
            }
            return BadRequest();

        }

        /// <summary>
        /// 从User中删除指定的UserClaim
        /// </summary>
        /// <param name="id">ApplicationUser</param>
        /// <param name="userClaimType"></param>
        /// <returns>如果删除成功，返回OkResult，否则返回Empty</returns>
        [HttpPost]
        [AutoValidateAntiforgeryToken]
        
        public async Task<IActionResult> DeleteUserClaimFromUser(Guid id, string userClaimType)
        {
            bool result = false;
            if (id.Equals(Guid.Empty))
            {
                return new JsonResult(new { success = false, id = id });
            }
            var userModel = await _userManager.Users.Include(a => a.Claims).Where(a => a.Id == id).FirstOrDefaultAsync();
            if(userModel != null)
            {
                var userClaim = userModel.Claims.FirstOrDefault(a => a.ClaimType == userClaimType);
                if(userClaim != null)
                {                   
                    var removeResult =await _userManager.RemoveClaimAsync(userModel, userClaim.ToClaim());
                    result= removeResult.Succeeded;
                }
            }
            if (result)
            {
                return new JsonResult(new { success = true, id = id });
            }
            else
            {
                return new JsonResult(new { success = false, id = id });
            }
        }
        #endregion
    }
}