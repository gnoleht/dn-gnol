using System;
using System.Collections.Generic;
using System.Text;

namespace LV.Db.Common
{
    public class DataTableAttribute : Attribute
    {
        internal string tableName;

        public DataTableAttribute(string tableName)
        {
            this.tableName = tableName;
        }
    }
}
