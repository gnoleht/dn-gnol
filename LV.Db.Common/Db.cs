using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;

namespace LV.Db.Common
{
    public class Db : IDisposable,IDbContext
    {
        private IDb db;

        public Hashtable CacheTableInfo { get; private set; }
        public string Value { get; private set; }

        public Db()
        {
            if (Globals.ProviderType == null)
            {
                if (Globals.DbType==DbTypes.None)
                {

                }
                var location = System.Reflection.Assembly.GetEntryAssembly().Location;
                var directory = System.IO.Path.GetDirectoryName(location);
                Globals.ProviderAssembly = utils.LoadFile(directory, string.Format("LV.Db.{0}.dll", Globals.DbType));
                Globals.ProviderType = Globals.ProviderAssembly.DefinedTypes.First(p => p.Name == "Database");
                Globals.Compiler = Globals.ProviderAssembly.CreateInstance(Globals.ProviderAssembly.DefinedTypes.First(p => p.Name == "Compiler").FullName) as ICompiler;

            }
            this.db = Globals.ProviderAssembly.CreateInstance(Globals.ProviderType.FullName) as IDb;
        }

       

       

        public void BeginTran(DbIsolationLevel level)
        {
            this.db.BeginTran(level);
        }

        public void Commit()
        {
            this.db.Commit();
        }

        public void RollBack()
        {
            this.db.RollBack();
        }

        public Db WithSchema(string schema)
        {
            this.db.SetSchema(schema);
            return this;
        }

        public QuerySet<T> Query<T>(string schema)
        {
            var ret = QueryBuilder.FromEntity<T>();
            ret.Schema = schema;
            ret.dbContext = this.db;
            return ret;
        }

        public QuerySet<T> Query<T>()
        {
            var ret = QueryBuilder.FromEntity<T>();
            ret.dbContext = this.db;
            string schema = this.db.GetSchema();
            if (!string.IsNullOrEmpty(schema))
            {
                ret.Schema = schema;
            }
            return ret;
        }

        public Db(string ConnectionString)
        {
            if (Globals.ProviderType == null)
            {
                if (Globals.DbType == DbTypes.None)
                {

                }
                var location = System.Reflection.Assembly.GetEntryAssembly().Location;
                var directory = System.IO.Path.GetDirectoryName(location);
                Globals.ProviderAssembly = utils.LoadFile(directory, string.Format("LV.Db.{0}.dll", Globals.DbType));
                Globals.ProviderType = Globals.ProviderAssembly.DefinedTypes.First(p => p.Name == "Database");
                Globals.Compiler = Globals.ProviderAssembly.CreateInstance(Globals.ProviderAssembly.DefinedTypes.First(p => p.Name == "Compiler").FullName) as ICompiler;

            }
            this.db = Globals.ProviderAssembly.CreateInstance(Globals.ProviderType.FullName) as IDb;
            this.db.SetConnectionString(ConnectionString);
        }
        /// <summary>
        /// Just a select statement from nothing such as: 'select 1+1 as Test'
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="Selector"></param>
        /// <returns></returns>
        public QuerySet<T> FromNothing<T>(Expression<Func<object,T>> Selector)
        {
            var ret = QueryBuilder.FromNothing<T>(Selector);
            ret.dbContext = this.db;
            return ret;
        }

        public IList<T> ExecToList<T>(string SQL, params object[] pars)
        {
            return this.db.ExecToList<T>(SQL, pars);
        }

        public DataTable ExecToDataTable(string SQL,params object[] pars)
        {
            return this.db.ExecToDataTable(SQL, pars);
        }

        public void Dispose()
        {
            this.db.CloseConnection();
        }

