#pragma once

namespace nf
{
    template <typename T>
    struct row_numbered
    {
        row_numbered(::size_t rowNum, const T& _value) : _row_num(rowNum), _value(_value)
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

        const T& value() const
        {
            return _value;
        }

    private:
        ::size_t _row_num;
        T _value;
    };
}
