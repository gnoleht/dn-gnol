//using System;
//using System.Collections.Generic;
//using System.Linq.Expressions;
//using System.Text;

//namespace LV.Db.Common
//{
//    /// <summary>
//    /// Query set supporting
//    /// </summary>
//    /// <typeparam name="T"></typeparam>
//    public interface IQuerySet<T>
//    {
//        /// <summary>
//        /// Select some fields in a table
//        /// </summary>
//        /// <typeparam name="T2"></typeparam>
//        /// <param name="selector">Example: p=>new {p.field1,aliasField=p.field2}</param>
//        /// <returns></returns>
//        IQuerySet<T2> Select<T2>(Expression<Func<T, T2>> selector);
//        /// <summary>
//        /// Where clause in SQL statement
//        /// </summary>
//        /// <param name="expr"></param>
//        /// <returns></returns>
//        IQuerySet<T> Filter(Expression<Func<T, bool>> expr);
//        /// <summary>
//        /// Group by clause. You must select some field after group by
//        /// You can not group by without select some fields
//        /// </summary>
//        /// <typeparam name="T2"></typeparam>
//        /// <param name="GroupFields">Example p=>new {p.field1, p.field2+''}</param>
//        /// <param name="selector">Exmaple p=>new {total=SqlFuncs.Sum(p.field3+p.field4)} </param>
//        /// <returns></returns>
//        IQuerySet<T> GroupBy(Expression<Func<T,object>> GroupFields);
//        /// <summary>
//        /// Get list of selected fields
//        /// </summary>
//        /// <returns></returns>
//        List<string> GetFields();
//        /// <summary>
//        /// Translate to SQL with input parameters if parameters is necessary
//        /// </summary>
//        /// <returns></returns>
//        string ToSQLString();
//        /// <summary>
//        /// Get all parameters
//        /// </summary>
//        /// <returns></returns>
//        IList<object> GetParams();
//        /// <summary>
//        /// Sort asc
//        /// Example: p=>new {p.Code,p.Name} that mean you would like to sort 'code' and 'name' by ascending
//        /// </summary>
//        /// <param name="Expr"></param>
//        /// <returns></returns>
//        IQuerySet<T> SortAsc(Expression<Func<T, object>> Expr);
//        /// <summary>
//        /// Sort asc, but differenc way: Example p=>p.code,p=>p.name
//        /// </summary>
//        /// <param name="Expr"></param>
//        /// <returns></returns>
//        IQuerySet<T> SortAsc(params Expression<Func<T, object>>[] Expr);
//        /// <summary>
//        /// Sort desc
//        /// Example: p=>new {p.Code,p.Name} that mean you would like to sort 'code' and 'name' by ascending
//        /// </summary>
//        /// <param name="Expr"></param>
//        /// <returns></returns>
//        IQuerySet<T> SortDesc(Expression<Func<T, object>> Expr);
//        /// <summary>
//        /// Sort desc, but differenc way: Example p=>p.code,p=>p.name
//        /// </summary>
//        /// <param name="Expr"></param>
//        /// <returns></returns>
//        IQuerySet<T> SortDesc(params Expression<Func<T, object>>[] Expr);
//        string GetTableName();
//        //void AddField(string fieldName);
//        //void AddParam(object value);
//        //void SetTableName(string tableName);
//        //void SetDataSource(string sql);


//    }
//}
