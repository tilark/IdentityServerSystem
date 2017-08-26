using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using System.Net.Http;
using IdentityModel.Client;
using Newtonsoft.Json.Linq;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Options;
using IdentityServerSystem.Models.GetUserInfoFromWebApiViewModels;
using Newtonsoft.Json;
using IdentityServerSystem.Models.ManageUserViewModels;
using Microsoft.AspNetCore.Identity;
using IdentityServerSystem.Models;

namespace IdentityServerSystem.Controllers
{
    //[Authorize(Policy = "Administrator")]
    /// <summary>
    /// 从Web Api获得人员信息，包括User Id，Name, EmployeeNo
    /// </summary>
    public class GetUserInfoFromWebApiController : Controller
    {
        private readonly IOptions<HumanResourceIdentityOption> _userInfoFromHumanResourceOption;
        private readonly UserManager<ApplicationUser> _userManager;

        public GetUserInfoFromWebApiController(UserManager<ApplicationUser> userManager, IOptions<HumanResourceIdentityOption> userInfoFromHumanResourceOption)
        {
            this._userManager = userManager;
            this._userInfoFromHumanResourceOption = userInfoFromHumanResourceOption;
        }
        public IActionResult Index()
        {
            return View();
        }

       

        #region 从人力资源系统中的WebApi中获得用户信息
        public IActionResult GetUserInfoFromHumanResourceSystem()
        {
            return View();
        }

        #region 创建人员信息及登录帐号
        /// <summary>
        /// 创建用户信息，如果不存在，则创建如果存在，则不处理。
        /// </summary>
        /// <returns>返回处理后的信息</returns>
        [HttpPost]
        [AutoValidateAntiforgeryToken]
        public async Task<IActionResult> CreateUserIfNotExistFromWebApi()
        {

            var resultViewModel = await CreateUserInfoIfNotExistFromHumanResourceManagerSystem();

            //需将处理的信息显示在界面上           
            return View("HandleWebApiContentResult", resultViewModel);
        }

        /// <summary>
        /// 创建用户信息，如果不存在，则创建如果存在，则不处理。
        /// 从/api/v2/WebApiPersonInfoes获取信息
        /// </summary>
        /// <returns></returns>
        private async Task<HandleWebApiCotentResult> CreateUserInfoIfNotExistFromHumanResourceManagerSystem()
        {
            HandleWebApiCotentResult result = new HandleWebApiCotentResult();

            // Call Cliennt
            result = await GetPersonInfoJsonStringFromHumanResourceSystem();

            if (result.Successed)
            {

                var newUserInfoModels = TransferWebApiUserInfosToCreateUserViewModelForCreateNewUser(result.ResponseContent);

                if (newUserInfoModels != null && newUserInfoModels.Count > 0)
                {
                    result.TotalContentCount = newUserInfoModels.Count;
                    foreach (var user in newUserInfoModels)
                    {
                        var userResult = await user.CreateUserAsync(_userManager);
                        if (userResult != null)
                        {
                            result.SuccessCount++;
                        }
                    }
                }
            }
            return result;
        }
        #endregion

        #region 更新人员信息
        /// <summary>
        /// 查询数据库中是否存在该用户，如果存在，则判断是否有登录帐号是否与得到的工号信息相同，如果不相同，则修改登录帐号，相同则不处理。
        /// </summary>
        /// <returns></returns>
        public async Task<IActionResult> UpdatePersonInfoFromWebApi()
        {
            var resultViewModel = await UpdatePersonInfoFromHumanResourceSystem();
            //需将处理的信息显示在界面上           
            return View("HandleWebApiContentResult", resultViewModel);
        }
        /// <summary>
        /// 查询数据库中是否存在该用户，如果存在，则判断是否有登录帐号是否与得到的工号信息相同，如果不相同，则修改登录帐号，相同则不处理。
        /// </summary>
        /// <returns></returns>
        private async Task<HandleWebApiCotentResult> UpdatePersonInfoFromHumanResourceSystem()
        {
            HandleWebApiCotentResult result = new HandleWebApiCotentResult();

            result = await GetPersonInfoJsonStringFromHumanResourceSystem();

            if (result.Successed)
            {
                var newUserInfoModels = TransferWebApiUserInfosToEditUserViewModelForUpdateUser(result.ResponseContent);

                if (newUserInfoModels != null && newUserInfoModels.Count > 0)
                {
                    result.TotalContentCount = newUserInfoModels.Count;
                    foreach (var user in newUserInfoModels)
                    {
                        var userResult = await user.UpdateUserInfo(_userManager);
                        if (userResult != null)
                        {
                            result.SuccessCount++;
                        }
                    }
                }
            }
            return result;
        }
        #endregion

