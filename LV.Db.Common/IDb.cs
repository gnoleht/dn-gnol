using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Linq.Expressions;
using System.Text;

namespace LV.Db.Common
{
    public interface IDb
    {
        
        void CloseConnection();
        DataTable ExecToDataTable(string SQL, params object[] pars);
        IList<T> ExecToList<T>(string SQL, params object[] pars);
        DataActionResult InsertData(string SqlCommandText, string Schema, string TableName, TableInfo tblInfo, List<object> InputParams);
        DataActionResult DeleteData(string SqlCommandText, string Schema, string TableName, TableInfo tblInfo, List<object> InputParams);
        DataActionResult UpdateData(string SqlCommandText, string Schema, string TableName, TableInfo tblInfo, List<object> InputParams);
        void SetConnectionString(string connectionString);
        void SetSchema(string schema);
        
        string GetSchema();

        void BeginTran(DbIsolationLevel level);
        void Commit();

        void RollBack();
        DataActionResult ExecCommand(string sql, bool returnError,object[] v);
        string GetConnectionString();
        TableInfo GetTableInfo(string ConnectionString, string schema, string table_name);
    }
}