namespace LV.Db.Mongo
{
    public class GroupInfo<T1,T2>
    {
        
        public T1 Id { get; set; }
        public T2 Selector { get; set; }
    }
}