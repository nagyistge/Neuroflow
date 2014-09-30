import aliases;
import std.range;
import std.algorithm;
import std.array;
import layer;
import layerbehavior;
import learningbehavior;
import std.container;
import std.typecons;
import std.conv;

size_t[][T] collectLearningBehaviors(T)(IndexedLayer[] layers)
if (is (T : LearningBehavior))
{
	size_t[][T] result;
	foreach (layer; layers)
	{
		foreach (b; layer[1].behaviors[])
		{
			auto behavior = cast(T)(b);
			if (behavior !is null)
			{
				auto indexes = result.get(behavior, null);
				if (indexes is null)
				{
					result[behavior] = [ layer[0] ];
				}
				else if (indexes.filter!(i => i == layer[0]).takeOne.empty)
				{
					result[behavior] ~= layer[0];
				}
			}
		}
	}
	return result;
}

unittest //collectLearningBehaviors
{
	import randomizeweightsuniform;
	import gradientdescentlearning;
	
	auto b1 = new RandomizeWeightsUniform(1.0f);
	auto b2 = new RandomizeWeightsUniform(2.0f);
	auto b3 = new GradientDescentLearning(1.0f, 1.0f, SupervisedWeightUpdateMode.online);
	auto b4 = new GradientDescentLearning(1.0f, 1.0f, SupervisedWeightUpdateMode.offline);
	auto b4alt = new GradientDescentLearning(1.0f, 1.0f, SupervisedWeightUpdateMode.offline);

	auto layers = 
	[ 
		tuple(to!size_t(0), new Layer(1, b1)), 
        tuple(to!size_t(1), new Layer(1, b2)), 
        tuple(to!size_t(2), new Layer(1, b3)),
        tuple(to!size_t(3), new Layer(1, b4)), 
        tuple(to!size_t(4), new Layer(1, b1)), 
        tuple(to!size_t(5), new Layer(1, b2)), 
        tuple(to!size_t(6), new Layer(1, b3)), 
        tuple(to!size_t(7), new Layer(1, b4)), 
        tuple(to!size_t(8), new Layer(1, b4alt))
	];
	layers = chain(layers, layers, layers).array;

	auto collectedRWU = collectLearningBehaviors!RandomizeWeightsUniform(layers);

	assert(collectedRWU.length == 2);
    assert(collectedRWU[b1] == [ to!size_t(0), to!size_t(4) ]);
    assert(collectedRWU[b2] == [ to!size_t(1), to!size_t(5) ]);

	auto collectedGDL = collectLearningBehaviors!GradientDescentLearning(layers);

	assert(collectedGDL.length == 2);
    assert(collectedGDL[b3] == [ to!size_t(2), to!size_t(6) ]);
    assert(collectedGDL[b4] == [ to!size_t(3), to!size_t(7), to!size_t(8) ]);
}