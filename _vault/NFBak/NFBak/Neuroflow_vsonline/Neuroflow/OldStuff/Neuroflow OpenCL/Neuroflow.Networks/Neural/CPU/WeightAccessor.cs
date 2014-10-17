using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Neuroflow.Networks.Neural.CPU
{
    public static class WeightAccessor
    {
        const float MaxWeigh = 4.0f, MinWeight = -4.0f;

        public unsafe static int GetWeightValueIndex(int upperValueIndex, int lowerValueIndex, int lowerLayerSize)
        {
            return upperValueIndex * lowerLayerSize + lowerValueIndex;
        }

        //public unsafe static void UpdateWeight(float* valueBuffer, int weightValueIndex, float update)
        //{
        //    valueBuffer[weightValueIndex] += update;

        //    //float w = valueBuffer[weightValueIndex] + update;
        //    //if (w < MinWeight) w = MinWeight; else if (w > MaxWeigh) w = MaxWeigh;
        //    //valueBuffer[weightValueIndex] = w;
        //}

        /*public unsafe static float GetWeight(float* valueBuffer, int weightValueBeginIndex, int upperValueIndex, int lowerValueIndex, int lowerLayerSize)
        {
            return valueBuffer[weightValueBeginIndex + (upperValueIndex * lowerLayerSize + lowerValueIndex)];
        }

        public unsafe static void SetWeight(float* valueBuffer, int weightValueBeginIndex, int upperValueIndex, int lowerValueIndex, int lowerLayerSize, float value)
        {
            valueBuffer[weightValueBeginIndex + (upperValueIndex * lowerLayerSize + lowerValueIndex)] = value;
        }

        public unsafe static void AddToWeight(float* valueBuffer, int weightValueBeginIndex, int upperValueIndex, int lowerValueIndex, int lowerLayerSize, float value)
        {
            valueBuffer[weightValueBeginIndex + (upperValueIndex * lowerLayerSize + lowerValueIndex)] += value;
        }

        public unsafe static float AddToWeightRet(float* valueBuffer, int weightValueBeginIndex, int upperValueIndex, int lowerValueIndex, int lowerLayerSize, float value)
        {
            return valueBuffer[weightValueBeginIndex + (upperValueIndex * lowerLayerSize + lowerValueIndex)] += value;
        }

        */
    }
}
