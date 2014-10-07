import computationcontext;
public import learningbehavior;
public import trainingnode;
import std.traits;

interface LearningImpl
{
	void initialize();
}

class LearningImplOf(C : ComputationContext, T : LearningBehavior) : LearningImpl
{
	protected this(C context, LearningBehavior behavior, TrainingNode[] nodes)
	{
		assert(context);
		assert(behavior);
		assert(nodes && nodes.length);

		_context = context;
        _behavior = cast(T)behavior;
		assert(_behavior !is null);
		_nodes = nodes.dup;
	}

	@property static string behaviorName()
	{
		return fullyQualifiedName!T;
	}

	protected @property C context()
	{
		return _context;
	}

	protected @property T behavior()
	{
		return _behavior;
	}

	protected @property TrainingNode[] nodes()
	{
		return _nodes;
	}

	abstract void initialize();

	private C _context;

	private T _behavior;

	TrainingNode[] _nodes;
}