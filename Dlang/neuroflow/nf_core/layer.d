import layerbehavior;
import layerdescription;
import layerconnections;
import std.container;
import std.algorithm;
import std.range;

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

    @property ref Array!LayerDescription desciptions()
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

	@property bool hasRecurrentConnections() const
	{
		return !takeOne(_inputConnections.connectedLayers(FlowDirection.oneWayToSource)).empty ||
			!takeOne(_inputConnections.connectedLayers(FlowDirection.twoWay)).empty ||
			!takeOne(_outputConnections.connectedLayers(FlowDirection.oneWayToSource)).empty ||
			!takeOne(_outputConnections.connectedLayers(FlowDirection.twoWay)).empty;
	}

	auto inputLayers()
	{
		assert(false, "TODO");
	}

	auto outputLayers()
	{
		assert(false, "TODO");
	}

	Layer getInputLayer(size_t connectionIndex)
	{
		assert(false, "TODO");
	}

	Layer getOutputLayer(size_t connectionIndex)
	{
		assert(false, "TODO");
	}

    private size_t _size;

    private Array!LayerBehavior _behaviors;

    private Array!LayerDescription _descriptions;

    private LayerConnections _inputConnections;

    private LayerConnections _outputConnections;
}