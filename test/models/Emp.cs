using LV.Db.Common;
using System;
using System.Collections.Generic;
using System.Text;

namespace test.models
{
    [DataTable("Employees")]
    public class Emp
    {
        public int id { get; set; }
        public string EmployeeCode { get;  set; }
        public string DepartmentCode { get;  set; }
        public string LastName { get;  set; }
        public string FirstName { get;  set; }
        public double Salary { get;  set; }
        public DateTime Created { get; internal set; }

        public Emp()
        {

        }
    }
}
