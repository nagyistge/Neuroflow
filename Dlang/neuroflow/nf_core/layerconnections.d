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
    enum ConnectionType { input, output };

    private struct OtherLayer
    {
        FlowDirection flowDirection;

        Layer layer;
    }

    this(ConnectionType type, Layer connectedLayer)
    {
        assert(connectedLayer);

        _type = type;
        _connectedLayer = connectedLayer;
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
            _otherLayers ~= OtherLayer(direction, layer);
        }
    }

    bool remove(Layer layer)
    {
        enforce(layer, "Layer is null.");

        auto len = _otherLayers.length;
        _otherLayers = .remove!(ol => ol.layer == layer)(_otherLayers);
        if (len != _otherLayers.length)
        {
            assert(_otherLayers.length + 1 == len);
            suppressed(
            layer, 
            {
                if (_type == ConnectionType.output)
                {
                    layer.inputConnections.remove(_connectedLayer);
                }
                else
                {
                    layer.outputConnections.remove(_connectedLayer);
                }
            }); 
            return true;
        }
        return false;
    }

    void clear()
    {
        foreach (ol;_otherLayers)
        {
            if (_type == ConnectionType.output)
            {
                ol.layer.inputConnections.remove(_connectedLayer);
            }
            else
            {
                ol.layer.outputConnections.remove(_connectedLayer);
            }
        }
        _otherLayers.length = 0;
    }

    auto connectedLayers(FlowDirection direction)
    {
        return _otherLayers.filter!(ol => ol.flowDirection == direction).map!(ol => ol.layer);
    }
    
    private OtherLayer[] _otherLayers;

    private bool _suppressOtherSideUpdate = false;

    private ConnectionType _type;

    private Layer _connectedLayer;

    private void suppressed(Layer layer, void delegate() method)
    {
        if (!_suppressOtherSideUpdate)
        {
            layer.inputConnections._suppressOtherSideUpdate = true;
            layer.inputConnections._suppressOtherSideUpdate = true;
            try
            {                
                method();
            }
            finally
            {
                layer.inputConnections._suppressOtherSideUpdate = false;
                layer.inputConnections._suppressOtherSideUpdate = false;
            }
        }
    }
}