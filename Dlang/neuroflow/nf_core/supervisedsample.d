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

    ~this()
    {
        foreach(entry; _entries) destroy(entry);
        _entries.length = 0;
    }

    @property SupervisedSampleEntry[] entries()
    {
        return entries;
    }

    SupervisedSampleEntry add(SupervisedSampleEntry entry)
    {
        _entries ~= entry;
        return entry;
    }
    
    SupervisedSampleEntry add(DataArray input)
    {
        auto entry = new SupervisedSampleEntry(input);
        _entries ~= entry;
        return entry;
    }
    
    SupervisedSampleEntry add(DataArray input, DataArray desiredOutput, DataArray actualOutput)
    {
        auto entry = new SupervisedSampleEntry(input, desiredOutput, actualOutput);
        _entries ~= entry;
        return entry;
    }

    private SupervisedSampleEntry[] _entries;
}