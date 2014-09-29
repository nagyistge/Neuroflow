import std.typecons;
import devicearray;

alias RowCol = Tuple!(size_t, size_t);

alias DoneFunc = void delegate(Throwable);

alias GetDeviceArray = DeviceArray delegate();