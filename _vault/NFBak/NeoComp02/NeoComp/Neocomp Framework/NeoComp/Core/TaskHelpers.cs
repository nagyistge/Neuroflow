using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;

namespace NeoComp.Core
{
    //public static class TaskHelpers
    //{
    //    public static bool IsCanceled(Task task)
    //    {
    //        if (task == null) return false;
    //        return task.IsCancellationRequested;
    //    }

    //    public static IEnumerable<T> AsCancelable<T>(this IEnumerable<T> items)
    //    {
    //        items.IsNotNull("items");
    //        var task = Task.Current;
    //        if (task == null)
    //        {
    //            return items;
    //        }
    //        else
    //        {
    //            return AsCancelable(items, task);
    //        }
    //    }

    //    private static IEnumerable<T> AsCancelable<T>(IEnumerable<T> items, Task task)
    //    {
    //        foreach (var item in items)
    //        {
    //            if (task.IsCancellationRequested) yield break;
    //            yield return item;
    //        }
    //    }
    //}
}
