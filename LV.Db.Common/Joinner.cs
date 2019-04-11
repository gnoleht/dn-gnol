using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Linq.Expressions;
using System.Text;

namespace LV.Db.Common
{
    /// <summary>
    /// This class is responsibility for join exprssion builder
    /// </summary>
    internal class Joinner
    {
        /// <summary>
        /// Make a join expression
        /// </summary>
        /// <param name="Expr">Logical join clause </param>
        /// <param name="LeftTable">Left table name</param>
        /// <param name="RightTable">Right table name</param>
        /// <param name="parameters"></param>
        /// <param name="Params">Some parameter will be generate when compile a join expression</param>
        /// <returns></returns>
        private static string GextExpr(Expression Expr,string[] Schemas,  string[] Tables, ReadOnlyCollection<ParameterExpression> parameters, ref List<object> Params)
        {
            if (Expr is MemberExpression)
            {
                var mbx = Expr as MemberExpression;
                var mx = Expr as MemberExpression;
                var next = mx.Expression;
                while(next is MemberExpression)
                {
                    mx = next as MemberExpression;
                    next = mx.Expression;

                }
                var px = mx.Expression as ParameterExpression;
                var index = parameters.IndexOf(px);
                if (index>-1)
                {
                    return Globals.Compiler.GetFieldName(Schemas[index], Tables[index], mbx.Member.Name);
                }
                var pIndex = Params.Count;
                var val = Expression.Lambda(Expr).Compile().DynamicInvoke();
                Params.Add(val);
                return "{" + pIndex.ToString() + "}";
            }
            if (Expr is BinaryExpression)
            {
                var bx = Expr as BinaryExpression;
                var op = utils.GetOp(bx.NodeType);
                string left = GextExpr(bx.Left, Schemas, Tables, parameters, ref Params);
                string right = GextExpr(bx.Right, Schemas, Tables, parameters, ref Params);
                return left + " " + op + " " + right;
            }
            if (Expr is ConstantExpression)
            {
                var pIndex = Params.Count;
                Params.Add(((ConstantExpression)Expr).Value);
                return "{" + pIndex.ToString() + "}";
            }
            if (Expr is MethodCallExpression)
            {
                var pIndex = Params.Count;
                var val = Expression.Lambda(Expr).Compile().DynamicInvoke();
                Params.Add(val);
                return "{" + pIndex.ToString() + "}";
            }
            if (Expr is UnaryExpression)
            {
                var ux = Expr as UnaryExpression;
                return GextExpr(ux.Operand, Schemas, Tables, parameters, ref Params);
            }
            throw new NotImplementedException();
        }
        /// <summary>
        /// Make a join expression
        /// </summary>
        /// <param name="Expr">Logical join clause </param>
        /// <param name="LeftTable">Left table name</param>
        /// <param name="RightTable">Right table name</param>
        /// <param name="parameters"></param>
        /// <param name="Params">Some parameter will be generate when compile a join expression</param>
        /// <returns></returns>
        internal static string GetJoinExpr(Expression Expr,string[] Schemas, string[] Tables, ReadOnlyCollection<ParameterExpression> parameters, ref List<object> Params)
        {
            if (Expr is BinaryExpression)
            {
                var bx = Expr as BinaryExpression;
                var op = utils.GetOp(bx.NodeType);
                string left = GextExpr(bx.Left, Schemas, Tables, parameters, ref Params);
                string right = GextExpr(bx.Right, Schemas, Tables, parameters, ref Params);
                return left + " " + op + " " + right;
            }

            throw new NotImplementedException();
        }
    }
}
