using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;

namespace LV.Db.Common
{
    /// <summary>
    /// Queryabl instance generator
    /// </summary>
    public class QueryBuilder
    {
        

        public static QuerySet<T> Join<T1,T2,T>(QuerySet<T1> qr1, QuerySet<T2> qr2, Expression<Func<T1, T2, bool>> JoinClause, Expression<Func<T1, T2, T>> selecttor)
        {
            return BuilJoin(qr1, qr2, JoinClause, selecttor,"inner");

        }
        public static QuerySet<T> LeftJoin<T1, T2, T>(QuerySet<T1> qr1, QuerySet<T2> qr2, Expression<Func<T1, T2, bool>> JoinClause, Expression<Func<T1, T2, T>> selecttor)
        {
            return BuilJoin(qr1, qr2, JoinClause, selecttor, "left");

        }

        public static QuerySet<T> FromNothing<T>(Expression<Func<object,T>> Selector)
        {
            var ret = new QuerySet<T>();
            var Params = new List<object>();
            ret.fields= SelectorCompiler.GetFields(Selector.Body, new string[] { }, new string[] { }, Selector.Parameters, ref Params);
            ret.isFromNothing = true;
            ret.Params.AddRange(Params);
            return ret;
        }

        [System.Diagnostics.DebuggerStepThrough]
        public static QuerySet<T> FromEntity<T>(string schema)
        {
            var items = typeof(T).CustomAttributes.ToList();
            var TableAttr = typeof(T).CustomAttributes.Where(p => p.AttributeType ==typeof(DataTableAttribute)).FirstOrDefault();
            
            if(TableAttr is null)
            {
                throw (new Exception(string.Format("It looks like you forgot set DataTable for class {0}", typeof(T).FullName)));
            }
            return new QuerySet<T>(schema, TableAttr.ConstructorArguments[0].Value.ToString());
        }
        [System.Diagnostics.DebuggerStepThrough]
        public static QuerySet<T> FromEntity<T>()
        {
            return FromEntity<T>("");
        }
        public static QuerySet<T> RightJoin<T1, T2, T>(QuerySet<T1> qr1, QuerySet<T2> qr2, Expression<Func<T1, T2, bool>> JoinClause, Expression<Func<T1, T2, T>> selecttor)
        {
            return BuilJoin(qr1, qr2, JoinClause, selecttor, "right");

        }

        public static QuerySet<T> From<T>(string schema, string tableName)
        {
            return new QuerySet<T>(schema, tableName);
        }

        private static QuerySet<T> BuilJoin<T1, T2, T>(QuerySet<T1> qr1, QuerySet<T2> qr2, Expression<Func<T1, T2, bool>> JoinClause, Expression<Func<T1, T2, T>> selecttor,string JoinType)
        {
            var q1 = qr1 as QuerySet<T1>;
            var q2 = qr2 as QuerySet<T2>;

            if (q1.IsSubQuery())
            {

            }
            if (q2.IsSubQuery())
            {

            }
            var ret = new QuerySet<T>();
            
            ret.joinTimes = (q1.joinTimes + 1) * 10 + q2.joinTimes;

            var ltName = "l" + ret.joinTimes.ToString();
            var rtName = "r" + ret.joinTimes.ToString();
            var Params = new List<object>();
            Params.AddRange(q1.Params);
            Params.AddRange(q2.Params);
            var joinParams = new List<object>();
            var strJoin = Joinner.GetJoinExpr(JoinClause.Body,new string[] {null,null },new string[] {ltName,rtName }, JoinClause.Parameters, ref joinParams);
            strJoin = utils.RepairParameters(strJoin, Params, joinParams);
            Params.AddRange(joinParams);
            var leftSource = (!q1.IsSubQuery())? utils.GetSource(q1):"("+q1.ToSQLString()+")";
            var rightSource = (!q2.IsSubQuery()) ? utils.GetSource(q2) : "(" + q2.ToSQLString() + ")";
            rightSource = utils.RepairParameters(rightSource, q1.Params, q2.Params);
            ret.datasource = leftSource+" as "+ Globals.Compiler.GetQName("", ltName)+
                            " " + JoinType+ " join " + rightSource +" as "+ Globals.Compiler.GetQName("",rtName) + " on " + strJoin;
            var refTables = new List<string>();
            var refParameters = new List<ParameterExpression>();
            if (q1.table_names != null)
            {
                refTables.AddRange(q1.table_names);
            }

            if (q2.table_names != null)
            {
                refTables.AddRange(q2.table_names);
            }
            refTables.Add(ltName);
            refTables.Add(rtName);
            if (q1.fieldParams != null)
            {
                refParameters.AddRange(q1.fieldParams);
            }
            if (q2.fieldParams != null)
            {
                refParameters.AddRange(q2.fieldParams);
            }
            refParameters.AddRange(selecttor.Parameters);
            var retFields = SelectorCompiler.GetFields(selecttor.Body,new string[] {null,null}, refTables.ToArray(), refParameters.ToArray(), ref Params);
            ret.fields = retFields;
            ret.fieldParams = new List<ParameterExpression>();
            ret.fieldParams.AddRange(refParameters);
            ret.parametersExpr = selecttor.Parameters;
            ret.Params = Params;
            ret.table_names = refTables.ToArray();
            return ret;
        }



       
        public static QuerySet<T> CrossJoin<T1, T2, T>(QuerySet<T1> qr1, QuerySet<T2> qr2, Expression<Func<T1, T2, T>> resultSelector)
        {
            return BuilCrossJoin<T1, T2, T>(qr1, qr2, resultSelector);
        }

