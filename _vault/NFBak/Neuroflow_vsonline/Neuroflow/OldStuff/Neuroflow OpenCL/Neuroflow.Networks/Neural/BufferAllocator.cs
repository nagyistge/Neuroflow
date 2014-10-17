using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics.Contracts;
using Neuroflow.Core;

namespace Neuroflow.Networks.Neural
{
    public class BufferAllocator
    {
        int index;

        public int Size
        {
            get { return index; }
        }

        public IntRange Alloc(int count)
        {
            Contract.Requires(count > 0);

            var r = IntRange.CreateExclusive(index, index + count);
            index += count;
            return r;
        }
    }
}
