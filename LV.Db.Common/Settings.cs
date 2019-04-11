using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LV.Db.Common
{
    public class Settings
    {
        const string DLLPATH = "LV.Db.{0}.dll";
        public static void SetConnectionString(DbTypes DbType,string ConnectionString)
        {
            Globals.DbType = DbType;
            Globals.ConnectionString = ConnectionString;
            if (Globals.ProviderType == null)
            {
                var location = System.Reflection.Assembly.GetEntryAssembly().Location;
                var directory = System.IO.Path.GetDirectoryName(location);
                Globals.ProviderAssembly = utils.LoadFile(directory, string.Format(DLLPATH, Globals.DbType));
                Globals.ProviderType = Globals.ProviderAssembly.DefinedTypes.First(p => p.Name == "Database");
                Globals.Compiler = Globals.ProviderAssembly.CreateInstance(Globals.ProviderAssembly.DefinedTypes.First(p => p.Name == "Compiler").FullName) as ICompiler;
                //var ret = Assembly.Load(string.Format("LV.Db.{0}", Globals.DbType));

            }
        }
    }
}