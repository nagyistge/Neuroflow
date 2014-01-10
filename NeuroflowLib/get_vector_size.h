#pragma once

namespace nf
{
    inline idx_t get_vector_size(idx_t size)
    {
        if ((size % 16) == 0) return 16;
        if ((size % 8) == 0) return 8;
        if ((size % 4) == 0) return 4;
        if ((size % 2) == 0) return 2;
        return 1;
    }

    inline idx_t get_vector_size(idx_t size1, idx_t size2)
    {
        if ((size1 % 16) == 0 && (size2 % 16) == 0) return 16;
        if ((size1 % 8) == 0 && (size2 % 8) == 0) return 8;
        if ((size1 % 4) == 0 && (size2 % 4) == 0) return 4;
        if ((size1 % 2) == 0 && (size2 % 2) == 0) return 2;
        return 1;
    }

    inline idx_t get_vector_size(idx_t size1, idx_t size2, idx_t size3)
    {
        idx_t s1 = get_vector_size(size1, size2);
        idx_t s2 = get_vector_size(size2, size3);
        return s1 < s2 ? s1 : s2;
    }

    inline idx_t get_vector_size(idx_t size1, idx_t size2, idx_t size3, idx_t size4)
    {
        idx_t s1 = get_vector_size(size1, size2, size3);
        idx_t s2 = get_vector_size(size3, size4);
        return s1 < s2 ? s1 : s2;
    }

    inline idx_t get_vector_size(idx_t size1, idx_t size2, idx_t size3, idx_t size4, idx_t size5)
    {
        idx_t s1 = get_vector_size(size1, size2, size3, size4);
        idx_t s2 = get_vector_size(size4, size5);
        return s1 < s2 ? s1 : s2;
    }

    inline idx_t get_vector_size(idx_t size1, idx_t size2, idx_t size3, idx_t size4, idx_t size5, idx_t size6)
    {
        idx_t s1 = get_vector_size(size1, size2, size3, size4, size5);
        idx_t s2 = get_vector_size(size5, size6);
        return s1 < s2 ? s1 : s2;
    }
}