#pragma once

#include "linqlike_base.h"
#include <boost/optional.hpp>
#include <vector>
#include <algorithm>
#include <assert.h>

namespace linqlike
{
    template <typename F1, typename F2 = _dummy, typename F3 = _dummy, typename F4 = _dummy>
    struct _order_by
    {
        _order_by(const F1& f1, dir dir1) :
            _selectValue1(std::make_pair(f1, dir1))
        {
        }

        _order_by(const F1& f1, dir dir1, const F2& f2, dir dir2) :
            _selectValue1(std::make_pair(f1, dir1)),
            _selectValue2(std::make_pair(f2, dir2))
        {
        }

        _order_by(const F1& f1, dir dir1, const F2& f2, dir dir2, const F3& f3, dir dir3) :
            _selectValue1(std::make_pair(f1, dir1)),
            _selectValue2(std::make_pair(f2, dir2)),
            _selectValue3(std::make_pair(f3, dir3))
        {
        }

        _order_by(const F1& f1, dir dir1, const F2& f2, dir dir2, const F3& f3, dir dir3, const F4& f4, dir dir4) :
            _selectValue1(std::make_pair(f1, dir1)),
            _selectValue2(std::make_pair(f2, dir2)),
            _selectValue3(std::make_pair(f3, dir3)),
            _selectValue4(std::make_pair(f4, dir4))
        {
        }

        const boost::optional<std::pair<F1, dir>>& select_value_1() const
        {
            return _selectValue1;
        }

        const boost::optional<std::pair<F2, dir>>& select_value_2() const
        {
            return _selectValue2;
        }

        const boost::optional<std::pair<F3, dir>>& select_value_3() const
        {
            return _selectValue3;
        }

        const boost::optional<std::pair<F4, dir>>& select_value_4() const
        {
            return _selectValue4;
        }

    private:
        boost::optional<std::pair<F1, dir>> _selectValue1;
        boost::optional<std::pair<F2, dir>> _selectValue2;
        boost::optional<std::pair<F3, dir>> _selectValue3;
        boost::optional<std::pair<F4, dir>> _selectValue4;
    };

    template <typename F>
    _order_by<F> order_by(const F& selectValue, dir dir = dir::asc)
    {
        return _order_by<F>(selectValue, dir);
    }

    template <typename F1, typename F2>
    _order_by<F1, F2> order_by(const F1& selectValue1, dir dir1, const F2& selectValue2, dir dir2)
    {
        return _order_by<F1, F2>(selectValue1, dir1, selectValue2, dir2);
    }

    template <typename F1, typename F2, typename F3>
    _order_by<F1, F2, F3> order_by(const F1& selectValue1, dir dir1, const F2& selectValue2, dir dir2, const F3& selectValue3, dir dir3)
    {
        return _order_by<F1, F2, F3>(selectValue1, dir1, selectValue2, dir2, selectValue3, dir3);
    }

    template <typename F1, typename F2, typename F3, typename F4>
    _order_by<F1, F2, F3, F4> order_by(const F1& selectValue1, dir dir1, const F2& selectValue2, dir dir2, const F3& selectValue3, dir dir3, const F4& selectValue4, dir dir4)
    {
        return _order_by<F1, F2, F3, F4>(selectValue1, dir1, selectValue2, dir2, selectValue3, dir3, selectValue4, dir4);
    }

    template <typename TColl, typename F, typename T = TColl::value_type>
    auto operator|(TColl& coll, const _order_by<F, _dummy, _dummy, _dummy>& orderBy)
    {
        return enumerable<T>([=]() mutable
        {
            return enumerable<T>::pull_type([=](enumerable<T>::push_type& yield) mutable
            {
                std::vector<T> values;
                for (auto& v : coll) values.push_back(v);
                
                auto& selectValueF = (*orderBy.select_value_1()).first;
                auto& selectValueD = (*orderBy.select_value_1()).second;
                std::function<bool(T&, T&)> comparer;
                if (selectValueD == dir::asc)
                {
                    comparer = [=](T& v1, T& v2) { return selectValueF(v1) < selectValueF(v2); };
                }
                else
                {
                    comparer = [=](T& v1, T& v2) { return selectValueF(v2) < selectValueF(v1); };
                }

                std::sort(values.begin(), values.end(), comparer);

                for (auto& v : values)
                {
                    yield(v);
                }
            });
        });
    }

