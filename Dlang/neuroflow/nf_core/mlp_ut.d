import layer;
import std.range;
import std.algorithm;
import std.conv;
import layerordercomparer;
import std.array;
import computationcontext;
import activationdescription;
import gradientdescentlearning;
import randomizeweightsuniform;
import mlpinitpars;
import computationcontextfactory;

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
	assert(((2 * 4 + 4) + (4 * 1 + 1)) == numWeights, "Number of weights is " ~ to!string(numWeights) ~ " which is wrong value.");
	
	return mlp;
}

auto createFFMlpWithTraining(ComputationContext ctx, float rndStrength, bool online, float rate)
{
	auto wrnd = new RandomizeWeightsUniform(rndStrength);
	auto algo = new GradientDescentLearning(rate, online ? 0.25f : 0.8f, false, online ? SupervisedWeightUpdateMode.online : SupervisedWeightUpdateMode.offline);

	auto layers =
	[
		new Layer(1),
		new Layer(16, new ActivationDescription(ActivationFunction.sigmoid, 1.7f), wrnd, algo),
		new Layer(16, new ActivationDescription(ActivationFunction.sigmoid, 1.7f), wrnd, algo),
		new Layer(1, new ActivationDescription(ActivationFunction.linear, 1.7f), wrnd, algo)
	];

	layers[0].outputConnections.addOneWay(layers[1]);
	layers[1].outputConnections.addOneWay(layers[2]);
	layers[2].outputConnections.addOneWay(layers[3]);

	auto mlp = ctx.neuralNetworkFactory.createMLP(layers);

	return mlp;
}

auto createRecMlpWithTraining(ComputationContext ctx, float rndStrength, bool online, float rate, GradientComputationMethod gcm)
{
	enum hidden1Size = 12;
	enum hidden2Size = 8;

	auto wrnd = new RandomizeWeightsUniform(rndStrength);
	auto algo = new GradientDescentLearning(rate, online ? 0.25f : 0.8f, false, online ? SupervisedWeightUpdateMode.online : SupervisedWeightUpdateMode.offline);

	auto layers =
	[
		new Layer(1),
		new Layer(hidden1Size, new ActivationDescription(ActivationFunction.sigmoid, 1.7f), wrnd, algo),
		new Layer(hidden2Size, new ActivationDescription(ActivationFunction.sigmoid, 1.7f), wrnd, algo),
		new Layer(1, new ActivationDescription(ActivationFunction.linear, 1.7f), wrnd, algo)
	];

	layers[0].outputConnections.addTwoWay(layers[1]);
	layers[1].outputConnections.addTwoWay(layers[2]);
	layers[2].outputConnections.addTwoWay(layers[3]);

	auto pars = new MLPInitPars();
	pars.gradientComputationMethod = gcm;

	auto mlp = ctx.neuralNetworkFactory.createMLP(layers, pars);

	return mlp;
}

void doGetAndSetWeights(ComputationContext ctx)
{
    auto mlp = createMlp(ctx);
    size_t numWeights = mlp.numberOfWeights;
    auto weights = ctx.dataArrayFactory.create(numWeights);
    float[] weightValues;
	weightValues.length = numWeights;

    mlp.getWeights(weights);

    weights.read(0, numWeights, &weightValues[0], 0);

    foreach (v; weightValues)
    {
        assert(v == 0.0);
    }

    for (size_t i = 0; i < numWeights; i++) weightValues[i] = 0.11f;
    weights.write(&weightValues[0], 0, numWeights, 0);
    mlp.setWeights(weights);

    for (size_t i = 0; i < numWeights; i++) weightValues[i] = 0.99f;
    weights.write(&weightValues[0], 0, numWeights, 0);
    for (size_t i = 0; i < numWeights; i++) weightValues[i] = 0.0f;
    weights.read(0, numWeights, &weightValues[0], 0);
    foreach (v; weightValues)
    {
        assert(0.99f == v);
    }

    mlp.getWeights(weights);
    weights.read(0, numWeights, &weightValues[0], 0);
    foreach (v; weightValues)
    {
        assert(0.11f == v);
    }
}

unittest
{
	// Test get and set weights

	// Native:
	auto ctx = ComputationContextFactory.instance.createContext(NativeContext);
	doGetAndSetWeights(ctx);
}