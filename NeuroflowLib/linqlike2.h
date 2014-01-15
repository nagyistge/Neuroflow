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
        typedef boost::coroutines::coroutine<T> coro_t;
        typedef typename coro_t::pull_type pull_type;
        typedef typename coro_t::push_type push_type;
        typedef std::function<pull_type()> pull_factory_t;

        explicit enumerable(pull_factory_t&& pullFactory) :
            _pullFactory(std::move(pullFactory))
        {
        }

        pull_type run() const
        {
            return _pullFactory();
        }

        pull_type operator*() const
        {
            return run();
        }

    private:
        pull_factory_t _pullFactory;
    };

    template <typename TIterator, typename T = typename TIterator::value_type>
    enumerable<T> from_iterators(TIterator& begin, TIterator& end)
    {
        return enumerable<T>([=]()
        {
            return enumerable<T>::pull_type([=](enumerable<T>::push_type& sink)
            {
                std::for_each(begin, end, [&](T& v)
                {
                    sink(v);
                });
            });
        });
    }

    template <typename TIterator, typename T = typename TIterator::value_type>
    enumerable<T> from_iterators(const TIterator& begin, const TIterator& end)
    {
        return enumerable<T>([=]()
        {
            return enumerable<T>::pull_type([=](enumerable<T>::push_type& sink)
            {
                std::for_each(begin, end, [&](const T& v)
                {
                    sink(v);
                });
            });
        });
    }
}

