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

        friend struct neural_network_factory;

        typedef std::vector<linq::row_numbered<layer_ptr>> ordered_layers_t;

        const boost::property_tree::ptree& properties() const;
        gradient_computation_method gradient_computation_method() const;
        idx_t max_bptt_iterations() const;
        idx_t input_size() const;
        idx_t output_size() const;
        idx_t number_of_weights() const;
        void get_weights(const data_array_ptr& to) const;
        void set_weights(const data_array_ptr& from);
        void compute(const data_array_ptr& inputs, const data_array_ptr& outputs);
        void compute(const data_array_collection_t& inputs, const data_array_collection_t& outputs);
        void train(const data_array_ptr& input, const data_array_ptr& desiredOutputs, const data_array_ptr& actualOutputs);
        void train(const supervised_sample_entry& sampleEntry);
        void train(const supervised_sample& sample);
        void train(const supervised_batch& batch);

    private:
        multilayer_perceptron(const computation_context_ptr& context, layers_t& layers, const optional_properties_t& properties);

        properties_t _properties;
        nf::gradient_computation_method _gradientComputationMethod;
        idx_t _maxBpttIterations;
        ordered_layers_t _layers;
        bool _isTrainingEnabled;
        bool _isGradientsCalculated;
        bool _doBackpropagate;
        bool _doFFBP;
        bool _doBPTT;
        bool _doRTLR;
        bool _isRecurrent;
        bool _calculateGlobalOnlineError;
        bool _calculateGlobalOfflineError;
        bool _isTrainingInitialized;
        rtlr _rtlr;
        device_array_ptr _netInputs;
        device_array_ptr _netOutputs;
        device_array_ptr _globalOfflineError;
        device_array_ptr _globalOnlineError;
        nf_object_ptr _calculateGlobalErrorState;
        std::list<nf_object_ptr> _computationStates;
        nf_object_ptr _setOutputState;
        device_array_management_ptr _daMan;
        compute_activation_ptr _computeActivation;
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
        std::function<void()> computeFunc;

        void create_structure(std::map<idx_t, layer_info>& infos);
        void create_compute();
        idx_t get_layer_index(const layer_ptr& layer);
        activation_description get_activation_desc(idx_t layerIndex);
        const device_array_ptr& get_net_values(idx_t layerIndex) const;
        void compute_sample_entry(const device_array_ptr& inputs, const device_array_ptr& outputs);
        void setup_net_values(const device_array_ptr& inputs, const device_array_ptr& outputs);
    };
}