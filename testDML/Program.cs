using LV.Db.Common;
using System;

namespace testDML
{
    class Program
    {
        static void Main(string[] args)
        {
            Settings.SetConnectionString(DbTypes.PostgreSQL, "Server=172.16.7.76;Port=5432;User Id=postgres;Password=123456;Database=tamletest;");
            using (var db = new Db())
            {
                //var ret = db.InsertData("account", new
                //{
                //    username = "t1155167",
                //    password = "3421",
                //    email = "t123561677@gmail.com",
                //    created_on = DateTime.Now,
                //    last_login = DateTime.Now
                //});

                //var ret = db.UpdateData("account",
                //    p => p.username == "t22" && p.email == "t22@gmail.com",
                //    new
                //    {
                //        username = "t11551679",
                //        password = "29",
                //        email = "t1235616779@gmail.com",
                //        created_on = DateTime.Now,
                //        last_login = DateTime.Now
                //    });
                //List<Account> accounts = new List<Account>();                

                var ret = db.DeleteData<Account>("", "account", p => p.user_id == 2);



                //var ret = db.InsertData("account_role", new
                //{
                //    user_id = 1,
                //    role_id = 1,
                //    grant_date = DateTime.Now
                //});

                //var ret = db.InsertData("users", new
                //{                    
                //    username = "t1155167",
                //    password = "3421",
                //    Email = "t123561677@gmail.com"                    
                //});

                //if (ret.Error != null)
                //{
                //    // success
                //    if (ret.Error.ErrorType == DataActionErrorTypeEnum.None)
                //    {

                //    }
                //    // error foreign key
                //    if (ret.Error.ErrorType == DataActionErrorTypeEnum.ForeignKey)
                //    {
                //        var fields = ret.Error.Fields;
                //        var tabls = ret.Error.RefTables;
                //    }
                //    // duplicate data for unique fields
                //    if (ret.Error.ErrorType == DataActionErrorTypeEnum.DuplicateData)
                //    {
                //        var fields = ret.Error.Fields;                        
                //    }
                //    // data are null for required fields
                //    if (ret.Error.ErrorType == DataActionErrorTypeEnum.MissingFields)
                //    {
                //        var fields = ret.Error.Fields;
                //    }
                //}

            }
            Console.WriteLine("Hello World!");
        }
    }
}
