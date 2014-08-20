#pragma once

namespace Neuroflow
{

template<typename T>
ref class NativePtr
{
    T* ptr;
    bool owned;

public:
    NativePtr(T* ptr) : ptr(ptr), owned(false) { }
    NativePtr(T* ptr, bool owned) : ptr(ptr), owned(owned) { }

    ~NativePtr() { this->!NativePtr(); }

    !NativePtr()
    {
        if (owned && ptr)
        {
            delete ptr;
            ptr = nullptr;
        }
    }

internal:
    property T* Ptr
    {
        T* get() { return ptr; }
    }
};

}