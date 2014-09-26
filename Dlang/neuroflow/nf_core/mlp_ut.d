import layer;
import std.range;
import std.algorithm;
import layerordercomparer;
import std.array;
import computationcontext;
import activationdescription;

unittest
{
    // Layer order comparer tests

	// 1
    auto layers = [ new Layer(2), new Layer(4), new Layer(1) ];

    layers[1].outputConnections.addOneWay(layers[0]);
    layers[0].outputConnections.addOneWay(layers[2]);

    assert(layers[1].inputConnections.connectedLayers(FlowDirection.oneWay).walkLength == 0);
    assert(layers[1].inputConnections.connectedLayers(FlowDirection.oneWayToSource).walkLength == 0);
    assert(layers[1].inputConnections.connectedLayers(FlowDirection.twoWay).walkLength == 0);
    assert(layers[1].outputConnections.connectedLayers(FlowDirection.oneWay).walkLength == 1);
    assert(layers[1].outputConnections.connectedLayers(FlowDirection.oneWayToSource).walkLength == 0);
    assert(layers[1].outputConnections.connectedLayers(FlowDirection.twoWay).walkLength == 0);

    auto copyOfLayers = layers.dup;
	auto comp = LayerOrderComparer();

	copyOfLayers.sort!((a, b) => comp.less(a, b));
    
    assert(copyOfLayers[0].size == 4);
	assert(copyOfLayers[1].size == 2);
	assert(copyOfLayers[2].size == 1);

	// 2
	layers = [ new Layer(2), new Layer(4), new Layer(1) ];

    layers[2].outputConnections.addOneWay(layers[1]);
    layers[1].outputConnections.addOneWay(layers[0]);

    copyOfLayers = layers.dup;
	comp = LayerOrderComparer();

	copyOfLayers.sort!((a, b) => comp.less(a, b));

    assert(copyOfLayers[0].size == 1);
	assert(copyOfLayers[1].size == 4);
	assert(copyOfLayers[2].size == 2);

	// 3
	layers = [ new Layer(2), new Layer(4), new Layer(1) ];

    layers[0].outputConnections.addOneWay(layers[1]);
    layers[1].outputConnections.addOneWay(layers[2]);

    copyOfLayers = layers.dup;
	comp = LayerOrderComparer();

	copyOfLayers.sort!((a, b) => comp.less(a, b));

    assert(copyOfLayers[0].size == 2);
	assert(copyOfLayers[1].size == 4);
	assert(copyOfLayers[2].size == 1);
}

auto createMlp(ComputationContext ctx)
{
	auto layers =
	[
		new Layer(2),
		new Layer(4, new ActivationDescription(ActivationFunction.sigmoid, 1.7f)),
		new Layer(1, new ActivationDescription(ActivationFunction.linear, 1.7f))
	];

	layers[0].outputConnections.addOneWay(layers[1]);
	layers[1].outputConnections.addOneWay(layers[2]);

	auto mlp = ctx.neuralNetworkFactory.createMLP(layers);

	size_t numWeights = mlp.numberOfWeights();
	assert(((2 * 4 + 4) + (4 * 1 + 1)) == numWeights);
	
	return mlp;
}

/*

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
*/