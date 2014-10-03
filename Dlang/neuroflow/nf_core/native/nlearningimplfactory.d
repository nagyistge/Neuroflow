import learningimplfactory;
import ncomputationcontext;
import nrandomizeweightsuniformimpl;
import ngradientdescentlearningimpl;

class NLearningImplFactory : LearningImplFactory
{
	this(NComputationContext context)
	{
		CreateLearningImpl[string] myFactories;

		myFactories[NRandomizeWeightsUniformImpl.behaviorName] = (behavior, nodes) => new NRandomizeWeightsUniformImpl(context, behavior, nodes);
		myFactories[NGradientDescentLearningImpl.behaviorName] = (behavior, nodes) => new NGradientDescentLearningImpl(context, behavior, nodes);

		super(myFactories);
	}
}