    template <typename TColl, typename F1, typename F2, typename T = TColl::value_type>
    auto operator|(TColl& coll, const _order_by<F1, F2, _dummy, _dummy>& orderBy)
    {
        return enumerable<T>([=]() mutable
        {
            return enumerable<T>::pull_type([=](enumerable<T>::push_type& yield) mutable
            {
                typedef std::function<int(T&, T&)> comp_t;

                std::vector<T> values;
                for (auto& v : coll) values.push_back(v);

                auto& selectValueF1 = (*orderBy.select_value_1()).first;
                auto& selectValueD1 = (*orderBy.select_value_1()).second;
                auto& selectValueF2 = (*orderBy.select_value_2()).first;
                auto& selectValueD2 = (*orderBy.select_value_2()).second;
                
                comp_t comparer1 = [=](T& v1, T& v2)
                {
                    auto cv1 = selectValueF1(v1);
                    auto cv2 = selectValueF1(v2);
                    return cv1 == cv2 ? 0 : (cv1 < cv2 ? -1 : 1);
                };
                if (selectValueD1 == dir::desc)
                {
                    comparer1 = [=](T& v1, T& v2) { return -comparer1(v1, v2); };
                }

                comp_t comparer2 = [=](T& v1, T& v2)
                {
                    auto cv1 = selectValueF2(v1);
                    auto cv2 = selectValueF2(v2);
                    return cv1 == cv2 ? 0 : (cv1 < cv2 ? -1 : 1);
                };
                if (selectValueD2 == dir::desc)
                {
                    comparer2 = [=](T& v1, T& v2) { return -comparer2(v1, v2); };
                }

                auto comparer = [=](T& v1, T& v2)
                {
                    int c = comparer1(v1, v2);
                    if (c < 0) return true;
                    if (c > 0) return false;
                    c = comparer2(v1, v2);
                    if (c < 0) return true;
                    return false;
                };

                std::sort(values.begin(), values.end(), comparer);

                for (auto& v : values)
                {
                    yield(v);
                }
            });
        });
    }

    template <typename TColl, typename F1, typename F2, typename F3, typename T = TColl::value_type>
    auto operator|(TColl& coll, const _order_by<F1, F2, F3, _dummy>& orderBy)
    {
        return enumerable<T>([=]() mutable
        {
            return enumerable<T>::pull_type([=](enumerable<T>::push_type& yield) mutable
            {
                typedef std::function<int(T&, T&)> comp_t;

                std::vector<T> values;
                for (auto& v : coll) values.push_back(v);

                auto& selectValueF1 = (*orderBy.select_value_1()).first;
                auto& selectValueD1 = (*orderBy.select_value_1()).second;
                auto& selectValueF2 = (*orderBy.select_value_2()).first;
                auto& selectValueD2 = (*orderBy.select_value_2()).second;
                auto& selectValueF3 = (*orderBy.select_value_3()).first;
                auto& selectValueD3 = (*orderBy.select_value_3()).second;

                comp_t comparer1 = [=](T& v1, T& v2)
                {
                    auto cv1 = selectValueF1(v1);
                    auto cv2 = selectValueF1(v2);
                    return cv1 == cv2 ? 0 : (cv1 < cv2 ? -1 : 1);
                };
                if (selectValueD1 == dir::desc)
                {
                    comparer1 = [=](T& v1, T& v2) { return -comparer1(v1, v2); };
                }

                comp_t comparer2 = [=](T& v1, T& v2)
                {
                    auto cv1 = selectValueF2(v1);
                    auto cv2 = selectValueF2(v2);
                    return cv1 == cv2 ? 0 : (cv1 < cv2 ? -1 : 1);
                };
                if (selectValueD2 == dir::desc)
                {
                    comparer2 = [=](T& v1, T& v2) { return -comparer2(v1, v2); };
                }

                comp_t comparer3 = [=](T& v1, T& v2)
                {
                    auto cv1 = selectValueF3(v1);
                    auto cv2 = selectValueF3(v2);
                    return cv1 == cv2 ? 0 : (cv1 < cv2 ? -1 : 1);
                };
                if (selectValueD3 == dir::desc)
                {
                    comparer3 = [=](T& v1, T& v2) { return -comparer3(v1, v2); };
                }

                auto comparer = [=](T& v1, T& v2)
                {
                    int c = comparer1(v1, v2);
                    if (c < 0) return true;
                    if (c > 0) return false;
                    c = comparer2(v1, v2);
                    if (c < 0) return true;
                    if (c > 0) return false;
                    c = comparer3(v1, v2);
                    if (c < 0) return true;
                    return false;
                };

                std::sort(values.begin(), values.end(), comparer);

                for (auto& v : values)
                {
                    yield(v);
                }
            });
        });
    }

