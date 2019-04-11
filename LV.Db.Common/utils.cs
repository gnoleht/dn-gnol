using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;

namespace LV.Db.Common
{
    internal class utils
    {
        static string[] Operators = new string[]
        {
            "Add"
        };
        static string[] OperatorMapping = new string[]
        {
            "+"
        };
        /// <summary>
        /// This method Load Assembly from dll_file in directory
        /// </summary>
        /// <param name="directory">Absolute path to directory where contains dll_file</param>
        /// <param name="dll_file">dll_file</param>
        /// <returns></returns>
        internal static System.Reflection.Assembly LoadFile(string directory, string dll_file)
        {
            System.Reflection.Assembly ass = null;

#if NET_CORE
            // Requires nuget - System.Runtime.Loader
            ass = System.Runtime.Loader.AssemblyLoadContext.Default
                   .LoadFromAssemblyPath(Path.Combine(directory,dll_file);
#else
            ass = System.Reflection.Assembly.LoadFile(directory+"\\"+dll_file);
#endif 
            // System.Type myType = ass.GetType("Custom.Thing.SampleClass");
            // object myInstance = Activator.CreateInstance(myType);
            return ass;
        }

        internal static string GetOp(ExpressionType NodeType)
        {
            if (NodeType == ExpressionType.Add)
            {
                return "+";
            }
            if (NodeType == ExpressionType.Multiply)
            {
                return "*";
            }
            if (NodeType == ExpressionType.Equal)
            {
                return "=";
            }
            if (NodeType == ExpressionType.NotEqual)
            {
                return "!=";
            }
            if (NodeType == ExpressionType.AndAlso)
            {
                return "and";
            }
            if (NodeType == ExpressionType.OrElse)
            {
                return "or";
            }
            if (NodeType == ExpressionType.GreaterThan)
            {
                return ">";
            }
            if (NodeType == ExpressionType.GreaterThanOrEqual)
            {
                return ">=";
            }
            if (NodeType == ExpressionType.LessThan)
            {
                return "<";
            }
            if (NodeType == ExpressionType.LessThanOrEqual)
            {
                return "<=";
            }
            throw new NotImplementedException();
        }

        internal static string RepairParameters(string txt, List<object> params1, List<object> params2)
        {
            var ret = txt;
            var index = 0;
            foreach(var p in params2)
            {
                var oldIndex = index;
                var newIndex = params1.Count + index;
                ret = ret.Replace("{" + oldIndex + "}", "{$##" + newIndex + "##$}");
                index++;
            }
            return ret.Replace("{$##", "{").Replace("##$}", "}");
        }

        internal static string GetSource<T1>(QuerySet<T1> q1)
        {
            if (!string.IsNullOrEmpty(q1.datasource))
            {
                return q1.datasource;
            }
            if (q1.fields.Count > 0 || (!string.IsNullOrEmpty(q1.where)))
            {
                return "(" + q1.ToSQLString() + ")";
            }
            else
            {
                return Globals.Compiler.GetQName(q1.Schema, q1.table_name);
            }
            throw new NotImplementedException();
        }
    }
}
