#pragma once

#include "linqlike_base.h"

namespace linqlike
{
    template <typename T>
    struct row_numbered
    {
        row_numbered(::size_t rowNum, T& _value) : _row_num(rowNum), _value(_value) 
        {
        }
        
        row_numbered(const row_numbered<T>& other) : _row_num(other._row_num), _value(other._value) { }
        
        row_numbered<T>& operator=(const row_numbered<T>& other)
        {
            _row_num = other._row_num;
            _value = other._value;
            return *this;
        }

        bool operator==(const row_numbered<T>& other) const
        {
            return _row_num == other._row_num && &_value == &other._value;
        }

        ::size_t row_num() const
        {
            return _row_num;
        }

        T& value() const
        {
            return _value;
        }

    private:
        ::size_t _row_num;
        T& _value;
    };

    struct row_num
    {
    };

    template <typename TColl, typename T = TColl::value_type>
    enumerable<row_numbered<T>> operator|(TColl& coll, const row_num& rowNum)
    {
        TColl* pcoll = &coll;
        return enumerable<row_numbered<T>>([=]() mutable
        {
            return  enumerable<row_numbered<T>>::pull_type([=](enumerable<row_numbered<T>>::push_type& yield) mutable
            {
                ::size_t rowNum = 0;
                for (auto& v : *pcoll)
                {
                    yield(row_numbered<T>(rowNum++, v));
                }
            });
        });
    }
}