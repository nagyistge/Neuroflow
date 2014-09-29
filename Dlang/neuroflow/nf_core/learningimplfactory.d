import learningimpl;
import trainingnode;
import learningbehavior;
import std.conv;
import std.exception;

alias CreateLearningImpl = LearningImpl delegate(in LearningBehavior, in TrainingNode[]);

class LearningImplFactory
{
	protected this(CreateLearningImpl[string] factories)
	{
		_factories = to!(immutable(CreateLearningImpl[string]))(factories);
	}

	LearningImpl createImpl(in LearningBehavior learningBehavior, in TrainingNode[] nodes)
	{
		assert(nodes);
		assert(nodes.length);

		enforce(_factories && _factories.length, "Learning implemetation factories map is empty.");

		auto typeName = learningBehavior.stringof;
		auto result = _factories.get(typeName, null);
		if (result != null)
		{
			auto impl = result(learningBehavior, nodes);
			if (impl) return impl;
		}

		throw new Exception("Learning for behavior type: '" ~ typeName ~ "' is not implemeted.");
	}

	private immutable(CreateLearningImpl[string]) _factories;
}
