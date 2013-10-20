using Neuroflow.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Neuroflow
{
    public abstract class VectorUtils
    {
        public abstract void Zero(IDeviceArray deviceArray);

        public abstract void RandomizeUniform(IDeviceArray values, float min, float max);

        public void CalculateMSE(DataArray input, DataArray desiredOutput, DataArray actualOutput, DataArray mseValues, int valueIndex)
        {
            CalculateMSE(new SupervisedSampleEntry(input, desiredOutput, actualOutput), mseValues, valueIndex);
        }

        public void CalculateMSE(SupervisedSampleEntry sampleEntry, DataArray mseValues, int valueIndex)
        {
            Args.Requires(() => sampleEntry, () => sampleEntry != null && sampleEntry.HasOutput);

            CalculateMSE(new SupervisedSample(sampleEntry), mseValues, valueIndex);
        }

        public void CalculateMSE(SupervisedSample sample, DataArray mseValues, int valueIndex)
        {
            CalculateMSE(new SupervisedBatch(sample), mseValues, valueIndex);
        }

        public void CalculateMSE(SupervisedBatch batch, DataArray mseValues, int valueIndex)
        {
            Args.Requires(() => batch, () => batch != null);
            Args.Requires(() => batch, () => batch.Count > 0);
            Args.Requires(() => mseValues, () => mseValues != null);
            Args.Requires(() => valueIndex, () => valueIndex >= 0 && valueIndex < mseValues.Size);

            DoCalculateMSE(batch, mseValues, valueIndex);
        }

        protected abstract void DoCalculateMSE(SupervisedBatch batch, DataArray mseValues, int valueIndex);
    }
}
