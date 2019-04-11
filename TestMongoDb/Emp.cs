namespace TestMongoDb
{
    internal class Emp
    {
        public string Code { get; internal set; }
        public string Name { get; internal set; }
        public string FirstName { get; internal set; }
        public string LastName { get; internal set; }
        public Dept Dep { get; internal set; }
    }
}