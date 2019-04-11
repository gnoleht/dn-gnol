using MongoDB.Bson;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq.Expressions;
using System.Text;

namespace LV.Db.Mongo
{
    internal class Gobbler
    {
        public const string MultiParamsFuncs = ";concat;substr;";
        public const string SingleParamFunc = ";toLower;";
        internal static BsonDocument GetFields(Expression Expr,bool IsExpr)
        {
            BsonDocument ret = new BsonDocument() ;
           
            if (Expr is NewExpression)
            {
                var nx = Expr as NewExpression;
                var memberIndex = 0;
                foreach (var x in nx.Arguments)
                {
                    var retField = Gobbler.Gobble(x, IsExpr);
                    if (retField is string)
                    {
                        
                        ret.Add(new BsonElement((string)retField, 1));
                    }
                    else if(retField is Hashtable)
                    {
                        if(x is BinaryExpression)
                        {
                            ret.Add(new BsonElement(nx.Members[memberIndex].Name,BsonValue.Create(retField)));
                        }
                        else if(x is MethodCallExpression)
                        {
                            var retCompile = Gobble(x, true);
                            ret.Add(new BsonElement(nx.Members[memberIndex].Name, BsonValue.Create(retCompile)));
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
                    memberIndex++;
                }
            }
            else
            {
                throw new NotImplementedException();
            }
            return ret;
        }

        internal static BsonDocument GetFilter(Expression[] matchExpr)
        {
            var ret = new BsonDocument();
            foreach(var x in matchExpr)
            {
                var retX = GetFilter(x,false);
                if( retX is BsonDocument)
                {
                    ret.Add((BsonDocument)retX);
                }
                else
                {
                    throw new NotImplementedException();
                }
                
                //ret.Add(BsonValue.Create(retX));
            }
            return ret;
        }

        private static object GetFilter(Expression expr,bool IsExpr)
        {
            if(expr is LambdaExpression)
            {
                var Body = ((LambdaExpression)expr).Body;
                return GetFilter(Body, IsExpr);

            }
            if(expr is BinaryExpression)
            {
                var ux = expr as BinaryExpression;
                var op = GetOp(ux.NodeType);
                if (op == "==")
                {
                    var left = GetFilter(ux.Left, IsExpr);
                    var right = GetFilter(ux.Right,true);
                    return new BsonDocument((string)left,(BsonValue)right);
                }
            }
            if(expr is MemberExpression)
            {
                var mb = expr as MemberExpression;
                var ret= mb.Member.Name;
                if (mb.Expression is MemberExpression)
                {
                    ret = (string)GetFilter(mb.Expression,true)+"."+ret;
                }
                if(mb.Expression is BinaryExpression)
                {
                    var bx = mb.Expression as BinaryExpression;
                    if (bx.NodeType == ExpressionType.ArrayIndex)
                    {
                        var left = GetFilter(bx.Left, IsExpr);
                        var index = Expression.Lambda(bx.Right).Compile().DynamicInvoke();
                        return ((string)left) +"." + index + "." + mb.Member.Name;

                    }
                    else
                    {
                        throw new NotImplementedException();
                    }
                }
                return ret;
            }
            if(expr is ConstantExpression)
            {
                return BsonValue.Create(Expression.Lambda(expr).Compile().DynamicInvoke());
            }
            throw new NotImplementedException();
        }

        private static object Gobble(Expression Expr,bool IsExpr)
        {
            if(Expr is MemberExpression)
            {
                var md = Expr as MemberExpression;
                return ((IsExpr)?"$":"")+md.Member.Name;
            }
            if(Expr is BinaryExpression)
            {
                var bx = Expr as BinaryExpression;
                var ret = new Hashtable();
                ret[Gobbler.GetOp(bx.NodeType)] = new object[]
                {
                    Gobble(bx.Left,true),
                    Gobble(bx.Right,true)
                };
                return ret;
            }
            if(Expr is ConstantExpression)
            {
                var val = Expression.Lambda(Expr).Compile().DynamicInvoke();
                return val;
            }
            if(Expr is MethodCallExpression)
            {
                var cx = Expr as MethodCallExpression;
                if(cx.Method.ReflectedType == typeof(Funcs))
                {
                    if (Gobbler.MultiParamsFuncs.IndexOf(";"+cx.Method.Name.ToLower()+";")>-1)
                    {
                        var ret = new Hashtable();
                        ret["$" + cx.Method.Name.ToLower()] = Gobbler.Gobble(cx.Arguments,true);
                        return ret;
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
            throw new NotImplementedException();
        }

        private static object[] Gobble(ReadOnlyCollection<Expression> arguments, bool IsExpr)
        {
            var ret = new List<object>();
            foreach(var x in arguments)
            {
                ret.Add(Gobble(x, IsExpr));
            }
            return ret.ToArray();
        }

        private static string GetOp(ExpressionType nodeType)
        {
            if (nodeType == ExpressionType.Add)
            {
                return "$add";
            }
            if (nodeType == ExpressionType.Equal)
            {
                return "==";
            }
            throw new NotImplementedException();
        }
    }
}
