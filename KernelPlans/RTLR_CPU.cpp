kernel void ComputeGradientsRTLR_V0_CPU(
    global float* pValuesOfWeights
    , int uLayersCount
    , int maxULayerSize
    , int p_i_j_l_ValuesLayerIndex_0_0
    , int p_i_j_l_ValuesSize_0_0
    , global float* weights_0_0
    , int p_i_j_l_ValuesLayerIndex_1_0
    , int p_i_j_l_ValuesSize_1_0
    , global float* weights_1_0
    , int p_i_j_l_ValuesLayerIndex_2_0
    , int p_i_j_l_ValuesSize_2_0
    , global float* weights_2_0
    , int p_i_j_l_ValuesLayerIndex_3_0
    , int p_i_j_l_ValuesSize_3_0
    , global float* weights_3_0
    , int p_i_j_k_ValuesSize_0
    , global float* netDerivValues_0
    , int p_i_j_l_ValuesLayerIndex_0_1
    , int p_i_j_l_ValuesSize_0_1
    , global float* weights_0_1
    , int p_i_j_l_ValuesLayerIndex_1_1
    , int p_i_j_l_ValuesSize_1_1
    , global float* weights_1_1
    , int p_i_j_l_ValuesLayerIndex_2_1
    , int p_i_j_l_ValuesSize_2_1
    , global float* weights_2_1
    , int p_i_j_l_ValuesLayerIndex_3_1
    , int p_i_j_l_ValuesSize_3_1
    , global float* weights_3_1
    , int p_i_j_k_ValuesSize_1
    , global float* netDerivValues_1
    , int p_i_j_l_ValuesLayerIndex_0_2
    , int p_i_j_l_ValuesSize_0_2
    , global float* weights_0_2
    , int p_i_j_l_ValuesLayerIndex_1_2
    , int p_i_j_l_ValuesSize_1_2
    , global float* weights_1_2
    , int p_i_j_l_ValuesLayerIndex_2_2
    , int p_i_j_l_ValuesSize_2_2
    , global float* weights_2_2
    , int p_i_j_l_ValuesLayerIndex_3_2
    , int p_i_j_l_ValuesSize_3_2
    , global float* weights_3_2
    , int p_i_j_k_ValuesSize_2
    , global float* netDerivValues_2
    , int p_i_j_l_ValuesLayerIndex_0_3
    , int p_i_j_l_ValuesSize_0_3
    , global float* weights_0_3
    , int p_i_j_l_ValuesLayerIndex_1_3
    , int p_i_j_l_ValuesSize_1_3
    , global float* weights_1_3
    , int p_i_j_l_ValuesLayerIndex_2_3
    , int p_i_j_l_ValuesSize_2_3
    , global float* weights_2_3
    , int p_i_j_l_ValuesLayerIndex_3_3
    , int p_i_j_l_ValuesSize_3_3
    , global float* weights_3_3
    , int p_i_j_k_ValuesSize_3
    , global float* netDerivValues_3
    , int iLayerIndex
    , global float* inputs
    , int inputsSize // + bias (null) = 1, inputs: size
    , global float* outputs
    , global float* desiredOutputs
    , local float* tmpGradients // size = local size
    , global float* gradients
    , global float* gradientSums)
{
    int localId = get_local_id(0);

    int ijValueIndex = get_group_id(0);
    int iValueIndex = ijValueIndex / inputsSize;
    int jValueIndex = ijValueIndex % inputsSize;

    int outputLayerIndex = // ?

    tmpGradients[localId] = 0.0f;

    barrier(CLK_LOCAL_MEM_FENCE);

    int pValuesOfWeightsSize2 = uLayersCount * maxULayerSize;
    int block = pValuesOfWeightsSize2 / localSize + (pValuesOfWeightsSize2 % localSize != 0 ? 1 : 0);
    int kLayerAndValueIndex = localId * block;
    int max = kLayerAndValueIndex + block;
    if (max > pValuesOfWeightsSize2) max = pValuesOfWeightsSize2;
    while (kLayerAndValueIndex < max)
    {
        int kLayerIndex = kLayerAndValueIndex / maxULayerSize;
        int kValueIndex = kLayerAndValueIndex % maxULayerSize;
        int kLayerSize = GetKLayerSize(p_i_j_k_ValuesSize_0, p_i_j_k_ValuesSize_1, p_i_j_k_ValuesSize_2, p_i_j_k_ValuesSize_3, kLayerIndex);
        
        if (kValueIndex < kLayerSize)
        {
            bool computeGradient = kLayerIndex == outputLayerIndex && outputs != null && desiredOutputs != null;

            float sum = (iLayerIndex == kLayerIndex && iValueIndex == kValueIndex) ? (inputs != null ? inputs[jValueIndex] : 1.0f) : 0.0f;

            sum += ComputeForward_Sum(GetPValuesPtr(pValuesOfWeights, uLayersCount, maxULayerSize, p_i_j_l_ValuesLayerIndex_0), p_i_j_l_ValuesSize_0, weights_0, kValueIndex);
            if (p_i_j_l_ValuesLayerIndex_1 != -1) sum += ComputeForward_Sum(GetPValuesPtr(pValuesOfWeights, uLayersCount, maxULayerSize, p_i_j_l_ValuesLayerIndex_1), p_i_j_l_ValuesSize_1, weights_1, kValueIndex);
            if (p_i_j_l_ValuesLayerIndex_2 != -1) sum += ComputeForward_Sum(GetPValuesPtr(pValuesOfWeights, uLayersCount, maxULayerSize, p_i_j_l_ValuesLayerIndex_2), p_i_j_l_ValuesSize_2, weights_2, kValueIndex);
            if (p_i_j_l_ValuesLayerIndex_3 != -1) sum += ComputeForward_Sum(GetPValuesPtr(pValuesOfWeights, uLayersCount, maxULayerSize, p_i_j_l_ValuesLayerIndex_3), p_i_j_l_ValuesSize_3, weights_3, kValueIndex);
            
            float p = netDerivValues[kValueIndex] * sum;
            GetPValuesPtr(pValuesOfWeights, uLayersCount, maxULayerSize, kLayerIndex)[kValueIndex] = p;
            
            if (computeGradients) tmpGradients[localId] += (desiredOutputs[kValueIndex] - outputs[kValueIndex]) * p;
        }

        kLayerAndValueIndex++;
    }

    barrier(CLK_LOCAL_MEM_FENCE);

    ComputeGradinetsRTLR_SetGradients(tmpGradients, gradients, gradientSums, gradientIndex);
}

