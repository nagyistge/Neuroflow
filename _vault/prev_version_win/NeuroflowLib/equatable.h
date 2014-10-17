#pragma once

#include "nfdev.h"

namespace nf
{
    struct equatable : nf_object
    {
        virtual bool equals(const equatable& other) const = 0;
        virtual ::size_t get_hash_code() const = 0;
    };

    inline bool operator==(const equatable& e1, const equatable& e2)
    {
        return e1.equals(e2);
    }
}

namespace std 
{
    template <>
    struct hash<nf::equatable>
    {
        std::size_t operator()(const nf::equatable& e) const
        {
            return e.get_hash_code();
        }
    };
}