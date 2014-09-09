import layerbehavior;
import layerdescription;

class Layer
{
    this(size_t size, in immutable(Object)[] args ...)
    {
        _size = size;
        foreach (arg; args)
        {
            auto b = cast(immutable(LayerBehavior))arg;
            if (b)
            {
                _behaviors ~= b;
                continue;
            }
            auto d = cast(immutable(LayerDescription))arg;
            if (d)
            {
                _descriptions ~= d;
                continue;
            }
        }
    }

    @property size_t size() const
    {
        return _size;
    }

    @property immutable(LayerBehavior)[] behaviors()
    {
        return _behaviors;
    }

    @property immutable(LayerDescription)[] desciptions()
    {
        return _descriptions;
    }

    private size_t _size;

    private immutable(LayerBehavior)[] _behaviors;

    private immutable(LayerDescription)[] _descriptions;
}