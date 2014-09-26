import computationcontextfactory;
import computationcontext;
import std.algorithm;

unittest
{
    void testCopyData(ComputationContext ctx)
    {
        float[] values = [ 0.0f, 1.1f, 2.2f, 3.3f, 4.4f ];
        float[2] target;
        auto valuesArray = ctx.dataArrayFactory.createConst(&values[0], 1, 2);
        auto targetArray = ctx.dataArrayFactory.create(2, 100.0f);
        
        assert(valuesArray !is null);
        assert(valuesArray.size == 2);
        
        valuesArray.read(0, 2, &target[0], 0);
        assert(target[0] == 1.1f);
        assert(target[1] == 2.2f);
        
        // Verify is target is filled:
        targetArray.read(0, 2, &target[0], 0);
        foreach (v; target)
        {
            assert(v == 100.0f);
        }
        
        ctx.deviceArrayManagement.copy(valuesArray, 0, targetArray, 0, 2);
        
        targetArray.read(0, 2, &target[0], 0);
        assert(target[0] == 1.1f);
        assert(target[1] == 2.2f);
    }

    void testPooling(ComputationContext ctx)
    {
        float[] values;
        values.length = 100;
        
        auto pool = ctx.deviceArrayManagement.createPool(true);
        auto a1 = pool.createArray2(10, 10);
        auto a2 = pool.createArray(100);
        auto da = ctx.dataArrayFactory.create(100, 9.9f);
        
        da.read(0, 100, &values[0], 0);
        foreach (v; values)
        {
            assert(v == 9.9f);
        }
        
        ctx.deviceArrayManagement.copy(a1, 0, da, 0, 100);
        da.read(0, 100, &values[0], 0);
        foreach (v; values)
        {
            assert(v == 0.0f);
        }
        
        ctx.deviceArrayManagement.copy(a2, 0, da, 0, 100);
        da.read(0, 100, &values[0], 0);
        foreach (v; values)
        {
            assert(v == 0.0f);
        }
        
        fill(values, 1.0f);
        da.write(&values[0], 0, 100, 0);
        ctx.deviceArrayManagement.copy(da, 1, a1, 1, 99);
        ctx.utils.zero(da);
        ctx.deviceArrayManagement.copy(a1, 0, da, 0, 100);
        da.read(0, 100, &values[0], 0);
        assert(values[0] == 0.0f);
        foreach(v; values[1 .. $])
        {
            assert(v == 1.0f);
        }
        
        ctx.deviceArrayManagement.copy(a1, 0, a2, 1, 2);
        ctx.deviceArrayManagement.copy(a2, 0, da, 0, 100);
        da.read(0, 100, &values[0], 0);
        assert(values[0] == 0.0f);
        assert(values[1] == 0.0f);
        assert(values[2] == 1.0f);
        foreach(v; values[3 .. $])
        {
            assert(v == 0.0f);
        }
                
        pool.zero();
        
        ctx.deviceArrayManagement.copy(a1, 0, da, 0, 100);
        da.read(0, 100, &values[0], 0);
        foreach (v; values)
        {
            assert(v == 0.0f);
        }
        
        ctx.deviceArrayManagement.copy(a2, 0, da, 0, 100);
        da.read(0, 100, &values[0], 0);
        foreach (v; values)
        {
            assert(v == 0.0f);
        }
    }

    // Native
    auto ctx = ComputationContextFactory.instance.createContext(NativeContext);
    assert(ctx);    
    testCopyData(ctx);
    testPooling(ctx);
}