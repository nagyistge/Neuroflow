#include "stdafx.h"
#include "CppUnitTest.h"

#include <nf.h>
#include <layer_order_comparer.h>
#include <linqlike.h>

using namespace Microsoft::VisualStudio::CppUnitTestFramework;
using namespace std;
using namespace nf;
using namespace linqlike;

namespace NeuroflowNativeUT
{
	TEST_CLASS(multilayer_perceptron_tests)
	{
	public:

        BEGIN_TEST_METHOD_ATTRIBUTE(layer_order_comparer_test)
            TEST_METHOD_ATTRIBUTE(L"Category", L"MLP")
            TEST_METHOD_ATTRIBUTE(L"Platform", L"CPP")
        END_TEST_METHOD_ATTRIBUTE()
        TEST_METHOD(layer_order_comparer_test)
        {
            vector<layer_ptr> layers =
            {
                make_layer(2),
                make_layer(4),
                make_layer(1)
            };

            layers[0]->output_connections().add_one_way(layers[1]);
            layers[1]->output_connections().add_one_way(layers[2]);

            Assert::AreEqual(idx_t(0), layers[0]->input_connections().connected_layers(flow_direction::all) | size());
            Assert::AreEqual(idx_t(1), layers[0]->output_connections().connected_layers(flow_direction::all) | size());

            auto sorted = layers | sort(layer_order_comparer()) | row_num() | to_vector();

            Assert::AreEqual(idx_t(3), sorted.size());
            Assert::AreEqual(idx_t(0), sorted[0].row_num());
            Assert::AreEqual(idx_t(1), sorted[1].row_num());
            Assert::AreEqual(idx_t(2), sorted[2].row_num());
        }
		
        BEGIN_TEST_METHOD_ATTRIBUTE(cpp_get_and_set_weights)
            TEST_METHOD_ATTRIBUTE(L"Category", L"MLP")
            TEST_METHOD_ATTRIBUTE(L"Platform", L"CPP")
            END_TEST_METHOD_ATTRIBUTE()
		TEST_METHOD(cpp_get_and_set_weights)
		{
            try
            {
                auto ctx = computation_context_factory().create_context(cpp_context);
                do_get_and_set_weights(ctx);
            }
            catch (exception& ex)
            {
                Logger::WriteMessage(ex.what());
                throw;
            }
		}

        BEGIN_TEST_METHOD_ATTRIBUTE(ocl_get_and_set_weights_cpu)
            TEST_METHOD_ATTRIBUTE(L"Category", L"MLP")
            TEST_METHOD_ATTRIBUTE(L"Platform", L"OCL")
            TEST_METHOD_ATTRIBUTE(L"Device", L"OCL CPU")
        END_TEST_METHOD_ATTRIBUTE()
        TEST_METHOD(ocl_get_and_set_weights_cpu)
        {
            try
            {
                auto ctx = computation_context_factory().create_context(ocl_context, L"CPU");
                do_get_and_set_weights(ctx);
            }
            catch (exception& ex)
            {
                Logger::WriteMessage(ex.what());
                throw;
            }
        }

        BEGIN_TEST_METHOD_ATTRIBUTE(ocl_get_and_set_weights_gpu)
            TEST_METHOD_ATTRIBUTE(L"Category", L"MLP")
            TEST_METHOD_ATTRIBUTE(L"Platform", L"OCL")
            TEST_METHOD_ATTRIBUTE(L"Device", L"OCL GPU")
        END_TEST_METHOD_ATTRIBUTE()
        TEST_METHOD(ocl_get_and_set_weights_gpu)
        {
            try
            {
                auto ctx = computation_context_factory().create_context(ocl_context, L"GPU");
                do_get_and_set_weights(ctx);
            }
            catch (exception& ex)
            {
                Logger::WriteMessage(ex.what());
                throw;
            }
        }

        BEGIN_TEST_METHOD_ATTRIBUTE(cpp_compute)
            TEST_METHOD_ATTRIBUTE(L"Category", L"MLP")
            TEST_METHOD_ATTRIBUTE(L"Platform", L"CPP")
        END_TEST_METHOD_ATTRIBUTE()
        TEST_METHOD(cpp_compute)
        {
            try
            {
                auto ctx = computation_context_factory().create_context(cpp_context);
                do_compute(ctx);
            }
            catch (exception& ex)
            {
                Logger::WriteMessage(ex.what());
                throw;
            }
        }

        BEGIN_TEST_METHOD_ATTRIBUTE(cpp_gd_ff_online_training)
            TEST_METHOD_ATTRIBUTE(L"Category", L"MLP")
            TEST_METHOD_ATTRIBUTE(L"Platform", L"CPP")
        END_TEST_METHOD_ATTRIBUTE()
        TEST_METHOD(cpp_gd_ff_online_training)
        {
            try
            {
                auto ctx = computation_context_factory().create_context(cpp_context);
                do_gd_ff_training(ctx, 0.3f, true, 0.1f);
            }
            catch (exception& ex)
            {
                Logger::WriteMessage(ex.what());
                throw;
            }
        }

        BEGIN_TEST_METHOD_ATTRIBUTE(cpp_gd_ff_offline_training)
            TEST_METHOD_ATTRIBUTE(L"Category", L"MLP")
            TEST_METHOD_ATTRIBUTE(L"Platform", L"CPP")
        END_TEST_METHOD_ATTRIBUTE()
        TEST_METHOD(cpp_gd_ff_offline_training)
        {
            try
            {
                auto ctx = computation_context_factory().create_context(cpp_context);
                do_gd_ff_training(ctx, 0.3f, false, 0.1f);
            }
            catch (exception& ex)
            {
                Logger::WriteMessage(ex.what());
                throw;
            }
        }

        BEGIN_TEST_METHOD_ATTRIBUTE(cpp_gd_rtlr_online_training)
            TEST_METHOD_ATTRIBUTE(L"Category", L"MLP")
            TEST_METHOD_ATTRIBUTE(L"Platform", L"CPP")
        END_TEST_METHOD_ATTRIBUTE()
        TEST_METHOD(cpp_gd_rtlr_online_training)
        {
            try
            {
                auto ctx = computation_context_factory().create_context(cpp_context);
                do_gd_rec_training(ctx, 0.3f, true, 0.01f, gradient_computation_method::rtlr);
            }
            catch (exception& ex)
            {
                Logger::WriteMessage(ex.what());
                throw;
            }
        }

        BEGIN_TEST_METHOD_ATTRIBUTE(cpp_gd_rtlr_offline_training)
            TEST_METHOD_ATTRIBUTE(L"Category", L"MLP")
            TEST_METHOD_ATTRIBUTE(L"Platform", L"CPP")
            END_TEST_METHOD_ATTRIBUTE()
            TEST_METHOD(cpp_gd_rtlr_offline_training)
        {
            try
            {
                auto ctx = computation_context_factory().create_context(cpp_context);
                do_gd_rec_training(ctx, 0.3f, false, 0.01f, gradient_computation_method::rtlr);
            }
            catch (exception& ex)
            {
                Logger::WriteMessage(ex.what());
                throw;
            }
        }

        BEGIN_TEST_METHOD_ATTRIBUTE(cpp_gd_bptt_online_training)
            TEST_METHOD_ATTRIBUTE(L"Category", L"MLP")
            TEST_METHOD_ATTRIBUTE(L"Platform", L"CPP")
        END_TEST_METHOD_ATTRIBUTE()
        TEST_METHOD(cpp_gd_bptt_online_training)
        {
            try
            {
                auto ctx = computation_context_factory().create_context(cpp_context);
                do_gd_rec_training(ctx, 0.3f, true, 0.01f, gradient_computation_method::bptt);
            }
            catch (exception& ex)
            {
                Logger::WriteMessage(ex.what());
                throw;
            }
        }

        BEGIN_TEST_METHOD_ATTRIBUTE(cpp_gd_bptt_offline_training)
            TEST_METHOD_ATTRIBUTE(L"Category", L"MLP")
            TEST_METHOD_ATTRIBUTE(L"Platform", L"CPP")
        END_TEST_METHOD_ATTRIBUTE()
        TEST_METHOD(cpp_gd_bptt_offline_training)
        {
            try
            {
                auto ctx = computation_context_factory().create_context(cpp_context);
                do_gd_rec_training(ctx, 0.3f, false, 0.01f, gradient_computation_method::bptt);
            }
            catch (exception& ex)
            {
                Logger::WriteMessage(ex.what());
                throw;
            }
        }

        void do_get_and_set_weights(const computation_context_ptr& ctx)
        {
            auto mlp = create_mlp(ctx);
            idx_t numWeights = mlp->number_of_weights();
            auto weights = ctx->data_array_factory()->create(numWeights);
            vector<float> weightValues(numWeights);
            mlp->get_weights(weights);
            weights->read(0, numWeights, &weightValues[0], 0).wait();
            for (float v : weightValues) Assert::AreEqual(0.0f, v);

            for (idx_t i = 0; i < numWeights; i++) weightValues[i] = 0.11f;
            weights->write(&weightValues[0], 0, numWeights, 0).wait();
            mlp->set_weights(weights);

            for (idx_t i = 0; i < numWeights; i++) weightValues[i] = 0.99f;
            weights->write(&weightValues[0], 0, numWeights, 0).wait();
            for (idx_t i = 0; i < numWeights; i++) weightValues[i] = 0.0f;
            weights->read(0, numWeights, &weightValues[0], 0).wait();
            for (float v : weightValues) Assert::AreEqual(0.99f, v);

            mlp->get_weights(weights);
            weights->read(0, numWeights, &weightValues[0], 0).wait();
            for (float v : weightValues) Assert::AreEqual(0.11f, v);
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
                Assert::AreEqual(0.0f, outputValues[i]);
            }

            mlp->compute(inputs, outputs);

            outputs->read(0, outputsSize, &outputValues[0], 0).wait();
            for (idx_t i = 0; i < outputsSize; i++)
            {
                Assert::AreNotEqual(0.0f, outputValues[i]);
            }
        }

