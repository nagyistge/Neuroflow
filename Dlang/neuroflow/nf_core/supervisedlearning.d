import learningimpl;
import devicearray;

enum SupervisedLearningIterationType
{
	online = 1,
	offline = 2
}

interface SupervisedLearning : LearningImpl
{
	@property SupervisedLearningIterationType iterationType() const;

	void run(size_t iterationCount, DeviceArray error) nothrow;
}