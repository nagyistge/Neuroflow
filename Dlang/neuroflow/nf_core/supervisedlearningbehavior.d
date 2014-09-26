import learningbehavior;

enum SupervisedWeightUpdateMode
{
    offline,
    online
}

enum SupervisedOptimizationType
{
    none,
    gradientBased,
    global
}

class SupervisedLearningBehavior : LearningBehavior
{
    abstract @property SupervisedOptimizationType optimizationType() const;

    abstract @property SupervisedWeightUpdateMode weightUpdateMode() const;
}