    template <typename TColl, typename F1, typename F2, typename F3, typename F4, typename T = TColl::value_type>
    auto operator|(TColl& coll, const _order_by<F1, F2, F3, F4>& orderBy)
    {
        return enumerable<T>([=]() mutable
        {
            return enumerable<T>::pull_type([=](enumerable<T>::push_type& yield) mutable
            {
                typedef std::function<int(T&, T&)> comp_t;

                std::vector<T> values;
                for (auto& v : coll) values.push_back(&v);

                auto& selectValueF1 = (*orderBy.select_value_1()).first;
                auto& selectValueD1 = (*orderBy.select_value_1()).second;
                auto& selectValueF2 = (*orderBy.select_value_2()).first;
                auto& selectValueD2 = (*orderBy.select_value_2()).second;
                auto& selectValueF3 = (*orderBy.select_value_3()).first;
                auto& selectValueD3 = (*orderBy.select_value_3()).second;
                auto& selectValueF4 = (*orderBy.select_value_4()).first;
                auto& selectValueD4 = (*orderBy.select_value_4()).second;

                comp_t comparer1 = [=](T& v1, T& v2)
                {
                    auto cv1 = selectValueF1(v1);
                    auto cv2 = selectValueF1(v2);
                    return cv1 == cv2 ? 0 : (cv1 < cv2 ? -1 : 1);
                };
                if (selectValueD1 == dir::desc)
                {
                    comparer1 = [=](T& v1, T& v2) { return -comparer1(v1, v2); };
                }

                comp_t comparer2 = [=](T& v1, T& v2)
                {
                    auto cv1 = selectValueF2(v1);
                    auto cv2 = selectValueF2(v2);
                    return cv1 == cv2 ? 0 : (cv1 < cv2 ? -1 : 1);
                };
                if (selectValueD2 == dir::desc)
                {
                    comparer2 = [=](T& v1, T& v2) { return -comparer2(v1, v2); };
                }

                comp_t comparer3 = [=](T& v1, T& v2)
                {
                    auto cv1 = selectValueF3(v1);
                    auto cv2 = selectValueF3(v2);
                    return cv1 == cv2 ? 0 : (cv1 < cv2 ? -1 : 1);
                };
                if (selectValueD3 == dir::desc)
                {
                    comparer3 = [=](T& v1, T& v2) { return -comparer3(v1, v2); };
                }

                comp_t comparer4 = [=](T& v1, T& v2)
                {
                    auto cv1 = selectValueF4(v1);
                    auto cv2 = selectValueF4(v2);
                    return cv1 == cv2 ? 0 : (cv1 < cv2 ? -1 : 1);
                };
                if (selectValueD4 == dir::desc)
                {
                    comparer4 = [=](T& v1, T& v2) { return -comparer4(v1, v2); };
                }

                auto comparer = [=](T& v1, T& v2)
                {
                    int c = comparer1(v1, v2);
                    if (c < 0) return true;
                    if (c > 0) return false;
                    c = comparer2(v1, v2);
                    if (c < 0) return true;
                    if (c > 0) return false;
                    c = comparer3(v1, v2);
                    if (c < 0) return true;
                    if (c > 0) return false;
                    c = comparer4(v1, v2);
                    if (c < 0) return true;
                    return false;
                };

                std::sort(values.begin(), values.end(), comparer);

                for (auto& v : values)
                {
                    yield(v);
                }
            });
        });
    }
}