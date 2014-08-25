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

multilayer_perceptron_ptr create_ff_mlp_with_training(const computation_context_ptr& ctx, float rndStrength, bool online, float rate)
{
    auto wrnd = make_randomize_weights_uniform(rndStrength);
    auto algo = make_gradient_descent_learning(rate, online ? 0.25f : 0.8f, false, online ? weight_update_mode::online : weight_update_mode::offline);
    vector<layer_ptr> layers =
    {
        make_layer(1),
        make_layer(16, make_activation_description(activation_function::sigmoid, 1.7f), wrnd, algo),
        make_layer(16, make_activation_description(activation_function::sigmoid, 1.7f), wrnd, algo),
        make_layer(1, make_activation_description(activation_function::linear, 1.1f), wrnd, algo)
    };

    layers[0]->output_connections().add_one_way(layers[1]);
    layers[1]->output_connections().add_one_way(layers[2]);
    layers[2]->output_connections().add_one_way(layers[3]);

    auto mlp = ctx->neural_network_factory()->create_multilayer_perceptron(layers);

    return move(mlp);
}

multilayer_perceptron_ptr create_rec_mlp_with_training(const computation_context_ptr& ctx, float rndStrength, bool online, float rate, nf::gradient_computation_method gcm)
{
    idx_t hidden1Size = 12;
    idx_t hidden2Size = 8;
    auto wrnd = make_randomize_weights_uniform(rndStrength);
    auto algo = make_gradient_descent_learning(rate, online ? 0.25f : 0.8f, false, online ? weight_update_mode::online : weight_update_mode::offline);
    vector<layer_ptr> layers =
    {
        make_layer(1),
        make_layer(hidden1Size, make_activation_description(activation_function::sigmoid, 1.7f), wrnd, algo),
        make_layer(hidden2Size, make_activation_description(activation_function::sigmoid, 1.7f), wrnd, algo),
        make_layer(3, make_activation_description(activation_function::linear, 1.1f), wrnd, algo)
    };

    layers[0]->output_connections().add_two_way(layers[1]);
    layers[1]->output_connections().add_two_way(layers[2]);
    layers[2]->output_connections().add_two_way(layers[3]);

    mlp_init_pars pars;
    pars.gradient_computation_method = gcm;
    auto mlp = ctx->neural_network_factory()->create_multilayer_perceptron(layers, &pars);

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
    for (float v : weightValues)
    {
        BOOST_REQUIRE_EQUAL(0.0f, v);
    }

    for (idx_t i = 0; i < numWeights; i++) weightValues[i] = 0.11f;
    weights->write(&weightValues[0], 0, numWeights, 0).wait();
    mlp->set_weights(weights);

    for (idx_t i = 0; i < numWeights; i++) weightValues[i] = 0.99f;
    weights->write(&weightValues[0], 0, numWeights, 0).wait();
    for (idx_t i = 0; i < numWeights; i++) weightValues[i] = 0.0f;
    weights->read(0, numWeights, &weightValues[0], 0).wait();
    for (float v : weightValues)
    {
        BOOST_REQUIRE_EQUAL(0.99f, v);
    }

    mlp->get_weights(weights);
    weights->read(0, numWeights, &weightValues[0], 0).wait();
    for (float v : weightValues)
    {
        BOOST_REQUIRE_EQUAL(0.11f, v);
    }
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

float normalize(float value, float min, float max)
{
    return ((value - min) / (max - min)) * 2.0f - 1.0f;
}

data_array_ptr to_data_array(const computation_context_ptr& ctx, const vector<float>& values)
{
    return ctx->data_array_factory()->create(&values[0], 0, values.size());
}

void runner(const string& name, const computation_context_ptr& ctx, const multilayer_perceptron_ptr& mlp, supervised_batch& batch, int maxIterations)
{
    auto errors = ctx->data_array_factory()->create(maxIterations);
    vector<float> mses(maxIterations);

    bool first = true;
    boost::chrono::high_resolution_clock::time_point start;
    for (idx_t it = 0; it < maxIterations; it++)
    {
        mlp->training(batch);

        if (first)
        {
            auto weights = ctx->data_array_factory()->create(mlp->number_of_weights());
            mlp->get_weights(weights);
            vector<float> weightValues(weights->size());
            weights->read(0, weights->size(), &weightValues[0], 0).wait();
            BOOST_REQUIRE((from(weightValues) >> sum()) != 0.0f);
            first = false;

            start = boost::chrono::high_resolution_clock::now();
        }

        ctx->utils()->calculate_mse(batch, errors, it);
    }

    errors->read(0, maxIterations, &mses[0], 0).wait();

    boost::chrono::duration<double> sec = boost::chrono::high_resolution_clock::now() - start;

    float lastMse = 0.0f;
    stringstream s;
    s << name << ":" << endl << "Ellapsed: " << sec << endl;
    for (float mse : mses)
    {
        s << "Error: " << mse << endl;
        lastMse = mse;
    }
    BOOST_REQUIRE(lastMse < 0.0001f);

    BOOST_TEST_MESSAGE(s.str());
}

void do_compute(const computation_context_ptr& ctx)
{
    auto mlp = create_mlp(ctx);
    idx_t numWeights = mlp->number_of_weights();
    auto weights = ctx->data_array_factory()->create(numWeights);
    vector<float> weightValues(numWeights);
    for (idx_t i = 0; i < numWeights; i++) weightValues[i] = 0.11f;
    weights->write(&weightValues[0], 0, numWeights, 0).wait();
    mlp->set_weights(weights);

    idx_t inputsSize = mlp->input_size();
    auto inputs = ctx->data_array_factory()->create(inputsSize);
    vector<float> inputValues(inputsSize);
    for (idx_t i = 0; i < inputsSize; i++) inputValues[i] = 0.22f;
    inputs->write(&inputValues[0], 0, inputsSize, 0).wait();

    idx_t outputsSize = mlp->output_size();
    auto outputs = ctx->data_array_factory()->create(outputsSize);
    vector<float> outputValues(outputsSize);

    outputs->read(0, outputsSize, &outputValues[0], 0).wait();
    for (idx_t i = 0; i < outputsSize; i++)
    {
        BOOST_REQUIRE_EQUAL(0.0f, outputValues[i]);
    }

    mlp->compute(inputs, outputs);

    outputs->read(0, outputsSize, &outputValues[0], 0).wait();
    for (idx_t i = 0; i < outputsSize; i++)
    {
        BOOST_REQUIRE(outputValues[i] != 0.0f);
    }
}

void do_gd_ff_training(const string& ctxName, const computation_context_ptr& ctx, float rndStrength, bool online, float rate)
{
    auto mlp = create_ff_mlp_with_training(ctx, rndStrength, online, rate);

    const float maxInput = 4.0f;
    const float minInput = -4.0f;
    const float maxOutput = 16.0f;
    const float minOutput = 0.0f;
    supervised_batch batch;
    batch.push_back(
        to_data_array(ctx, vector<float>({ normalize(-4.0f, minInput, maxInput) })),
        to_data_array(ctx, vector<float>({ normalize(16.0f, minOutput, maxOutput) })),
        ctx->data_array_factory()->create(1));
    batch.push_back(
        to_data_array(ctx, vector<float>({ normalize(-3.0f, minInput, maxInput) })),
        to_data_array(ctx, vector<float>({ normalize(9.0f, minOutput, maxOutput) })),
        ctx->data_array_factory()->create(1));
    batch.push_back(
        to_data_array(ctx, vector<float>({ normalize(-2.0f, minInput, maxInput) })),
        to_data_array(ctx, vector<float>({ normalize(4.0f, minOutput, maxOutput) })),
        ctx->data_array_factory()->create(1));
    batch.push_back(
        to_data_array(ctx, vector<float>({ normalize(-1.0f, minInput, maxInput) })),
        to_data_array(ctx, vector<float>({ normalize(1.0f, minOutput, maxOutput) })),
        ctx->data_array_factory()->create(1));
    batch.push_back(
        to_data_array(ctx, vector<float>({ normalize(0.0f, minInput, maxInput) })),
        to_data_array(ctx, vector<float>({ normalize(0.0f, minOutput, maxOutput) })),
        ctx->data_array_factory()->create(1));
    batch.push_back(
        to_data_array(ctx, vector<float>({ normalize(1.0f, minInput, maxInput) })),
        to_data_array(ctx, vector<float>({ normalize(1.0f, minOutput, maxOutput) })),
        ctx->data_array_factory()->create(1));
    batch.push_back(
        to_data_array(ctx, vector<float>({ normalize(2.0f, minInput, maxInput) })),
        to_data_array(ctx, vector<float>({ normalize(4.0f, minOutput, maxOutput) })),
        ctx->data_array_factory()->create(1));
    batch.push_back(
        to_data_array(ctx, vector<float>({ normalize(3.0f, minInput, maxInput) })),
        to_data_array(ctx, vector<float>({ normalize(9.0f, minOutput, maxOutput) })),
        ctx->data_array_factory()->create(1));
    batch.push_back(
        to_data_array(ctx, vector<float>({ normalize(4.0f, minInput, maxInput) })),
        to_data_array(ctx, vector<float>({ normalize(16.0f, minOutput, maxOutput) })),
        ctx->data_array_factory()->create(1));

    runner((online ? string("Online ") : string("Offline ")) + ctxName + " Feed-Forward GD Training", ctx, mlp, batch, 1000);
}

void do_gd_rec_training(const string& ctxName, const computation_context_ptr& ctx, float rndStrength, bool online, float rate, gradient_computation_method gcm)
{
    auto mlp = create_rec_mlp_with_training(ctx, rndStrength, online, rate, gcm);

    supervised_batch batch;

    supervised_sample sample;
    sample.push_back(
        to_data_array(ctx, vector<float>({ -1.0f })),
        null,
        null);
    sample.push_back(
        to_data_array(ctx, vector<float>({ -1.0f })),
        null,
        null);
    sample.push_back(
        to_data_array(ctx, vector<float>({ -1.0f })),
        to_data_array(ctx, vector<float>({ -1.0f, -1.0f, -1.0f })),
        ctx->data_array_factory()->create(3));
    batch.push_back(sample);

    sample = supervised_sample();
    sample.push_back(
        to_data_array(ctx, vector<float>({ -1.0f })),
        null,
        null);
    sample.push_back(
        to_data_array(ctx, vector<float>({ -1.0f })),
        null,
        null);
    sample.push_back(
        to_data_array(ctx, vector<float>({ 1.0f })),
        to_data_array(ctx, vector<float>({ -1.0f, -1.0f, 1.0f })),
        ctx->data_array_factory()->create(3));
    batch.push_back(sample);

    sample = supervised_sample();
    sample.push_back(
        to_data_array(ctx, vector<float>({ -1.0f })),
        null,
        null);
    sample.push_back(
        to_data_array(ctx, vector<float>({ 1.0f })),
        null,
        null);
    sample.push_back(
        to_data_array(ctx, vector<float>({ -1.0f })),
        to_data_array(ctx, vector<float>({ -1.0f, 1.0f, -1.0f })),
        ctx->data_array_factory()->create(3));
    batch.push_back(sample);

    sample = supervised_sample();
    sample.push_back(
        to_data_array(ctx, vector<float>({ -1.0f })),
        null,
        null);
    sample.push_back(
        to_data_array(ctx, vector<float>({ 1.0f })),
        null,
        null);
    sample.push_back(
        to_data_array(ctx, vector<float>({ 1.0f })),
        to_data_array(ctx, vector<float>({ -1.0f, 1.0f, 1.0f })),
        ctx->data_array_factory()->create(3));
    batch.push_back(sample);

    sample = supervised_sample();
    sample.push_back(
        to_data_array(ctx, vector<float>({ 1.0f })),
        null,
        null);
    sample.push_back(
        to_data_array(ctx, vector<float>({ -1.0f })),
        null,
        null);
    sample.push_back(
        to_data_array(ctx, vector<float>({ -1.0f })),
        to_data_array(ctx, vector<float>({ 1.0f, -1.0f, -1.0f })),
        ctx->data_array_factory()->create(3));
    batch.push_back(sample);

    sample = supervised_sample();
    sample.push_back(
        to_data_array(ctx, vector<float>({ 1.0f })),
        null,
        null);
    sample.push_back(
        to_data_array(ctx, vector<float>({ -1.0f })),
        null,
        null);
    sample.push_back(
        to_data_array(ctx, vector<float>({ 1.0f })),
        to_data_array(ctx, vector<float>({ 1.0f, -1.0f, 1.0f })),
        ctx->data_array_factory()->create(3));
    batch.push_back(sample);

    sample = supervised_sample();
    sample.push_back(
        to_data_array(ctx, vector<float>({ 1.0f })),
        null,
        null);
    sample.push_back(
        to_data_array(ctx, vector<float>({ 1.0f })),
        null,
        null);
    sample.push_back(
        to_data_array(ctx, vector<float>({ -1.0f })),
        to_data_array(ctx, vector<float>({ 1.0f, 1.0f, -1.0f })),
        ctx->data_array_factory()->create(3));
    batch.push_back(sample);

    sample = supervised_sample();
    sample.push_back(
        to_data_array(ctx, vector<float>({ 1.0f })),
        null,
        null);
    sample.push_back(
        to_data_array(ctx, vector<float>({ 1.0f })),
        null,
        null);
    sample.push_back(
        to_data_array(ctx, vector<float>({ 1.0f })),
        to_data_array(ctx, vector<float>({ 1.0f, 1.0f, 1.0f })),
        ctx->data_array_factory()->create(3));
    batch.push_back(sample);

    string onlineStr((online ? string("Online ") : string("Offline ")));
    string gcmStr((gcm == nf::gradient_computation_method::bptt) ? string("BPTT") : string("RTLR"));

    runner(onlineStr + ctxName + " Feed-Forward GD " + gcmStr + " Training", ctx, mlp, batch, gcm == gradient_computation_method::rtlr ? 30 : 1000);
}

BOOST_AUTO_TEST_CASE(cpp_compute)
{
    auto ctx = computation_context_factory().create_context(cpp_context);
    do_compute(ctx);
}

BOOST_AUTO_TEST_CASE(cpp_gd_ff_online_training)
{
    auto ctx = computation_context_factory().create_context(cpp_context);
    do_gd_ff_training("CPP", ctx, 0.3f, true, 0.1f);
}

BOOST_AUTO_TEST_CASE(cpp_gd_ff_offline_training)
{
    auto ctx = computation_context_factory().create_context(cpp_context);
    do_gd_ff_training("CPP", ctx, 0.3f, false, 0.1f);
}

BOOST_AUTO_TEST_CASE(cpp_gd_rtlr_online_training)
{
    auto ctx = computation_context_factory().create_context(cpp_context);
    do_gd_rec_training("CPP", ctx, 0.3f, true, 0.01f, gradient_computation_method::rtlr);
}

BOOST_AUTO_TEST_CASE(cpp_gd_rtlr_offline_training)
{
    auto ctx = computation_context_factory().create_context(cpp_context);
    do_gd_rec_training("CPP", ctx, 0.3f, false, 0.01f, gradient_computation_method::rtlr);
}

BOOST_AUTO_TEST_CASE(cpp_gd_bptt_online_training)
{
    auto ctx = computation_context_factory().create_context(cpp_context);
    do_gd_rec_training("CPP", ctx, 0.3f, true, 0.01f, gradient_computation_method::bptt);
}

BOOST_AUTO_TEST_CASE(cpp_gd_bptt_offline_training)
{
    auto ctx = computation_context_factory().create_context(cpp_context);
    do_gd_rec_training("CPP", ctx, 0.3f, false, 0.01f, gradient_computation_method::bptt);
}

BOOST_AUTO_TEST_SUITE_END()
