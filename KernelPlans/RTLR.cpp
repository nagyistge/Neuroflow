kernel void ComputeGradientsRTLR_V0_CPU(
    global float* pValuesOfWeights
    , int uLayersCount
    , int maxULayerSize
    , int p_i_j_l_LayerIndex_0_0
    , int p_i_j_l_LayerSize_0_0
    , global float$* weights_0_0
    , int p_i_j_l_LayerIndex_1_0
    , int p_i_j_l_LayerSize_1_0
    , global float$* weights_1_0
    , int p_i_j_l_LayerIndex_2_0
    , int p_i_j_l_LayerSize_2_0
    , global float$* weights_2_0
    , int p_i_j_l_LayerIndex_3_0
    , int p_i_j_l_LayerSize_3_0
    , global float$* weights_3_0
    , int p_i_j_k_LayerSize_0
    , global float* netDerivValues_0
    , int p_i_j_l_LayerIndex_0_1
    , int p_i_j_l_LayerSize_0_1
    , global float$* weights_0_1
    , int p_i_j_l_LayerIndex_1_1
    , int p_i_j_l_LayerSize_1_1
    , global float$* weights_1_1
    , int p_i_j_l_LayerIndex_2_1
    , int p_i_j_l_LayerSize_2_1
    , global float$* weights_2_1
    , int p_i_j_l_LayerIndex_3_1
    , int p_i_j_l_LayerSize_3_1
    , global float$* weights_3_1
    , int p_i_j_k_LayerSize_1
    , global float* netDerivValues_1
    , int p_i_j_l_LayerIndex_0_2
    , int p_i_j_l_LayerSize_0_2
    , global float$* weights_0_2
    , int p_i_j_l_LayerIndex_1_2
    , int p_i_j_l_LayerSize_1_2
    , global float$* weights_1_2
    , int p_i_j_l_LayerIndex_2_2
    , int p_i_j_l_LayerSize_2_2
    , global float$* weights_2_2
    , int p_i_j_l_LayerIndex_3_2
    , int p_i_j_l_LayerSize_3_2
    , global float$* weights_3_2
    , int p_i_j_k_LayerSize_2
    , global float* netDerivValues_2
    , int p_i_j_l_LayerIndex_0_3
    , int p_i_j_l_LayerSize_0_3
    , global float$* weights_0_3
    , int p_i_j_l_LayerIndex_1_3
    , int p_i_j_l_LayerSize_1_3
    , global float$* weights_1_3
    , int p_i_j_l_LayerIndex_2_3
    , int p_i_j_l_LayerSize_2_3
    , global float$* weights_2_3
    , int p_i_j_l_LayerIndex_3_3
    , int p_i_j_l_LayerSize_3_3
    , global float$* weights_3_3
    , int p_i_j_k_LayerSize_3
    , global float* netDerivValues_3
    , int iLayerIndex
    , global float* inputs
    , int inputsSize // + bias (null) = 1, inputs: size
    , global float* outputs
    , global float* desiredOutputs
    , local float* tmpGradients // size = local size
    , local float* tmpSums // size = maxULayerSize * local size 2
    , global float* gradients
    , global float* gradientSums)
{
    int localId = get_local_id(0);
    int localSize = get_local_size(0);
    int localId1 = get_local_id(1);
    int localSize1 = get_local_size(1);

    int ijValueIndex = get_group_id(0);
    int iValueIndex = ijValueIndex / inputsSize;
    int jValueIndex = ijValueIndex % inputsSize;

    if (localId1 == 0)
    {
        tmpGradients[localId] = 0.0f;
        barrier(CLK_LOCAL_MEM_FENCE);
    }

    int pValuesOfWeightsSize2 = uLayersCount * maxULayerSize;
    int block = pValuesOfWeightsSize2 / localSize + (pValuesOfWeightsSize2 % localSize != 0 ? 1 : 0);
    int kLayerAndValueIndex = localId * block;
    int max = kLayerAndValueIndex + block;
    if (max > pValuesOfWeightsSize2) max = pValuesOfWeightsSize2;
    while (kLayerAndValueIndex < max)
    {
        int kLayerIndex = kLayerAndValueIndex / maxULayerSize;
        int kValueIndex = kLayerAndValueIndex % maxULayerSize;
        int kLayerSize = PickIntValueByLayerIndex(p_i_j_k_LayerSize_0, p_i_j_k_LayerSize_1, p_i_j_k_LayerSize_2, p_i_j_k_LayerSize_3, kLayerIndex);
        
        if (kValueIndex < kLayerSize)
        {
            bool computeGradient = (kLayerIndex == uLayersCount - 1) && outputs != null && desiredOutputs != null;
            local float* tmpSum = tmpSums + kLayerIndex * localSize1;

            int p_i_j_l_LayerIndex = PickIntValueByLayerIndex(p_i_j_l_LayerIndex_0_0, p_i_j_l_LayerIndex_0_1, p_i_j_l_LayerIndex_0_2, p_i_j_l_LayerIndex_0_3, kLayerIndex);
            int p_i_j_l_LayerSize = PickIntValueByLayerIndex(p_i_j_l_LayerSize_0_0, p_i_j_l_LayerSize_0_1, p_i_j_l_LayerSize_0_2, p_i_j_l_LayerSize_0_3, kLayerIndex);
            global float$* weights = PickFPValueByLayerIndex$(weights_0_0, weights_0_1, weights_0_2, weights_0_3, kLayerIndex);
            ComputeForwardRTLR_Sum$((global float$*)GetPValuesPtr(pValuesOfWeights, uLayersCount, maxULayerSize, p_i_j_l_LayerIndex), p_i_j_l_LayerSize, weights, kValueIndex, tmpSum, true);
            
            p_i_j_l_LayerIndex = PickIntValueByLayerIndex(p_i_j_l_LayerIndex_1_0, p_i_j_l_LayerIndex_1_1, p_i_j_l_LayerIndex_1_2, p_i_j_l_LayerIndex_1_3, kLayerIndex);
            if (p_i_j_l_LayerIndex != -1)
            {
                p_i_j_l_LayerSize = PickIntValueByLayerIndex(p_i_j_l_LayerSize_1_0, p_i_j_l_LayerSize_1_1, p_i_j_l_LayerSize_1_2, p_i_j_l_LayerSize_1_3, kLayerIndex);
                weights = PickFPValueByLayerIndex$(weights_1_0, weights_1_1, weights_1_2, weights_1_3, kLayerIndex);
                ComputeForwardRTLR_Sum$((global float$*)GetPValuesPtr(pValuesOfWeights, uLayersCount, maxULayerSize, p_i_j_l_LayerIndex), p_i_j_l_LayerSize, weights, kValueIndex, tmpSum, false);
            }

            p_i_j_l_LayerIndex = PickIntValueByLayerIndex(p_i_j_l_LayerIndex_2_0, p_i_j_l_LayerIndex_2_1, p_i_j_l_LayerIndex_2_2, p_i_j_l_LayerIndex_2_3, kLayerIndex);
            if (p_i_j_l_LayerIndex != -1)
            {
                p_i_j_l_LayerSize = PickIntValueByLayerIndex(p_i_j_l_LayerSize_2_0, p_i_j_l_LayerSize_2_1, p_i_j_l_LayerSize_2_2, p_i_j_l_LayerSize_2_3, kLayerIndex);
                weights = PickFPValueByLayerIndex$(weights_2_0, weights_2_1, weights_2_2, weights_2_3, kLayerIndex);
                ComputeForwardRTLR_Sum$((global float$*)GetPValuesPtr(pValuesOfWeights, uLayersCount, maxULayerSize, p_i_j_l_LayerIndex), p_i_j_l_LayerSize, weights, kValueIndex, tmpSum, false);
            }

            p_i_j_l_LayerIndex = PickIntValueByLayerIndex(p_i_j_l_LayerIndex_3_0, p_i_j_l_LayerIndex_3_1, p_i_j_l_LayerIndex_3_2, p_i_j_l_LayerIndex_3_3, kLayerIndex);
            if (p_i_j_l_LayerIndex != -1)
            {
                p_i_j_l_LayerSize = PickIntValueByLayerIndex(p_i_j_l_LayerSize_3_0, p_i_j_l_LayerSize_3_1, p_i_j_l_LayerSize_3_2, p_i_j_l_LayerSize_3_3, kLayerIndex);
                weights = PickFPValueByLayerIndex$(weights_3_0, weights_3_1, weights_3_2, weights_3_3, kLayerIndex);
                ComputeForwardRTLR_Sum$((global float$*)GetPValuesPtr(pValuesOfWeights, uLayersCount, maxULayerSize, p_i_j_l_LayerIndex), p_i_j_l_LayerSize, weights, kValueIndex, tmpSum, false);
            }

            ReduceRTLR_Sum$(tmpSum);
            
            if (localId1 == 0)
            {
                float sum = tmpSum[0];
                sum += (iLayerIndex == kLayerIndex && iValueIndex == kValueIndex) ? (inputs != null ? inputs[jValueIndex] : 1.0f) : 0.0f;
                global float* netDerivValues = PickFPValueByLayerIndex(netDerivValues_0, netDerivValues_1, netDerivValues_2, netDerivValues_3, kLayerIndex);
                float p = netDerivValues[kValueIndex] * sum;
                GetPValuesPtr(pValuesOfWeights, uLayersCount, maxULayerSize, kLayerIndex)[kValueIndex] = p;

                if (computeGradient) tmpGradients[localId] += (desiredOutputs[kValueIndex] - outputs[kValueIndex]) * p;
            }
        }

        kLayerAndValueIndex++;
        barrier(CLK_LOCAL_MEM_FENCE);
    }

    if (localId1 == 0)
    {
        if (gradients != null || gradientSums != null)
        {
            ComputeGradinetsRTLR_SetGradients(tmpGradients, gradients, gradientSums);
        }
    }
}

