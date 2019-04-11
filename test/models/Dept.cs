using LV.Db.Common;
using System;
using System.Collections.Generic;
using System.Text;

namespace test.models
{
    [DataTable("department")]
    public class Dept
    {
        public string DepartmentCode { get; internal set; }
        public string DepartmentName { get; internal set; }
        public string ParentCode { get; set; }
    }
}
