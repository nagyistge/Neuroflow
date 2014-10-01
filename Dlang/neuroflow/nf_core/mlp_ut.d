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
import mlp;
import supervisedbatch;
import std.datetime;
import std.stdio;
import dataarray;

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
	auto algo = new GradientDescentLearning(rate, online ? 0.25f : 0.8f, online ? SupervisedWeightUpdateMode.online : SupervisedWeightUpdateMode.offline);

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
	auto algo = new GradientDescentLearning(rate, online ? 0.25f : 0.8f, online ? SupervisedWeightUpdateMode.online : SupervisedWeightUpdateMode.offline);

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

void doCompute(ComputationContext ctx)
{
    auto mlp = createMlp(ctx);
    size_t numWeights = mlp.numberOfWeights;
    auto weights = ctx.dataArrayFactory.create(numWeights);
    auto weightValues = new float[numWeights];
    for (size_t i = 0; i < numWeights; i++) weightValues[i] = 0.11f;
    weights.write(&weightValues[0], 0, numWeights, 0);
    mlp.setWeights(weights);

    size_t inputsSize = mlp.inputSize;
    auto inputs = ctx.dataArrayFactory.create(inputsSize);
    auto inputValues = new float[inputsSize];
    for (size_t i = 0; i < inputsSize; i++) inputValues[i] = 0.22f;
    inputs.write(&inputValues[0], 0, inputsSize, 0);

    size_t outputsSize = mlp.outputSize;
    auto outputs = ctx.dataArrayFactory.create(outputsSize);
    auto outputValues = new float[outputsSize];

    outputs.read(0, outputsSize, &outputValues[0], 0);
    for (size_t i = 0; i < outputsSize; i++)
    {
        assert(0.0f == outputValues[i]);
    }

    mlp.compute(inputs, outputs);

    outputs.read(0, outputsSize, &outputValues[0], 0);
    for (size_t i = 0; i < outputsSize; i++)
    {
        assert(outputValues[i] != 0.0f);
    }
}

unittest // Test compute
{
	// Native:
	auto ctx = ComputationContextFactory.instance.createContext(NativeContext);
	doCompute(ctx);
}

float normalize(float value, float min, float max)
{
    return ((value - min) / (max - min)) * 2.0f - 1.0f;
}

DataArray toDataArray(ComputationContext ctx, float[] slice)
{
	return ctx.dataArrayFactory.createConst(&(slice[0]), 0, slice.length);
}

void runner(string name, ComputationContext ctx, MLP mlp, SupervisedBatch batch, int maxIterations)
{
    auto errors = ctx.dataArrayFactory.create(maxIterations);
    auto mses = new float[maxIterations];

    bool first = true;
	StopWatch sw;
	
    for (size_t it = 0; it < maxIterations; it++)
    {
        mlp.training(batch);

        if (first)
        {
            auto weights = ctx.dataArrayFactory.create(mlp.numberOfWeights);
            mlp.getWeights(weights);
            auto weightValues = new float[weights.size];
            weights.read(0, weights.size, &weightValues[0], 0);
            assert(weightValues.sum!() != 0.0f);
            first = false;

            sw.start();
        }

        ctx.utils.calculateMSE(batch, errors, it);
    }

    errors.read(0, maxIterations, &mses[0], 0);

	sw.stop();

	writefln("%s:\nEllapsed: %s nsecs", name, sw.peek.nsecs);

    size_t bidx = maxIterations - 10;
    if (bidx < 0) bidx = 0;
    for (size_t i = bidx; i < mses.length; i++)
    {
        writefln("Iteration %s. Error: %s", i + 1, mses[i]);
    }
}

void doGDFFTraining(string ctxName, ComputationContext ctx, float rndStrength, bool online, float rate)
{
    auto mlp = createFFMlpWithTraining(ctx, rndStrength, online, rate);

    const float maxInput = 4.0f;
    const float minInput = -4.0f;
    const float maxOutput = 16.0f;
    const float minOutput = 0.0f;
    SupervisedBatch batch;
    batch.add(
				toDataArray(ctx, [ normalize(-4.0f, minInput, maxInput) ]),
				toDataArray(ctx, [ normalize(16.0f, minOutput, maxOutput) ]),
				ctx.dataArrayFactory.create(1));
    batch.add(
				toDataArray(ctx, [ normalize(-3.0f, minInput, maxInput) ]),
				toDataArray(ctx, [ normalize(9.0f, minOutput, maxOutput) ]),
				ctx.dataArrayFactory.create(1));
    batch.add(
				toDataArray(ctx, [ normalize(-2.0f, minInput, maxInput) ]),
				toDataArray(ctx, [ normalize(4.0f, minOutput, maxOutput) ]),
				ctx.dataArrayFactory.create(1));
    batch.add(
				toDataArray(ctx, [ normalize(-1.0f, minInput, maxInput) ]),
				toDataArray(ctx, [ normalize(1.0f, minOutput, maxOutput) ]),
				ctx.dataArrayFactory.create(1));
    batch.add(
				toDataArray(ctx, [ normalize(0.0f, minInput, maxInput) ]),
				toDataArray(ctx, [ normalize(0.0f, minOutput, maxOutput) ]),
				ctx.dataArrayFactory.create(1));
    batch.add(
				toDataArray(ctx, [ normalize(1.0f, minInput, maxInput) ]),
				toDataArray(ctx, [ normalize(1.0f, minOutput, maxOutput) ]),
				ctx.dataArrayFactory.create(1));
    batch.add(
				toDataArray(ctx, [ normalize(2.0f, minInput, maxInput) ]),
				toDataArray(ctx, [ normalize(4.0f, minOutput, maxOutput) ]),
				ctx.dataArrayFactory.create(1));
    batch.add(
				toDataArray(ctx, [ normalize(3.0f, minInput, maxInput) ]),
				toDataArray(ctx, [ normalize(9.0f, minOutput, maxOutput) ]),
				ctx.dataArrayFactory.create(1));
    batch.add(
				toDataArray(ctx, [ normalize(4.0f, minInput, maxInput) ]),
				toDataArray(ctx, [ normalize(16.0f, minOutput, maxOutput) ]),
				ctx.dataArrayFactory.create(1));

    runner((online ? "Online " : "Offline ") ~ ctxName ~ " Feed-Forward GD Training", ctx, mlp, batch, 1000);
}

unittest // Trainings
{
	// Native, GD, FF, Online
	auto ctx = ComputationContextFactory.instance.createContext(NativeContext);
    doGDFFTraining("CPP", ctx, 0.3f, true, 0.1f);
}