void ComputeForwardRTLR_Sum$(global float$* inputs, int inputsSize, global float$* weights, int idx, local float* tmpSum, bool start)
{
    int localId1 = get_local_id(1);
    int localSize1 = get_local_size(1);

    if (start) tmpSum[localId1] = 0.0f;
    barrier(CLK_LOCAL_MEM_FENCE);

    int block = inputsSize / localSize1 + (inputsSize % localSize1 != 0 ? 1 : 0);
    int idx = localId1 * block;
    int max = idx + block;
    if (max > size) max = size;
    while (idx <  max)
    {
        tmpSum[localId1] += SumComponents$(inputs[x] * Get2$(weights, x, idx, inputsSize));

        idx++;
    }

    barrier(CLK_LOCAL_MEM_FENCE);
}

void ReduceRTLR_Sum$(local float* tmpSum)
{
    int localId1 = get_local_id(1);
    int localSize1 = get_local_size(1);

    for (int offset = localSize / 2; offset > 0; offset = offset / 2)
    {
        if (localId1 < offset)
        {
            tmpSum[localId1] += tmpSum[localId1 + offset];
        }

        barrier(CLK_LOCAL_MEM_FENCE);
    }
}

inline global float$* PickFPValueByLayerIndex$(global float$* v0, global float$* v1, global float$* v2, global float$* v3, int idx)
{
    return idx == 0 ? v0 : (idx == 1 ? v1 : (idx == 2 ? v2 : v3));
}

inline int PickIntValueByLayerIndex(int v0, int v1, int v2, int v3, int idx)
{
    return idx == 0 ? v0 : (idx == 1 ? v1 : (idx == 2 ? v2 : v3));
}

inline global float* GetPValuesPtr(global float* pValuesOfWeights, int uLayersCount, int maxULayerSize, int kLayerIndex)
{
    int ijValueIndex = get_group_id(0);
    return pValuesOfWeights + (ijValueIndex * uLayersCount * maxULayerSize) + (kLayerIndex * maxULayerSize);
}

void ComputeGradinetsRTLR_SetGradients(local float* tmpGradients, global float* gradients, global float* gradientSums)
{
    Reduce_Sum(tmpGradients);

    if (get_local_id(0) == 0)
    {
        int ijValueIndex = get_group_id(0);
        if (gradients != null) gradients[ijValueIndex] = tmpGradients[0];
        if (gradientSums != null) gradientSums[ijValueIndex] += tmpGradients[0];
    }
}
