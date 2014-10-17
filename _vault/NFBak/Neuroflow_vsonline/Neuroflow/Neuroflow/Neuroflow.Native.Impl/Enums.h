#pragma once

enum class ForwardComputationMethod
{
    FeedForward,
    BPTT,
    RTLR
};

enum class NeuralNetworkResetTarget
{
    Outputs = 1, 
    Errors = 2, 
    Gradients = 4, 
    GradientSums = 8,
    Ps = 16,
    Algorithms = 32,
    All = Outputs | Errors | Gradients | GradientSums | Ps | Algorithms
}; 
