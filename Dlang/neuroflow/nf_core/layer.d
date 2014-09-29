import layerbehavior;
import layerdescription;
public import layerconnections;
import std.container;
import std.algorithm;
import std.range;
import std.exception;

class Layer
{
    this(size_t size, in Object[] args ...)
    {
        _size = size;
        foreach (arg; args)
        {
            auto b = cast(LayerBehavior)arg;
            if (b)
            {
                _behaviors ~= b;
                continue;
            }
            auto d = cast(LayerDescription)arg;
            if (d)
            {
                _descriptions ~= d;
                continue;
            }
        }

        _inputConnections = new LayerConnections(LayerConnections.ConnectionType.input, this);        
        _outputConnections = new LayerConnections(LayerConnections.ConnectionType.output, this);
    }

    @property size_t size() const
    {
        return _size;
    }

    @property ref Array!LayerBehavior behaviors()
    {
        return _behaviors;
    }

    @property ref Array!LayerDescription descriptions()
    {
        return _descriptions;
    }

    @property LayerConnections inputConnections()
    {
        return _inputConnections;
    }

    @property LayerConnections outputConnections()
    {
        return _outputConnections;
    }

	bool hasRecurrentConnections() 
	{
		return !takeOne(_inputConnections.connectedLayers(FlowDirection.oneWayToSource)).empty ||
			!takeOne(_inputConnections.connectedLayers(FlowDirection.twoWay)).empty ||
			!takeOne(_outputConnections.connectedLayers(FlowDirection.oneWayToSource)).empty ||
			!takeOne(_outputConnections.connectedLayers(FlowDirection.twoWay)).empty;
	}

	auto inputLayers()
	{
		return _inputConnections.connectedLayers(FlowDirection.oneWay)
            .chain(_inputConnections.connectedLayers(FlowDirection.twoWay))
            .chain(_outputConnections.connectedLayers(FlowDirection.twoWay))
            .chain(_outputConnections.connectedLayers(FlowDirection.oneWayToSource));
	}

	auto outputLayers()
	{
        return _outputConnections.connectedLayers(FlowDirection.oneWay)
            .chain(_outputConnections.connectedLayers(FlowDirection.twoWay))
            .chain(_inputConnections.connectedLayers(FlowDirection.twoWay))
            .chain(_inputConnections.connectedLayers(FlowDirection.oneWayToSource));
	}

	Layer getInputLayer(size_t connectionIndex)
	{
        auto l = takeOne(zip(sequence!"n", inputLayers).filter!(i => i[0] == connectionIndex).map!(i => i[1]));
        enforce(!l.empty, "Input layer not found, connection index value " ~ connectionIndex.stringof ~ " was out of range.");
		return l.front;
	}

	Layer getOutputLayer(size_t connectionIndex)
	{
        auto l = takeOne(zip(sequence!"n", outputLayers).filter!(i => i[0] == connectionIndex).map!(i => i[1]));
        enforce(!l.empty, "Output layer not found, connection index value " ~ connectionIndex.stringof ~ " was out of range.");
        return l.front;
	}

    private size_t _size;

    private Array!LayerBehavior _behaviors;

    private Array!LayerDescription _descriptions;

    private LayerConnections _inputConnections;

    private LayerConnections _outputConnections;
}