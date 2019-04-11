using LV.Db.Common;
using System;

namespace testSelect
{
    class Program
    {
        static void Main(string[] args)
        {
            var HashPassword = "123wsdkdfl";
            Settings.SetConnectionString(DbTypes.PostgreSQL, "");
            var selectFromWhere = QueryBuilder.From<Users>("user").Filter(p => p.Username == "root" && p.Password==HashPassword);
            var selectFromSubQuery = QueryBuilder.From<Users>("user")
                .Filter(p => p.Username == "root" && p.Password == HashPassword).Select(p => new
                {
                    Log=p.LastName+" "+p.FirstName,
                    LoginCount2=p.LoginCount
                });
            //Console.WriteLine(selectFromWhere);
            Console.WriteLine("------------------------------------");
            Console.WriteLine(selectFromSubQuery);
            Console.ReadKey();
        }
    }
}
