import supervisedsample;
import supervisedsampleentry;
import dataarray;

class SupervisedBatch
{
    this()
    {
        _samples.reserve(16);
    }

    this(SupervisedSample sample)
    {
        _samples ~= sample;
    }

    this(SupervisedSampleEntry entry)
    {
        _samples ~= new SupervisedSample(entry);
    }

    this(DataArray input)
    {
        _samples ~= new SupervisedSample(new SupervisedSampleEntry(input));
    }
    
    this(DataArray input, DataArray desiredOutput, DataArray actualOutput)
    {
        _samples ~= new SupervisedSample(new SupervisedSampleEntry(input, desiredOutput, actualOutput));
    }

    @property SupervisedSample[] samples()
    {
        return _samples;
    }

    void add(SupervisedSample sample)
    {
        _samples ~= sample;
    }
    
    void add(SupervisedSampleEntry entry)
    {
        _samples ~= new SupervisedSample(entry);
    }
    
    void add(DataArray input)
    {
        _samples ~= new SupervisedSample(new SupervisedSampleEntry(input));
    }
    
    void add(DataArray input, DataArray desiredOutput, DataArray actualOutput)
    {
        _samples ~= new SupervisedSample(new SupervisedSampleEntry(input, desiredOutput, actualOutput));
    }

    private SupervisedSample[] _samples;
}