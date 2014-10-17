__kernel void ActivationNeuron(global float* inputs, global float* weights, global float* outputs, global int* inputCount, global float* alpha)
{
    int outputIndex = get_global_id(0);
 
    float sum = 0.0;
 
    for (int inputIndex = 0; inputIndex < inputCount[0]; inputIndex++)
    {
        int weightIndex = inputCount[0] * outputIndex + inputIndex;
        sum += inputs[inputIndex] * weights[weightIndex];
    }
 
    outputs[outputIndex] = (2.0 / (1.0 + native_exp(-alpha[0] * sum))) - 1.0;
}