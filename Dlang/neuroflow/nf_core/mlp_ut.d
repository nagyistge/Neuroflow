import layer;
import std.range;

unittest
{
    auto layers = [ new Layer(2), new Layer(4), new Layer(1) ];

    layers[0].outputConnections.addOneWay(layers[1]);
    layers[1].outputConnections.addOneWay(layers[2]);

    assert(layers[0].inputConnections.connectedLayers(FlowDirection.oneWay).walkLength == 0);
    assert(layers[0].inputConnections.connectedLayers(FlowDirection.oneWayToSource).walkLength == 0);
    assert(layers[0].inputConnections.connectedLayers(FlowDirection.twoWay).walkLength == 0);
    assert(layers[0].outputConnections.connectedLayers(FlowDirection.oneWay).walkLength == 1);
    assert(layers[0].outputConnections.connectedLayers(FlowDirection.oneWayToSource).walkLength == 0);
    assert(layers[0].outputConnections.connectedLayers(FlowDirection.twoWay).walkLength == 0);

    auto copyOfLayers = layers.dup;
}