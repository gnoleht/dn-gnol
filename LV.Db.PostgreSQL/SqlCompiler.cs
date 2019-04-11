//using LV.Db.Common;
//using System;
//using System.Collections.Generic;
//using System.Linq.Expressions;
//using System.Text;

//namespace LV.Db.PostgreSQL
//{
//    public class SqlCompiler : ISqlCompiler
//    {
//        public string GetFieldInfo(object field,CompileState State)
//        {
//            if(field is string)
//            {
//                return string.Format(@"""{0}""",field.ToString());
//            }
//            else if(field is StrExpression)
//            {
//                var x = (StrExpression)field;
               
//                return GetFieldInfo(x.Left, State) +" " + x.Operator + " "+ GetFieldInfo(x.Right, State);
//            }
//            else if(field is ConstantExpression)
//            {
//                var ret = "{" + State.ParamList.Count.ToString() + "}";
//                State.ParamList.Add(((ConstantExpression)field).Value);
//                return ret;
//            }
           
//            return "Error";
//        }

//        public string GetSelectedFields(List<object> fields)
//        {
//            var ret = "";
//            var f = new List<string>();
//            var State = new CompileState();
//            foreach (var item in fields)
//            {
//                if(item is string){
//                    f.Add((string)item);
//                }
//                else if(item is StrExpression)
//                {
//                    f.Add(this.GetFieldInfo(item, State));
//                }
//            }
//            return string.Join(",", f.ToArray());
           
//        }
//    }
//}
