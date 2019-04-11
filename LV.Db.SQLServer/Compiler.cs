using LV.Db.Common;
using System;
using System.Collections.Generic;
using System.Text;

namespace LV.Db.SQLServer
{
    public class Compiler : ICompiler
    {
        public string CreateAliasTable(string tableName, string alias)
        {
            return string.Format(@"{0} as {1}", tableName, alias);
        }

        public string CreateSQLGetIdentity(string schema, string table_name, TableInfo tbl)
        {
            throw new NotImplementedException();
        }

        public string GetFieldName(string schema, string table_name, string name)
        {
            if (string.IsNullOrEmpty(schema))
            {
                return string.Format(@"[{0}].[{1}]", table_name, name);
            }
            else
            {
                return string.Format(@"[{0}].[{1}].[{2}]",schema, table_name, name);
            }
            
        }

        

        public string GetQName(string schema, string name)
        {
            if (string.IsNullOrEmpty(schema))
            {
                return @"[" + name + @"]";
            }
            else
            {
                return string.Format(@"[{0}].[{1}]", schema, name);
            }
        }

        public string GetQName(string tableName)
        {
            return @"[" + tableName + @"]";
        }

        public string GetSqlFunction(string name)
        {
            if(name == "Len")
            {
                return "Len({0})";
            }
            if (name == "Month")
            {
                return "Month({0})";
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
                return "IsNull";
            }
            throw new NotImplementedException();
        }

        public string MakeAlias(string field, string alias)
        {
            return field + @" as """ + alias + @"""";
        }

        public string MakeLimitRow(string sql, List<SortingInfo> sortList, List<string> fields, int selectTop)
        {
            throw new NotImplementedException();
        }
    }
}
