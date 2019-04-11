using System;
using System.Collections.Generic;
using System.Data;
using System.Linq.Expressions;
using System.Text;

namespace LV.Db.Common
{
    public interface IDbContext
    {
        IList<T> ExecToList<T>(string SQL, params object[] pars);
        DataTable ExecToDataTable(string SQL, params object[] pars);
        InsertDataResult<T> InsertData<T>(string Schema, string TableName, object Data, bool ReturnError = false);
        UpdateDataResult<T> UpdateData<T>(string Schema,string TableName, Expression<Func<T, bool>> Where, object Data, bool ReturnError = false);
        DeleteDataResult<T> DeleteData<T>(string Schema, string TableName, Expression<Func<T, bool>> Where, bool ReturnError = false);
        QuerySet<T> FromNothing<T>(Expression<Func<object, T>> Selector);

        void BeginTran(DbIsolationLevel level);
        void Commit();

        void RollBack();

    }
}
