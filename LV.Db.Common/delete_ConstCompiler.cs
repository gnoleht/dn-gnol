//using System;
//using System.Collections;
//using System.Collections.Generic;
//using System.Linq;
//using System.Linq.Expressions;
//using System.Reflection;
//using System.Text;

//namespace LV.Db.Common
//{
//    internal class ConstCompiler
//    {
       

//        internal static object GetValue(MemberExpression mb)
//        {

//            if(mb.Expression is ConstantExpression)
//            {
//                return GetValue((ConstantExpression)mb.Expression, mb.Member);
//            }
//            if(mb.Expression is MemberExpression)
//            {
//                var ret = GetValue((MemberExpression)mb.Expression);
//                if (mb.Member is System.Reflection.FieldInfo)
//                {
//                    System.Reflection.FieldInfo F = mb.Member as System.Reflection.FieldInfo;
//                    ret = F.GetValue(ret);
//                }
//                if (mb.Member is System.Reflection.PropertyInfo)
//                {
//                    System.Reflection.PropertyInfo F = mb.Member as System.Reflection.PropertyInfo;
//                    ret = F.GetValue(ret);
//                }
//                return ret;
//            }
//            if(mb.Expression is ParameterExpression)
//            {
//                var px = mb.Expression as ParameterExpression;
//                return new DataFieldMember()
//                {
//                    Field_Type= mb.Member.ReflectedType,
//                    Field_Name = mb.Member.Name
//                };
//            }
//            if(mb.Expression is MethodCallExpression)
//            {
//                var ret= Expression.Lambda(mb.Expression).Compile().DynamicInvoke();
//                if (mb.Member is System.Reflection.FieldInfo)
//                {
//                    System.Reflection.FieldInfo F = mb.Member as System.Reflection.FieldInfo;
//                    ret = F.GetValue(ret);
//                }
//                if (mb.Member is System.Reflection.PropertyInfo)
//                {
//                    System.Reflection.PropertyInfo F = mb.Member as System.Reflection.PropertyInfo;
//                    ret = F.GetValue(ret);
//                }
//                return ret;
//            }
          
//            throw new NotImplementedException();
//        }

//        private static object GetValue(ConstantExpression Expr, MemberInfo member)
//        {
//            Type t = Expr.Value.GetType();
//            var x = t.InvokeMember(member.Name, BindingFlags.GetField,
//                                   null, Expr.Value, null);

//            return x;
//        }

//        internal static object GetValue(MethodCallExpression mx)
//        {
//            throw new NotImplementedException();
//        }
//    }
//}
