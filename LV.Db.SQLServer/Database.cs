using LV.Db.Common;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace LV.Db.SQLServer
{
    public class Database : IDb
    {
        private SqlConnection cnn;
        private string schema;
        private SqlTransaction tran;
        public Database()
        {
            this.ConnectionString = Globals.ConnectionString;
        }

        public SqlConnection Connection
        {
            get
            {
                if (cnn == null) cnn = new SqlConnection(this.ConnectionString);
                return cnn;
            }
        }

        private void OpenConnect()
        {
            if (this.Connection.State != ConnectionState.Open)
            {
                this.Connection.Open();
            }
        }
        public void BeginTran(DbIsolationLevel level)
        {
            this.OpenConnect();

            if (tran==null)
                tran = this.Connection.BeginTransaction();
            else
            {
                throw new Exception("Transaction is use");
            }
        }

        public void Commit()
        {
            if (tran != null)
                tran.Commit();
            tran = null;
        }

        public void RollBack()
        {
            if (tran != null)
                tran.Rollback();
            tran = null;
        }

        public string ConnectionString { get; private set; }

        public void CloseConnection()
        {
            this.RollBack();
            this.Connection.Close();
            this.Connection.Dispose();
        }
        public void SetConnectionString(string connectionString)
        {
            this.ConnectionString = ConnectionString;
        }
        public void SetSchema(string schema)
        {
            this.schema = schema;
        }
        public string GetSchema()
        {
            return this.schema;
        }
        public DataTable ExecToDataTable(string SQL, params object[] pars)
        {
            if (this.cnn.State != System.Data.ConnectionState.Open)
            {
                this.cnn.Open();
            }
            SqlCommand command = new SqlCommand(SQL,this.cnn);
            var paramIndex = 0;
            foreach(var p in pars)
            {
                var paramName = string.Format("@p{0}", paramIndex);
                command.CommandText = command.CommandText.Replace("{" + paramIndex.ToString() + "}", paramName);
                var sqlParam = new SqlParameter();
                sqlParam.ParameterName = paramName;
                if (p != null)
                {
                    sqlParam.Value = p;
                }
                else
                {
                    sqlParam.Value = DBNull.Value;
                }
                command.Parameters.Add(sqlParam);
                paramIndex++;

            }
            var adp = new SqlDataAdapter(command);
            var tbl = new DataTable();
            adp.Fill(tbl);
            return tbl;
        }

        public IList<T> ExecToList<T>(string SQL, params object[] pars)
        {
            var ret = new List<T>();
            var dataTable = this.ExecToDataTable(SQL, pars);
            DataColumn x;

            var cols = dataTable.Columns.Cast<DataColumn>().Join(typeof(T).GetProperties().Cast<PropertyInfo>(), p => p.ColumnName,q=>q.Name, (p, q) => new
            {
                name=p.ColumnName,
                property = q
            });
            foreach(DataRow r in dataTable.Rows)
            {
                var item = (T)Activator.CreateInstance(typeof(T));
                foreach(var col in cols)
                {
                    if (r[col.name] != DBNull.Value)
                    {
                        col.property.SetValue(item, r[col.name]);
                    }
                    
                }
                ret.Add(item);
            }
            return ret;
        }

        #region Implemmetation of DML
        /// <summary>
        /// Insert into table, DML statement will be excuted here 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="tableName"></param>
        /// <param name="data"></param>
        /// <returns></returns>
        public InsertDataResult<T> InsertData<T>(string tableName, T data)
        {
            if (data == null) return null;
            var type = typeof(T);

            var fieldList = type.GetProperties().Select(o=>new {
                Name=o.PropertyType,
                Value=o.GetValue(data,null),
                Type=o.PropertyType
            }).ToList();

            var InnserQuery = string.Format("INSERT INTO {0}({1}) VALUES ({2})", 
                tableName,
                string.Join(",",fieldList.Select(o=>o.Name)), 
                string.Join(",", fieldList.Select(o => "@" + o.Name)));

            using(SqlCommand cm = new SqlCommand(InnserQuery,this.Connection))
            {
                this.OpenConnect();
                cm.Transaction = this.tran;
                cm.Parameters.AddRange(fieldList.Select(o => new SqlParameter() {
                    ParameterName = "@" + o.Name,
                    Value = o.Value
                }).ToArray());

                cm.ExecuteScalar();
            }
            

            return new InsertDataResult<T>();
        }

        public DataActionResult InsertData(string SqlCommandText, string Schema, string TableName, TableInfo tblInfo, List<object> InputParams)
        {
            throw new NotImplementedException();
        }

        public DataActionResult DeleteData(string SqlCommandText, string Schema, string TableName, TableInfo tblInfo, List<object> InputParams)
        {
            throw new NotImplementedException();
        }

        public DataActionResult UpdateData(string SqlCommandText, string Schema, string TableName, TableInfo tblInfo, List<object> InputParams)
        {
            throw new NotImplementedException();
        }

        public DataActionResult ExecCommand(string sql, bool returnError, object[] v)
        {
            TableInfo table = new TableInfo();
            using (SqlCommand command = this.Connection.CreateCommand())
            {
                if(string.IsNullOrEmpty(schema))
                    command.CommandText = String.Format("SELECT TOP 0 * FROM [{0}]", table_name);
                else
                    command.CommandText = String.Format("SELECT TOP 0 * FROM [{0}].[{1}]", schema, table_name);
                command.CommandType = CommandType.Text;

                command.Transaction = this.tran;

                SqlDataReader reader = command.ExecuteReader(CommandBehavior.SchemaOnly);

                while (reader.Read())
                {

                }
            }
            return table;
        }

        public string GetConnectionString()
        {
            throw new NotImplementedException();
        }

        public TableInfo GetTableInfo(string ConnectionString, string schema, string table_name)
        {
            throw new NotImplementedException();
        }
















        #endregion
    }
}
