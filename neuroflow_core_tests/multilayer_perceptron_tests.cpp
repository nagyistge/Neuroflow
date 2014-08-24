#include <boost/test/unit_test.hpp>
#include "stdafx.h"
#include "nf.h"
#include "layer_order_comparer.h"

using namespace nf;
using namespace std;
using namespace linq;

BOOST_AUTO_TEST_SUITE(computation_context_tests)

BOOST_AUTO_TEST_CASE(layer_order_comparer_test)
{
    vector<layer_ptr> layers =
    {
        make_layer(2),
        make_layer(4),
        make_layer(1)
    };

    layers[0]->output_connections().add_one_way(layers[1]);
    layers[1]->output_connections().add_one_way(layers[2]);

    BOOST_REQUIRE_EQUAL(idx_t(0), from(layers[0]->input_connections().connected_layers(flow_direction::all)) >> count());
    BOOST_REQUIRE_EQUAL(idx_t(1), from(layers[0]->output_connections().connected_layers(flow_direction::all)) >> count());

    // Copy leayers
    auto copyOfLayers = from(layers) >> to_vector();
    // Sort
    layer_order_comparer comparer;
    sort(copyOfLayers.begin(), copyOfLayers.end(), [&](const layer_ptr& l1, const layer_ptr& l2) { return comparer(l1, l2) < 0; });
    // Project:
    idx_t idx = 0;
    auto sorted = from(copyOfLayers) >> select([&](const layer_ptr& l) { return row_numbered<layer_ptr>(idx++, l); }) >> to_vector();

    BOOST_REQUIRE_EQUAL(idx_t(3), sorted.size());
    BOOST_REQUIRE_EQUAL(idx_t(0), sorted[0].row_num());
    BOOST_REQUIRE_EQUAL(idx_t(1), sorted[1].row_num());
    BOOST_REQUIRE_EQUAL(idx_t(2), sorted[2].row_num());
}

static multilayer_perceptron_ptr create_mlp(const computation_context_ptr& ctx)
{
    vector<layer_ptr> layers =
    {
        make_layer(2),
        make_layer(4, make_activation_description(activation_function::sigmoid, 1.7f)),
        make_layer(1, make_activation_description(activation_function::linear, 1.1f))
    };
    layers[0]->output_connections().add_one_way(layers[1]);
    layers[1]->output_connections().add_one_way(layers[2]);

    auto mlp = ctx->neural_network_factory()->create_multilayer_perceptron(layers);
    idx_t numWeights = mlp->number_of_weights();
    BOOST_REQUIRE_EQUAL(idx_t((2 * 4 + 4) + (4 * 1 + 1)), numWeights);

    return move(mlp);
}

void do_get_and_set_weights(const computation_context_ptr& ctx)
{
    auto mlp = create_mlp(ctx);
    idx_t numWeights = mlp->number_of_weights();
    auto weights = ctx->data_array_factory()->create(numWeights);
    vector<float> weightValues(numWeights);
    mlp->get_weights(weights);
    weights->read(0, numWeights, &weightValues[0], 0).wait();
    for (float v : weightValues) BOOST_REQUIRE_EQUAL(0.0f, v);

    for (idx_t i = 0; i < numWeights; i++) weightValues[i] = 0.11f;
    weights->write(&weightValues[0], 0, numWeights, 0).wait();
    mlp->set_weights(weights);

    for (idx_t i = 0; i < numWeights; i++) weightValues[i] = 0.99f;
    weights->write(&weightValues[0], 0, numWeights, 0).wait();
    for (idx_t i = 0; i < numWeights; i++) weightValues[i] = 0.0f;
    weights->read(0, numWeights, &weightValues[0], 0).wait();
    for (float v : weightValues) BOOST_REQUIRE_EQUAL(0.99f, v);

    mlp->get_weights(weights);
    weights->read(0, numWeights, &weightValues[0], 0).wait();
    for (float v : weightValues) BOOST_REQUIRE_EQUAL(0.11f, v);
}

BOOST_AUTO_TEST_CASE(cpp_get_and_set_weights)
{
    auto ctx = computation_context_factory().create_context(cpp_context);
    do_get_and_set_weights(ctx);
}

BOOST_AUTO_TEST_CASE(ocl_get_and_set_weights_cpu)
{
    auto ctx = computation_context_factory().create_context(ocl_context, L"CPU");
    do_get_and_set_weights(ctx);
}

BOOST_AUTO_TEST_CASE(ocl_get_and_set_weights_gpu)
{
    auto ctx = computation_context_factory().create_context(ocl_context, L"GPU");
    do_get_and_set_weights(ctx);
}

BOOST_AUTO_TEST_SUITE_END()
