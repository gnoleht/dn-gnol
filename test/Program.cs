using LV.Db.Common;
using System;
using System.Dynamic;
using test.models;

namespace test
{
    class Program
    {
        
        static void Main(string[] args)
        {
            
            Settings.SetConnectionString(DbTypes.PostgreSQL, "Server=172.16.7.76;Port=5432;User Id=postgres;Password=123456;Database=hrm;");
            //Settings.SetConnectionString(DbTypes.SqlServer, "Server=172.16.7.76;User Id=sa;Password=l@cv!et2019;Database=lv_openedx;");
            using (var db = new Db().WithSchema("hr"))
            {
                //var ret = db.InsertData<Emp>("hr", "Employees", new 
                //{
                //    EmployeeCode="XXX",
                //    FirstName="Julia",
                //    LastName="More"
                //},true);
                var ret = db.DeleteData<Emp>("hr", "Employees", p => p.EmployeeCode == GetValue());
                //var ret = db.Query<Emp>().InsertItem(db, new
                //{
                //    EmployeeCode = "NV009",
                //    FirstName = "xxxxccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccc",
                //    //LastName="XXXXXXX"

                //}, false);
                //db.UpdateData()

                //var items = db.Query<Emp>().Skip(10);
                //var ret = db.InsertData<Emp>(new Emp()
                //{

                //});
                Console.WriteLine(ret);
                //var mx = db.FromNothing(p => new
                //{
                //    Test = (((1 + 12) * 8) / 5) % 11,
                //    Code = " " + "123"
                //});
                //var ret=mx.ToDataTable(db);
                //var mx = Query.FromNothing(p=>new {
                //    Test=(((1+12)*8)/5)%11
                //});
                //Console.WriteLine(mx);
                //var qrEmp = Query.FromEntity<Emp>("hr").Filter(p=>p.Salary==1986759.22);
                //var qr1 = Query.FromEntity<Dept>();
                //var qrDept = Query.FromEntity<Dept>("hr").LeftJoin(Query.FromEntity<Dept>(), (p, q) => p.ParentCode==q.DepartmentCode, (p, q) => new {
                //    dep1=p,
                //    dep2=q
                //});
                //Console.WriteLine(qrDept.ToString());
                //Console.WriteLine(qrDept.ToString());
                //var qr = qrEmp.Join(qrDept, (p, q) =>p.DepartmentCode==q.DepartmentCode ,(emp,dept)=> new { emp,dept });
                //Console.WriteLine(qr.Select(p=>new {
                //    p.dept.DepartmentName,
                //    FullName=p.emp.FirstName+" "+p.emp.LastName
                //}));


                Console.ReadKey();
            }
        }

        private static string GetValue()
        {
            return "XX";
        }

        private static string GetUser()
        {
            return "123";
        }

        private static void test(Func<object, object> p)
        {
            throw new NotImplementedException();
        }
    }

    
}
