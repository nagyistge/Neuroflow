using Neuroflow.Core;
using Neuroflow.Core.Vectors;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Neuroflow.Networks.Neural
{
    public interface IBufferOps
    {
        void WriteVector(NeuralComputationContext context, IntRange range, BufferedVector<float> values);

        void ReadArray(NeuralComputationContext context, IntRange range, float[] values);

        void Zero(NeuralComputationContext context, IntRange range);

        void ZeroAll(NeuralComputationContext context);

        void CopyBuffer(NeuralComputationContext context, IntRange source, IntRange target);

        void ComputeError(NeuralComputationContext context, IntRange outputBuffer, BufferedVector<float> desiredOutputVector, IntRange errorBuffer, IntRange accumulationBuffer);

        void CalculateAverageError(NeuralComputationContext context, IntRange accumulationBuffer);

        void Average(NeuralComputationContext context, int outputIndex, IntRange range);

        void AverageDist(NeuralComputationContext context, int outputIndex, IntRange range);
    }
}
