#pragma once

#include "linqlike_base.h"
#include <boost/optional.hpp>
#include <vector>
#include <algorithm>
#include <assert.h>

namespace linqlike
{
    template <typename F>
    struct _order_by
    {
        explicit _order_by(const F& selectValue, dir direction) :
            _selectValue(selectValue),
            _direction(direction)
        {
        }

        const F& selectValue() const
        {
            return _selectValue;
        }

        dir direction() const
        {
            return _direction;
        }

    private:
        F _selectValue;
        dir _direction;
    };

    template <typename F>
    struct _then_by : _order_by<F>
    {
        explicit _then_by(const F& selectValue, dir direction) :
            _order_by(selectValue, direction)
        {
        }
    };

    template <typename T, typename F>
    struct ordered_enumerable : enumerable<T>
    {
        ordered_enumerable(pull_factory_t&& pullFactory, const _order_by<F>& orderBy) :
            enumerable<T>(std::move(pullFactory)),
            _orderBy(orderBy)
        {
        }

        const _order_by<F> order_by() const
        {
            return _orderBy;
        }

    private:
        _order_by<F> _orderBy;
    };

    template <typename F>
    _order_by<F> order_by(const F& tran, dir direction = dir::asc)
    {
        return _order_by<F>(tran);
    }

    template <typename F>
    _then_by<F> then_by(const F& tran, dir direction = dir::asc)
    {
        return _then_by<F>(tran);
    }
}