//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Linq.Expressions;
//using System.Reflection;

//namespace LV.Db.Common
//{
//    public class delete_QuerySet2<T1, T2>
//    {
//        internal string leftTableName;
//        internal string rightTableName;
//        internal string rightDatasource;
//        internal string leftDatasource;
//        internal List<object> Params=new List<object>();
//        internal List<string> fields=new List<string>();
//        internal string joinExpression;
//        internal string leftAliasTable;
//        internal string rightAliasTable;
//        internal string joinType;
//        internal string where;
//        private List<SortingInfo> sortList =new List<SortingInfo>();

//        public string ToSQLString()
//        {
//            var select = "*.*";
//            if (fields.Count > 0)
//            {
//                select = string.Join(",", fields);
//            }
//            var From = this.BuildSourceFrom();
//            var ret = "select "+ select +" from "+ From;
//            if (!string.IsNullOrEmpty(this.where))
//            {
//                ret += " where " + this.where;
//            }
//            if (this.sortList.Count > 0)
//            {
//                var tmp = string.Join(",", this.sortList.Select(p => p.FieldName + " " + p.SortType).ToArray());
//                ret += " order by " + tmp;
//            }
//            return ret;
//        }

//        private string BuildSourceFrom()
//        {
//            var From = "";
            
//            if (!string.IsNullOrEmpty(this.leftDatasource))
//            {
//                From = "(" + this.leftDatasource + ") as " + Globals.Compiler.GetQName(this.leftAliasTable);
//            }
//            else
//            {
//                From = "" + Globals.Compiler.GetQName(this.leftTableName) + " as " + Globals.Compiler.GetQName(this.leftAliasTable);
//            }
//            From = From + " " + this.joinType + " join ";
//            if (!string.IsNullOrEmpty(this.rightDatasource))
//            {
//                From = From + "(" + this.rightDatasource + ") as " + Globals.Compiler.GetQName(this.rightAliasTable);
//            }
//            else
//            {
//                From = From + " " + Globals.Compiler.GetQName(this.rightTableName) + " as " + Globals.Compiler.GetQName(this.rightAliasTable);
//            }
//            From += " on " + this.joinExpression;
           
//            return From;
//        }

//        public delete_QuerySet2<T1,T2> SortAsc(Expression<Func<T1, T2, object>> expr)
//        {
//            var ret = this.Clone();
            
//            if (expr.Body is NewExpression)
//            {
//                var nx = expr.Body as NewExpression;
//                var pList = this.Params;
//                var mbIndex = 0;
//                foreach (var x in nx.Arguments)
//                {
//                    var fx = SelectorCompiler.Gobble<string>(x, new string[] { this.leftAliasTable, this.rightAliasTable },
//                       expr.Parameters,
//                        null, mbIndex, ref pList);
//                    ret.sortList.Add(new SortingInfo()
//                    {
//                        FieldName = fx,
//                        SortType = "asc"
//                    });
//                    mbIndex++;
//                }
//                return ret;
//            }

//            throw new NotImplementedException();
//        }
//        public delete_QuerySet2<T1, T2> SortDesc(Expression<Func<T1, T2, object>> expr)
//        {
//            var ret = this.Clone();

//            if (expr.Body is NewExpression)
//            {
//                var nx = expr.Body as NewExpression;
//                var pList = this.Params;
//                var mbIndex = 0;
//                foreach (var x in nx.Arguments)
//                {
//                    var fx = SelectorCompiler.Gobble<string>(x, new string[] { this.leftAliasTable, this.rightAliasTable },
//                       expr.Parameters,
//                        null, mbIndex, ref pList);
//                    ret.sortList.Add(new SortingInfo()
//                    {
//                        FieldName = fx,
//                        SortType = "desc"
//                    });
//                    mbIndex++;
//                }
//                return ret;
//            }

