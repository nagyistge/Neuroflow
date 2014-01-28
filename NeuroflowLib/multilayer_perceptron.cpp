#include "stdafx.h"
#include "multilayer_perceptron.h"
#include "prop_def.h"
#include "layer_order_comparer.h"
#include "layer_connections.h"
#include "supervised_learning_behavior.h"
#include "layer.h"

USING

multilayer_perceptron::multilayer_perceptron(const computation_context_ptr& context, layers_t& layers, const optional_properties_t& properties) :
    contexted(context)
{
    prop_def pd(_properties, properties);
    _gradient_computation_method = pd.defEnum(prop_gradient_computation_method, gradient_computation_method::feed_forward);

    _layers = layers | sort(layer_order_comparer()) | row_num() | to_vector();

    // It supposed to be cool, but looks like shit because of MSVC.    
#if (_MSC_VER)
    auto infos = _layers
        | select([](row_numbered<layer_ptr>& l) -> pair<idx_t, supervised_learning_behavior_ptr> { return make_pair(l.row_num(), l.value()->behaviors() | dcast<supervised_learning_behavior>() | first_or_default()); })
        | where([](pair<idx_t, supervised_learning_behavior_ptr>& r) { return r.second != null; })
        | select([](pair<idx_t, supervised_learning_behavior_ptr>& r)
    { 
        return layer_info(
            r.first, 
            r.second->weight_update_mode() == weight_update_mode::online, // is_online
            r.second->weight_update_mode() == weight_update_mode::offline, // is_offline
            r.second->optimization_type()); 
    });
#else
    auto infos = _layers
        | select([](auto& l) { return make_pair(l.row_num(), l.value()->behaviors() | dcast<supervised_learning_behavior>() | first_or_default()); })
        | where([](auto& r) { return r.second != null; })
        | select([](auto& r)
    {
        return layer_info(
            r.first,
            r.second->weight_update_mode() == weight_update_mode::online, // is_online
            r.second->weight_update_mode() == weight_update_mode::offline, // is_offline
            r.second->optimization_type());
    });
#endif
}

const boost::property_tree::ptree& multilayer_perceptron::properties() const
{
    return _properties;
}

nf::gradient_computation_method multilayer_perceptron::gradient_computation_method() const
{
    return _gradient_computation_method;
}