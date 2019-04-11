using System;

namespace testDML
{
    internal class Account
    {
        public int user_id { get; internal set; }
        public string username { get; internal set; }
        public string password { get; internal set; }
        public string email { get; internal set; }
        public DateTime created_on { get; internal set; }
        public DateTime last_login { get; internal set; }
    }
}
