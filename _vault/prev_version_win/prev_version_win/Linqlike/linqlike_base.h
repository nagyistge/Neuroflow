#pragma once

#include <boost/coroutine/all.hpp>
#include <type_traits>
#include <functional>
#include <iterator>
#include <exception>

#ifdef LINQLIKE_STACK_SIZE
#define _LINQLIKE_STACK_SIZE LINQLIKE_STACK_SIZE
#else
#define _LINQLIKE_STACK_SIZE 512
#endif

namespace linqlike
{
    template <typename T>
    T _wat()
    {
        throw std::runtime_error("wat");
    };

    enum class dir
    {
        asc, desc
    };

    struct _dummy { };

    inline void _throw_seq_empty()
    {
        throw std::runtime_error("Sequence contains no elements.");
    }

    template <typename T>
    struct enumerable
    {
        typedef T value_type;
        typedef boost::coroutines::coroutine<value_type> coro_t;
        typedef typename coro_t::pull_type pull_type;
        typedef typename coro_t::push_type push_type;
        typedef std::function<pull_type()> pull_factory_t;
        typedef typename boost::range_iterator<pull_type>::type pull_iterator_t;

        struct iterator : public std::iterator<std::input_iterator_tag, value_type>
        {
            iterator() { }

            explicit iterator(pull_type&& pull) :
                _pull(std::move(pull)),
                _it(boost::begin(_pull))
            {
            };

            iterator(const iterator& mit) : _it(mit._it) { }

            iterator& operator=(const iterator& mit)
            {
                _it = mit._it;
                return *this;
            }

            iterator(iterator&& other) : _it(std::move(other._it)) { }

            iterator& operator=(iterator&& other)
            {
                _it = std::move(other._it);
                return *this;
            }

            iterator& operator++() { ++_it; return *this; }
            iterator operator++(int) { iterator tmp(*this); operator++(); return tmp; }
            bool operator==(const iterator& rhs) { return _it == rhs._it; }
            bool operator!=(const iterator& rhs) { return !(*this == rhs); }
            value_type& operator*() { return *_it; }

        private:

            pull_type _pull;
            pull_iterator_t _it;
        };

        explicit enumerable(pull_factory_t&& pullFactory) :
            _pullFactory(std::move(pullFactory))
        {
        }

        iterator begin()
        {
            return iterator(_pullFactory());
        }

        iterator end()
        {
            return iterator();
        }

        static pull_type make_pull(std::function<void(push_type& yield)>&& impl)
        {
            return std::move(pull_type(std::move(impl), boost::coroutines::attributes(_LINQLIKE_STACK_SIZE * 1024)));
        }

    private:
        pull_factory_t _pullFactory;
    };
}