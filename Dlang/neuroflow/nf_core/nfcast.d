import fastcastable;
import std.exception;

T fastCast(T)(FastCastable obj, bool allowNull = false)
{
    return fastCastImpl!(T, "primaryPtr")(obj, allowNull);
}

T fastAltCast(T)(FastCastable obj, bool allowNull = false)
{
    return fastCastImpl!(T, "secondaryPtr")(obj, allowNull);
}

T fastCastImpl(T, string ptrName)(FastCastable obj, bool allowNull = false)
{
    if (obj is null) 
    {
        enforce(allowNull, "Object is null.");
        return null;
    }
    
    debug
    {
        auto result = cast(T)obj;
        if (obj is null) 
        {
            enforce(allowNull, "Object is not '" ~ T.stringof ~ "'.");
            return null;
        }
        else
        {
            return result;
        }
    }
    else
    {
        mixin("void* ptr = obj." ~ ptrName ~ ";");
        if (ptr !is null) return cast(T)ptr;
        auto result = cast(T)obj;
        if (obj is null) 
        {
            enforce(allowNull, "Object is not '" ~ T.stringof ~ "'.");
            return null;
        }
        else
        {
            mixin("obj." ~ ptrName ~ " = cast(void*)result;");
            return result;
        }
    }
}