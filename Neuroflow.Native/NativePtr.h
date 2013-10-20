#pragma once

namespace Neuroflow
{

template<typename T>
ref class NativePtr
{
    T* ptr;

public:
    NativePtr(T* ptr) : ptr(ptr) { }

    ~NativePtr() { this->!NativePtr(); }

    !NativePtr()
    {
        delete ptr;
    }

    property T* Ptr
    {
        T* get() { return ptr; }
    }
};

}