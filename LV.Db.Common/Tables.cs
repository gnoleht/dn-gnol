using System;
using System.Collections.Generic;
using System.Text;

namespace LV.Db.Common
{
    public class Tables
    {
        public Table this[string schema, string name]
        {
            get
            {
                return new Table(schema,name);
            }
                
        }
    }
}
