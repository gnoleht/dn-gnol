//using System;
//using System.Collections.Generic;
//using System.Linq.Expressions;
//using System.Text;

//namespace LV.Db.Common
//{
//    internal class MultiQueryCompiler
//    {
//        internal static string GetJoinExpression(Expression Expr, Type T1, Type T2, string tb1, string tb2)
//        {
//            if(Expr is BinaryExpression)
//            {
//                var fx = Expr as BinaryExpression;
//                var op = utils.GetOp(fx.NodeType);
//                var left = GetExpr(((BinaryExpression)Expr).Left, T1, T2, tb1, tb2);
//                var right = GetExpr(((BinaryExpression)Expr).Right, T1, T2, tb1, tb2);

//                return "" + left + " " + op + " " + right + "";

//            }
//            throw new NotImplementedException();
//        }

//        private static string GetExpr(Expression Expr, Type t1, Type t2, string tb1, string tb2)
//        {
//            if(Expr is MemberExpression)
//            {
                
//                var mb = Expr as MemberExpression;
//                var mbType = mb.Expression.Type;
//                var table_name = tb1;
//                if(mbType == t2)
//                {
//                    table_name = tb2;
//                }

//                return Globals.Compiler.GetFieldName(table_name, mb.Member.Name);
//            }
//            if (Expr is BinaryExpression)
//            {
//                var bx = Expr as BinaryExpression;
//                return GetJoinExpression(bx, t1, t2, tb1, tb2);
                
//            }
//            throw new NotImplementedException();
//        }
//    }
//}
