using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace IdentityServerSystem.Models.GetUserInfoFromWebApiViewModels
{
    /// <summary>
    /// 处理WebApi的数据的操作结果
    /// </summary>
    public class HandleWebApiCotentResult
    {
        public HandleWebApiCotentResult()
        {
            Successed = false;
            ErrorMessage = "NULL";
            TotalContentCount = 0;
            SuccessCount = 0;
        }
        public bool Successed { get; set; }
        public string ErrorMessage { get; set; }

        public string ResponseContent { get; set; }
        public int TotalContentCount { get; set; }
        public int SuccessCount { get; set; }

        public int FailedCount { get { return TotalContentCount - SuccessCount; } }
    }
}
