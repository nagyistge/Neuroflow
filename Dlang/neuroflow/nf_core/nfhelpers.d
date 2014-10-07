import std.range;

T getIndex2(T)(T i1, T i2, T size1)
{
	return i2 * size1 + i1;
}

auto nfSum(R)(R r) if (isInputRange!R && !isInfinite!R && is(typeof(r.front + r.front)))
{
    ElementType!R sum;
    foreach (x; r) sum += x;
    return sum;
}