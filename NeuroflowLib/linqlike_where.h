#pragma once

#include "linqlike_base.h"

namespace linqlike
{
    template <typename T>
    struct where_enumerator : continutation_enumerator<T>
    {
        typedef typename enumerable<T>::predicate_t predicate_t;

        where_enumerator(const base_ptr& baseEnum, const predicate_t& pred) :
            continutation_enumerator(baseEnum),
            _pred(pred)
        {
        }

        bool move_next() override
        {
            auto& be = base_enum();
            while (be->move_next())
            {
                if (_pred(be->current())) return true;
            }
            return false;
        }

    private:
        predicate_t _pred;
    };

    template <typename T>
    enumerable<T> enumerable<T>::where(const predicate_t& pred)
    {
        auto p = pred;
        auto f = this->_enumeratorFactory;
        return enumerable<T>(
            [=]()
        {
            return std::make_shared<where_enumerator<T>>(f(), p);
        });
    }
}