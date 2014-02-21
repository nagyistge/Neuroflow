using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics.Contracts;

namespace NeoComp.Evolution.Statistical
{
    public sealed class EnumOptUnit<T> : SetOptUnit<T>
        where T : struct
    {
        static EnumOptUnit()
        {
            if (!typeof(T).IsEnum) throw new InvalidOperationException(typeof(T).FullName + " is not an enum type.");
        }

        static HashSet<T> GetValues()
        {
            return new HashSet<T>(Enum.GetValues(typeof(T)).Cast<T>());
        }

        public EnumOptUnit(string id, int resolution)
            : base(id, resolution, GetValues())
        {
            Contract.Requires(!string.IsNullOrEmpty(id));
            Contract.Requires(resolution > 1);
        }
    }
}
