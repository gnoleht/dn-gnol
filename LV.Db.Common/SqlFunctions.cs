using System;
using System.Linq.Expressions;

namespace LV.Db.Common
{
    public static class SqlFuncs
    {
        public static int Len(object Field)
        {
            throw new NotImplementedException();
        }

        public static object Month(object Field)
        {
            throw new NotImplementedException();
        }

        public static object Call(string BuilinSqlFunctionName, params object[] Pars)
        {
            throw new NotImplementedException();
        }
        public static object Sum(object expr)
        {
            throw new NotImplementedException();
        }
        public static object Min(object expr)
        {
            throw new NotImplementedException();
        }
        public static object Max(object expr)
        {
            throw new NotImplementedException();
        }

        public static int IsNull(params object[] exprs)
        {
            throw new NotImplementedException();
        }

        public static object When(object Value, object ReturnValue)
        {
            throw new NotImplementedException();
        }
        public static object When(bool Value, object ReturnValue)
        {
            throw new NotImplementedException();
        }
        public static T Case<T>(object owner,params object[] Branches )
        {
            throw new NotImplementedException();
        }
        public static object Case(object owner, params object[] Branches)
        {
            throw new NotImplementedException();
        }
        
    }
    public class WhenStatement
    {
        private object expr;

        public WhenStatement()
        {
        }

        public WhenStatement(object expr)
        {
            this.expr = expr;
        }

        public ThenStatement When(bool v)
        {
            return new ThenStatement();
        }
    }
    public class ThenStatement
    {
        public bool Then(object Expr)
        {
            throw new NotImplementedException();
        }
    }
}