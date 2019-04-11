using LV.Db.Common;
using Npgsql;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace LV.Db.PostgreSQL
{
    public class Database : IDb
    {
        private NpgsqlConnection cnn;
        private string schema;
        
        public Database()
        {
            this.ConnectionString = Globals.ConnectionString;
            this.cnn = new NpgsqlConnection(this.ConnectionString);
        }
        public void SetConnectionString(string connectionString)
        {
            this.ConnectionString = connectionString;
            this.cnn = new NpgsqlConnection(this.ConnectionString);
        }
        public string ConnectionString { get; private set; }
        public void SetSchema(string schema)
        {
            this.schema = schema;
        }
        public string GetSchema()
        {
            return this.schema;
        }
        public void CloseConnection()
        {
            this.cnn.Close();
        }

        /// <summary>
        /// Insert into table, DML statement will be excuted here 
        /// </summary>       
        /// <param name="sql">This is sql query to insert data in to table in database</param>
        /// <returns></returns>
        public InsertDataResult<T> InsertIntoDatabase<T>(string SQL, params object[] pars)
        {
            InsertDataResult<T> insertDataResult = new InsertDataResult<T>();
            object result = null;
            try
            {
                if (cnn.State != ConnectionState.Open)
                {
                    cnn.Open();
                }
                NpgsqlCommand command = new NpgsqlCommand(SQL, cnn);
                var paramIndex = 0;
                foreach (var p in pars)
                {
                    var paramName = string.Format(":p{0}", paramIndex);
                    command.CommandText = command.CommandText.Replace("{" + paramIndex.ToString() + "}", paramName);
                    var sqlParam = new NpgsqlParameter();
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
                result = command.ExecuteScalar();    
                //Insert successfully
                if(result != null)
                {
                    insertDataResult.Error = new ErrorAction()
                    {
                        ErrorType = DataActionErrorTypeEnum.None                        
                    };
                    insertDataResult.RecordId = result;
                }
                if (cnn.State != ConnectionState.Closed)
                {
                    cnn.Close();
                }
            }
            catch (NpgsqlException ex)
            {
                Utils utils = new Utils();
                insertDataResult = utils.ProcessException<T>(ex, insertDataResult);
            }
            catch (Exception ex)
            {
                throw ex;
            }            
                   
            return insertDataResult;
        }

        public DataTable ExecToDataTable(string SQL, params object[] pars)
        {
            if (this.cnn.State != System.Data.ConnectionState.Open)
            {
                this.cnn.Open();
            }
            NpgsqlCommand command = new NpgsqlCommand(SQL,this.cnn);
            var paramIndex = 0;
            foreach(var p in pars)
            {
                var paramName = string.Format(":p{0}", paramIndex);
                command.CommandText = command.CommandText.Replace("{" + paramIndex.ToString() + "}", paramName);
                var sqlParam = new NpgsqlParameter();
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
            var adp = new NpgsqlDataAdapter(command);
            var tbl = new DataTable();
            adp.Fill(tbl);
            return tbl;
        }

        public IList<T> ExecToList<T>(string SQL, params object[] pars)
        {
            var isAnonymousType = Utils.IsAnonymousType(typeof(T));
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
                if (isAnonymousType)
                {
                    var args = new List<object>();
                    foreach (var col in cols)
                    {
                        if (r[col.name] != DBNull.Value)
                        {
                            args.Add(r[col.name]);
                            //col.property.SetValue(item, r[col.name]);
                        }
                        else
                        {
                            args.Add(null);
                        }

                    }
                    var item = (T)Activator.CreateInstance(typeof(T), args.ToArray());
                    ret.Add(item);
                }
                else
                {
                    var item = (T)Activator.CreateInstance(typeof(T));
                    foreach (var col in cols)
                    {
                        if (r[col.name] != DBNull.Value)
                        {
                            col.property.SetValue(item, r[col.name]);
                        }
                        else
                        {
                            col.property.SetValue(item, null);
                        }

                    }
                    ret.Add(item);
                }
                
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
            var type = typeof(T);
            var pro = type.GetProperties().ToList();
            var fieldList = pro.Select(p => new {
                name=p.Name,
                value=p.GetValue(data),
                paramIndex=pro.IndexOf(p)
            }).ToList();
            var fields = string.Join(",", fieldList.Select(p=> Globals.Compiler.GetQName(p.name)).ToArray());
            var strPro = string.Join(",", fieldList.Select(p => ":p"+p.paramIndex).ToArray());
            var tblName = Globals.Compiler.GetQName(tableName);            
            string sql = string.Format("INSERT INTO {0}({1}) VALUES({2}) RETURNING *", tblName, fields, strPro);
            InsertDataResult<T> insertDataResult = new InsertDataResult<T>();
            insertDataResult = InsertIntoDatabase<T>(sql, fieldList.Select(p=>p.value).ToArray());
            return insertDataResult;            
        }

        public DataActionResult UpdateData<T>(string tableName, Expression<Func<T, bool>> Where, object data)
        {
            throw new NotImplementedException();
        }

        public DeleteDataResult<T> DeleteData<T>(string tableName, string strWhere, List<object> inputParams)
        {
            DeleteDataResult<T> deleteDataResult = new DeleteDataResult<T>();
            var tblName = Globals.Compiler.GetQName(tableName);
            for (int i = 0; i < inputParams.Count; i++)
            {
                strWhere = strWhere.Replace("{" + i + "}", ":p" + i.ToString());
            }
            string sql = string.Format("DELETE FROM {0} WHERE {1}", tblName, strWhere);
            
            deleteDataResult = DeleteDataFromTable<T>(sql, inputParams.ToArray());
            return deleteDataResult;
        }

        /// <summary>
        /// Delete from certain table, DML statement will be excuted here 
        /// </summary>       
        /// <param name="sql">This is sql query to delete data from certain table in database</param>
        /// <returns></returns>
        public DeleteDataResult<T> DeleteDataFromTable<T>(string SQL, params object[] pars)
        {
            DeleteDataResult<T> deleteDataResult = new DeleteDataResult<T>();
            object result = null;
            try
            {
                if (cnn.State != ConnectionState.Open)
                {
                    cnn.Open();
                }
                NpgsqlCommand command = new NpgsqlCommand(SQL, cnn);
                var paramIndex = 0;
                foreach (var p in pars)
                {
                    var paramName = string.Format(":p{0}", paramIndex);
                    command.CommandText = command.CommandText.Replace("{" + paramIndex.ToString() + "}", paramName);
                    var sqlParam = new NpgsqlParameter();
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
                result = command.ExecuteNonQuery();
                //Delete successfully
                if (result != null)
                {
                    deleteDataResult.Error = new ErrorAction()
                    {
                        ErrorType = DataActionErrorTypeEnum.None
                    };
                }
                if (cnn.State != ConnectionState.Closed)
                {
                    cnn.Close();
                }
            }
            catch (NpgsqlException ex)
            {
                Utils utils = new Utils();
                deleteDataResult = utils.ProcessException<T>(ex, deleteDataResult);
            }
            catch (Exception ex)
            {
                throw ex;
            }

            return deleteDataResult;
        }

        /// <summary>
        /// Update data to db
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="tableName"></param>
        /// <param name="Where"></param>
        /// <param name="data"></param>
        /// <returns></returns>
        public UpdateDataResult<T> UpdateData<T>(string TableName, string StrWhere, List<object> InputParams, T data)
        {
            var type = typeof(T);
            var pro = type.GetProperties().ToList();
            var fieldList = pro.Select(p => new {
                name = p.Name,
                value = p.GetValue(data),
                paramIndex = pro.IndexOf(p)
            }).ToList();
            
            var fields = string.Join(",", fieldList.Select(p => Globals.Compiler.GetQName(p.name)).ToArray());
            var strPro = string.Join(",", fieldList.Select(p => @"""" + p.name + @"""" + "=" + ":p" + p.paramIndex).ToArray());
            var tblName = Globals.Compiler.GetQName(TableName);
            for (int i = 0; i < InputParams.Count; i++)
            {
                StrWhere = StrWhere.Replace("{" + i + "}", ":p" + (fieldList.Count + i).ToString());
            }
            string sql = string.Format("UPDATE {0} SET {1} WHERE {2}", tblName, strPro, StrWhere);            
            UpdateDataResult<T> updateDataResult = new UpdateDataResult<T>();
            // array of fields in table
            var arrField = fieldList.Select(p => p.value).ToArray();
            // array of fields in WHERE clause
            var arrInputParam = InputParams.ToArray();
            // arr total field = array of fields in table + array of fields in WHERE clause
            var arrTotalField = arrField.Concat(arrInputParam).ToArray();

            updateDataResult = UpdateDatabase<T>(sql, arrTotalField);
            return updateDataResult;
        }

        /// <summary>
        /// Update into table, DML statement will be excuted here 
        /// </summary>       
        /// <param name="sql">This is sql query to update data in to table in database</param>
        /// <returns></returns>
        public UpdateDataResult<T> UpdateDatabase<T>(string SQL, params object[] pars)
        {
            UpdateDataResult<T> updateDataResult = new UpdateDataResult<T>();
            object result = null;
            try
            {
                if (cnn.State != ConnectionState.Open)
                {
                    cnn.Open();
                }
                NpgsqlCommand command = new NpgsqlCommand(SQL, cnn);
                var paramIndex = 0;
                foreach (var p in pars)
                {
                    var paramName = string.Format(":p{0}", paramIndex);
                    command.CommandText = command.CommandText.Replace("{" + paramIndex.ToString() + "}", paramName);
                    var sqlParam = new NpgsqlParameter();
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
                result = command.ExecuteNonQuery();
                //Update successfully
                if (result != null)
                {
                    updateDataResult.Error = new ErrorAction()
                    {
                        ErrorType = DataActionErrorTypeEnum.None
                    };
                }
                if (cnn.State != ConnectionState.Closed)
                {
                    cnn.Close();
                }
            }
            catch (NpgsqlException ex)
            {
                Utils utils = new Utils();
                updateDataResult = utils.ProcessException<T>(ex, updateDataResult);
            }
            catch (Exception ex)
            {
                throw ex;
            }

            return updateDataResult;
        }

        public void BeginTran(DbIsolationLevel level)
        {
            throw new NotImplementedException();
        }

        public void Commit()
        {
            throw new NotImplementedException();
        }

        public void RollBack()
        {
            throw new NotImplementedException();
        }



        public string GetConnectionString()
        {
            return this.ConnectionString;
        }

        public TableInfo GetTableInfo(string ConnectionString, string schema, string table_name)
        {
            var ret = new TableInfo();

            var sql = @"SELECT column_name, column_default,is_nullable,data_type,character_maximum_length
                            FROM information_schema.columns
                            WHERE table_schema = :schema
                            AND table_name   = :table";
            var cnn = new Npgsql.NpgsqlConnection(ConnectionString);
            var cmd = new Npgsql.NpgsqlCommand("select * from " + Globals.Compiler.GetQName(schema, table_name), cnn);
            //cmd.Parameters.Add(new NpgsqlParameter()
            //{
            //    ParameterName="schema",
            //    Value=schema
            //});
            //cmd.Parameters.Add(new NpgsqlParameter()
            //{
            //    ParameterName = "table",
            //    Value = table_name
            //});
            DataTable tbl = new DataTable();
            //var adp = new Npgsql.NpgsqlDataAdapter(cmd);


            try
            {
                cnn.Open();
                tbl = cmd.ExecuteReader(CommandBehavior.KeyInfo | CommandBehavior.SchemaOnly).GetSchemaTable();

                //adp.Fill(tbl);
            }
            catch (Exception ex)
            {
                cnn.Close();
                throw (ex);
            }
            finally
            {
                cnn.Close();
            }
            ret.Columns = tbl.Rows.Cast<DataRow>().Select(p => new ColumInfo()
            {
                Name = p["ColumnName"].ToString(),
                IsUnique = (p["IsUnique"] == DBNull.Value) ? false : (bool)p["IsUnique"],
                IsAutoIncrement = (p["IsAutoIncrement"] == DBNull.Value) ? false : (bool)p["IsAutoIncrement"],
                IsKey = (p["IsKey"] == DBNull.Value) ? false : (bool)p["IsKey"],
                AllowDBNull = (p["AllowDBNull"] == DBNull.Value) ? false : (bool)p["AllowDBNull"],
                IsReadOnly = (p["IsReadOnly"] == DBNull.Value) ? false : (bool)p["IsReadOnly"],
                IsExpression = (p["IsReadOnly"] == DBNull.Value) ? false : (bool)p["IsReadOnly"],
                IsIdentity = (p["IsIdentity"] == DBNull.Value) ? false : (bool)p["IsIdentity"],
                DataType = p["DataType"],
                ColumnSize = (int)p["ColumnSize"]
                //DefaultValue = p["column_default"].ToString(),
                //IsAuto = p["column_default"].ToString().Split("(")[0] == "nextval",
                //AutoConstraint = ((p["column_default"].ToString().Split("(")[0] == "nextval") ? p["column_default"].ToString().Split("(")[1].Split("::")[0] : "")
            }).ToList();

            return ret;
        }

        public DataActionResult ExecCommand(string sql, bool ReturnError, params object[] Params)
        {
            var ret = new DataActionResult()
            {
                Data = Params
            };
            if (this.cnn.State != ConnectionState.Open)
            {
                this.cnn.Open();
            }
            var cmd = this.cnn.CreateCommand();
            var adp = new NpgsqlDataAdapter(cmd);
            cmd.CommandText = sql;
            var paramIndex = 0;
            foreach (var p in Params)
            {
                cmd.CommandText = cmd.CommandText.Replace("{" + paramIndex.ToString() + "}", ":p" + paramIndex.ToString());
                cmd.Parameters.Add(new NpgsqlParameter()
                {
                    ParameterName = ":p" + paramIndex.ToString(),
                    Value = ((p == null) ? DBNull.Value : p)
                });
                paramIndex++;
            }
            try
            {
                var tbl = new DataTable();
                adp.Fill(tbl);
                if (tbl.Rows.Count > 0)
                {
                    ret.NewID = new Hashtable();
                    foreach (DataColumn col in tbl.Columns)
                    {
                        ret.NewID[col.ColumnName] = tbl.Rows[0][col];
                    }
                }
            }
            catch (Npgsql.PostgresException ex)
            {
                if (ex.Code == "23505")
                {
                    var err = new ErrorAction()
                    {
                        ErrorType = DataActionErrorTypeEnum.DuplicateData,
                        Fields = ex.Detail.Split("(")[1].Split(")")[0].Split(",")
                    };
                    if (ReturnError)
                    {
                        ret.Error = err;
                        return ret;
                    }
                    else
                    {
                        throw (new DataActionError("Duplicate data at fields:" + string.Join(",", err.Fields))
                        {
                            Detail = err
                        });
                    }

                }
                else
                {
                    throw (ex);
                }
            }
            catch (Exception ex)
            {
                throw (ex);
            }
            return ret;
        }

        public DataActionResult UpdateData(string SqlCommandText, string Schema, string TableName, TableInfo tblInfo, List<object> InputParams)
        {
            try
            {
                var ret = this.ExecCommand(SqlCommandText, false, InputParams.ToArray());
                return ret;
            }
            catch(Exception ex)
            {
                throw (ex);
            }
            
            
        }

        public DataActionResult InsertData(string SqlCommandText, string Schema, string TableName, TableInfo tblInfo, List<object> InputParams)
        {
            throw new NotImplementedException();
        }

        public DataActionResult DeleteData(string SqlCommandText, string Schema, string TableName, TableInfo tblInfo, List<object> InputParams)
        {
            throw new NotImplementedException();
        }










        #endregion
    }
}
