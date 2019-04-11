using System;
using System.Collections.Generic;
using System.Text;
using MongoDB.Bson;
namespace TestMongoDb
{
    public class Apps
    {
        public object _id { get; set; }
        
        public View[] views { get; set; }
        public string name { get; set; }
        public string[] schemas { get; set; }
    }
}
