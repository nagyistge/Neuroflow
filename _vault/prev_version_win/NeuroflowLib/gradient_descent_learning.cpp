#include "stdafx.h"
#include "gradient_descent_learning.h"

USING

bool gradient_descent_learning::props_equals(const layer_behavior* other) const
{
    auto o = dynamic_cast<const gradient_descent_learning*>(other);
    if (!o) return false;
    return _learningRate == o->_learningRate &&
        _momentum == o->_momentum &&
        _smoothing == o->_smoothing &&
        _weightUpdateMode == o->_weightUpdateMode;
}

::size_t gradient_descent_learning::get_hash_code() const
{
    return hash<float>()(_learningRate) ^
        hash<float>()(_momentum) ^
        hash<bool>()(_smoothing) ^
        hash<int>()((int)_weightUpdateMode);
}

learning_algo_optimization_type gradient_descent_learning::optimization_type() const
{
    return learning_algo_optimization_type::gradient_based;
}