//using System;
//using System.Collections.Generic;
//using System.Linq.Expressions;
//using System.Reflection;

//namespace LV.Db.Common
//{
//    internal class Compiler3
//    {
//        internal static void GetFields<T>(Expression expr, IQuerySet<T> querySet)
//        {
//            if (expr is NewExpression)
//            {
//                var nx = expr as NewExpression;
//                var index = 0;
//                foreach (var m in nx.Members)
//                {
//                    Expression field = null;
//                    if (nx.Arguments.Count > index)
//                    {
//                        field = nx.Arguments[index];
//                    }
//                    CreateField(field, m, querySet);
//                    index++;
//                }
//            }
//        }

//        internal static void CreateField<T>(Expression field, MemberInfo m, IQuerySet<T> querySet)
//        {
//            if (field is MemberExpression)
//            {
//                var fieldName = ((MemberExpression)field).Member.Name;
//                if (fieldName != m.Name)
//                {
//                    fieldName = Globals.Compiler.GetFieldName(querySet.GetTableName(), fieldName) + " as " + Globals.Compiler.GetQName(m.Name);
//                    querySet.AddField(fieldName);
//                    return;
//                }
//                else
//                {
//                    fieldName = Globals.Compiler.GetFieldName(querySet.GetTableName(), fieldName);
//                    querySet.AddField(fieldName);
//                    return;
//                }


//            }
//            if (field is ConstantExpression)
//            {
//                var f = "{" + querySet.GetParams().Count.ToString() + "} as " + Globals.Compiler.GetQName(m.Name);
//                querySet.AddField(f);
//                querySet.AddParam(((ConstantExpression)field).Value);
//                return;
//            }
//            if (field is BinaryExpression)
//            {
//                var bx = field as BinaryExpression;
//                var op = utils.GetOp(bx.NodeType);
//                var f = GetExpr(bx.Left, querySet) + " " + op + " " + GetExpr(bx.Right, querySet);
//                querySet.AddField(f + " as  " + Globals.Compiler.GetQName(m.Name));
//                return;

//            }
//            if (field is MethodCallExpression)
//            {
//                var cx = field as MethodCallExpression;
//                if(cx.Type == typeof(DataField))
//                {
//                    var mc= cx.Object as MethodCallExpression;
//                    var valObj = Expression.Lambda(mc.Arguments[0]).Compile().DynamicInvoke();
//                    var val = Expression.Lambda(cx.Arguments[0]).Compile().DynamicInvoke();
//                    var f= Globals.Compiler.GetFieldName(valObj.ToString(), val.ToString());
//                    querySet.SetTableName(valObj.ToString());
//                    querySet.AddField(f + " as  " + Globals.Compiler.GetQName(m.Name));
//                    return;
//                }
//                //var op = utils.GetOp(bx.NodeType);
//                //var f = GetExpr(bx.Left, querySet) + " " + op + " " + GetExpr(bx.Right, querySet);
//                //querySet.AddField(f + " as  " + Globals.Compiler.GetQName(m.Name));
//                return;

//            }
//            throw new NotImplementedException();

//        }

//        internal static string Filter<T>(Expression Expr, QuerySet<T> querySet)
//        {
//            if (Expr is BinaryExpression)
//            {
//                var ret = GetExpr(Expr, querySet);
//                return ret;
//                ;
//            }
//            throw new NotImplementedException();
//        }

//        internal static string GetExpr<T>(Expression Expr, IQuerySet<T> querySet)
//        {
//            if (Expr is BinaryExpression)
//            {
//                var bx = Expr as BinaryExpression;
//                var op = utils.GetOp(bx.NodeType);
//                var left = GetExpr(bx.Left, querySet);
//                var right = GetExpr(bx.Right, querySet);
//                return left + " " + op + " " + right;
//            }
//            if (Expr is MemberExpression)
//            {
//                var ret= ConstCompiler.GetValue((MemberExpression)Expr);
//                if(ret is DataFieldMember)
//                {
//                    var retField = ret as DataFieldMember;
//                    var tableName = querySet.GetTableNameByType(retField.Field_Type);
//                    if (!string.IsNullOrEmpty(tableName))
//                    {
//                        return Globals.Compiler.GetFieldName(tableName, retField.Field_Name);
//                    }
//                    else
//                    {
//                        return Globals.Compiler.GetQName(retField.Field_Name);
//                    }
                    

//                }
//                else
//                {
                    
//                    var paramIndex = querySet.GetParams().Count;
                    
//                    querySet.AddParam(ret);
//                    return "{" + paramIndex + "}";
//                }
//            }
//            if (Expr is ConstantExpression)
//            {
//                var cx = Expr as ConstantExpression;


//                var paramIndex = querySet.GetParams().Count;
//                var val = Expression.Lambda(cx).Compile().DynamicInvoke();
//                querySet.AddParam(val);
//                return "{" + paramIndex + "}";
//            }
//            if(Expr is MethodCallExpression)
//            {
//                var mc = Expr as MethodCallExpression;
//                if(mc.Method.ReflectedType == typeof(SqlFuncs))
//                {
//                    if (mc.Method.Name == "Call")
//                    {
//                        var ParamsList = new List<string>();
//                        var fnName = ((ConstantExpression)mc.Arguments[0]).Value.ToString();
//                        for (var i=1;i<mc.Arguments.Count;i++)
//                        {
//                            ParamsList.Add(GetExpr(mc.Arguments[i], querySet));
//                        }
//                        return string.Format(fnName, ParamsList.ToArray());
//                    }
//                    else {
//                        var sqlFunctionInProvider = Globals.Compiler.GetSqlFunction(mc.Method.Name);
//                        if (string.IsNullOrEmpty(sqlFunctionInProvider))
//                        {
//                            throw new Exception(string.Format("{0} can not compile", mc.Method.Name));
//                        }

//                        var ParamsList = new List<string>();
//                        foreach (var arg in mc.Arguments)
//                        {
//                            ParamsList.Add(GetExpr(arg, querySet));
//                        }
//                        return string.Format(sqlFunctionInProvider, String.Join(",", ParamsList));
//                    }
//                }
//                var paramIndex = querySet.GetParams().Count;
//                var val = Expression.Lambda(Expr).Compile().DynamicInvoke();
//                querySet.AddParam(val);
//                return "{" + paramIndex + "}";
//            }
//            throw new NotImplementedException();
//        }

//        private static string GetRight<T>(MemberExpression expr, IQuerySet<T> querySet)
//        {
//            var val = ConstCompiler.GetValue(expr);
//            var paramIndex = querySet.GetParams().Count;
           
//            querySet.AddParam(val);
//            return "{" + paramIndex + "}";
//        }

//        internal static string GetExpr<T>(MemberExpression Expr, MemberInfo member, IQuerySet<T> querySet)
//        {
//            if (Expr.Expression is ConstantExpression)
//            {
//                var val = ConstCompiler.GetValue(Expr);
//                //return GetExpr((ConstantExpression)Expr.Expression, Expr.Member, querySet);
//            }
//            throw new NotImplementedException();
//        }


       
//    }
//}
