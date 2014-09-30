import std.typecons;
public import devicearray;
public import layer;

alias RowCol = Tuple!(size_t, size_t);

alias IndexedLayer = Tuple!(size_t, Layer);

alias DoneFunc = void delegate(Throwable);

alias GetDeviceArray = DeviceArray delegate();