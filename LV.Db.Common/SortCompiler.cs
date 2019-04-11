using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Text;

namespace LV.Db.Common
{
    internal class SortCompiler
    {
        internal static List<string> GetFields(Expression Expr)
        {
            var ret = new List<string>();
            if (Expr is NewExpression)
            {
                var nx=Expr as NewExpression;
                foreach(var x in nx.Arguments)
                {
                    var fields = GetFields(x);
                    ret.AddRange(fields);
                }
                return ret;
            }
            if(Expr is MemberExpression)
            {
                
                ret.Add(((MemberExpression)Expr).Member.Name);
                return ret;
            }
            if (Expr is UnaryExpression)
            {
                var ux = Expr as UnaryExpression;
                var fields = GetFields(ux.Operand);
                ret.AddRange(fields);
                return ret;
            }
            if(Expr is LambdaExpression)
            {
                var lx = Expr as LambdaExpression;
                var fields = GetFields(lx.Body);
                ret.AddRange(fields);
                return ret;
            }
            throw new NotImplementedException();
        }
    }
}
