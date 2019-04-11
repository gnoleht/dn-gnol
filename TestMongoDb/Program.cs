using LV.Db.Mongo;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Linq;

namespace TestMongoDb
{
    class Person
    {
        public string Name { get; set; }

        public int Age { get; set; }

        public IEnumerable<Pet> Pets { get; set; }

        public int[] FavoriteNumbers { get; set; }

        public HashSet<string> FavoriteNames { get; set; }

        public DateTime CreatedAtUtc { get; set; }
    }

    class Pet
    {
        public string Name { get; set; }
    }
    class Program
    {
        static void Main(string[] args)
        {
            //var client = new MongoClient();
            
            
            var credential = MongoCredential.CreateCredential("lms", "sys", "123456");
            var settings = new MongoClientSettings
            {
                Credentials = new[]
                {
                    credential
                },
                Server=new MongoServerAddress("172.16.7.67",27017)
                {
                    
                }
            };
            var client = new MongoClient(settings);
            var db = client.GetDatabase("lms");
            var apps = db.GetCollection<Apps>("sys.apps");
            var v = apps.Aggregate().Project(p => new
            {
                fullName=(p.views.Count()>0)?"xxx":"CCC"
            });
            //var collection = db.GetCollection<Person>("sys.trackings");
            //var queryable = collection.AsQueryable().Where(p=>p.Age>23|| p.Age<54);
            //object DbConnection = null;
            LV.Db.Mongo.ColQueryable<Apps> coll = new LV.Db.Mongo.ColQueryable<Apps>("sys.apps");
            //var app = new Apps();
            //app.Views = new View[] { };


            //var fx = coll.Match(p => p.views.Any(x=>x.is_public == true));
            //{
            //    p,
            //    Code2 = "1",
            //    Code3 = "B"
            //}).Project(p => new
            //{
            //    p.Fields.FullName,
            //    p.NewFields.Code3,
            //    p.NewFields.Code2
            //});
            //fx.Bucket(p => new { p.FullName }, new object[] { 1, 2, 3 }, p => new
            //{
            //    test=p.Code2
            //});
            //var v = fx.Group(p=>new {
            //    fx=2
            //},p=>new {
            //    c=1
            //});
            //v.Project(p => new
            //{
            //    m =p.Id.fx,
            //    p.Selector.c
            //});

            // m.Project(p => new { p.Code2});
            Console.WriteLine("Hello World!");
        }
    }

    internal class Test
    {
    }
}
