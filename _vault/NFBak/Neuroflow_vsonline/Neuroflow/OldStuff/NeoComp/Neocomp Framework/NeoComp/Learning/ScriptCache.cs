using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NeoComp.Caching;

namespace NeoComp.Learning
{
    public static class ScriptCache
    {
        static ScriptCache()
        {
            cache = new ASPNETCache();
        }
        
        static object sync = new object();

        static ICache cache;

        public static ICache Cache
        {
            get { lock (sync) return cache; }
            set { lock (sync) cache = value; }
        }
    }
}
