#pragma once

#include <boost/coroutine/all.hpp>
#include <boost/iterator.hpp>
#include <type_traits>
#include <functional>

namespace linqlike2
{
    template <typename T>
    struct enumerable
    {
        typedef typename boost::coroutines::coroutine<T> coro_t;
        typedef typename coro_t::pull_type pull_type;
        typedef typename std::function<pull_type()> pull_factory_t;

        explicit enumerable(typename pull_factory_t&& pullFactory) :
            _pullFactory(std::move(pullFactory))
        {
        }

        typename pull_type run() const
        {
            return _pullFactory();
        }

        pull_type operator*() const
        {
            return run();
        }

    private:
        typename pull_factory_t _pullFactory;
    };

    template <typename TIterator, typename T = typename TIterator::value_type>
    enumerable<T> from_iterators(const typename TIterator& begin, const typename TIterator& end)
    {
        typedef typename enumerable<T>::coro_t coro_t;
        auto b = begin;
        auto e = end;
        auto fact = [=]()
        { 
            return typename coro_t::pull_type([=](typename coro_t::push_type& sink)
            {
                std::for_each(b, e, [&](T& v)
                {
                    sink(v);
                });
            });
        };
        return enumerable<T>(std::move(fact));
    }
}

