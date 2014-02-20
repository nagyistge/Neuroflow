using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics.Contracts;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;

namespace NeoComp.Core
{
    internal static class CloneHelper
    {
        internal static T Clone<T>(T obj) where T : class
        {
            Contract.Requires(obj != null);

            try
            {
                using (var ms = new MemoryStream())
                {
                    var bf = new BinaryFormatter();
                    bf.Serialize(ms, obj);
                    ms.Seek(0, SeekOrigin.Begin);
                    return (T)bf.Deserialize(ms);
                }
            }
            catch
            {
                throw new InvalidOperationException(obj + " is not serializable.");
            }
        }
    }
}
