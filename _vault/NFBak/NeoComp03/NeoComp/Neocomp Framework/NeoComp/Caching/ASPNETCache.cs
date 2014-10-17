using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;
using System.Diagnostics.Contracts;
using System.Web.Caching;

namespace NeoComp.Caching
{
    public sealed class ASPNETCache : ICache
    {
        #region ASP.NET Cache Init

        static ASPNETCache() { }

        private static readonly object initSync = new object();

        private static HttpRuntime httpRuntime;

        internal static Cache Cache
        {
            get
            {
                EnsureHttpRuntime();
                return HttpRuntime.Cache;
            }
        }

        private static void EnsureHttpRuntime()
        {
            if (httpRuntime == null)
            {
                lock (initSync)
                {
                    if (httpRuntime == null) httpRuntime = new HttpRuntime();
                }
            }
        } 

        #endregion

        #region Constructor

        public ASPNETCache()
            : this(TimeSpan.FromHours(1), CacheItemPriority.Default)
        {
        }

        public ASPNETCache(TimeSpan expiration, CacheItemPriority priority)
        {
            this.expiration = expiration;
            this.priority = priority;
        }

        #endregion

        #region Fields

        TimeSpan expiration;

        CacheItemPriority priority;

        #endregion

        #region Impl

        public object this[string key]
        {
            get { return Cache[key]; }
        }

        public void Add(string key, object obj)
        {
            Cache.Add(key, obj, null, Cache.NoAbsoluteExpiration, expiration, priority, null);
        }

        public void Remove(string key)
        {
            Cache.Remove(key);
        }

        #endregion
    }
}
