using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace LV.Db.Common
{
    /// <summary>
    /// The library has been built for accessing across different types of RDBMS engine  with minimum effort such as: MS SQL server, PostgreSQL, MySQL and Oracle.
    /// The crucial function of the library is translated from Dot Net Lambda Expression into SQL which could be directly used in RDBMS engine
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public sealed class QuerySet<T>
    {
        /// <summary>
        /// The table name will be map with Database
        /// </summary>
        internal string table_name { get; set; }

        

        public QuerySet<T2> Join<T1,T2>(QuerySet<T1> qr, Expression<Func<T, T1, bool>> JoinClause, Expression<Func<T,T1,T2>> Selector)
        {
            return QueryBuilder.Join(this, qr, JoinClause, Selector);
        }
        public QuerySet<T2> LeftJoin<T1, T2>(QuerySet<T1> qr, Expression<Func<T, T1, bool>> JoinClause, Expression<Func<T, T1, T2>> Selector)
        {
            return QueryBuilder.LeftJoin(this, qr, JoinClause, Selector);
        }
        public QuerySet<T2> RightJoin<T1, T2>(QuerySet<T1> qr, Expression<Func<T, T1, bool>> JoinClause, Expression<Func<T, T1, T2>> Selector)
        {
            return QueryBuilder.RightJoin(this, qr, JoinClause, Selector);
        }

        /// <summary>
        /// List of selected fields. Each item in this list directly can be use in database
        /// Example:
        /// For PostgreSQL:
        ///             --------------------------------------------------------------------------------
        ///             |             Value                                                             |
        ///             --------------------------------------------------------------------------------
        ///             |"employees"."firstname"                                                        |
        ///             ---------------------------------------------------------------------------------
        ///             |"employees"."firstname"+' '+"employees"."lastname" as "fullname"               |
        ///             --------------------------------------------------------------------------------
        /// For MS SQL server:
        ///             --------------------------------------------------------------------------------
        ///             |             Value                                                             |
        ///             --------------------------------------------------------------------------------
        ///             |[employees].[firstname]                                                        |
        ///             ---------------------------------------------------------------------------------
        ///             |[employees].[firstname]+' '+[employees].[lastname] as [fullname]               |
        ///             --------------------------------------------------------------------------------             
        ///             
        /// For MySQl:
        ///             --------------------------------------------------------------------------------
        ///             |             Value                                                             |
        ///             --------------------------------------------------------------------------------
        ///             |`employees`.`firstname`                                                        |
        ///             ---------------------------------------------------------------------------------
        ///             |`employees`.`firstname`+' '+`employees`.`lastname` as `fullname`               |
        ///             --------------------------------------------------------------------------------      
        /// </summary>
        internal List<string> fields { get; set; }
        /// <summary>
        /// Sometime the query parser generate parameter instead of constant
        /// Example:
        /// p=>new {p.firtsname+" "+p.lastname} 
        /// will generate a struct looks like:
        /// 'select firstname+{0}+lastname' with {0} is position of parameter in this list
        /// </summary>
        internal List<object> Params { get; set; }
        public string Schema { get;  set; }

        /// <summary>
        /// after compiler pasre QuerySet.Filter the expression will be stored here
        /// </summary>
        internal string where;
        
        //internal string table_alias;
        internal string datasource;
        internal int sequenceJoin;
        internal List<SortingInfo> sortList =new List<SortingInfo>() ;
        internal List<string> groupbyList=new List<string>();
        
        internal int joinTimes;
        internal ReadOnlyCollection<ParameterExpression> parametersExpr;
        internal string[] table_names;
        internal List<ParameterExpression> fieldParams;
        internal bool isFromNothing;
        internal IDb dbContext;
        private int selectTop;
        private int skipNumber;

        /// <summary>
        /// make new instance QuerySet with table name
        /// </summary>
        /// <param name="table_name"></param>
        public QuerySet(string table_name)
        {
            this.table_name = table_name;
            this.fields = new List<string>();
            this.Params = new List<object>();
            
        }
        /// <summary>
        /// Set datasource, the data source maybe a sub query
        /// </summary>
        /// <param name="sql"></param>
        internal void SetDataSource(string sql)
        {
            this.datasource = sql;
        }

        public QuerySet()
        {
            this.fields = new List<string>();
            this.Params = new List<object>();
        }
        internal bool IsSubQuery()
        {
            var f = this.fields.Count(p => p.Substring(p.Length - 2,2) == ".*");

            return (this.fields.Count > 0 && f!=this.fields.Count &&(!(this.fields.Count==1 && this.fields[0]=="*"))) 
                || (!string.IsNullOrEmpty(this.where)) 
                || this.groupbyList.Count > 0
                || this.selectTop>0
                ||this.skipNumber>0;
        }
        public QuerySet(string schema, string tableName)
        {
            this.table_name = tableName;
            this.fields = new List<string>();
            this.Params = new List<object>();
            this.Schema = schema;
        }

        /// <summary>
        /// Where clause
        /// </summary>
        /// <param name="expr"></param>
        /// <returns></returns>
        public QuerySet<T> Filter(Expression<Func<T, bool>> expr)
        {
            var ret = this.Clone<T>();

            var ex = expr.Body as BinaryExpression;
            if (this.sequenceJoin > 0)
            {
                ret.sequenceJoin++;
                ret.SetDataSource("("+this.ToSQLString()+") as "+Globals.Compiler.GetQName("","t"+ret.sequenceJoin.ToString()));
                ret.fields.Clear();
                ret.SetTableName("t" + ret.sequenceJoin.ToString());
            }
           
            var lstParams = ret.Params;
            ret.where = SelectorCompiler.Gobble<string>(expr.Body,new string[] {ret.Schema }, new string[] { ret.GetTableName() }, expr.Parameters, null, -1, ref lstParams);
            ret.Params = lstParams;
            
            return ret;
        }
        /// <summary>
        /// Get list of params
        /// </summary>
        /// <returns></returns>
        public List<object> GetParams()
        {
            return this.Params;
        }
        /// <summary>
        /// Select clause
        /// </summary>
        /// <typeparam name="T2"></typeparam>
        /// <param name="selector">Example:p=>new {p.Code,p.Name,FullName=p.Code+" "+p.FirstName}</param>
        /// <returns></returns>
        public QuerySet<T2> Select<T2>(Expression<Func<T,T2>> selector)
        {
            var ret = this.Clone<T2>();
            var Params = this.Params;
            var tbls = this.table_names;
            if (!string.IsNullOrEmpty(this.table_name))
            {
                tbls = new string[] { this.table_name };
            }
            var selectFields = SelectorCompiler.GetFields(selector.Body,new string[] { ret.Schema}, tbls, selector.Parameters, ref Params);
            ret.Params = Params;
            ret.fields.Clear();
            ret.fields.AddRange(selectFields);
            return ret;
        }
        /// <summary>
        /// Create new instance from this instance
        /// </summary>
        /// <typeparam name="T2"></typeparam>
        /// <returns></returns>
        private QuerySet<T2> Clone<T2>()
        {
            var ret = new QuerySet<T2>(this.GetTableName());
            ret.dbContext = this.dbContext;
            ret.Params.AddRange(this.Params);
            ret.datasource = this.datasource;
            ret.table_name = this.table_name;
            ret.sequenceJoin = this.sequenceJoin;
            ret.fields.AddRange(this.fields);
            ret.Schema = this.Schema;
           
            ret.where = this.where;
            ret.sortList.AddRange(this.sortList);
            ret.groupbyList.AddRange(this.groupbyList);
            ret.table_names = new string[] { };
            if (this.table_names != null)
            {
                foreach (var x in this.table_names)
                {
                    ret.table_names.Append(x);
                }
            }
            
            ret.parametersExpr = this.parametersExpr;
            ret.joinTimes = this.joinTimes;
            if (this.fieldParams != null)
            {
                ret.fieldParams = new List<ParameterExpression>();
                ret.fieldParams.AddRange(this.fieldParams);
            }
            
            return ret;
        }
        /// <summary>
        /// Create real SQL statement with input parameters
        /// </summary>
        /// <returns></returns>
        public string ToSQLString()
        {
            if (this.datasource==this.table_name)
            {
                var _select = Globals.Compiler.GetQName(this.Schema, this.table_name) + ".*";
                var tableName = Globals.Compiler.GetQName(this.Schema, this.table_name);
                if (this.fields.Count > 0)
                {
                    _select = string.Join(",", this.fields.ToArray());
                }
                if (this.isFromNothing)
                {
                    return "select " + _select;
                }
                var sql = "select " + _select + " from " + tableName;
                if (!string.IsNullOrEmpty(this.where))
                {
                    sql = sql + " where " + this.where;
                }
                if (this.sortList.Count > 0)
                {
                    var sorts = new List<string>();
                    foreach (var f in this.sortList)
                    {
                        sorts.Add(Globals.Compiler.GetFieldName(this.Schema, tableName, f.FieldName) + " " + f.SortType);
                    }
                    sql = sql + " order by " + string.Join(",", sorts.ToArray());
                }
                return sql;
            }
            else
            {
                var _select = "*";
                if (!string.IsNullOrEmpty(this.table_name))
                {
                    _select = Globals.Compiler.GetQName(this.Schema, this.table_name) + ".*";
                }
                    
                var tableName = Globals.Compiler.GetQName(this.Schema, this.table_name);
                if (this.fields.Count > 0)
                {
                    _select = string.Join(",", this.fields.ToArray());
                }
                var sql = "select " + _select + " from "+((this.datasource!=null)? "" + this.datasource + "":  tableName);
                if (!string.IsNullOrEmpty(this.where))
                {
                    sql = sql + " where " + this.where;
                }
                var sorts = new List<string>();
                if (this.sortList.Count > 0)
                {
                    foreach(var f in this.sortList)
                    {
                        sorts.Add(Globals.Compiler.GetFieldName(this.Schema, tableName, f.FieldName) + " " + f.SortType);
                    }
                    sql = sql + " order by " + string.Join(",", sorts.ToArray());
                }
                if (this.selectTop > 0)
                {
                    sql = Globals.Compiler.MakeLimitRow(sql, this.sortList,this.fields, this.selectTop);
                }
                return sql;
            }
        }
        
        /// <summary>
        /// Make SQL statement for debugger
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            var sql = this.ToSQLString();
            var index = 0;
            foreach(var p in this.Params)
            {
                
                if (p == null)
                {
                    sql = sql.Replace("{" + index.ToString() + "}", "NULL");
                }
                else if(p is string)
                {
                    sql = sql.Replace("{" + index.ToString() + "}", "'" + p.ToString().Replace("'", "''") + "'");
                }
                else if(p is DateTime)
                {
                    sql = sql.Replace("{" + index.ToString() + "}", "'" + ((DateTime)p).ToString("yyyy-MM-dd hh:mm:ss") + "'");
                }
                else
                {
                    sql = sql.Replace("{" + index.ToString() + "}", "" + p.ToString()+ "");
                }
                index++;
            }
            return sql;
        }
        /// <summary>
        /// Get table name
        /// </summary>
        /// <returns></returns>
        public string GetTableName()
        {
            return this.table_name;
        }
        internal void SetTableName(string tableName)
        {
            this.table_name = tableName;
        }
        /// <summary>
        /// Get list of fields
        /// </summary>
        /// <returns></returns>
        public List<string> GetFields()
        {
            return this.fields;
        }

        public QuerySet<T> SortAsc(Expression<Func<T, object>> Expr)
        {
            var Body = ((LambdaExpression)Expr).Body;
            List<string> Fields = SortCompiler.GetFields(Body);
            foreach(var f in Fields)
            {
                this.sortList.Add(new SortingInfo()
                {
                    FieldName=f,
                    SortType="asc"
                });
            }

            return this;
        }

        public QuerySet<T> SortDesc(Expression<Func<T, object>> Expr)
        {
            var Body = ((LambdaExpression)Expr).Body;
            List<string> Fields = SortCompiler.GetFields(Body);
            foreach (var f in Fields)
            {
                this.sortList.Add(new SortingInfo()
                {
                    FieldName = f,
                    SortType = "desc"
                });
            }

            return this;
        }
        public QuerySet<T> Skip(int num)
        {
            var ret = this.Clone<T>();
            ret.skipNumber = num;
            return ret;
        }
        public QuerySet<T> SortAsc(params Expression<Func<T, object>>[] Expr)
        {
            foreach(var x in Expr)
            {
                if (x.Body is MemberExpression)
                {
                    var pList = new List<object>();
                    var mx = x.Body as MemberExpression;
                    var Fields = SelectorCompiler.Gobble<string>(x.Body, new string[] {this.Schema }, new string[] { this.table_name }, x.Parameters.ToList(), new MemberInfo[] { mx.Member }, 0, ref pList);

                }
                
                    //SortCompiler.GetFields(x);
                //foreach (var f in Fields)
                //{
                //    this.sortList.Add(new SortingInfo()
                //    {
                //        FieldName = f,
                //        SortType = "asc"
                //    });
                //}
            }
            return this;
        }
        public QuerySet<T> SortDesc(params Expression<Func<T, object>>[] Expr)
        {
            foreach (var x in Expr)
            {
                List<string> Fields = SortCompiler.GetFields(x);
                foreach (var f in Fields)
                {
                    this.sortList.Add(new SortingInfo()
                    {
                        FieldName = f,
                        SortType = "desc"
                    });
                }
            }
            return this;
        }

        public QuerySet<T> First()
        {
            var ret = this.Clone<T>();
            ret.selectTop = 1;
            return ret;
        }
        
        public QuerySet<T> GroupBy(Expression<Func<T, object>> GroupFields)
        {
            var ret = this.Clone<T>();
            var fieldBody = ((LambdaExpression)GroupFields).Body;
            var ParamList = ret.Params;
            var x = GroupFields.Parameters[0].Type == typeof(T);
            var tableNames = new List<string>();
            
            if (this.table_names != null)
            {
                tableNames.AddRange(this.table_names);
            }
            if (!string.IsNullOrEmpty(this.table_name))
            {
                tableNames.Add(this.table_name);
            }
            var groupFieldsList = SelectorCompiler.GetFields(fieldBody,new string[] { ret.Schema}, tableNames.ToArray(), GroupFields.Parameters.Cast<ParameterExpression>().ToList(),ref ParamList);
            
            ret.groupbyList.AddRange(groupFieldsList);
            return ret;
        }

        public IList<T> ToList(IDb db)
        {
            return db.ExecToList<T>(this.ToSQLString(), this.Params.ToArray());
        }

        public DataTable ToDataTable(Db db)
        {
            return db.ExecToDataTable(this.ToSQLString(), this.Params.ToArray());
        }

        public IList<T> ToList()
        {
            return this.ToList(this.dbContext);
        }
        public T Item(IDb db)
        {
            if (this.selectTop == 0)
            {
                this.selectTop = 1;
            }
            var retList = this.ToList(db);
            return retList.FirstOrDefault();

        }
        public T Item()
        {
            return this.Item(this.dbContext);

        }

        public InsertDataResult<T2> InsertItem<T2>(Db db, T2 DataItem,bool ReturnError=false)
        {
            return db.InsertData<T2>(this.Schema, this.table_name, DataItem, ReturnError);
        }
    }
}