import mlpforwardnode;
import tonative;
import activationdescription;
import nhelpers;

void ncaForward(Object context, ref MLPForwardNode[] nodes)
{
	foreach (node; nodes)
    {
        size_t layerSize = node.size;

        auto outputs = toArray(node.outputs());
        assert(outputs);

        auto biases = toArray(node.biases);
        assert(biases);

        auto derivates = toArray(node.derivates, true);

        float alpha = node.activation.alpha;

        for (size_t valueIdx = 0; valueIdx < layerSize; valueIdx++)
        {
            float sum = biases[valueIdx];
            foreach (ref weightedInput; node.weightedInputs)
            {
                auto weights = toArray(weightedInput.weights);
                auto inputs = toArray(weightedInput.inputs());
                assert(weights);
                assert(inputs);

                size_t inputsSize = inputs.length;
				for (size_t inputIdx = 0; inputIdx < inputsSize; inputIdx++)
                {
                    size_t widx = getIndex2(inputIdx, valueIdx, inputsSize);
                    assert(widx >= 0 && widx < weights.length);
                    sum += inputs[inputIdx] * weights[widx];
                }
            }

            if (node.activation.activationFunction == ActivationFunction.sigmoid)
            {
                outputs[valueIdx] = sigmoid(sum, alpha);
                if (derivates != null) derivates[valueIdx] = sigmoidDeriv(sum, alpha);
            }
            else // Linear
            {
                outputs[valueIdx] = linear(sum, alpha);
                if (derivates != null) derivates[valueIdx] = alpha;
            }
        }
    }
}