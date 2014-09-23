import layer;
import std.range;
import std.algorithm;
import layerordercomparer;
import std.array;

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
	auto comp = new LayerOrderComparer();

	copyOfLayers.sort!((a, b) => comp.less(a, b));
    
    assert(copyOfLayers[0].size == 4);
	assert(copyOfLayers[1].size == 2);
	assert(copyOfLayers[2].size == 1);

	// 2
	layers = [ new Layer(2), new Layer(4), new Layer(1) ];

    layers[2].outputConnections.addOneWay(layers[1]);
    layers[1].outputConnections.addOneWay(layers[0]);

    copyOfLayers = layers.dup;
	comp = new LayerOrderComparer();

	copyOfLayers.sort!((a, b) => comp.less(a, b));

    assert(copyOfLayers[0].size == 1);
	assert(copyOfLayers[1].size == 4);
	assert(copyOfLayers[2].size == 2);

	// 3
	layers = [ new Layer(2), new Layer(4), new Layer(1) ];

    layers[0].outputConnections.addOneWay(layers[1]);
    layers[1].outputConnections.addOneWay(layers[2]);

    copyOfLayers = layers.dup;
	comp = new LayerOrderComparer();

	copyOfLayers.sort!((a, b) => comp.less(a, b));

    assert(copyOfLayers[0].size == 2);
	assert(copyOfLayers[1].size == 4);
	assert(copyOfLayers[2].size == 1);
}