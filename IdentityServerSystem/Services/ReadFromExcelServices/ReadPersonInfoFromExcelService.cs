using IdentityServerSystem.Models;
using IdentityServerSystem.Models.GetUserInfoFromExcelViewModel;
using IdentityServerSystem.Models.ManageUserViewModels;
using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IdentityServerSystem.Services.ReadFromExcelServices
{
    /// <summary>
    /// 固定的Excel格式
    /// |Id|姓名|工号|科室|性别|人员类型|已删除|
    ///|---|---|---|---|---|---|
    ///|a3c6a1b9-8e54-46c9-a864-002b2cfc639e|姓名1|1234|ICU|女|护士|FALSE|
    ///startCellName 从姓名开始
    ///keyColumn为B列，即第2列
    /// </summary>
    public class ReadPersonInfoFromExcelService : ReadFromExcelService
    {
        private readonly UserManager<ApplicationUser> _userManager;

        public ReadPersonInfoFromExcelService(UserManager<ApplicationUser> userManager, string excelType, Stream fileStream, string startCellName = "B1", int mergeTitleRow = 1, int keyColumn = 1) : base(excelType, fileStream, startCellName, mergeTitleRow, keyColumn)
        {
            this._userManager = userManager;
        }

        /// <summary>
        /// 创建新用户
        /// </summary>
        /// <typeparam name="T">"Key"为各人员的Guid Id, Value为姓名、工号、科室、性别、人员类型、已删除</typeparam>
        /// <param name="messageFromExcel"></param>
        /// <returns></returns>
        protected override async Task<string> SaveDataFromExcelToDataBase<T>(Dictionary<string, T> messageFromExcel)
        {
            StringBuilder errorMsg = new StringBuilder(200);
            if(messageFromExcel != null && messageFromExcel.Count > 0)
            {
                var dataFromExcel = messageFromExcel as Dictionary<string, UserInfoFromExcelModel>;
                var newUserInfoModel = dataFromExcel.AsParallel().Where(a => a.Value.DeleteFlag != "TRUE").Select(a => new CreateUserViewModel { Id = Guid.Parse(a.Key), UserName = a.Value.EmployeeNo, FamilyName = a.Value.UserName[0].ToString(), FirstName = a.Value.UserName.Substring(1), Password = "123456", ConfirmPassword = "123456" }).ToList();
                foreach (var user in newUserInfoModel)
                {
                    await user.CreateUserAsync(_userManager);
                }
            }
            else
            {
                errorMsg.Append("读取Excel数据失败!");
            }
            return errorMsg.ToString();
        }
    }
}
