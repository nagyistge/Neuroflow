#pragma once

#include <functional>
#include <exception>
#include <memory>
#include <boost/iterator/iterator_facade.hpp>
#include <boost/optional.hpp>

namespace linqlike
{
    template <typename T>
    struct enumerable
    {
        struct _Result_sample
        {
            static T sample()
            {
                throw std::logic_error("DO NOT CALL THIS.");
            }
        };

        struct enumerator
        {
            virtual bool move_next() = 0;
            virtual T& current() const = 0;
        };

        typedef std::shared_ptr<enumerator> enumerator_ptr;
        typedef std::function<enumerator_ptr()> enumerator_factory_t;

        typedef std::function<bool(const T&)> predicate_t;

        template <typename TEnumerator, typename TV>
        struct iter : public boost::iterator_facade<iter<TEnumerator, TV>, TV, boost::forward_traversal_tag>
        {
            friend class boost::iterator_core_access;

            iter() : _end(true) { }
            explicit iter(const TEnumerator& owner) : _owner(owner)
            {
            }

        private:
            bool _end = false;
            TEnumerator _owner = NULL;

            bool equal(const iter<TEnumerator, TV>& other) const
            {
                if (_end == other._end) return true;
                return this->_owner == other._owner;
            }

            void increment()
            {
                if (!_owner->move_next()) _end = true;
            }

            TV& dereference() const
            {
                return _owner->current();
            }
        };

        typedef iter<enumerator_ptr, T> iterator;
        typedef iter<const enumerator_ptr, const T> const_iterator;

        explicit enumerable(const enumerator_factory_t& enumeratorFactory) : _enumeratorFactory(enumeratorFactory) { }

        iterator begin()
        {
            return iterator(_enumeratorFactory());
        }

        iterator end()
        {
            return iterator();
        }

        const_iterator cbegin() const
        {
            return const_iterator(_enumeratorFactory());
        }

        const_iterator cend() const
        {
            return const_iterator();
        }

        // -- OPERATORS:

        enumerable<T> where(const predicate_t& pred);
        
        enumerable<T> concat(const enumerable<T>& other);
        
        template <typename TF>
        auto select(const TF& func)->enumerable<decltype(func(_Result_sample::sample()))>;

        // -- OPERATORS

    private:

        enumerator_factory_t _enumeratorFactory;
    };

    template <typename TIterator, typename T>
    struct iterator_enumerator : enumerable<T>::enumerator
    {
        iterator_enumerator(const TIterator& begin, const TIterator& end) :
        _current(begin),
        _end(end)
        {
        }

        bool move_next() override
        {
            return ++_current != _end;
        }

        T& current() const override
        {
            if (_current == _end) throw std::logic_error("Enumerator ended.");
            return *_current;
        }

    private:
        TIterator _current, _end;
    };

    template <typename T>
    struct continutation_enumerator : enumerable<T>::enumerator
    {
        typedef typename enumerable<T>::enumerator_ptr base_ptr;

        continutation_enumerator(const base_ptr& baseEnum) : _baseEnum(baseEnum) { }

        T& current() const final
        {
            return _baseEnum->current();
        }

    protected:
        const base_ptr& base_enum() const
        {
            return _baseEnum;
        }

    private:
        base_ptr _baseEnum;
    };

    template <typename T, typename R>
    struct transform_enumerator : enumerable<R>::enumerator
    {
        typedef typename enumerable<T>::enumerator_ptr base_ptr;

        transform_enumerator(const base_ptr& baseEnum) : _baseEnum(baseEnum) { }

    protected:
        const base_ptr& base_enum() const
        {
            return _baseEnum;
        }

    private:
        base_ptr _baseEnum;
    };
}