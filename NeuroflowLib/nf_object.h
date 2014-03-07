#pragma once

#include <memory>
#include <string>

namespace nf
{
    struct nf_object : std::enable_shared_from_this<nf_object>
    {
        std::string type_name() const
        {
            return typeid(*this).name();
        }

        template<typename T>
        T* _Fast_cast()
        {
            if (_fp == nullptr) _fp = reinterpret_cast<void*>(dynamic_cast<T*>(this));
            return reinterpret_cast<T*>(_fp);
        }

        template<typename T>
        T* _Fast_cast_alt()
        {
            if (_fpAlt == nullptr) _fpAlt = reinterpret_cast<void*>(dynamic_cast<T*>(this));
            return reinterpret_cast<T*>(_fpAlt);
        }

        virtual ~nf_object() = default;

    protected:
        template <typename T>
        std::shared_ptr<T> shared_this()
        {
            return std::dynamic_pointer_cast<T>(shared_from_this());
        }

    private:
        void* _fp = nullptr;
        void* _fpAlt = nullptr;
    };

    template <typename T>
    inline T* _fast_cast(nf_object* obj)
    {
        if (obj == nullptr) return nullptr;
        return obj->_Fast_cast<T>();
    }

    template <typename T>
    inline T* _fast_cast_alt(nf_object* obj)
    {
        if (obj == nullptr) return nullptr;
        return obj->_Fast_cast_alt<T>();
    }
}