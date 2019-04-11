using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;

namespace LV.Db.Common
{
    internal class SelectorCompiler
    {
        private static int GetTableIndex(MemberExpression expr, IList<ParameterExpression> parameters)
        {
            var item = parameters.FirstOrDefault(p => p.Type == expr.Type && p.Name == expr.Member.Name);
            return parameters.IndexOf(item);
        }
        private static int GetTableIndexByType(MemberExpression expr, IList<ParameterExpression> parameters)
        {
            var item = parameters.FirstOrDefault(p => p.Type == expr.Member.ReflectedType);
            return parameters.IndexOf(item);
        }
        internal static List<string> GetFields(Expression Expr,string[] Schemas, string[] tables, IList<ParameterExpression> parameters, ref List<object> paramList)
        {
            var ret = new List<string>();
            if(Expr is NewExpression)
            {
                var nx = Expr as NewExpression;
                var memberIndex = 0;
                //if(nx.Arguments.Count==2 && nx.Arguments[0]==parameters[0] && nx.Arguments[1] == parameters[1])
                //{
                //    return new List<string>();
                //}
                foreach(var p in nx.Arguments)
                {
                    if ((p is MemberExpression) && (((MemberExpression)p).Expression is ParameterExpression))
                    {
                        var aliasName=((((MemberExpression)p).Member.Name)!=nx.Members[memberIndex].Name)?nx.Members[memberIndex].Name:"";
                        int indexofTable = GetTableIndex((MemberExpression)p,parameters);
                        if (indexofTable > -1)
                        {
                            ret.Add(Globals.Compiler.GetQName(Schemas[indexofTable], tables[indexofTable]) + ".*");
                            
                        }
                        else
                        {
                            indexofTable = GetTableIndexByType((MemberExpression)p, parameters);
                            var schema = "";

                            if (indexofTable > -1)
                            {
                                if(indexofTable< Schemas.Length)
                                {
                                    schema = Schemas[indexofTable];
                                }
                                var fieldName = Globals.Compiler.GetFieldName(schema, tables[indexofTable], ((MemberExpression)p).Member.Name);
                                if (!string.IsNullOrEmpty(aliasName))
                                {
                                    fieldName = fieldName +" as "+ Globals.Compiler.GetQName("", aliasName);
                                }
                                ret.Add(fieldName);
                                
                            }
                            else
                            {
                                throw new NotImplementedException();
                            }
                            
                        }
                        //var vx = 1;
                    }
                    else
                    {
                        var x = p;
                        
                        if (x is MemberExpression)
                        {
                            var mx = x as MemberExpression;
                            var n = mx.Expression;
                            while (n is MemberExpression)
                            {
                                mx = n as MemberExpression;
                                n = mx.Expression;

                            }
                            x = n;
                        }
                        var retField = Gobble<object>(x,Schemas, tables, parameters, nx.Members.Cast<MemberInfo>().ToArray(), memberIndex, ref paramList);
                        var aliasName =  nx.Members[memberIndex].Name ;
                        if (retField is string)
                        {
                            if(p is ConstantExpression)
                            {
                                ret.Add((string)retField+" as "+ Globals.Compiler.GetQName("",aliasName));
                            }
                            else
                            {
                                ret.Add((string)retField);
                            }
                            
                           
                        }
                        else if (retField is TableInfo)
                        {
                            var tblInfo = retField as TableInfo;
                            if (memberIndex < parameters.Count)
                            {
                                tblInfo.name = tables[memberIndex];
                            }
                            if (p is ParameterExpression)
                            {
                                
                                ret.Add(Globals.Compiler.GetQName(tblInfo.schema, tblInfo.name) + ".*");
                                
                            }
                            else if (p is MemberExpression)
                            {
                                if (string.IsNullOrEmpty(((TableInfo)retField).name))
                                {
                                    ret.Add(Globals.Compiler.GetFieldName(tblInfo.schema, tblInfo.name, ((MemberExpression)p).Member.Name));
                                    
                                }
                                else
                                {
                                    ret.Add(Globals.Compiler.GetFieldName(tblInfo.schema, tblInfo.name, ((MemberExpression)p).Member.Name)+" as "+Globals.Compiler.GetQName("", tblInfo.alias));
                                    
                                }
                                
                            }
                            else
                            {
                                throw new NotImplementedException();
                            }
                        }
                        else
                        {
                            throw new NotImplementedException();
                        }
                    }

                    memberIndex++;
                }
                return ret;
            }
            if(Expr is MemberExpression)
            {
                var mx = Expr as MemberExpression;
                var next = mx.Expression;
                while(next is MemberExpression)
                {
                    mx = next as MemberExpression;
                    next = mx.Expression;
                }
                int index = GetIndexOfMember(parameters, mx.Expression);
                if (index > -1 && index < tables.Length)
                {
                    ret.Add(Globals.Compiler.GetFieldName(Schemas[index], tables[index], ((MemberExpression)Expr).Member.Name));
                    return ret;
                }
                throw new NotImplementedException();

            }
            if(Expr is BinaryExpression)
            {
                var bx = Expr as BinaryExpression;
                var op = utils.GetOp(bx.NodeType);
                var left = Gobble<string>(bx.Left,Schemas, tables, parameters, null, -1, ref paramList);
                var right = Gobble<string>(bx.Right, Schemas, tables, parameters, null, -1, ref paramList);
                ret.Add(left + " " + op + " " + right);
                return ret;
            }
            if(Expr is MethodCallExpression)
            {
                ret.Add(Gobble<string>(Expr, Schemas, tables, parameters, null,-1,ref paramList));
                return ret;

            }
            throw new NotImplementedException();
        }

        