        void do_gd_ff_training(const computation_context_ptr& ctx, float rndStrength, bool online, float rate)
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

            runner(ctx, mlp, batch, 1000);
        }

        void do_gd_rec_training(const computation_context_ptr& ctx, float rndStrength, bool online, float rate, gradient_computation_method gcm)
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

            runner(ctx, mlp, batch, gcm == gradient_computation_method::rtlr ? 30 : 1000);
        }

        void runner(const computation_context_ptr& ctx, const multilayer_perceptron_ptr& mlp, supervised_batch& batch, int maxIterations)
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
                    Assert::IsTrue((from(weightValues) | sum()) != 0.0f);
                    first = false;

                    start = boost::chrono::high_resolution_clock::now();
                }

                ctx->utils()->calculate_mse(batch, errors, it);
            }

            errors->read(0, maxIterations, &mses[0], 0).wait();

            boost::chrono::duration<double> sec = boost::chrono::high_resolution_clock::now() - start;

            float lastMse = 0.0f;
            stringstream s;
            s << "Ellapsed: " << sec << endl;
            for (float mse : mses)
            {
                s << "Error: " << mse << endl;
                lastMse = mse;
            }
            //Assert::IsTrue(lastMse < 0.0001f);

            Logger::WriteMessage(s.str().c_str());
        }

        static float normalize(float value, float min, float max)
        {
            return ((value - min) / (max - min)) * 2.0f - 1.0f;
        }

        static data_array_ptr to_data_array(const computation_context_ptr& ctx, vector<float>& values)
        {
            return ctx->data_array_factory()->create(&values[0], 0, values.size());
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
            Assert::AreEqual(idx_t((2 * 4 + 4) + (4 * 1 + 1)), numWeights);

            return move(mlp);
        }

        static multilayer_perceptron_ptr create_ff_mlp_with_training(const computation_context_ptr& ctx, float rndStrength, bool online, float rate)
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

        static multilayer_perceptron_ptr create_rec_mlp_with_training(const computation_context_ptr& ctx, float rndStrength, bool online, float rate, nf::gradient_computation_method gcm)
        {
#if _DEBUG
            idx_t hs = 8;
#else
            idx_t hs = gcm == gradient_computation_method::rtlr ? 32 : 8;
#endif
            auto wrnd = make_randomize_weights_uniform(rndStrength);
            auto algo = make_gradient_descent_learning(rate, online ? 0.25f : 0.8f, false, online ? weight_update_mode::online : weight_update_mode::offline);
            vector<layer_ptr> layers =
            {
                make_layer(1),
                make_layer(hs, make_activation_description(activation_function::sigmoid, 1.7f), wrnd, algo),
                make_layer(hs, make_activation_description(activation_function::sigmoid, 1.7f), wrnd, algo),
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
	};
}