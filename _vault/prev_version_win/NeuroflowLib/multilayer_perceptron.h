#pragma once

#include "nfdev.h"
#include "contexted.h"
#include "device_array_group.h"
#include "device_array2_group.h"
#include "rtlr.h"
#include "activation_description.h"

namespace nf
{
    struct multilayer_perceptron : contexted<computation_context>, virtual nf_object
    {
        friend struct neural_network_factory;
        friend struct rtlr;

        typedef std::vector<linq::row_numbered<layer_ptr>> ordered_layers_t;

        gradient_computation_method gradient_computation_method() const;
        idx_t max_bptt_iterations() const;
        idx_t input_size() const;
        idx_t output_size() const;
        idx_t number_of_weights() const;
        void get_weights(const data_array_ptr& to) const;
        void set_weights(const data_array_ptr& from);
        void compute(const data_array_ptr& inputs, const data_array_ptr& outputs);
        void compute(const data_array_collection_t& inputs, const data_array_collection_t& outputs);
        void training(const data_array_ptr& input, const data_array_ptr& desiredOutputs, const data_array_ptr& actualOutputs);
        void training(const supervised_sample_entry& sampleEntry);
        void training(const supervised_sample& sample);
        void training(supervised_batch& batch);

    private:
        struct layer_info
        {
            layer_info(idx_t index, bool isOnline, bool isOffline, learning_algo_optimization_type optimization_type) :
                index(index),
                is_online(isOnline),
                is_offline(isOffline),
                optimization_type(optimization_type)
            { }

            idx_t index;
            bool is_online, is_offline;
            learning_algo_optimization_type optimization_type;

            bool operator==(const layer_info& other) const
            {
                return index == other.index;
            }
        };

        typedef std::vector<std::tuple<device_array_collection_t, boost::optional<device_array_collection_t>, boost::optional<device_array_collection_t>>> values_for_training_t;

        multilayer_perceptron(const computation_context_ptr& context, layers_t& layers, const mlp_init_pars* properties);

        nf::gradient_computation_method _gradientComputationMethod = nf::gradient_computation_method::feed_forward;
        idx_t _maxBpttIterations = 0;
        ordered_layers_t _layers;
        bool _isTrainingEnabled = false;
        bool _isGradientsCalculated = false;
        bool _doBackpropagate = false;
        bool _doFFBP = false;
        bool _doBPTT = false;
        bool _doRTLR = false;
        bool _isRecurrent = false;
        bool _calculateGlobalOnlineError = false;
        bool _calculateGlobalOfflineError = false;
        bool _isTrainingInitialized = false;
        boost::optional<rtlr> _rtlr;
        device_array* _netInputs;
        device_array_ptr _bpttNetInputs;
        device_array* _netOutputs;
        device_array_ptr _bpttNetOutputs;
        device_array* _netDesiredOutputs;
        device_array_ptr _bpttNetDesiredOutputs;
        device_array_ptr _globalOfflineErrors;
        device_array_ptr _globalOnlineErrors;
        nf_object_ptr _calculateGlobalErrorState;
        std::list<nf_object_ptr> _computationStates;
        nf_object_ptr _setOutputState;
        device_array_management_ptr _daMan;
        compute_activation_ptr _computeActivation;
        learning_impl_factory_ptr _learningImplFactory;
        device_array_pool_ptr _gradientsPool;
        device_array_pool_ptr _gradientSumsPool;
        device_array_group _outputs;
        device_array_group _netValueDerivates;
        device_array_group _biases;
        device_array_group _errors;
        device_array2_group _weights;
        device_array_group _biasGradients;
        device_array_group _biasGradientSums;
        device_array2_group _gradients;
        device_array2_group _gradientSums;
        std::function<void(idx_t)> _computeFunc;
        std::function<void(idx_t, gradient_computation_formula, idx_t)> _trainFunc;
        std::function<void()> _initLearningFunc;
        std::function<void(idx_t, const device_array_ptr&)> _offlineLearningFunc;
        std::function<void(const device_array_ptr&)> _onlineLearningFunc;

        void create_structure(std::map<idx_t, layer_info>& infos);
        void create_compute();
        void create_training(std::map<idx_t, layer_info>& infos);
        void create_impls();
        idx_t get_layer_index(const layer_ptr& layer);
        activation_description get_activation_desc(idx_t layerIndex);
        device_array* get_net_values(idx_t layerIndex) const;
        device_array* get_net_desired_outputs() const;
        void compute_sample_entry(const data_array_ptr& inputs, const data_array_ptr& outputs, const data_array_ptr& desiredOutputs, idx_t offset = 0);
        void setup_net_values(const data_array_ptr& inputs, const data_array_ptr& outputs, const data_array_ptr& desiredOutputs, idx_t offset);
        template<typename I>
        std::shared_ptr<I> create_learning_impl(const learning_behavior_ptr& behavior, const std::vector<idx_t>& forLayerIndexes, const values_for_training_t& values);
        void verify_training_enabled();
        void ensure_training_initialized();
        void feed_forward_training(supervised_batch& batch);
        void bppt_training(supervised_batch& batch);
        void rtlr_training(supervised_batch& batch);
        void global_optimization_training(supervised_batch& batch);
        const data_array_ptr& find_actual_output(supervised_sample& sample, idx_t entryIndex);

        void zero_global_offline_errors();
        void zero_errors();
        void zero_outputs();
        void zero_gradients();
        void zero_gradient_sums();
    };
}