        internal static T Gobble<T>(Expression Expr,string[] schemas, string[] tables, IList<ParameterExpression> parameters, MemberInfo[] members, int memberIndex,ref List<object> paramList)
        {
            if (Expr is MemberExpression)
            {
                var mx = Expr as MemberExpression;
                var m = Expr as MemberExpression;
                var next = m.Expression;
                while(next is MemberExpression)
                {
                    m = next as MemberExpression;
                    next = m.Expression;
                }

                int index = GetIndexOfMember(parameters, next);
                if (index == -1)
                {
                    var val = Expression.Lambda(mx).Compile().DynamicInvoke();
                    object retVal = "{" + paramList.Count.ToString() + "}";
                    paramList.Add(val);
                    return (T)retVal;
                }
                object ret= (Globals.Compiler.GetFieldName(schemas[index], tables[index], mx.Member.Name));
                
                if (typeof(T) == typeof(AliasFieldExpression))
                {
                    object retObj = new AliasFieldExpression()
                    {
                        Expresion= ret.ToString(),
                        Alias= ""
                    };
                    return (T)retObj;
                }
                if (members != null && members.Length > memberIndex && memberIndex >= 0)
                {
                    ret = ret.ToString() + " as " + Globals.Compiler.GetQName("", members[memberIndex].Name);
                }
                return (T)ret;

            }
            if(Expr is ConstantExpression)
            {
                object paramName = "{" + paramList.Count.ToString() + "}";
                paramList.Add(Expression.Lambda(Expr).Compile().DynamicInvoke());
                return (T)paramName;

            }
            if(Expr is BinaryExpression)
            {
                
                var bx = Expr as BinaryExpression;
                int index = GetIndexOfMember(parameters, Expr);
                var op = utils.GetOp(bx.NodeType);
                var left = Gobble<string>(bx.Left,schemas, tables, parameters, null, memberIndex,ref paramList);
                var right = Gobble<string>(bx.Right,schemas, tables, parameters, null, memberIndex, ref paramList);
                object ret =left + " " + op + " " + right;
                if (op == "=")
                {
                    if (right ==null)
                    {
                        
                        ret = left + " is Null";
                        return (T)ret;
                    }
                }
                if (op == "!=")
                {
                    if (right == null)
                    {

                        ret = left + " is not Null";
                        return (T)ret;
                    }
                }
                if (typeof(T) == typeof(string)) {
                    if (members != null && members.Length > memberIndex && memberIndex >= 0)
                    {
                        ret = ret.ToString() + " as " + Globals.Compiler.GetQName("",members[memberIndex].Name);
                    }
                    return (T)ret;
                }
                if (typeof(T) == typeof(AliasFieldExpression))
                {
                    object retObj = new AliasFieldExpression()
                    {

                    };
                    if (members != null && members.Length > memberIndex && memberIndex >= 0)
                    {
                        ((AliasFieldExpression)retObj).Expresion = ret.ToString();
                        ((AliasFieldExpression)retObj).Alias = Globals.Compiler.GetQName("",members[memberIndex].Name);
                       
                    }
                    return (T)retObj;
                }
                return (T)ret;


            }
            if(Expr is MethodCallExpression)
            {
                var cx = Expr as MethodCallExpression;
                if (cx.Method.ReflectedType == typeof(SqlFuncs))
                {
                    if (cx.Method.Name == "Case")
                    {
                        int index = GetIndexOfMember(parameters, Expr);
                        object retLogical = CommonSQLFunctionsParser.ParseCase(cx, schemas,tables, parameters, members, memberIndex, ref paramList);
                        if(members!=null && members.Count() > 0)
                        {
                            return (T)((object)(retLogical+" as " +Globals.Compiler.GetQName("",members[0].Name)));
                        }
                        return (T)retLogical;
                    }
                    if (cx.Method.Name == "COALESCE")
                    {
                        int index = GetIndexOfMember(parameters, Expr);
                        object retLogical = CommonSQLFunctionsParser.ParseCOALESCE(cx,schemas, tables, parameters, members, memberIndex, ref paramList);
                        if (members != null && members.Count() > 0)
                        {
                            return (T)((object)(retLogical + " as " + Globals.Compiler.GetQName("",members[0].Name)));
                        }
                        return (T)retLogical;
                    }
                    object ret = Globals.Compiler.GetSqlFunction(cx.Method.Name) + "({0})";
                    var args = new List<string>();
                    AliasFieldExpression af = null;
                    foreach (var x in cx.Arguments)
                    {
                        af = Gobble<AliasFieldExpression>(x,schemas, tables, parameters, members, memberIndex, ref paramList);
                        args.Add(af.Expresion);
                    }
                    ret = string.Format(ret.ToString(), string.Join(",", args.ToArray()))+ " as "+af.Alias;
                    return (T)ret;


                }
                else
                {
                    var ret = Expression.Lambda(Expr).Compile().DynamicInvoke();
                    if (ret == null)
                    {
                        return (T)ret;
                    }
                    else
                    {
                        object retExpr = "{" + paramList.Count + "}";
                        paramList.Add(retExpr);
                        return (T)retExpr;
                    }
                }
                
            }
            if(Expr is UnaryExpression)
            {
                var ux = Expr as UnaryExpression;
                var ret = Gobble<T>(ux.Operand,schemas, tables, parameters, members,  memberIndex, ref paramList);
                return ret;

            }
            if(Expr is ParameterExpression)
            {
                var index = parameters.IndexOf((ParameterExpression)Expr);
                if (index > -1)
                {
                    var mbExpr = members[memberIndex];
                    if (mbExpr != null)
                    {
                        object retTableInfo = null;

                        var pItem = parameters.FirstOrDefault(p => p.Type == ((PropertyInfo)mbExpr).PropertyType);
                        if (pItem != null)
                        {
                            retTableInfo = new TableInfo()
                            {
                                name = tables[index]
                            };
                        }
                        else
                        {
                            retTableInfo = new TableInfo()
                            {
                                name = tables[index],
                                alias = mbExpr.Name
                            };
                        }

                        //return (T)((object)("table!" + tables[index]));
                        return (T)retTableInfo;
                    }
                    else
                    {
                        throw new NotImplementedException();
                    }
                }
            }
            if(Expr is NewExpression)
            {
                var val=Expression.Lambda(Expr).Compile().DynamicInvoke();
                object ret = "{" + paramList.Count + "}";
                paramList.Add(ret);
                return (T)ret;
                

                //var nx = Expr as NewExpression;
            }
            throw new NotImplementedException();
        }

        private static int GetIndexOfMember(IList<ParameterExpression> parameters, Expression expression)
        {
            if (parameters == null)
            {
                return -1;
            }
            var ret =0;
            foreach(var x in parameters)
            {
                if(x == expression)
                {
                    return ret;
                }
                ret++;
            }
            return -1;
        }
    }
}