        #region The implementation of DML
        
       
        public UpdateDataResult<T> UpdateData<T>(string Schema,string TableName, Expression<Func<T, bool>> Where,object DataItem, bool ReturnError=false)
        {
            if (DataItem == null)
            {
                throw new Exception("It looks like you forgot set data parameter for update command");
            }
            var ret = new UpdateDataResult<T>();
            var properties = DataItem.GetType().GetProperties().ToList();
            var Params = properties.Select(p => new ParamInfo()
            {
                Name = p.Name,
                Value = p.GetValue(DataItem),
                Index = properties.IndexOf(p),
                DataType = p.PropertyType
            }).ToList();
            var tbl = this.GetTableInfo(this.db.GetConnectionString(), Schema, TableName);
            //Check require fields:
            var requiredFields = this.GetRequireFieldsForUpdate(tbl, Params);
            var invalidDataType = this.GetInvalidDataTypeFields(tbl, Params);
            if (requiredFields.Count() > 0)
            {
                var err = new ErrorAction()
                {
                    ErrorType = DataActionErrorTypeEnum.MissingFields,
                    Fields = requiredFields.Select(p => p.Name).ToArray()
                };
                if (ReturnError)
                {
                    ret.Error = err;
                    return ret;
                }
                else
                {
                    throw (new DataActionError("Missing fields:" + string.Join(",", err.Fields))
                    {
                        Detail = err
                    });
                }
            }
            if (invalidDataType.Count() > 0)
            {
                var err = new ErrorAction()
                {
                    ErrorType = DataActionErrorTypeEnum.InvalidDataType,
                    Fields = invalidDataType.Select(p => p.Name).ToArray()
                };
                ret.Error = err;
                if (ReturnError)
                {
                    ret.Error = err;
                    return ret;
                }
                else
                {
                    throw (new DataActionError("Invalid data type at fields:" + string.Join(",", err.Fields))
                    {
                        Detail = err
                    });
                }

            }

            var exceedFields = this.GetExceedFields(tbl, Params);
            if (exceedFields.Count > 0)
            {
                var err = new ErrorAction()
                {
                    ErrorType = DataActionErrorTypeEnum.ExceedSize,
                    Fields = exceedFields.Select(p => p.Name).ToArray()
                };
                ret.Error = err;

                if (ReturnError)
                {
                    ret.Error = err;
                    return ret;
                }
                else
                {
                    throw (new DataActionError("Exceed field len at fields:" + string.Join(",", err.Fields))
                    {
                        Detail = err
                    });
                }
            }

            var ex = Where.Body as BinaryExpression;
            var lstParams = new List<object>();
            lstParams.AddRange(Params.Select(p => p.Value));
            var strWhere = SelectorCompiler.Gobble<string>(Where.Body, new string[] { Schema }, new string[] { TableName }, Where.Parameters, null, -1, ref lstParams);
            var Sql = "Update " + Globals.Compiler.GetQName(Schema, TableName) +
                " set " + String.Join(",", Params.Select(p => Globals.Compiler.GetQName("", p.Name) + "={" + p.Index + "}").ToArray()) + " where " +
                strWhere;
            var retUpdate= this.db.UpdateData(Sql, Schema, TableName,tbl,lstParams);
            ret.Data = retUpdate.Data;
            ret.DataItem = retUpdate.DataItem;
            ret.EffectedRowCount = retUpdate.EffectedRowCount;
            ret.Error = retUpdate.Error;
            ret.NewID = retUpdate.NewID;
            return ret;

        }

        

        public DeleteDataResult<T> DeleteData<T>(string Schema, string TableName, Expression<Func<T, bool>> Where,bool ReturnError=false)
        {
            //throw new NotImplementedException();
            var ex = Where.Body as BinaryExpression;
            var lstParams = new List<object>();
            string strWhere = SelectorCompiler.Gobble<string>(Where.Body, new string[] { Schema }, new string[] { TableName }, Where.Parameters, null, -1, ref lstParams);
            var sql = "delete from " + Globals.Compiler.GetQName(Schema, TableName) + " Where " + strWhere;
            var ret= this.db.ExecCommand(sql, ReturnError, lstParams.ToArray());
            return null;
        }
        private List<ColumInfo> GetRequireFields(TableInfo tbl, IEnumerable<ParamInfo> Params)
        {
            return tbl.Columns.Where(p => (!p.AllowDBNull) && ((!p.IsAutoIncrement) &&
                                                      (!p.IsExpression) && (!p.IsIdentity) && (!p.IsReadOnly)))
                                                      .Where(p => !Params.Any(x => x.Name == p.Name & x.Value != null))
                                                    .ToList();
        }
        private List<ColumInfo> GetRequireFieldsForUpdate(TableInfo tbl, List<ParamInfo> Params)
        {
            return tbl.Columns.Where(p => (!p.AllowDBNull) && ((!p.IsAutoIncrement) &&
                                                      (!p.IsExpression) && (!p.IsIdentity) && (!p.IsReadOnly)))
                                                      .Join(Params,p=>p.Name,q=>q.Name,(p,q)=>new {
                                                          p.Name,
                                                          q.Value
                                                      })
                                                      .Where(p =>p.Value== null).Select(p=>new ColumInfo()
                                                      {
                                                          Name=p.Name
                                                      })
                                                    .ToList();
        }
        private List<ExceedFieldInfo> GetExceedFields(TableInfo tbl, IEnumerable<ParamInfo> Params)
        {
            return tbl.Columns.Where(p => (Type)p.DataType == typeof(string)).Join(Params.Where(x => x.DataType == typeof(string)), p => p.Name, q => q.Name, (p, q) => new ExceedFieldInfo()
            {
                Name = p.Name,
                ColumnSize = p.ColumnSize,
                Value = Value = q.Value.ToString()
            }).Where(p => p.Value.Count() > p.ColumnSize).ToList();
        }
        private List<ParamInfo> GetInvalidDataTypeFields(TableInfo tbl, IEnumerable<ParamInfo> Params)
        {
            return tbl.Columns.Join(Params, p => p.Name, q => q.Name, (p, q) => new ParamInfo()
            {
                type1 = p.DataType as Type,
                type2 = q.DataType,
                Name = p.Name
            }).Where(p => p.type1 != p.type2).ToList();
        }
       

