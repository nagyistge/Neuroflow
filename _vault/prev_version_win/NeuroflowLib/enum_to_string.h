#pragma once

#include <string>
#include <algorithm>
#include <vector>
#include <exception>

#define ENUM_STRINGS(T, ...) \
    enum class T; \
    enum_string_values_t enum_strings<T>::data = { __VA_ARGS__ }; \
    inline std::ostream& operator << (std::ostream& os, const T obj) \
    { \
    os << enum_to_string<T>(obj); \
    return os;\
    } \
    inline std::istream& operator >> (std::istream& is, T& obj) \
    { \
    std::string value; \
    is >> value; \
    obj = string_to_enum<T>(value); \
    return is; \
    } 

namespace nf
{
    typedef const std::vector<const char*> enum_string_values_t;

    template<typename T>
    struct enum_strings
    {
        static enum_string_values_t data;
    };
    

    template<typename T>
    std::string enum_to_string(T e)
    {
        ::size_t idx = static_cast<::size_t>(e);
        return enum_strings<T>::data.at(idx);
    }

    template <typename T>
    T string_to_enum(const std::string& str)
    {
        static auto begin = std::begin(enum_strings<T>::data);
        static auto end = std::end(enum_strings<T>::data);

        auto find = std::find(begin, end, str);
        if (find != end)
        {
            return static_cast<T>(std::distance(begin, find));
        }

        throw std::invalid_argument("Supplyed argument 'str' doesn't match any of possible enum values.");
    }

    template<typename T>
    std::wstring enum_to_wstring(T e)
    {
        auto str = enum_to_string(e);
        return std::wstring(str.begin(), str.end());
    }

    template <typename T>
    T wstring_to_enum(const std::wstring& wstr)
    {
        auto str = std::string(wstr.begin(), wstr.end());
    }
}