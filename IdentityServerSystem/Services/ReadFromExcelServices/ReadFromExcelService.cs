using ExcelWithEpplusCore;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace IdentityServerSystem.Services.ReadFromExcelServices
{
    public abstract class ReadFromExcelService
    {
        private string excelType;
        private Stream fileStream;
        private string startCellName;
        private int keyColumn;
        private int mergeTitleRow;


        public ReadFromExcelService(string excelType, Stream fileStream, string startCellName = "B1", int mergeTitleRow = 1, int keyColumn = 1)
        {
            this.excelType = excelType;
            this.fileStream = fileStream;
            this.startCellName = startCellName;
            this.mergeTitleRow = mergeTitleRow;
            this.keyColumn = keyColumn;
        }
        /// <summary>
        /// 验证Excel的格式是否为指定格式
        /// </summary>
        /// <returns></returns>
        protected virtual bool ValidExcelFormat()
        {
            if (excelType.Equals(ExcelEntityFactory.GetInstance().Excel2007ContentType))
            {
                return true;
            }
            return false;
        }

        /// <summary>
        /// 从Excel中读取数据，
        /// </summary>
        /// <typeparam name="T">与Excel对应的数据模型</typeparam>
        /// <param name="fileStream">文件Stream</param>
        /// <param name="startCellName">数据开始的单元格，如Key在A列，只有一个标题，则数据开始的单元格为"B1"</param>
        /// <returns></returns>
        protected virtual Dictionary<string, T> GetMessageFromExcelWithFileStream<T>() where T : new()
        {

            var factory = ExcelEntityFactory.GetInstance();
            var TPropertyNameDisplayAttributeNameDic = GetTPropertyNameDisplayAttributeNameDic<T>();
            var result = factory.CreateReadFromExcel().ExcelToEntityDictionary<T>(TPropertyNameDisplayAttributeNameDic, fileStream, out StringBuilder errorMesg, null, startCellName, mergeTitleRow, keyColumn);
            return result;
        }

        /// <summary>
        /// 将Excel表中的数据写入到数据库中，返回出现的错误信息
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="messageFromExcel"></param>
        /// <returns>返回添加到数据库中出现的错误信息</returns>
        protected abstract Task<string> SaveDataFromExcelToDataBase<T>(Dictionary<string, T> messageFromExcel) where T : new();

        /// <summary>
        /// 执行顺序
        /// </summary>
        public virtual async Task<string> ExcuteAsync<T>() where T : new()
        {
            if (ValidExcelFormat())
            {
                var messageFromExcel = GetMessageFromExcelWithFileStream<T>();
                return await SaveDataFromExcelToDataBase<T>(messageFromExcel);
            }
            else
            {
                return "Excel格式错误，只接受.xlsx的文件格式";
            }
        }
        /// <summary>
        /// 获取类中属性的DisplayAttribute值，该值作为Value，属性名作为Key
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        private Dictionary<string, string> GetTPropertyNameDisplayAttributeNameDic<T>()
        {
            List<PropertyInfo> propertyInfoList = new List<PropertyInfo>(typeof(T).GetProperties());
            var propertyNameDisplayAttributeNameDic = new Dictionary<string, string>();
            foreach (var property in propertyInfoList)
            {
                //var displayName2 = property.GetCustomAttribute<DisplayNameAttribute>(true).DisplayName;
                propertyNameDisplayAttributeNameDic[property.Name] = (property.GetCustomAttributes(typeof(DisplayAttribute), true).FirstOrDefault() as DisplayAttribute).GetName();
            }
            return propertyNameDisplayAttributeNameDic;
        }
    }
}
