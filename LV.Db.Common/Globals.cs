using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace LV.Db.Common
{
    /// <summary>
    /// this is a global variable where hold all global variables
    /// </summary>
    public static class Globals
    {
        /// <summary>
        /// Current ConnectionString, you can not change after init
        /// </summary>
        public static string ConnectionString { get; internal set; }
        /// <summary>
        /// Current db type
        /// </summary>
        public static DbTypes DbType { get; internal set; }
        /// <summary>
        /// The type of provider where Database accessing is impletemented
        /// </summary>
        public static TypeInfo ProviderType { get; internal set; }
        /// <summary>
        /// The Assembly of provider where Database accessing is impletemented
        /// </summary>
        public static Assembly ProviderAssembly { get; internal set; }
        /// <summary>
        /// This is switcher before SQL statement was compiler
        /// Exmaple:
        /// PostgreSQL is select "my table".* from "my table"
        /// MySQL is select `my table`.* from `my table`
        /// For the new type of RDBMS you should impletment this Compiler
        /// </summary>
        public static ICompiler Compiler { get; internal set; }
        public static Hashtable TableInfo { get; set; }
    }
}