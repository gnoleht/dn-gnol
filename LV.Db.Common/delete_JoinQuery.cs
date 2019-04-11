//using System;
//using System.Collections.Generic;
//using System.Linq.Expressions;

//namespace LV.Db.Common
//{
//    internal class JoinQuery<T1, T2>
//    {

//        internal string strJoin;
//        internal List<string> fields;
//        internal string tableName;
       

//        internal List<object> Params = new List<object>();
//        internal string _where="";
       
       
        

//        public List<object> GetParams()
//        {
//            return this.Params;
//        }
//        internal JoinQuery(QuerySet<T1> qr1, IQuerySet<T2> qr2, Expression<Func<T1, T2, bool>> OnClause, string JoinType)
//        {
//            return ;
//        }

        

//        public string ToSQLString()
//        {
//            if (this.fields.Count > 0)
//            {
//                var select = "select " + String.Join(",", this.fields) + " from " + this.tableName;
//                if (!string.IsNullOrEmpty(this._where))
//                {
//                    select += " Where " + this._where;
//                }
//                return select;
//            }
//            else
//            {
//                var select = "select * from " + this.tableName;
//                if (!string.IsNullOrEmpty(this._where))
//                {
//                    select += " Where " + this._where;
//                }
//                return select;
//            }
//            throw new NotImplementedException();
//        }

//        public override string ToString()
//        {
//            var retSQL= this.ToSQLString();
//            foreach(var p in this.Params)
//            {
//                if (p == null)
//                {
//                    retSQL = retSQL.Replace("{" + this.Params.IndexOf(p).ToString() + "}", "NULL");
//                }
//                else if (p is string)
//                {
//                    retSQL = retSQL.Replace("{" + this.Params.IndexOf(p).ToString() + "}", "'"+p.ToString().Replace("'","\'")+"'");
//                }
//                else
//                {
//                    retSQL = retSQL.Replace("{" + this.Params.IndexOf(p).ToString() + "}",  p.ToString());
//                }
//            }
//            return retSQL;
//        }
//        public QuerySet<T> Select<T>(Expression<Func<T1, T2, T>> expr)
//        {
//            throw new NotImplementedException();
//        }

        
//    }
//}