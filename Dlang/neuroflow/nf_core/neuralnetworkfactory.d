import mlpinitpars;
import layer;
import mlp;
import std.exception;
import computationcontext;

class NeuralNetworkFactory
{
    this(ComputationContext context)
    {
        assert(context);

        _context = context;
    }

    MLP createMLP(Layer[] layers, MLPInitPars initPars = null)
    {
        enforce(layers !is null && layers.length > 0, "Layers expected.");

        return new MLP(_context, layers, initPars);
    }

    private ComputationContext _context;
}