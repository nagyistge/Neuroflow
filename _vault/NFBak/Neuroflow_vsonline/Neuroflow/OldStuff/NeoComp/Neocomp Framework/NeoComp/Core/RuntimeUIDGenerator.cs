using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NeoComp.Core
{
    public sealed class RuntimeUIDGenerator
    {
        public RuntimeUIDGenerator() : this(false) { }
        
        public RuntimeUIDGenerator(bool synchronized)
        {
            this.synchronized = synchronized;
        }
        
        bool synchronized;

        object sync;

        private object Sync
        {
            get { return sync ?? (sync = new object()); }
        }

        uint current;

        public uint Next()
        {
            if (synchronized) return NextSync();
            return current++;
        }

        private uint NextSync()
        {
            lock (Sync)
            {
                return current++;
            }
        }
    }
}