        #region 更改人员登录帐号
        /// <summary>
        /// 更改人员登录帐号，如果人力资源系统中已标记为删除，则在此处需删除登录帐号
        /// </summary>
        /// <returns></returns>
        /// 
        [HttpPost]
        [AutoValidateAntiforgeryToken]
        public async Task<IActionResult> UpdatePersonInfoAccountFromWebApi()
        {
            var resultViewModel = await UpdateUserAccountFromHumanResourceSystem();
            //需将处理的信息显示在界面上           
            return View("HandleWebApiContentResult", resultViewModel);
        }
        /// <summary>
        /// 更改人员登录帐号，如果人力资源系统中已标记为删除，则在此处需删除登录帐号
        /// 查找人员信息，如果找到，则判断登录帐号是工号是相同，如果不相同，则需更改登录帐号，如果相同则不处理。
        /// </summary>
        /// <returns>从WebpApi中获得结果</returns>
        private async Task<HandleWebApiCotentResult> UpdateUserAccountFromHumanResourceSystem()
        {
            HandleWebApiCotentResult result = new HandleWebApiCotentResult();

            result = await GetPersonInfoJsonStringFromHumanResourceSystem();

            if (result.Successed)
            {
                //获得人员删除标记为false的人员，更改其登录帐号
                var newUserInfoModels = TransferWebApiUserInfosToChangeUserAccountViewModelForUpdateUserAccount(result.ResponseContent);
                if (newUserInfoModels != null && newUserInfoModels.Count > 0)
                {
                    result.TotalContentCount += newUserInfoModels.Count;
                    foreach (var user in newUserInfoModels)
                    {
                        var userResult = await user.ChangeUserAccountAsync(_userManager);
                        if (userResult != null && userResult.Succeeded)
                        {
                            result.SuccessCount++;
                        }
                    }
                }
                //获得人员删除标记为true的人员，删除其登录帐号
                var deleteUserInfoModels = TransferWebApiUserInfosToDeleteUserViewModelForUpdateUserAccount(result.ResponseContent);
                if (deleteUserInfoModels != null && deleteUserInfoModels.Count > 0)
                {
                    result.TotalContentCount += deleteUserInfoModels.Count;
                    foreach (var user in deleteUserInfoModels)
                    {
                        var userResult = await user.DeleteUserAsync(_userManager);
                        if (userResult != null && userResult.Succeeded)
                        {
                            result.SuccessCount++;
                        }
                    }
                }
            }
            return result;
        }

      

        #endregion
        #endregion     

        #region Private 从WebApi中获得人员信息的Json字符串
        /// <summary>
        /// 通过构建Http Client从人力资源系统中获得人员信息的Json字符串
        /// </summary>
        /// <returns>获取成功，返回获取的信息。获取失败，返回错误信息</returns>
        private async Task<HandleWebApiCotentResult> GetPersonInfoJsonStringFromHumanResourceSystem()
        {
            HandleWebApiCotentResult result = new HandleWebApiCotentResult();
            // Call Cliennt
            var client = new HttpClient();
            var respone = await client.GetAsync(_userInfoFromHumanResourceOption.Value.PersonInfosUriNoAuthorize);
            if (!respone.IsSuccessStatusCode)
            {
                result.Successed = false;
                result.ErrorMessage = respone.StatusCode.ToString() + respone.RequestMessage.ToString();
            }
            result.Successed = true;
            result.ResponseContent = await respone.Content.ReadAsStringAsync();
            return result;
        }
        #endregion
        #region Private 从WebApi中获得的字符串转换成目标结构

