import layer;
import mlp;
import std.exception;
import computationcontext;
import mlpinitpars;

class MLP
{
    this(ComputationContext ctx, Layer[] layers, MLPInitPars initPars = null)
    {
        assert(ctx);
        assert(layers);
        assert(layers.length);
    }
}