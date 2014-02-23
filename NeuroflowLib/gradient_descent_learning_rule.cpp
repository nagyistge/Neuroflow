#include "stdafx.h"
#include "gradient_descent_learning_rule.h"

USING

bool gradient_descent_learning_rule::props_equals(const layer_behavior* other) const
{
    if (!supervised_learning_behavior::props_equals(other)) return false;
    auto o = dynamic_cast<const gradient_descent_learning_rule*>(other);
    if (!o) return false;
    return _learningRate == o->_learningRate &&
        _momentum == o->_momentum &&
        _smoothing == o->_smoothing;
}

learning_algo_optimization_type gradient_descent_learning_rule::optimization_type() const
{
    return learning_algo_optimization_type::gradient_based;
}