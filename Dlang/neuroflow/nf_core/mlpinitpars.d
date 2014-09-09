enum GradientComputationMethod
{
    None,
    FeedForward,
    BPTT,
    RTLR
}

class MLPInitPars
{
    GradientComputationMethod gradientComputationMethod = GradientComputationMethod.FeedForward;
}