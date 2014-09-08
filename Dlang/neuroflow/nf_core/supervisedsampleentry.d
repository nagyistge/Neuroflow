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

    private DataArray _input, _desiredOutput, _actualOutput;
}