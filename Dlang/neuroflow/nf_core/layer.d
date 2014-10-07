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

    @property LayerBehavior[] behaviors()
    {
        return _behaviors;
    }

    @property LayerDescription[] descriptions()
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

    ForwardRange!Layer inputLayers()
	{
		return _inputConnections.connectedLayers(FlowDirection.oneWay)
            .chain(_inputConnections.connectedLayers(FlowDirection.twoWay))
            .chain(_outputConnections.connectedLayers(FlowDirection.twoWay))
            .chain(_outputConnections.connectedLayers(FlowDirection.oneWayToSource))
            .inputRangeObject;
	}

    ForwardRange!Layer outputLayers()
	{
        return _outputConnections.connectedLayers(FlowDirection.oneWay)
            .chain(_outputConnections.connectedLayers(FlowDirection.twoWay))
            .chain(_inputConnections.connectedLayers(FlowDirection.twoWay))
            .chain(_inputConnections.connectedLayers(FlowDirection.oneWayToSource))
            .inputRangeObject;
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

    private LayerBehavior[] _behaviors;

    private LayerDescription[] _descriptions;

    private LayerConnections _inputConnections;

    private LayerConnections _outputConnections;

    override int opCmp(Object o) const
    {
        if (typeid(this) != typeid(o)) 
        {
            return typeid(this).opCmp(typeid(o));
        }

        size_t thisVal = cast(size_t)(cast(void*)this);
        size_t oVal = cast(size_t)(cast(void*)o);

        return thisVal < oVal ? -1 : thisVal > oVal ? 1 : 0;
    }
}