import supervisedsampleentry;
import dataarray;

class SupervisedSample
{
    this()
    {
        _entries.reserve(1);
    }

    this(SupervisedSampleEntry entry)
    {
        _entries ~= entry;
    }

    this(DataArray input)
    {
        _entries ~= new SupervisedSampleEntry(input);
    }

    this(DataArray input, DataArray desiredOutput, DataArray actualOutput)
    {
        _entries ~= new SupervisedSampleEntry(input, desiredOutput, actualOutput);
    }

    @property SupervisedSampleEntry[] entries()
    {
        return entries;
    }

    void add(SupervisedSampleEntry entry)
    {
        _entries ~= entry;
    }
    
    void add(DataArray input)
    {
        _entries ~= new SupervisedSampleEntry(input);
    }
    
    void add(DataArray input, DataArray desiredOutput, DataArray actualOutput)
    {
        _entries ~= new SupervisedSampleEntry(input, desiredOutput, actualOutput);
    }

    private SupervisedSampleEntry[] _entries;
}