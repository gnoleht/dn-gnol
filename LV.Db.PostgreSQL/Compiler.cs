using LV.Db.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LV.Db.PostgreSQL
{
    public class Compiler : ICompiler
    {
        public string CreateAliasTable(string tableName, string alias)
        {
            return string.Format(@"{0} as {1}", tableName, alias);
        }

        public string CreateSQLGetIdentity(string schema, string table_name, TableInfo tbl)
        {
            var autoFields = tbl.Columns.Where(p => p.IsAutoIncrement).Select(p => p.Name);
            if (autoFields.Count() == 0)
            {
                return "";
            }
            var sqlReturn = "select " + string.Join(",", autoFields.Select(p => @"max(""" + p + @""") as """ + p + @"""").ToArray())+ " from "+this.GetQName(schema,table_name);
            return sqlReturn;
        }

        public string GetFieldName(string schema, string table_name, string name)
        {
            if (string.IsNullOrEmpty(schema))
            {
                return string.Format(@"""{0}"".""{1}""", table_name, name);
            }
            else
            {
                return string.Format(@"""{0}"".""{1}"".""{2}""",schema, table_name, name);
            }
        }

       

        public string GetQName(string schema, string name)
        {
            if (string.IsNullOrEmpty(schema))
            {
                return @"""" + name + @"""";
            }
            else
            {
                return string.Format( @"""{0}"".""{1}""",schema,name);
            }
            
        }

        public string GetQName(string tableName)
        {
            return GetQName("", tableName);
        }

        public string GetSqlFunction(string name)
        {
            if(name == "Len")
            {
                return "length({0})";
            }
            if (name == "Month")
            {
                return "EXTRACT(MONTH FROM {0})";
            }
            if (name == "Sum")
            {
                return "sum";
            }
            if (name == "Min")
            {
                return "min";
            }
            if (name == "Max")
            {
                return "max";
            }
            if (name == "When")
            {
                return "when";
            }
            if(name== "IsNull")
            {
                return "COALESCE";
            }
            throw new NotImplementedException();
        }

        public string MakeAlias(string field, string alias)
        {
            return field + @" as """ + alias + @"""";
        }

        public string MakeLimitRow(string sql, List<SortingInfo> sortList, List<string> fields, int selectTop)
        {
            return sql = sql + " limit " + selectTop;
        }
    }
}
