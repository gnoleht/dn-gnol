//using System;
//using System.Collections.Generic;
//using System.Linq.Expressions;
//using System.Text;
//using System.Linq;
//namespace LV.Db.Common
//{
//    public class Compiler
//    {
        

//        internal static void Build<T>(Expression expr, QuerySet<T> qr, string alias)
//        {
//            if(expr is BinaryExpression)
//            {
//                var b = (BinaryExpression)expr;
//                var field = Compiler.GetExpr(b, qr);
//                if(!string.IsNullOrEmpty(alias))
//                {
//                    qr.fields.Add(Globals.Compiler.MakeAlias(field,alias));
//                }
//                else
//                {
//                    qr.fields.Add(field);
//                }
                
//                return;
//            }
//            if(expr is MemberExpression)
//            {
//                var m = (MemberExpression)expr;
//                var field = Compiler.GetMemberExpr(m, qr);
//                if (!string.IsNullOrEmpty(alias))
//                {
//                    qr.fields.Add(Globals.Compiler.MakeAlias(field, alias));
//                }
//                else
//                {
//                    qr.fields.Add(field);
//                }
//                return;
//            }
//            if(expr is MethodCallExpression)
//            {
//                var m = (MethodCallExpression)expr;
//                var field= GetCallExpr(m, qr);
//                qr.fields.Add(field);
//                return;
//            }
//            if(expr is UnaryExpression)
//            {
//                var field =GetUnaryExpr((UnaryExpression)expr, qr);
//                qr.fields.Add(field);
//                return;
//            }
//            throw new NotImplementedException();
//        }

        

//        private static string GetCallExpr<T>(MethodCallExpression expr, QuerySet<T> qr)
//        {
//            if (expr.Type == typeof(DataField))
//            {
//                if (expr.Method.Name == "Alias")
//                {
//                    var bx = expr.Object as BinaryExpression;
//                    var field = GetBinExpr(bx, qr, ((ConstantExpression)expr.Arguments[0]).Value.ToString());
//                    return field;
//                }
//                else
//                {
//                    var field = expr.Method.Invoke(qr, new object[] { ((ConstantExpression)expr.Arguments[0]).Value });
//                    return ((LV.Db.Common.DataField)field).name;
//                }
                
//            }
//            throw new NotImplementedException();
//        }

//        private static string GetBinExpr<T>(BinaryExpression expr, QuerySet<T> qr, string Alias)
//        {
//            var field = Compiler.GetExpr(expr, qr);
//            if (!string.IsNullOrEmpty(Alias))
//            {
//                return Globals.Compiler.MakeAlias(field, Alias);
//            }
//            else
//            {
//                return field;
//            }
//        }

//        internal static string GetExpr<T>(BinaryExpression b, QuerySet<T> qr)
//        {
//            string left = Compiler.GetExpr(b.Left, qr);
//            string right = Compiler.GetExpr(b.Right, qr);
//            string op = Compiler.GetOp(b, qr);
//            return left + " " + op + right;
//            throw new NotImplementedException();
//        }

//        private static string GetOp<T>(BinaryExpression b, QuerySet<T> qr)
//        {
//            return utils.GetOp(b.NodeType);
            
//        }

//        private static string GetExpr<T>(Expression expr, QuerySet<T> qr)
//        {
//            if(expr is MemberExpression)
//            {
//                return Compiler.GetMemberExpr((MemberExpression)expr, qr);
//            }
//            if(expr is ConstantExpression)
//            {
//                return Compiler.GetConstExpr((ConstantExpression)expr, qr);
//            }
//            if(expr is BinaryExpression)
//            {
//                return GetExpr((BinaryExpression)expr, qr);
//            }
//            if (expr is MethodCallExpression)
//            {
//                return GetCallExpr((MethodCallExpression)expr, qr);
//            }
//            if(expr is UnaryExpression)
//            {
//                return GetUnaryExpr((UnaryExpression)expr, qr);
//            }if(expr is ParameterExpression)
//            {
//                return GetParamExpr((ParameterExpression)expr, qr);
//            }
//            throw new NotImplementedException();
//        }

//        private static string GetParamExpr<T>(ParameterExpression expr, QuerySet<T> qr)
//        {
//            var tblItem = qr.types.FirstOrDefault(p => p.cslType == expr.Type);
//            if (tblItem == null)
//            {
//                return String.Empty;
//            }
//            throw new NotImplementedException();
//        }

//        private static string GetUnaryExpr<T>(UnaryExpression expr, QuerySet<T> qr)
//        {
//            var v = expr.Operand;
//            if(v is ConstantExpression)
//            {
//                var paramName = "{" + qr.Params.Count.ToString() + "}";
//                qr.Params.Add(((ConstantExpression)v).Value);
//                return paramName;

//            }
//            if (v is MemberExpression)
//            {
//                return GetMemberExpr((MemberExpression)v, qr);
//            }
//            throw new NotImplementedException();
//        }

//        private static string GetConstExpr<T>(ConstantExpression expr, QuerySet<T> qr)
//        {
//            var paramName = "{" + qr.Params.Count.ToString() + "}";
//            qr.Params.Add(expr.Value);
//            return paramName;
//        }

//        private static string GetMemberExpr<T>(MemberExpression expr, QuerySet<T> qr)
//        {
//            var x = new List<string>();
//            var tblItem = qr.types.FirstOrDefault(p => p.cslType == typeof(T));
//            if(tblItem==null)
//            {
//                return Globals.Compiler.GetFieldName(qr.GetTableName(), expr.Member.Name);
//            }
//            else
//            {
//                return Globals.Compiler.GetFieldName(tblItem.tableName, expr.Member.Name);
//            }
            
            
//            if(expr.Expression is ConstantExpression)
//            {
//                return GetConstExpr((ConstantExpression)expr.Expression,qr);
//            }
//            if (expr.Expression is MemberExpression)
//            {
//                return GetMemberExpr((MemberExpression)expr.Expression, qr);
//            }
//            if (expr.Expression is ParameterExpression)
//            {
//                return GetParamExpr((ParameterExpression)expr.Expression, qr);
//            }
//            throw new NotImplementedException();


//        }
//    }
//}
