#pragma once

#include "linqlike_base.h"

namespace linqlike
{
    template <typename T, typename R, typename F>
    struct select_enumerator : transform_enumerator<T, R>
    {
        select_enumerator(const base_ptr& baseEnum, const F& func) :
        transform_enumerator(baseEnum),
        _func(func),
        _value(std::move(_func(baseEnum->current())))
        {
        }

        bool move_next() override
        {
            auto& be = base_enum();
            if (be->move_next())
            {
                _value = std::move(_func(be->current()));
                return true;
            }
            else
            {
                _value.reset();
                return false;
            }
        }

        R& current() const override
        {
            if (!_value.is_initialized()) throw std::logic_error("Enumerator ended.");
            return (R&)(*_value);
        }

    private:
        F _func;
        boost::optional<R> _value;
    };

    template <typename T>
    template <typename TF>
    auto enumerable<T>::select(const TF& func)->enumerable<decltype(func(_Result_sample::sample()))>
    {
        typedef typename decltype(func(_Result_sample::sample())) TR;
        auto t = func;
        auto f = this->_enumeratorFactory;
        return enumerable<TR>(
            [=]()
        {
            return std::make_shared<select_enumerator<T, TR, TF>>(f(), t);
        });
    }
}