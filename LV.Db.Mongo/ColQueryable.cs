using MongoDB.Bson;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Text;

namespace LV.Db.Mongo
{
    public class ColQueryable<T>
    {
        public string CollectionName { get; private set; }
        public BsonArray Stages { get; private set; }

        public ColQueryable(string CollectionName)
        {
            this.CollectionName = CollectionName;
            this.Stages = new BsonArray();
        }
        public ColQueryable<T2> Clone<T2>()
        {
            var ret = new ColQueryable<T2>(this.CollectionName);
            ret.Stages.AddRange(this.Stages);
            //foreach(DictionaryEntry item in this.Stages)
            //{
            //    ret.Stages.Add(item.Key, item.Value);
            //}
            return ret;
            
        }
        #region Pipeline aggregation
        public ColQueryable<T2> Project<T2>(Expression<Func<T, T2>> Selectors)
        {
            BsonDocument ret = Gobbler.GetFields(Selectors.Body, false);
            var match = new BsonDocument("$project",BsonValue.Create(ret));
            var retQr = this.Clone<T2>();
            retQr.Stages.Add(match);
            return retQr;
            
        }
        public ColQueryable<T> Match(params Expression<Func<T, bool>>[] MatchExpr)
        {
            object ret = Gobbler.GetFilter(MatchExpr);
            var match = new BsonDocument("$match", BsonValue.Create(ret));
            var retQr = this.Clone<T>();
            retQr.Stages.Add(match);
            return retQr;
        }
        public ColQueryable<ClsAddFieldsObject<T,T2>> AddFields<T2>(Expression<Func<T, T2>> Expr)
        {
            throw new NotImplementedException();
        }
        public ColQueryable<T1> Bucket<T1>(Expression<Func<T,object>> GroupBy, object[] Boundaries, Expression<Func<T,T1>> output,object Default=null)
        {
            throw new NotImplementedException();
        }
        public ColQueryable<T1> Count<T1>(Expression<Func<T, T1>> Expr)
        {
            throw new NotImplementedException();
        }
        public ColQueryable<GroupCollection<T1,T2>> Group<T1,T2>(Expression<Func<T,T1>> Id, Expression<Func<T,T2>> Selectors)
        {
            throw new NotImplementedException();
        }
        public ColQueryable<T1> BucketAuto<T1>(Expression<Func<T, object>> GroupBy, int Buckets, Expression<Func<T, T1>> output, string granularity = null)
        {
            throw new NotImplementedException();
        }
        public ColQueryable<T2> Facet<T2>(Expression<Func<T, T2>> Expr)
        {
            throw new NotImplementedException();
        }
        #endregion
        public List<T> Find(object dbConnection, params Expression<Func<T, bool>>[] MatchExpr)
        {
            throw new NotImplementedException();
        }
        public ActionResult Delete(object dbConnection, params Expression<Func<T, bool>>[] MatchExpr)
        {
            throw new NotImplementedException();
        }
        public ActionResult Update<T2>(object dbConnection, object Setter, params Expression<Func<T, bool>>[] MatchExpr)
        {
            throw new NotImplementedException();
        }
        public ActionResult InsertOne(object dbConnection, T DataItem)
        {
            throw new NotImplementedException();
        }
        public ActionResult Insert(object dbConnection, List<T> Items)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Find one item
        /// </summary>
        /// <param name="dbConnection">Mongodb connection</param>
        /// <param name="MatchExpr">conditional </param>
        /// <returns></returns>
        public T FindOne(object dbConnection, params Expression<Func<T, bool>>[] MatchExpr)
        {
            throw new NotImplementedException();
        }

        
    }
    
    public class GroupCollection<T1,T2>
    {
        public T1 Id { get; set; }
        public T2 Selector { get; set; }
    }

}
