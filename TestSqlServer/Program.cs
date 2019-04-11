using LV.Db.Common;
using System;

namespace TestSqlServer
{
    class Program
    {
        static void Main(string[] args)
        {
            Settings.SetConnectionString(DbTypes.SqlServer, "Server=172.16.7.67;Port=5433;User Id=postgres;Password=123456;Database=lv_openedx;");
            // var x = new Department();
            //var y = new Employee();
            //y.code = "";
            using (Db db = new Db())
            {
                var a = db.Query<Employee>().Filter(o => o.Salary >= 1 && o.code=="ff");
            }

            //var qrEmp = Query.From<Employee>("employee").GroupBy(p => new
            //{
            //    p.code
            //}).Select(p => new
            //{
            //    MaxSalary = SqlFuncs.Call("nnn",p.Salary)
            //});

            //var qrPostion = Query.From<Position>("position");
            //var qrEmpAndDept = Query.Join(qrDep, qrEmp, (emp, dep) => emp.department_code == dep.code, (emp, dep) => new
            //{
            //    emp,
            //    dep

            //});

            //var qrResult = Query.LeftJoin(qrEmpAndDept, qrPostion, (p, q) => p.emp.position_code == q.code,(p,q)=> new {p.dep.code,p.emp.position_code});


            //Console.WriteLine(qrEmp);
        }
    }
}
