using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Neuroflow.Data
{
    public sealed class SupervisedSampleEntry : IDisposable
    {
        public SupervisedSampleEntry(DataArray input)
        {
            Args.Requires(() => input, () => input != null);

            Input = input;
        }

        public SupervisedSampleEntry(DataArray input, DataArray desiredOutput, DataArray actualOutput)
        {
            Args.Requires(() => input, () => input != null);
            Args.Requires(() => desiredOutput, () => desiredOutput != null);
            Args.Requires(() => actualOutput, () => actualOutput != null);
            Args.Requires(() => actualOutput, () => desiredOutput.Size == actualOutput.Size);

            Input = input;
            DesiredOutput = desiredOutput;
            ActualOutput = actualOutput;
        }

        public DataArray Input { get; private set; }

        public DataArray DesiredOutput { get; private set; }

        public DataArray ActualOutput { get; private set; }

        public bool HasOutput
        {
            get { return DesiredOutput != null && ActualOutput != null; }
        }

        bool disposed;

        public void Dispose()
        {
            if (!disposed)
            {
                disposed = true;

                Input.Dispose();
                if (DesiredOutput != null) DesiredOutput.Dispose();
                if (ActualOutput != null) ActualOutput.Dispose();

                GC.SuppressFinalize(this);
            }
        }
    }
}