        private static QuerySet<T> BuilCrossJoin<T1, T2, T>(QuerySet<T1> qr1, QuerySet<T2> qr2, Expression<Func<T1, T2, T>> resultSelector)
        {
            var Params = new List<object>();

            var retQr = new QuerySet<T>();
            retQr.sequenceJoin = 1;
            var table1 = "T" + retQr.sequenceJoin.ToString();
            retQr.sequenceJoin++;
            var table2 = "T" + retQr.sequenceJoin.ToString();
            var source1 = Globals.Compiler.CreateAliasTable(Globals.Compiler.GetQName(qr1.Schema, qr1.GetTableName()), Globals.Compiler.GetQName("",table1));
            var source2 = Globals.Compiler.CreateAliasTable(Globals.Compiler.GetQName(qr1.Schema, qr2.GetTableName()), Globals.Compiler.GetQName("",table2));
            if (qr1.GetFields().Count > 0 ||(!string.IsNullOrEmpty(((QuerySet <T1>) qr1).where)))
            {
                source1 = Globals.Compiler.CreateAliasTable("(" + qr1.ToSQLString() + ")", Globals.Compiler.GetQName("",table1));
            }
            if (qr2.GetFields().Count > 0 || (!string.IsNullOrEmpty(((QuerySet<T2>)qr2).where)))
            {
                source2 = Globals.Compiler.CreateAliasTable("(" + qr2.ToSQLString() + ")", Globals.Compiler.GetQName("",table2));
            }
            
            for (var i = 0; i < qr2.GetParams().Count; i++)
            {
                var nIndex = qr1.GetParams().Count + Params.Count + i;
                source2 = source2.Replace("{" + i.ToString() + "}", "{$##" + nIndex.ToString() + "##$}");
            }
            string FromSource = source1 + "," + source2 ;
            FromSource = FromSource.Replace("{$##", "{").Replace("##$}", "}");
            retQr.SetDataSource(FromSource);
            retQr.Params.AddRange(qr1.GetParams());
            retQr.Params.AddRange(Params);
            retQr.Params.AddRange(qr2.GetParams());
            return retQr;
        }
        /// <summary>
        /// Make Query set from class and tablename
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="TableName"></param>
        /// <returns></returns>
        public static QuerySet<T> From<T>(string TableName)
        {
            if(Globals.DbType== DbTypes.None)
            {
                var t = typeof(Settings).FullName;
                throw new Exception(string.Format(@"It looks like you forgot call {0}.SetConnectionString",t));
            }
            return new QuerySet<T>(TableName);
        }

       
    }
}
