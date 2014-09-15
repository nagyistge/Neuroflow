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
    abstract SupervisedOptimizationType optimizationType() const;

    abstract SupervisedWeightUpdateMode weightUpdateMode() const;
}