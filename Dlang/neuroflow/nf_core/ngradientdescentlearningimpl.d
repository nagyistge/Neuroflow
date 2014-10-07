import learningimpl;
import ncomputationcontext;
import supervisedlearning;
import gradientdescentlearning;
import ndevicearray;
import utils;
import tonative;

class NGradientDescentLearningImpl : LearningImplOf!(NComputationContext, GradientDescentLearning), SupervisedLearning
{
    this(NComputationContext context, LearningBehavior behavior, TrainingNode[] nodes)
    {
        super(context, behavior, nodes);
    }
    
    @property SupervisedLearningIterationType iterationType()
    {
        return behavior.weightUpdateMode == SupervisedWeightUpdateMode.online ? SupervisedLearningIterationType.online : SupervisedLearningIterationType.offline;
    }
    
    override void initialize()
    {
        if (_deltas is null || _deltas.length == 0)
        {
            _deltas.reserve = nodes.length;
            foreach (ref node; nodes)
            {
                _deltas ~= cast(NDeviceArray)(context.deviceArrayManagement.createArray(false, node.weights.size));
            }
        }
        else
        {
            foreach (d; _deltas) context.utils.zero(d);
        }
    }   
    
    void run(size_t iterationCount, DeviceArray error)
    {
        if (behavior.weightUpdateMode == SupervisedWeightUpdateMode.online)
        {
            size_t idx = 0;
            foreach (ref node; nodes)
            {
                updateWeightsOnline(_deltas[idx].array, toArray(node.weights), toArray(node.gradients));
                idx++;
            }
        }
        else
        {
            size_t idx = 0;
            float ic = iterationCount;
            foreach (ref node; nodes)
            {
                updateWeightsOffline(_deltas[idx].array, toArray(node.weights), toArray(node.gradientSums), ic);
                idx++;
            }
        }
    }
    
    private void updateWeightsOnline(float[] deltas, float[] weights, float[] gradients)
    {
        assert(weights);
        assert(gradients);
        size_t size = weights.length;
        assert(gradients.length == size);
        assert(deltas.length == size);
        float rate = behavior.learningRate;
        float momentum = behavior.momentum;
        if (behavior.smoothing)
        {
            float smoothV = 1.0f - momentum;
            for (size_t idx = 0; idx < size; idx++)
            {
                float update = gradients[idx] * rate;
                float lastUpdate = deltas[idx];
                update = (lastUpdate * momentum) + (update * smoothV);
                weights[idx] += update;
                deltas[idx] = update;
            }
        }
        else
        {
            for (size_t idx = 0; idx < size; idx++)
            {
                float update = gradients[idx] * rate;
                float lastUpdate = deltas[idx];
                update = (lastUpdate * momentum) + update;
                weights[idx] += update;
                deltas[idx] = update;
            }
        }
    }
    
    private void updateWeightsOffline(float[] deltas, float[] weights, float[] gradientSums, in float iterationCount)
    {
        assert(weights);
        assert(gradientSums);
        size_t size = weights.length;
        assert(gradientSums.length == size);
        assert(deltas.length == size);
        float rate = behavior.learningRate;
        float momentum = behavior.momentum;
        if (behavior.smoothing)
        {
            float smoothV = 1.0f - momentum;
            for (size_t idx = 0; idx < size; idx++)
            {
                float update = (gradientSums[idx] * rate) / iterationCount;
                float lastUpdate = deltas[idx];
                update = (lastUpdate * momentum) + (update * smoothV);
                weights[idx] += update;
                deltas[idx] = update;
            }
        }
        else
        {
            for (size_t idx = 0; idx < size; idx++)
            {
                float update = (gradientSums[idx] * rate) / iterationCount;
                float lastUpdate = deltas[idx];
                update = (lastUpdate * momentum) + update;
                weights[idx] += update;
                deltas[idx] = update;
            }
        }
    }
    
    private NDeviceArray[] _deltas;
}