        /// <summary>
        /// 转换成DeleteUserViewModel，获得删除标记为true的人员信息
        /// </summary>
        /// <param name="responseContent"></param>
        /// <returns></returns>
        private List<DeleteUserViewModel> TransferWebApiUserInfosToDeleteUserViewModelForUpdateUserAccount(string responseContent)
        {
            if (!String.IsNullOrEmpty(responseContent))
            {
                try
                {
                    var userInfoList = JsonConvert.DeserializeObject<List<UserInfoFromHumanResourceModel>>(responseContent);
                    var newUserInfoModel = userInfoList.AsParallel().Where(a => a.deleted == true).Select(a => new DeleteUserViewModel { Id = a.userId }).ToList();
                    return newUserInfoModel;
                }
                catch (Exception)
                {
                    return null;
                }

            }
            return null;
        }


        /// <summary>
        /// 将从WebApi中获得的字符串转换成ChangeUserAccountViewModel
        /// </summary>
        /// <param name="content">从Web Api中获得的字符串</param>
        /// <returns>返回List<ChangeUserAccountViewModel> </returns>
        private List<ChangeUserAccountViewModel> TransferWebApiUserInfosToChangeUserAccountViewModelForUpdateUserAccount(string content)
        {
            if (!String.IsNullOrEmpty(content))
            {
                try
                {
                    var userInfoList = JsonConvert.DeserializeObject<List<UserInfoFromHumanResourceModel>>(content);
                    var newUserInfoModel = userInfoList.AsParallel().Where(a => a.deleted==false).Select(a => new ChangeUserAccountViewModel { Id = a.userId, NewUserName = a.employeeNo }).ToList();
                    return newUserInfoModel;
                }
                catch (Exception)
                {
                    return null;
                }

            }
            return null;
        }

        /// <summary>
        /// 将从WebApi中获得的字符串转换成目标结构，创建登录帐号，过滤已删除的用户，不添加
        /// UserInfoFromHumanResourceModel为人力资源系统返回的Json格式。将人员的工号作为登陆名
        /// </summary>
        /// <param name="content">WebApi中获得的字符串</param>
        /// <returns></returns>
        private List<CreateUserViewModel> TransferWebApiUserInfosToCreateUserViewModelForCreateNewUser(string content)
        {
            if (!String.IsNullOrEmpty(content))
            {
                try
                {
                    var userInfoList = JsonConvert.DeserializeObject<List<UserInfoFromHumanResourceModel>>(content);
                    var newUserInfoModel = userInfoList.AsParallel().Where(a => a.deleted == false).Select(a => new CreateUserViewModel { Id = a.userId, UserName = a.employeeNo, FamilyName = a.userName[0].ToString(), FirstName = a.userName.Substring(1).ToString(), Password = "123456", ConfirmPassword = "123456" }).ToList();
                    return newUserInfoModel;
                }
                catch (Exception)
                {
                    return null;
                }

            }
            return null;
        }

        /// <summary>
        /// 将从WebApi中获得的字符串转换成目标结构，创建登录帐号，过滤已删除的用户，不添加
        /// UserInfoFromHumanResourceModel为人力资源系统返回的Json格式。将人员的工号作为登陆名
        /// </summary>
        /// <param name="content">WebApi中获得的字符串</param>
        /// <returns></returns>
        private List<EditUserViewModel> TransferWebApiUserInfosToEditUserViewModelForUpdateUser(string content)
        {
            if (!String.IsNullOrEmpty(content))
            {
                try
                {
                    var userInfoList = JsonConvert.DeserializeObject<List<UserInfoFromHumanResourceModel>>(content);
                    var newUserInfoModel = userInfoList.AsParallel().Where(a => a.deleted == false).Select(a => new EditUserViewModel { Id = a.userId, UserName = a.employeeNo, FamilyName = a.userName[0].ToString(), FirstName = a.userName.Substring(1).ToString() }).ToList();
                    return newUserInfoModel;
                }
                catch (Exception)
                {
                    return null;
                }
            }
            return null;
        }
        #endregion

    }
}