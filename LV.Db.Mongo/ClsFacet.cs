namespace LV.Db.Mongo
{
    public class ClsFacet<T>
    {
        private ColQueryable<T> qr;

        public ClsFacet(ColQueryable<T> qr)
        {
            this.qr = qr;
        }
    }
}