using LV.Db.Common;

namespace TestSqlServer
{
    [DataTable("Employee")]
    internal class Employee
    {
        public string position_code { get; internal set; }
        public string code { get; internal set; }
        public int Salary { get; internal set; }
    }
}