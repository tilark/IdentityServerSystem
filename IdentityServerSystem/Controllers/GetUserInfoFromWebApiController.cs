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
        public IActionResult GetUserInfoFromHumanResourceSystem()
        {
            return View();
        }
        /// <summary>
        /// 从HumanResource系统中获得科室信息，利用ClientID与Secret获取
        /// </summary>
        /// <returns></returns>
        /// 
        [HttpGet]
        public async Task<IActionResult> GetUserInfoFromHumanResourceSystemPost()
        {
            var result = await GetUserInfoFromWebApiForIdentityServer();
            if (String.IsNullOrEmpty(result))
            {
                return RedirectToAction("ListUsers", "ManageUsers");

            }
            ViewBag.ErrorMessage = result;
            //await TestGetUserInfoFromWebApi();
            return View("GetUserInfoFromHumanResourceSystem");
        }

        /// <summary>
        /// 从人力资源系统中获取用户信息，无认证
        /// </summary>
        /// <returns></returns>
        private async Task<string> GetUserInfoFromWebApiForIdentityServer()
        {
            string result = null;

            // Call Cliennt
            var client = new HttpClient();
            var respone = await client.GetAsync(_userInfoFromHumanResourceOption.Value.PersonInfosUriNoAuthorize);
            if (!respone.IsSuccessStatusCode)
            {
                result = respone.StatusCode.ToString() + respone.RequestMessage.ToString();
            }
            else
            {
                var content = await respone.Content.ReadAsStringAsync();

                if (!String.IsNullOrEmpty(content))
                {
                    var userInfoList = JsonConvert.DeserializeObject<List<UserInfoFromHumanResourceModel>>(content);
                    var newUserInfoModel = userInfoList.AsParallel().Select(a => new CreateUserViewModel { Id = a.userId, UserName = a.employeeNo, FamilyName = a.userName[0].ToString(), FirstName = a.userName.Substring(1).ToString(), Password = "123456", ConfirmPassword = "123456" }).ToList();

                    foreach (var user in newUserInfoModel)
                    {
                        await user.CreateUserAsync(_userManager);
                    }
                }
            }
            return result;
        }

        /// <summary>
        /// 使用帐号及密码获取WebApi
        /// </summary>
        /// <returns></returns>
        private async Task<string> GetUserInfoFromWebApiUsePassword()
        {
            string result = null;
            DiscoveryResponse disco = await DiscoveryClient.GetAsync(_userInfoFromHumanResourceOption.Value.Authority);
            var tokenClient = new TokenClient(disco.TokenEndpoint, "ClientHRMSPassword", "ClientHRMSPassword");
            //TokenResponse tokenResponse = await tokenClient.RequestClientCredentialsAsync(_userInfoFromHumanResourceOption.Value.Scope);
            TokenResponse tokenResponse = await tokenClient.RequestResourceOwnerPasswordAsync("administrator","52166057", "humanresourcesystem");

            if (tokenResponse.IsError)
            {
                result = tokenResponse.Error;
                return result;
            }
            // Call Cliennt
            var client = new HttpClient();
            client.SetBearerToken(tokenResponse.AccessToken);
            //var respone = await client.GetAsync(_userInfoFromHumanResourceOption.Value.PersonInfoListUri);
            var respone = await client.GetAsync("http://localhost:5002/personinfoes/getuserinfolist");

            if (!respone.IsSuccessStatusCode)
            {
                result = respone.StatusCode.ToString() + ":" + respone.RequestMessage.ToString();
            }
            else
            {
                var content = await respone.Content.ReadAsStringAsync();

                if (!String.IsNullOrEmpty(content))
                {
                    var userInfoList = JsonConvert.DeserializeObject<List<UserInfoFromHumanResourceModel>>(content);
                    var newUserInfoModel = userInfoList.AsParallel().Select(a => new CreateUserViewModel { Id = a.userId, UserName = a.employeeNo, FamilyName = a.userName[0].ToString(), FirstName = a.userName.Skip(0).ToString(), Password = "123456", ConfirmPassword = "123456" }).ToList();

                    foreach (var user in newUserInfoModel)
                    {
                        await user.CreateUserAsync(_userManager);
                    }
                }
            }
            return result;
        }
       

        #region 通过ClientCredentials获得WebApi的资源！，失败
        /// <summary>
        /// WebApi Uri:http://localhost:5002/departments/getdepartmentlist
        /// </summary>
        /// <returns></returns>
        public async Task TestGetUserInfoFromWebApi()
        {

            DiscoveryResponse disco = await DiscoveryClient.GetAsync("http://localhost:5000");
            var tokenClient = new TokenClient(disco.TokenEndpoint, "ClientHRMS", "humanresourcesystem");

            TokenResponse tokenResponse = await tokenClient.RequestClientCredentialsAsync("humanresourcesystem");

            if (tokenResponse.IsError)
            {
                return;
            }
            // Call Cliennt
            var client = new HttpClient();
            //client.SetBearerToken(tokenResponse.AccessToken);
            var respone = await client.GetAsync("http://localhost:5002/PersonInfoes/GetUserInfoList");
            if (!respone.IsSuccessStatusCode)
            {
                ViewBag.ErrorMessage = respone.StatusCode;
            }
            else
            {
                var content = await respone.Content.ReadAsStringAsync();

                ViewBag.GetContent = JArray.Parse(content);
            }
        }
        #endregion
    }
}