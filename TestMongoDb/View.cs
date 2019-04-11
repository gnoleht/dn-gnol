namespace TestMongoDb
{
    public class View
    {
        public bool is_public { get; internal set; }
        public string[] privileges { get; set; }
        public string name { get; set; }
        public string[] roles { get; set; }
        public string path { get; set; }
       
    }
}