using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Input;

namespace Neuroflow.Design.MVVM
{
    public sealed class HourGlass : IDisposable
    {
        Cursor currentCursor;

        public HourGlass()
        {
            currentCursor = Mouse.OverrideCursor;
            Mouse.OverrideCursor = Cursors.Wait;
        }

        public void Dispose()
        {
            Mouse.OverrideCursor = currentCursor;
        }
    }
}
