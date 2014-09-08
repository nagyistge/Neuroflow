import std.exception;
import dataarray;

class SupervisedSampleEntry
{
    this(DataArray input)
    {
        enforce(input !is null, "Input expected.");

        _input = input;
    }

    this(DataArray input, DataArray desiredOutput, DataArray actualOutput)
    {
        enforce(input, "Input expected.");
        enforce(desiredOutput, "Desired output expected.");
        enforce(actualOutput, "Actual output expected.");
        
        _input = input;
        _desiredOutput = desiredOutput;
        _actualOutput = actualOutput;
    }

    ~this()
    {
        auto idoSame = _input is _desiredOutput;
        destroy(_input);
        _input = null;
        if (_desiredOutput !is null) 
        {
            if (!idoSame) destroy(_desiredOutput);
            _desiredOutput = null;
        }
        if (_actualOutput !is null) 
        {
            destroy(_actualOutput);
            _actualOutput = null;
        }
    }

    @property DataArray input()
    {
        return _input;
    }

    @property DataArray desiredOutput()
    {
        return _desiredOutput;
    }

    @property DataArray actualOutput()
    {
        return _actualOutput;
    }

    @property bool hasOutput()
    {
        return _actualOutput !is null;
    }

    private DataArray _input = null, _desiredOutput = null, _actualOutput = null;
}