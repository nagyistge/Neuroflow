enum GradientComputationMethod
{
    none,
    feedForward,
    bptt,
    rtlr
}

class MLPInitPars
{
    GradientComputationMethod gradientComputationMethod = GradientComputationMethod.feedForward;
}