import computationcontextfactory;
import computationcontext;
import std.conv;
import supervisedbatch;
import supervisedsample;
import std.math;
import std.typecons;

unittest
{
    void testZero(ComputationContext ctx)
    {
        float[] values = [ 0.1f, 1.1f, 2.2f, 3.3f, 4.4f ];
        auto valuesArray = ctx.dataArrayFactory.create(&values[0], 0, values.length);
        
        valuesArray.read(0, values.length, &values[0], 0);
        foreach (v; values)
        {
            assert(v != 0.0f);
        }
        
        ctx.utils.zero(valuesArray);
        
        valuesArray.read(0, values.length, &values[0], 0);
        foreach (v; values)
        {
            assert(v == 0.0f);
        }
    }
    
    float calcMSEInner(float[] desired, float[] current)
    {
        float mse = 0.0f;
        for (size_t i = 0; i < desired.length; i++)
        {
            float v = (desired[i] - current[i]) * 0.5f;
            mse += v * v;
        }
        return mse / to!float(desired.length);
    }
    
    float calcMSE(float[][][] desired, float[][][] current)
    {
        assert(desired.length == current.length);
        
        float count = 0.0f;
        float mse = 0.0f;
        for (size_t i1 = 0; i1 < desired.length; i1++)
        {
            auto d1 = desired[i1];
            auto c1 = current[i1];
            assert(d1.length == c1.length);
            for (size_t i2 = 0; i2 < d1.length; i2++)
            {
                auto d2 = d1[i2];
                auto c2 = c1[i2];
                assert(d2.length == c2.length);
                
                mse += calcMSEInner(d2, c2);
                count++;
            }
        }
        
        return mse / count;
    }
    
    void testCalculateMSE(ComputationContext ctx)
    {
        float[][][] desired =
        [
            [
                [ 0.0345436f, 0.1345345f, 0.234346f ],
                [ 0.2784f, 0.6376768f, 0.9465477f ]
            ],
            [
                [ 1.0f, 0.26576765f, 0.7376888f ],
                [ 0.183675457f, 0.437677f, 0.633776376357f ]
            ]
        ];
        
        float[][][] current =
        [
            [
                [ 0.1f, 0.5f, 0.8f ],
                [ 0.9f, 0.6f, 0.3f ]
            ],
            [
                [ 1.0f, 0.2f, 0.5f ],
                [ 0.3f, 0.1f, 0.3f ]
            ]
        ];
        
        float mse = calcMSE(desired, current);
        
        auto batch = scoped!SupervisedBatch;
        auto resultValues = ctx.dataArrayFactory.create(15, 8.0f);
        
        assert(desired.length == current.length);
        for (size_t i1 = 0; i1 < desired.length; i1++)
        {
            auto d1 = desired[i1];
            auto c1 = current[i1];
            assert(d1.length == c1.length);
            auto sample = batch.add();
            for (size_t i2 = 0; i2 < d1.length; i2++)
            {
                auto d2 = d1[i2];
                auto c2 = c1[i2];
                assert(d2.length == c2.length);
                auto da = ctx.dataArrayFactory.createConst(&d2[0], 0, d2.length);
                auto ca = ctx.dataArrayFactory.createConst(&c2[0], 0, c2.length);
                sample.add(da, da, ca);
            }
        }
        
        float[] result;
        result.length = 15;
        
        ctx.utils.calculateMSE(batch, resultValues, 1);        
        resultValues.read(0, result.length, &result[0], 0);
        
        assert(result[0] == 8.0f);
        assert(abs(mse - result[1]) < 0.000001f);
        assert(result[2] == 8.0f);
        assert(result[14] == 8.0f);
    }

    // Native
    auto ctx = ComputationContextFactory.instance.createContext(NativeContext);
    try
    {
        testZero(ctx);
        testCalculateMSE(ctx);
    }
    finally
    {
        destroy(ctx);
    }
}