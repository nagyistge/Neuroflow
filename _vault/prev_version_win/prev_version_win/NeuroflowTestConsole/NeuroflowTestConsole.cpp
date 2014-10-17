// NeuroflowTestConsole.cpp : Defines the entry point for the console application.
//

#include "stdafx.h"
#include "nf.h"

using namespace std;
using namespace nf;
using namespace linqlike;

float normalize(float value, float min, float max)
{
    return ((value - min) / (max - min)) * 2.0f - 1.0f;
}

data_array_ptr to_data_array(const computation_context_ptr& ctx, vector<float>& values)
{
    return ctx->data_array_factory()->create(&values[0], 0, values.size());
}

static multilayer_perceptron_ptr create_mlp_with_training(const computation_context_ptr& ctx, float rndStrength, bool online, float rate)
{
    auto wrnd = make_randomize_weights_uniform(rndStrength);
    auto algo = make_gradient_descent_learning(rate, 0.8f, false, online ? weight_update_mode::online : weight_update_mode::offline);
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

int _tmain(int argc, _TCHAR* argv[])
{
    auto ctx = computation_context_factory().create_context(cpp_context);
    auto mlp = create_mlp_with_training(ctx, 0.3f, true, 0.1f);

    const float maxInput = 4.0f;
    const float minInput = -4.0f;
    const float maxOutput = 16.0f;
    const float minOutput = 0.0f;
    supervised_batch batch;
    /*batch.push_back(
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
    ctx->data_array_factory()->create(1));*/

    batch.push_back(
        to_data_array(ctx, vector<float>({ normalize(-4.0f, minInput, maxInput) })),
        to_data_array(ctx, vector<float>({ 0.0f })),
        ctx->data_array_factory()->create(1));
    batch.push_back(
        to_data_array(ctx, vector<float>({ normalize(-3.0f, minInput, maxInput) })),
        to_data_array(ctx, vector<float>({ 0.0f })),
        ctx->data_array_factory()->create(1));
    batch.push_back(
        to_data_array(ctx, vector<float>({ normalize(-2.0f, minInput, maxInput) })),
        to_data_array(ctx, vector<float>({ 0.0f })),
        ctx->data_array_factory()->create(1));
    batch.push_back(
        to_data_array(ctx, vector<float>({ normalize(-1.0f, minInput, maxInput) })),
        to_data_array(ctx, vector<float>({ 0.0f })),
        ctx->data_array_factory()->create(1));
    batch.push_back(
        to_data_array(ctx, vector<float>({ normalize(0.0f, minInput, maxInput) })),
        to_data_array(ctx, vector<float>({ 0.0f })),
        ctx->data_array_factory()->create(1));
    batch.push_back(
        to_data_array(ctx, vector<float>({ normalize(1.0f, minInput, maxInput) })),
        to_data_array(ctx, vector<float>({ 0.0f })),
        ctx->data_array_factory()->create(1));
    batch.push_back(
        to_data_array(ctx, vector<float>({ normalize(2.0f, minInput, maxInput) })),
        to_data_array(ctx, vector<float>({ 0.0f })),
        ctx->data_array_factory()->create(1));
    batch.push_back(
        to_data_array(ctx, vector<float>({ normalize(3.0f, minInput, maxInput) })),
        to_data_array(ctx, vector<float>({ 0.0f })),
        ctx->data_array_factory()->create(1));
    batch.push_back(
        to_data_array(ctx, vector<float>({ normalize(4.0f, minInput, maxInput) })),
        to_data_array(ctx, vector<float>({ 0.0f })),
        ctx->data_array_factory()->create(1));

    const idx_t maxIterations = 1000;
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
            first = false;

            start = boost::chrono::high_resolution_clock::now();
        }

        ctx->utils()->calculate_mse(batch, errors, it);
    }

    errors->read(0, maxIterations, &mses[0], 0).wait();

    boost::chrono::duration<double> sec = boost::chrono::high_resolution_clock::now() - start;

    cout << "Ellapsed: " << sec << endl;
    /*for (float mse : mses)
    {
        cout << "Error: " << mse << endl;
    }*/

    return 0;
}