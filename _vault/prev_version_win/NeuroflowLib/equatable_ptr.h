#pragma once

#include "nfdev.h"
#include "equatable.h"

namespace nf
{
    template <typename T>
    struct equatable_ptr
    {
        explicit equatable_ptr(const std::shared_ptr<T>& ptr) :
            _ptr(ptr)
        {
        }

        const std::shared_ptr<T> ptr() const
        {
            return _ptr;
        }

        bool equals(const equatable_ptr<T>& other) const
        {
            return _ptr ? (other._ptr ? _ptr->equals(*other._ptr) : false) : !(other._ptr);
        }

        ::size_t get_hash_code() const
        {
            return _ptr ? _ptr->get_hash_code() : ::size_t(0);
        }

    private:
        std::shared_ptr<T> _ptr;
    };

    template <typename T>
    inline bool operator==(const equatable_ptr<T>& e1, const equatable_ptr<T>& e2)
    {
        return e1.equals(e2);
    }

    template <typename T>
    inline equatable_ptr<T> make_equatable_ptr(const std::shared_ptr<T>& ptr)
    {
        return std::move(equatable_ptr<T>(ptr));
    }
}

namespace std
{
    template <typename T>
    struct hash<nf::equatable_ptr<T>>
    {
        std::size_t operator()(const nf::equatable_ptr<T>& e) const
        {
            return e.get_hash_code();
        }
    };
}