#pragma once

#include <boost/coroutine/all.hpp>
#include <type_traits>
#include <functional>
#include <iterator>

namespace linqlike
{
    template <typename T>
    struct enumerable
    {
        typedef T value_type;
        typedef boost::coroutines::coroutine<T> coro_t;
        typedef typename coro_t::pull_type pull_type;
        typedef typename coro_t::push_type push_type;
        typedef std::function<pull_type()> pull_factory_t;
        typedef typename boost::range_iterator<pull_type>::type pull_iterator_t;
        typedef typename std::iterator_traits<pull_iterator_t>::value_type pull_iterator_value_t;
        typedef pull_iterator_t iterator;

        struct enumerable_iterator : public std::iterator<std::input_iterator_tag, pull_iterator_value_t>
        {
            enumerable_iterator() { }

            explicit enumerable_iterator(pull_type&& pull) :
                _pull(std::move(pull)),
                _it(boost::begin(_pull))
            {
            };

            enumerable_iterator(const enumerable_iterator& mit) : _it(mit._it) { }
            enumerable_iterator& operator=(const enumerable_iterator& mit)
            {
                _it = mit->_it;
                return *this;
            }

            enumerable_iterator& operator++() { ++_it; return *this; }
            enumerable_iterator operator++(int) { enumerable_iterator tmp(*this); operator++(); return tmp; }
            bool operator==(const enumerable_iterator& rhs) { return _it == rhs._it; }
            bool operator!=(const enumerable_iterator& rhs) { return _it != rhs._it; }
            pull_iterator_value_t& operator*() { return *_it; }

        private:

            pull_type _pull;
            pull_iterator_t _it;
        };

        explicit enumerable(pull_factory_t&& pullFactory) :
            _pullFactory(std::move(pullFactory))
        {
        }

        enumerable_iterator begin()
        {
            return enumerable_iterator(_pullFactory());
        }

        enumerable_iterator end()
        {
            return enumerable_iterator();
        }

    private:
        pull_factory_t _pullFactory;
    };
}