#include "../stdafx.h"
#include "cpp_randomize_weights_uniform.h"
#include "cpp_device_array.h"
#include "cpp_computation_context.h"
#include "../random_generator.h"

USING

cpp_randomize_weights_uniform::cpp_randomize_weights_uniform(const std::weak_ptr<cpp_computation_context>& context, const learning_behavior_ptr& behavior, const training_node_collection_ptr& nodes) :
learning_impl_of(context, behavior, nodes)
{
}

void cpp_randomize_weights_uniform::initialize()
{
    auto ctx = lock_context();
    auto& rnd = ctx->rnd();
    float min = 0.0f - behavior()->strength();
    float max = 0.0f + behavior()->strength();
    for (auto& node : *nodes())
    {
        auto cppw = _fast_cast_alt<cpp_device_array>(node.weights().get());
        assert(cppw);
        idx_t size = cppw->size();
        float* ptr = cppw->ptr();
        for (idx_t i = 0; i < size; i++) ptr[i] = rnd.next(min, max);
        assert(ptr);
    }
}
