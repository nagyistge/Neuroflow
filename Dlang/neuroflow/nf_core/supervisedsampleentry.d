import std.exception;
import dataarray;

class SupervisedSampleEntry
{
    this(DataArray inputs)
    {
        enforce(inputs !is null, "Input expected.");

        _inputs = inputs;
    }

    this(DataArray inputs, DataArray desiredOutputs, DataArray actualOutputs)
    {
        enforce(inputs, "Inputs expected.");
        enforce(desiredOutputs, "Desired outputs expected.");
        enforce(actualOutputs, "Actual outputs expected.");
        
        _inputs = inputs;
        _desiredOutputs = desiredOutputs;
        _actualOutputs = actualOutputs;
    }

    ~this()
    {
        auto idoSame = _inputs is _desiredOutputs;
        destroy(_inputs);
        _inputs = null;
        if (_desiredOutputs !is null) 
        {
            if (!idoSame) destroy(_desiredOutputs);
            _desiredOutputs = null;
        }
        if (_actualOutputs !is null) 
        {
            destroy(_actualOutputs);
            _actualOutputs = null;
        }
    }

    @property DataArray inputs()
    {
        return _inputs;
    }

    @property DataArray desiredOutputs()
    {
        return _desiredOutputs;
    }

    @property DataArray actualOutputs()
    {
        return _actualOutputs;
    }

    @property bool hasOutput()
    {
        return _actualOutputs !is null;
    }

    private DataArray _inputs = null, _desiredOutputs = null, _actualOutputs = null;
}