using LV.Db.Common;
using Npgsql;
using System;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;

namespace LV.Db.PostgreSQL
{
    public class Utils
    {
        /// <summary>
        /// Process exception when excute Insert
        /// </summary>
        /// <param name="ex">Exception</param>
        /// <returns>InsertDataResult<T></returns>
        public InsertDataResult<T> ProcessException <T>(NpgsqlException ex, InsertDataResult<T> insertDataResult)
        {
            /*
             * Yêu cầu sửa lại như sau:
             * Các lỗi kg xử lý được thì phải throw exception
             * Đoạn code này chỉ mới bắt 2 lỗi 23505,23502 còn các lỗi khác thì kg bắt
             */
            //Duplicate unique fields case
            if (((PostgresException)ex).SqlState == "23505")
            {
                string[] fields = new string[] { };
                string detail = ((PostgresException)ex).Detail;
                string leftStr = detail.Split('=')[0];
                string strFieldNames = leftStr.Split('(')[1].Split(')')[0];
                string[] str = strFieldNames.Split(',');
                fields = str.Select(p => p.Replace(@"""", "")?.Replace(@"\", "")).ToArray();
                insertDataResult.Error = new ErrorAction()
                {
                    ErrorType = DataActionErrorTypeEnum.DuplicateData,
                    Fields = fields
                };
            }
            //Require fields are null
            if (((PostgresException)ex).SqlState == "23502")
            {
                string[] fields = new string[1];
                string message = ((PostgresException)ex).Message;                
                fields[0] = Regex.Split(message, @"\""")[1];
                insertDataResult.Error = new ErrorAction()
                {
                    ErrorType = DataActionErrorTypeEnum.MissingFields,
                    Fields = fields
                };
            }
            return insertDataResult;
        }

        /// <summary>
        /// Process exception when excute Update
        /// </summary>
        /// <param name="ex">Exception</param>
        /// <returns>UpdateDataResult<T></returns>
        public UpdateDataResult<T> ProcessException<T>(NpgsqlException ex, UpdateDataResult<T> updateDataResult)
        {
            //Duplicate unique fields case
            if (((PostgresException)ex).SqlState == "23505")
            {
                string[] fields = new string[] { };
                string detail = ((PostgresException)ex).Detail;
                string leftStr = detail.Split('=')[0];
                string strFieldNames = leftStr.Split('(')[1].Split(')')[0];
                string[] str = strFieldNames.Split(',');
                fields = str.Select(p => p.Replace(@"""", "")?.Replace(@"\", "")).ToArray();
                updateDataResult.Error = new ErrorAction()
                {
                    ErrorType = DataActionErrorTypeEnum.DuplicateData,
                    Fields = fields
                };
            }
            //Require fields are null
            if (((PostgresException)ex).SqlState == "23502")
            {
                string[] fields = new string[1];
                string message = ((PostgresException)ex).Message;
                fields[0] = Regex.Split(message, @"\""")[1];
                updateDataResult.Error = new ErrorAction()
                {
                    ErrorType = DataActionErrorTypeEnum.MissingFields,
                    Fields = fields
                };
            }
            return updateDataResult;
        }

        /// <summary>
        /// Process exception when excute Delete
        /// </summary>
        /// <param name="ex">Exception</param>
        /// <returns>deleteDataResult<T></returns>
        public DeleteDataResult<T> ProcessException<T>(NpgsqlException ex, DeleteDataResult<T> deleteDataResult)
        {
            //Foreign key exception
            if (((PostgresException)ex).SqlState == "23503")
            {
                string[] fields = new string[1];
                string[] tables = new string[1];
                string detail = ((PostgresException)ex).Detail;
                string leftStr = detail.Split('=')[0];
                string tableName = Regex.Split(detail, @"""")[1].Replace(@"""","");
                string fieldName = leftStr.Split('(')[1].Split(')')[0];
                fields[0] = fieldName;
                tables[0] = tableName;
                deleteDataResult.Error = new ErrorAction()
                {
                    ErrorType = DataActionErrorTypeEnum.ForeignKey,
                    Fields = fields,
                    RefTables = tables
                };
            }            
            return deleteDataResult;
        }

        internal static bool IsAnonymousType(Type type)
        {
            if (type.IsGenericType)
            {
                var d = type.GetGenericTypeDefinition();
                if (d.IsClass && d.IsSealed && d.Attributes.HasFlag(TypeAttributes.NotPublic))
                {
                    var attributes = d.GetCustomAttributes(typeof(CompilerGeneratedAttribute), false);
                    if (attributes != null && attributes.Length > 0)
                    {
                        //WOW! We have an anonymous type!!!
                        return true;
                    }
                }
            }
            return false;

        }
    }
}
