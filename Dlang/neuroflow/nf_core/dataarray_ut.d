import computationcontextfactory;
import nfdefs;
import computationcontext;

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
        
        valuesArray.read(null, 0, 2, &target[0], 0);
        assert(target[0] == 1.1f);
        assert(target[1] == 2.2f);
        
        // Verify is target is filled:
        targetArray.read(null, 0, 2, &target[0], 0);
        foreach (v; target)
        {
            assert(v == 100.0f);
        }
        
        ctx.deviceArrayManagement.copy(valuesArray, 0, targetArray, 0, 2);
        
        targetArray.read(null, 0, 2, &target[0], 0);
        assert(target[0] == 1.1f);
        assert(target[1] == 2.2f);
    }
    
    auto ctx = ComputationContextFactory.instance.createContext(NativeContext);
    assert(ctx);
    
    testCopyData(ctx);
}