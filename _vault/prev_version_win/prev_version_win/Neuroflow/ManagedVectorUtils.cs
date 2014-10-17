using Neuroflow.Data;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Neuroflow
{
    unsafe internal class ManagedVectorUtils : VectorUtils
    {
        public override void RandomizeUniform(IDeviceArray values, float min, float max)
        {
            Args.Requires(() => values, () => values != null);

            var array = ((ManagedArray)values).InternalArray;
            for (int i = 0; i < array.Length; i++)
            {
                array[i] = RandomGenerator.NextFloat(min, max);
            }
        }

        protected override void DoCalculateMSE(SupervisedBatch batch, DataArray mseValues, int valueIndex)
        {
            var mseA = mseValues.ToManaged();
            fixed (float* pMseA = mseA.InternalArray)
            {
                var msePtr = mseA.ToPtr(pMseA);

                msePtr[valueIndex] = 0.0f;
                float count = 0.0f;
                foreach (var sample in batch)
                {
                    foreach (var entry in sample)
                    {
                        if (entry.HasOutput)
                        {
                            var actualOutputs = entry.ActualOutput.ToManaged();
                            var desiredOutputs = entry.DesiredOutput.ToManaged();
                            fixed (float* pAO = actualOutputs.InternalArray, pDO = desiredOutputs.InternalArray)
                            {
                                var aoPtr = actualOutputs.ToPtr(pAO);
                                var doPtr = desiredOutputs.ToPtr(pDO);
                                float cMse = 0.0f;

                                for (int x = 0; x < actualOutputs.Size; x++)
                                {
                                    float error = (doPtr[x] - aoPtr[x]) * 0.5f;
                                    cMse += error * error;
                                }
                                msePtr[valueIndex] += cMse / (float)actualOutputs.Size;

                                count++;
                            }
                        }
                    }
                }

                if (count != 0.0f) msePtr[valueIndex] /= count;
            }
        }

        override public void Zero(IDeviceArray deviceArray)
        {
            var a = deviceArray.ToManaged().InternalArray;
            ZeroMemory(a);
        }

        unsafe internal static void ZeroMemory(float[] array)
        {
            Debug.Assert(array != null);
            fixed (float* p = array)
            {
                Rtl.ZeroMemory(new IntPtr(p), new IntPtr(sizeof(float) * array.Length));
            }
        }
    }
}