//            throw new NotImplementedException();
//        }
//        public delete_QuerySet2<T1,T2> Select(Expression<Func<T1, T2, object>> Selector)
//        {
//            var ret= this.Clone();
//            if (Selector.Body is NewExpression)
//            {
//                var nx = Selector.Body as NewExpression;
//                var pList = this.Params;
//                var mbIndex = 0;
//                foreach (var x in nx.Arguments)
//                {
//                    var fx = SelectorCompiler.Gobble<string>(x, new string[] { this.leftAliasTable, this.rightAliasTable },
//                       Selector.Parameters,
//                        nx.Members.Cast<MemberInfo>().ToArray(), mbIndex, ref pList);
//                    ret.fields.Add(fx);
//                    mbIndex++;
//                }
//                return ret;
//            }
            
//            throw new NotImplementedException();
//        }
//        public delete_QuerySet2<T1,T2> Filter(Expression<Func<T1, T2, bool>> filter)
//        {
//            if(filter.Body is BinaryExpression)
//            {
//                var ret = this.Clone();
//                var bx = filter.Body as BinaryExpression;
//                var op = utils.GetOp(bx.NodeType);
//                var Params = ret.Params;
//                var left = SelectorCompiler.Gobble<string>(bx.Left,new string[] { ret.leftAliasTable, ret.rightAliasTable}, filter.Parameters, null, -1, ref Params);
//                var right = SelectorCompiler.Gobble<string>(bx.Right, new string[] { ret.leftAliasTable, ret.rightAliasTable }, filter.Parameters, null, -1, ref Params);
//                ret.where = left + " " + op + " " + right;
//                return ret;
//            }
//            throw new NotImplementedException();
//        }

//        private delete_QuerySet2<T1, T2> Clone()
//        {
//            var ret = new delete_QuerySet2<T1, T2>();
//            ret.fields.AddRange(this.fields);
//            ret.joinExpression = this.joinExpression;
//            ret.joinType = this.joinType;
//            ret.leftAliasTable = this.leftAliasTable;
//            ret.leftDatasource = this.leftDatasource;
//            ret.Params.AddRange(this.Params);
//            ret.rightAliasTable = this.rightAliasTable;
//            ret.rightDatasource = this.rightDatasource;
//            ret.rightTableName = this.rightTableName;
//            ret.leftTableName = this.leftTableName;
//            ret.sortList.AddRange(this.sortList);
//            return ret;
//        }

//        public QuerySet<T> ToSubQuery<T>(Expression<Func<T1, T2, T>> Selector)
//        {
//            var ret = new QuerySet<T>();
//            ret.table_name = "T" + ret.sequenceJoin.ToString();
//            ret.sequenceJoin++;
//            if(Selector.Body is NewExpression)
//            {
//                var nx = Selector.Body as NewExpression;
//                var pList = this.Params;
//                var mbIndex = 0;
//                foreach(var x in nx.Arguments)
//                {
//                    var fx = SelectorCompiler.Gobble<string>(x, new string[] { this.leftAliasTable, this.rightAliasTable }, 
//                       Selector.Parameters,
//                        nx.Members.Cast<MemberInfo>().ToArray(), mbIndex, ref pList);
//                    ret.fields.Add(fx);
//                    mbIndex++;
//                }
//                ret.datasource = this.BuildSourceFrom();
//                ret.Params.AddRange(this.Params);
//                return ret;
//            }
            
//            throw new NotImplementedException();
//        }

//        public override string ToString()
//        {
//            var sql = this.ToSQLString();
//            var index = 0;
//            foreach (var p in this.Params)
//            {
               
//                if (p == null)
//                {
//                    sql = sql.Replace("{" + index.ToString() + "}", "NULL");
//                }
//                else if (p is string)
//                {
//                    sql = sql.Replace("{" + index.ToString() + "}", "'" + p.ToString().Replace("'", "''") + "'");
//                }
//                else if (p is DateTime)
//                {
//                    sql = sql.Replace("{" + index.ToString() + "}", "'" + ((DateTime)p).ToString("yyyy-MM-dd hh:mm:ss") + "'");
//                }
//                else
//                {
//                    sql = sql.Replace("{" + index.ToString() + "}", "" + p.ToString() + "");
//                }
//                index++;
//            }
//            return sql;
//        }
//    }
//}