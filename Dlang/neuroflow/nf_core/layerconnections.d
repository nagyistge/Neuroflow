import layer;
import std.exception;
import std.algorithm;

enum FlowDirection
{
    oneWay,
    twoWay,
    oneWayToSource
}

class LayerConnections
{
    private enum ConnectionType { input, output };

    private struct OtherLayer
    {
        FlowDirection flowDirection;

        Layer layer;
    }

    void addOneWay(Layer layer)
    {
        add(layer, FlowDirection.oneWay);
    }

    void addTwoWay(Layer layer)
    {
        add(layer, FlowDirection.twoWay);
    }

    void addOneWayToSource(Layer layer)
    {
        add(layer, FlowDirection.oneWayToSource);
    }

    void add(Layer layer, FlowDirection direction)
    {
        enforce(layer, "Layer is null.");
        if (!_otherLayers.any!(ol => ol.layer == layer))
        {
            suppressed(
            layer, 
            () =>
            {
                if (_type == ConnectionType.output)
                {
                    layer.inputConnections.add(_connectedLayer, direction);
                }
                else
                {
                    layer.outputConnections.add(_connectedLayer, direction);
                }
            });            
        }
    }

    bool remove(Layer layer)
    {
        enforce(layer, "Layer is null.");
    }

    void clear();

    LayerConnections connectedLayers(FlowDirection direction) const;
    
    private this(ConnectionType type)
    {
        _type = type;
    }
    
    private OtherLayer[] _otherLayers;

    private bool _suppressOtherSideUpdate = false;

    private ConnectionType _type;

    private Layer _connectedLayer;
    
    private void init(Layer connectedLayer)
    {
        assert(mustInit);
        assert(connectedLayer);

        _connectedLayer = connectedLayer;
    }

    private bool mustInit() const
    {
        return _connectedLayer is null;
    }

    private void suppressed(Layer layer, in void delegate() method);
}