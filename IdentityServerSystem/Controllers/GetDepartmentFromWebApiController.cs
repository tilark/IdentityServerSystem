using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using IdentityModel.Client;
using System.Net.Http;
using Newtonsoft.Json.Linq;

namespace IdentityServerSystem.Controllers
{
    /// <summary>
    /// 测试从Web Api中获取数据
    /// </summary>
    public class GetDepartmentFromWebApiController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }

        /// <summary>
        /// 从HumanResource系统中获得科室信息，利用ClientID与Secret获取
        /// </summary>
        /// <returns></returns>
        public async Task<IActionResult> GetDepartmentMessageFromHumanResourceSystem()
        {
            await TestGetIdentityServerResource();
            return View();
        }
        #region 通过ClientCredentials获得WebApi的资源！，成功
        /// <summary>
        /// WebApi Uri:http://localhost:5002/departments/getdepartmentlist
        /// </summary>
        /// <returns></returns>
        public async  Task TestGetIdentityServerResource()
        {

            DiscoveryResponse disco = await DiscoveryClient.GetAsync("http://localhost:5000");
            var tokenClient = new TokenClient(disco.TokenEndpoint, "ClientIDWithEditOwned", "secret3");

            TokenResponse tokenResponse = await tokenClient.RequestClientCredentialsAsync("scope.editownered");

            if (tokenResponse.IsError)
            {
                return;
            }
            // Call Cliennt
            var client = new HttpClient();
            client.SetBearerToken(tokenResponse.AccessToken);
            var respone = await client.GetAsync("http://localhost:5002/departments/getdepartmentlist");
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