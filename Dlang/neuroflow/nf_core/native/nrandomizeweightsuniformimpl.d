import learningimpl;
import ncomputationcontext;
import randomizeweightsuniform;
import utils;

class NRandomizeWeightsUniformImpl : LearningImplOf!(NComputationContext, RandomizeWeightsUniform)
{
	this(NComputationContext context, LearningBehavior behavior, TrainingNode[] nodes)
	{
		super(context, behavior, nodes);
	}

	override void initialize()
	{
		float min = -behavior.strength;
		float max = behavior.strength;
		foreach (node; nodes)
		{
			context.utils.randomizeUniform(node.weights, min, max);
		}
	}
}
