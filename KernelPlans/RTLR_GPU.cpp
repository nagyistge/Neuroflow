void ComputeGradinetsRTLR_Layer_CPU$(
    global float$* p_i_j_l_Values_0
    , int p_i_j_l_ValuesSize_0
    , global float$* weights_0
    , global float$* p_i_j_l_Values_1
    , int p_i_j_l_ValuesSize_1
    , global float$* weights_1
    , global float$* p_i_j_l_Values_2
    , int p_i_j_l_ValuesSize_2
    , global float$* weights_2
    , global float$* p_i_j_l_Values_3
    , int p_i_j_l_ValuesSize_3
    , global float$* weights_3
    , global float* p_i_j_k_Values
    , int p_i_j_k_ValuesSize
    , global float* netDerivValues
    , int iValueIndex
    , global float* inputs
    , int inputIndex
    , local float* tmpGradients
    , global float* outputs
    , global float* desiredOutputs)
{
    int localSize = get_local_size(0);
    int localId = get_local_id(0);
    int block = p_i_j_k_ValuesSize / localSize + (p_i_j_k_ValuesSize % localSize != 0 ? 1 : 0);
    int kValueIndex = localId * block;
    int max = kValueIndex + block;
    if (max > p_i_j_k_ValuesSize) max = p_i_j_k_ValuesSize;
    while (kValueIndex < max)
    {
        float sum = iValueIndex == kValueIndex ? (inputs != null ? inputs[inputIndex] : 1.0f) : 0.0f;
        sum += ComputeForward_Sum$(p_i_j_l_Values_0, p_i_j_l_ValuesSize_0, weights_0, kValueIndex);
        if (p_i_j_l_Values_1 != null) sum += ComputeForward_Sum$(p_i_j_l_Values_1, p_i_j_l_ValuesSize_1, weights_1, kValueIndex);
        if (p_i_j_l_Values_2 != null) sum += ComputeForward_Sum$(p_i_j_l_Values_2, p_i_j_l_ValuesSize_2, weights_2, kValueIndex);
        if (p_i_j_l_Values_3 != null) sum += ComputeForward_Sum$(p_i_j_l_Values_3, p_i_j_l_ValuesSize_3, weights_3, kValueIndex);
        float p = netDerivValues[kValueIndex] * sum;
        p_i_j_k_Values[kValueIndex] = p;
        if (tmpGradients != null) tmpGradients[localId] += (desiredOutputs[kValueIndex] - outputs[kValueIndex]) * p;
        kValueIndex++;
    }
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

kernel void ComputeGradientsRTLR_V0_CPU$(
    global float$* p_i_j_l_Values_0_0
    , int p_i_j_l_ValuesSize_0_0
    , global float$* weights_0_0
    , global float$* p_i_j_l_Values_1_0
    , int p_i_j_l_ValuesSize_1_0
    , global float$* weights_1_0
    , global float$* p_i_j_l_Values_2_0
    , int p_i_j_l_ValuesSize_2_0
    , global float$* weights_2_0
    , global float$* p_i_j_l_Values_3_0
    , int p_i_j_l_ValuesSize_3_0
    , global float$* weights_3_0
    , global float* p_i_j_k_Values_0
    , int p_i_j_k_ValuesSize_0
    , global float* netDerivValues_0
    , global float$* p_i_j_l_Values_0_1
    , int p_i_j_l_ValuesSize_0_1
    , global float$* weights_0_1
    , global float$* p_i_j_l_Values_1_1
    , int p_i_j_l_ValuesSize_1_1
    , global float$* weights_1_1
    , global float$* p_i_j_l_Values_2_1
    , int p_i_j_l_ValuesSize_2_1
    , global float$* weights_2_1
    , global float$* p_i_j_l_Values_3_1
    , int p_i_j_l_ValuesSize_3_1
    , global float$* weights_3_1
    , global float* p_i_j_k_Values_1
    , int p_i_j_k_ValuesSize_1
    , global float* netDerivValues_1
    , global float$* p_i_j_l_Values_0_2
    , int p_i_j_l_ValuesSize_0_2
    , global float$* weights_0_2
    , global float$* p_i_j_l_Values_1_2
    , int p_i_j_l_ValuesSize_1_2
    , global float$* weights_1_2
    , global float$* p_i_j_l_Values_2_2
    , int p_i_j_l_ValuesSize_2_2
    , global float$* weights_2_2
    , global float$* p_i_j_l_Values_3_2
    , int p_i_j_l_ValuesSize_3_2
    , global float$* weights_3_2
    , global float* p_i_j_k_Values_2
    , int p_i_j_k_ValuesSize_2
    , global float* netDerivValues_2
    , global float$* p_i_j_l_Values_0_3
    , int p_i_j_l_ValuesSize_0_3
    , global float$* weights_0_3
    , global float$* p_i_j_l_Values_1_3
    , int p_i_j_l_ValuesSize_1_3
    , global float$* weights_1_3
    , global float$* p_i_j_l_Values_2_3
    , int p_i_j_l_ValuesSize_2_3
    , global float$* weights_2_3
    , global float$* p_i_j_l_Values_3_3
    , int p_i_j_l_ValuesSize_3_3
    , global float$* weights_3_3
    , global float* p_i_j_k_Values_3
    , int p_i_j_k_ValuesSize_3
    , global float* netDerivValues_3
    , int iLayerIndex
    , int iValueIndex
    , global float* inputs
    , int inputIndex
    , global float* outputs
    , global float* desiredOutputs
    , local float* tmpGradients
    , global float* gradients
    , global float* gradientSums
    , int gradientIndex)
{
    tmpGradients[get_local_id(0)] = 0.0f;
    barrier(CLK_LOCAL_MEM_FENCE);
    int kLayerIndex;
    bool isLastLayer;
    kLayerIndex = 0;
    isLastLayer = p_i_j_k_Values_1 == null;
    ComputeGradinetsRTLR_Layer_CPU$(
        p_i_j_l_Values_0_0
        , p_i_j_l_ValuesSize_0_0
        , weights_0_0
        , p_i_j_l_Values_1_0
        , p_i_j_l_ValuesSize_1_0
        , weights_1_0
        , p_i_j_l_Values_2_0
        , p_i_j_l_ValuesSize_2_0
        , weights_2_0
        , p_i_j_l_Values_3_0
        , p_i_j_l_ValuesSize_3_0
        , weights_3_0
        , p_i_j_k_Values_0
        , p_i_j_k_ValuesSize_0
        , netDerivValues_0
        , iLayerIndex == kLayerIndex ? iValueIndex : -1
        , inputs
        , inputIndex
        , (isLastLayer && outputs != null) ? tmpGradients : null
        , isLastLayer ? outputs : null
        , isLastLayer ? desiredOutputs : null);
    barrier(CLK_LOCAL_MEM_FENCE);
    if (!isLastLayer)
    {
        kLayerIndex = 1;
        isLastLayer = p_i_j_k_Values_2 == null;
        ComputeGradinetsRTLR_Layer_CPU$(
            p_i_j_l_Values_0_1
            , p_i_j_l_ValuesSize_0_1
            , weights_0_1
            , p_i_j_l_Values_1_1
            , p_i_j_l_ValuesSize_1_1
            , weights_1_1
            , p_i_j_l_Values_2_1
            , p_i_j_l_ValuesSize_2_1
            , weights_2_1
            , p_i_j_l_Values_3_1
            , p_i_j_l_ValuesSize_3_1
            , weights_3_1
            , p_i_j_k_Values_1
            , p_i_j_k_ValuesSize_1
            , netDerivValues_1
            , iLayerIndex == kLayerIndex ? iValueIndex : -1
            , inputs
            , inputIndex
            , (isLastLayer && outputs != null) ? tmpGradients : null
            , isLastLayer ? outputs : null
            , isLastLayer ? desiredOutputs : null);
        barrier(CLK_LOCAL_MEM_FENCE);
    }
    if (!isLastLayer)
    {
        kLayerIndex = 2;
        isLastLayer = p_i_j_k_Values_3 == null;
        ComputeGradinetsRTLR_Layer_CPU$(
            p_i_j_l_Values_0_2
            , p_i_j_l_ValuesSize_0_2
            , weights_0_2
            , p_i_j_l_Values_1_2
            , p_i_j_l_ValuesSize_1_2
            , weights_1_2
            , p_i_j_l_Values_2_2
            , p_i_j_l_ValuesSize_2_2
            , weights_2_2
            , p_i_j_l_Values_3_2
            , p_i_j_l_ValuesSize_3_2
            , weights_3_2
            , p_i_j_k_Values_2
            , p_i_j_k_ValuesSize_2
            , netDerivValues_2
            , iLayerIndex == kLayerIndex ? iValueIndex : -1
            , inputs
            , inputIndex
            , (isLastLayer && outputs != null) ? tmpGradients : null
            , isLastLayer ? outputs : null
            , isLastLayer ? desiredOutputs : null);
        barrier(CLK_LOCAL_MEM_FENCE);
    }
    if (!isLastLayer)
    {
        kLayerIndex = 3;
        isLastLayer = true;
        ComputeGradinetsRTLR_Layer_CPU$(
            p_i_j_l_Values_0_3
            , p_i_j_l_ValuesSize_0_3
            , weights_0_3
            , p_i_j_l_Values_1_3
            , p_i_j_l_ValuesSize_1_3
            , weights_1_3
            , p_i_j_l_Values_2_3
            , p_i_j_l_ValuesSize_2_3
            , weights_2_3
            , p_i_j_l_Values_3_3
            , p_i_j_l_ValuesSize_3_3
            , weights_3_3
            , p_i_j_k_Values_3
            , p_i_j_k_ValuesSize_3
            , netDerivValues_3
            , iLayerIndex == kLayerIndex ? iValueIndex : -1
            , inputs
            , inputIndex
            , (isLastLayer && outputs != null) ? tmpGradients : null
            , isLastLayer ? outputs : null
            , isLastLayer ? desiredOutputs : null);
        barrier(CLK_LOCAL_MEM_FENCE);
    }
    ComputeGradinetsRTLR_SetGradients(tmpGradients, gradients, gradientSums, gradientIndex);
}