int GetKLayerSize(int p_i_j_k_ValuesSize_0, int p_i_j_k_ValuesSize_1, , int p_i_j_k_ValuesSize_2, int p_i_j_k_ValuesSize_3, int kLayerIndex)
{
    if (kLayerIndex == 0) return p_i_j_k_ValuesSize_0;
    if (kLayerIndex == 1) return p_i_j_k_ValuesSize_1;
    if (kLayerIndex == 2) return p_i_j_k_ValuesSize_2;
    return p_i_j_k_ValuesSize_3;
}

global float* GetPValuesPtr(global float* pValuesOfWeights, int uLayersCount, int maxULayerSize, int kLayerIndex)
{
    int ijValueIndex = get_group_id(0);
    return pValuesOfWeights + (ijValueIndex * uLayersCount * maxULayerSize) + kLayerIndex * maxULayerSize);
}

void ComputeGradinetsRTLR_SetGradients(local float* tmpGradients, global float* gradients, global float* gradientSums, int gradientsIndex)
{
    Reduce_Sum(tmpGradients);

    if (get_local_id(0) == 0)
    {
        if (gradients != null) gradients[gradientsIndex] = tmpGradients[0];
        if (gradientSums != null) gradientSums[gradientsIndex] += tmpGradients[0];
    }
}
