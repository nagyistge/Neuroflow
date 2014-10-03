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
				updateWeightsOnline(_deltas[idx], toNativeDeviceArray(node.weights), toNativeDeviceArray(node.gradients));
			}
		}
		else
		{
			size_t idx = 0;
			float ic = iterationCount;
			foreach (ref node; nodes)
			{
				updateWeightsOffline(_deltas[idx], toNativeDeviceArray(node.weights), toNativeDeviceArray(node.gradientSums), ic);
			}
		}
	}

	private void updateWeightsOnline(NDeviceArray deltas, NDeviceArray weights, NDeviceArray gradients)
	{
		assert(weights);
		assert(gradients);
		size_t size = weights.size;
		assert(gradients.size == size);
		assert(deltas.size == size);
		float[] weightsArr = weights.array;
		float[] gradientsArr = gradients.array;
		float[] deltasArr = deltas.array;
		float rate = behavior.learningRate;
		float momentum = behavior.momentum;
		if (behavior.smoothing)
		{
			float smoothV = 1.0f - momentum;
			for (size_t idx = 0; idx < size; idx++)
			{
				float update = gradientsArr[idx] * rate;
				float lastUpdate = deltasArr[idx];
				update = (lastUpdate * momentum) + (update * smoothV);
				weightsArr[idx] += update;
				deltasArr[idx] = update;
			}
		}
		else
		{
			for (size_t idx = 0; idx < size; idx++)
			{
				float update = gradientsArr[idx] * rate;
				float lastUpdate = deltasArr[idx];
				update = (lastUpdate * momentum) + update;
				weightsArr[idx] += update;
				deltasArr[idx] = update;
			}
		}
	}

	private void updateWeightsOffline(NDeviceArray deltas, NDeviceArray weights, NDeviceArray gradientSums, in float iterationCount)
	{
		assert(weights);
		assert(gradientSums);
		size_t size = weights.size;
		assert(gradientSums.size == size);
		assert(deltas.size == size);
		float[] weightsArr = weights.array;
		float[] gradientSumsArr = gradientSums.array;
		float[] deltasArr = deltas.array;
		float rate = behavior.learningRate;
		float momentum = behavior.momentum;
		if (behavior.smoothing)
		{
			float smoothV = 1.0f - momentum;
			for (size_t idx = 0; idx < size; idx++)
			{
				float update = (gradientSumsArr[idx] * rate) / iterationCount;
				float lastUpdate = deltasArr[idx];
				update = (lastUpdate * momentum) + (update * smoothV);
				weightsArr[idx] += update;
				deltasArr[idx] = update;
			}
		}
		else
		{
			for (size_t idx = 0; idx < size; idx++)
			{
				float update = (gradientSumsArr[idx] * rate) / iterationCount;
				float lastUpdate = deltasArr[idx];
				update = (lastUpdate * momentum) + update;
				weightsArr[idx] += update;
				deltasArr[idx] = update;
			}
		}
	}

	private NDeviceArray[] _deltas;
}