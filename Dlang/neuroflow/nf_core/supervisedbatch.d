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

    ~this()
    {
        foreach(sample; _samples) destroy(sample);
        _samples.length = 0;
    }

    @property SupervisedSample[] samples()
    {
        return _samples;
    }

    SupervisedSample add()
    {
        auto sample = new SupervisedSample();
        _samples ~= sample;
        return sample;
    }

    SupervisedSample add(SupervisedSample sample)
    {
        _samples ~= sample;
        return sample;
    }
    
    SupervisedSample add(SupervisedSampleEntry entry)
    {
        auto sample = new SupervisedSample(entry);
        _samples ~= sample;
        return sample;
    }
    
    SupervisedSample add(DataArray input)
    {
        auto sample = new SupervisedSample(new SupervisedSampleEntry(input));
        _samples ~= sample;
        return sample;
    }
    
    SupervisedSample add(DataArray input, DataArray desiredOutput, DataArray actualOutput)
    {
        auto sample = new SupervisedSample(new SupervisedSampleEntry(input, desiredOutput, actualOutput));
        _samples ~= sample;
        return sample;
    }

    private SupervisedSample[] _samples;
}