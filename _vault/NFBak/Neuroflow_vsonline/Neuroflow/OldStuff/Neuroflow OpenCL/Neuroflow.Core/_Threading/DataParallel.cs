using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using System.Diagnostics;

namespace Neuroflow.Core
{
    public struct DataParallelContext
    {
        public DataParallelContext(IntRange workItemRange, int workItemsCount)
        {
            this.workItemRange = workItemRange;
            this.workItemsCount = workItemsCount;
        }
        
        IntRange workItemRange;

        public IntRange WorkItemRange
        {
            get { return workItemRange; }
        }

        int workItemsCount;

        public int WorkItemsCount
        {
            get { return workItemsCount; }
        }
    }
    
    public static class DataParallel
    {
        static readonly int procCount = Environment.ProcessorCount;

        public static void Do(int workItemsCount, bool parallelize, Action<DataParallelContext> workItemRangeProcessor)
        {
            if (workItemsCount <= 0 || workItemRangeProcessor == null) return;

            if (!parallelize)
            {
                workItemRangeProcessor(new DataParallelContext(IntRange.CreateExclusive(0, workItemsCount), workItemsCount));
                return;
            }

            if (workItemsCount == 1)
            {
                workItemRangeProcessor(new DataParallelContext(IntRange.CreateFixed(0), workItemsCount));
            } 
            else if (workItemsCount < procCount)
            {
                Parallel.For(0, workItemsCount, (i) =>
                {
                    workItemRangeProcessor(new DataParallelContext(IntRange.CreateFixed(i), workItemsCount));
                });
            }
            else
            {
                int workSize = workItemsCount / procCount;
                int mod = workItemsCount % procCount;

                if (mod == 0)
                {
                    Parallel.For(0, procCount, (i) =>
                    {
                        int begin = i * workSize;
                        workItemRangeProcessor(new DataParallelContext(IntRange.CreateExclusive(begin, begin + workSize), workItemsCount));
                    });
                }
                else
                {
                    int to = 0;
                    var spinLock = new SpinLock(false);
                    Parallel.For(0, procCount, (i) =>
                    {
                        int begin = to;
                        bool lockTaken = false;
                        try
                        {
                            spinLock.Enter(ref lockTaken);
                            if (mod != 0)
                            {
                                to += workSize + 1;
                                mod--;
                            }
                            else
                            {
                                to += workSize;
                            }
                        }
                        finally
                        {
                            if (lockTaken) spinLock.Exit();
                        }
                        workItemRangeProcessor(new DataParallelContext(IntRange.CreateExclusive(begin, to), workItemsCount));
                    });
                }
            }
        }
    }
}
