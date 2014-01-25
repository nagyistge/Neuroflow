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
        TColl* pcoll = &coll;
        return enumerable<T>([=]() mutable
        {
            return enumerable<T>::pull_type([=](enumerable<T>::push_type& yield) mutable
            {
                typedef std::reference_wrapper<T> ref_t;

                std::vector<ref_t> values;
                for (auto& v : *pcoll) values.push_back(std::ref(v));
                
                auto& selectValueF = (*orderBy.select_value_1()).first;
                auto& selectValueD = (*orderBy.select_value_1()).second;
                std::function<bool(ref_t, ref_t)> comparer;
                if (selectValueD == dir::asc)
                {
                    comparer = [=](ref_t v1, ref_t v2) { return selectValueF(v1.get()) < selectValueF(v2.get()); };
                }
                else
                {
                    comparer = [=](ref_t v1, ref_t v2) { return selectValueF(v2.get()) < selectValueF(v1.get()); };
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
        TColl* pcoll = &coll;
        return enumerable<T>([=]() mutable
        {
            return enumerable<T>::pull_type([=](enumerable<T>::push_type& yield) mutable
            {
                typedef std::reference_wrapper<T> ref_t;
                typedef std::function<int(ref_t, ref_t)> comp_t;

                std::vector<ref_t> values;
                for (auto& v : *pcoll) values.push_back(std::ref(v));

                auto& selectValueF1 = (*orderBy.select_value_1()).first;
                auto& selectValueD1 = (*orderBy.select_value_1()).second;
                auto& selectValueF2 = (*orderBy.select_value_2()).first;
                auto& selectValueD2 = (*orderBy.select_value_2()).second;
                
                comp_t comparer1 = [=](ref_t v1, ref_t v2)
                {
                    auto cv1 = selectValueF1(v1.get());
                    auto cv2 = selectValueF1(v2.get());
                    return cv1 == cv2 ? 0 : (cv1 < cv2 ? -1 : 1);
                };
                if (selectValueD1 == dir::desc)
                {
                    comparer1 = [=](ref_t v1, ref_t v2) { return -comparer1(v1, v2); };
                }

                comp_t comparer2 = [=](ref_t v1, ref_t v2)
                {
                    auto cv1 = selectValueF2(v1.get());
                    auto cv2 = selectValueF2(v2.get());
                    return cv1 == cv2 ? 0 : (cv1 < cv2 ? -1 : 1);
                };
                if (selectValueD2 == dir::desc)
                {
                    comparer2 = [=](ref_t v1, ref_t v2) { return -comparer2(v1, v2); };
                }

                auto comparer = [=](ref_t v1, ref_t v2)
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
}