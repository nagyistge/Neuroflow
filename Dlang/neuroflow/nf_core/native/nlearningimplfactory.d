import learningimplfactory;

class NLearningImplFactory : LearningImplFactory
{
	this()
	{
		CreateLearningImpl[string] myFactories;
		super(myFactories);
	}
}