        public InsertDataResult<T> InsertData<T>(string Schema, string TableName, object DataItem, bool ReturnError = false)
        {
            if (DataItem == null)
            {
                throw new Exception("It looks like yop forgot set data ('Data' parameter is null)");
            }
            var ret = new InsertDataResult<T>();
            var properties = DataItem.GetType().GetProperties().ToList();
            var Params = properties.Select(p => new ParamInfo()
            {
                Name = p.Name,
                Value = p.GetValue(DataItem),
                Index = properties.IndexOf(p),
                DataType=p.PropertyType
            });
            var tbl = this.GetTableInfo(this.db.GetConnectionString(), Schema, TableName);
            //Check require fields:
            var requiredFields = this.GetRequireFields(tbl, Params);
            var invalidDataType = this.GetInvalidDataTypeFields(tbl, Params);
            if (requiredFields.Count() > 0)
            {
                var err=new ErrorAction()
                {
                    ErrorType = DataActionErrorTypeEnum.MissingFields,
                    Fields = requiredFields.Select(p => p.Name).ToArray()
                };
                if (ReturnError)
                {
                    ret.Error = err;
                    return ret;
                }
                else
                {
                    throw (new DataActionError("Missing fields:"+string.Join(",",err.Fields)) {
                        Detail=err
                    });
                }
            }
            if (invalidDataType.Count() > 0)
            {
                var err= new ErrorAction()
                {
                    ErrorType = DataActionErrorTypeEnum.InvalidDataType,
                    Fields = invalidDataType.Select(p => p.Name).ToArray()
                };
                ret.Error = err;
                if (ReturnError)
                {
                    ret.Error = err;
                    return ret;
                }
                else
                {
                    throw (new DataActionError("Invalid data type at fields:" + string.Join(",", err.Fields))
                    {
                        Detail = err
                    });
                }
                
            }
           
            var exceedFields = this.GetExceedFields(tbl, Params);
            if (exceedFields.Count > 0)
            {
                var err= new ErrorAction()
                {
                    ErrorType = DataActionErrorTypeEnum.ExceedSize,
                    Fields = exceedFields.Select(p => p.Name).ToArray()
                };
                ret.Error = err;

                if (ReturnError)
                {
                    ret.Error = err;
                    return ret;
                }
                else
                {
                    throw (new DataActionError("Exceed field len at fields:" + string.Join(",", err.Fields))
                    {
                        Detail = err
                    });
                }
            }
            var sqlGetIdentitySQL = Globals.Compiler.CreateSQLGetIdentity(Schema, TableName, tbl);
            var sql = "insert into " + Globals.Compiler.GetQName(Schema, TableName) +
                "(" +
                string.Join(",", Params.Select(p => Globals.Compiler.GetQName("", p.Name)).ToArray()) + ") "+
                "values("+ string.Join(",",Params.Select(p=>"{"+p.Index+"}").ToArray())+")";
            sql = sql + ";" + sqlGetIdentitySQL;
            var retInsert = this.db.ExecCommand(sql, ReturnError, Params.Select(p => p.Value).ToArray());
            retInsert.DataItem = DataItem;
            ret.Data = retInsert.Data;
            ret.DataItem = retInsert.DataItem;
            ret.EffectedRowCount = retInsert.EffectedRowCount;
            ret.Error = retInsert.Error;
            ret.NewID = retInsert.NewID;
            return ret;
        }

        

        public TableInfo GetTableInfo(string ConnectionString, string Schema, string TableName)
        {
            var hashkey = string.Format("cnn={0};schem={1};table={2}", ConnectionString, Schema, TableName);
            if (Globals.TableInfo == null)
            {
                Globals.TableInfo = new Hashtable();
            }
            if (Globals.TableInfo[hashkey] != null)
            {
                return (TableInfo)Globals.TableInfo[hashkey];
            }
            lock (Globals.TableInfo)
            {
                var ret = this.db.GetTableInfo(ConnectionString, Schema, TableName);
                Globals.TableInfo[hashkey] = ret;
            }
            return (TableInfo)Globals.TableInfo[hashkey];


        }

        





        #endregion
    }
}