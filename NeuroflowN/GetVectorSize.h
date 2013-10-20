#pragma once

#include <algorithm> 
#include "GetSize.h"

namespace NeuroflowN
{
    template <typename T1>
    unsigned GetVectorSize(T1 p1)
    {
        unsigned size = GetSize<T1>::Get(p1);
        if ((size % 16) == 0) return 16;
        if ((size % 8) == 0) return 8;
        if ((size % 4) == 0) return 4;
        if ((size % 2) == 0) return 2;
        return 1;
    }

    template <typename T1, typename T2>
    unsigned GetVectorSize(T1 p1, T2 p2)
    {
        unsigned size1 = GetVectorSize(p1);
        unsigned size2 = GetVectorSize(p2);
        if ((size1 % 16) == 0 && (size2 % 16) == 0) return 16;
        if ((size1 % 8) == 0 && (size2 % 8) == 0) return 8;
        if ((size1 % 4) == 0 && (size2 % 4) == 0) return 4;
        if ((size1 % 2) == 0 && (size2 % 2) == 0) return 2;
        return 1;
    }

    template <typename T1, typename T2, typename T3>
    unsigned GetVectorSize(T1 p1, T2 p2, T3 p3)
    {
        unsigned size1 = GetVectorSize(p1, p2);
        unsigned size2 = GetVectorSize(p2, p3);
        return min(size1, size2);
    }

    template <typename T1, typename T2, typename T3, typename T4>
    unsigned GetVectorSize(T1 p1, T2 p2, T3 p3, T4 p4)
    {
        unsigned size1 = GetVectorSize(p1, p2, p3);
        unsigned size2 = GetVectorSize(p3, p4);
        return min(size1, size2);
    }

    template <typename T1, typename T2, typename T3, typename T4, typename T5>
    unsigned GetVectorSize(T1 p1, T2 p2, T3 p3, T4 p4, T5 p5)
    {
        unsigned size1 = GetVectorSize(p1, p2, p3, p4);
        unsigned size2 = GetVectorSize(p4, p5);
        return min(size1, size2);
    }

    template <typename T1, typename T2, typename T3, typename T4, typename T5, typename T6>
    unsigned GetVectorSize(T1 p1, T2 p2, T3 p3, T4 p4, T5 p5, T6 p6)
    {
        unsigned size1 = GetVectorSize(p1, p2, p3, p4, p5);
        unsigned size2 = GetVectorSize(p5, p6);
        return min(size1, size2);
    }
}