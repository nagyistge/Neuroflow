#pragma once

#include "IList.h"
#include <functional>

namespace Neuroflow
{
    template<typename T, typename MT>
    class NativeList : public NeuroflowN::IList<T>
    {
        gcroot<System::Collections::Generic::IList<MT>^> managedList;

        gcroot<System::Collections::Generic::IList<System::Func<MT>^>^> managedFList;
        
        std::function<T(gcroot<MT>)> conv;

    public:
        NativeList(gcroot<System::Collections::Generic::IList<MT>^> managedList, std::function < T(gcroot<MT>)> conv) :
            managedList(managedList),
            conv(conv)
        {
        }

        NativeList(gcroot<System::Collections::Generic::IList<System::Func<MT>^>^> managedFList, std::function < T(gcroot<MT>)> conv) :
            managedFList(managedFList),
            conv(conv)
        {
        }

        unsigned GetCount() const override
        {
            auto ml = (System::Collections::Generic::IList<MT>^)managedList;
            if (ml != null) return ml->Count;
            auto mfl = (System::Collections::Generic::IList<System::Func<MT>^>^)managedFList;
            return mfl->Count;
        }

        virtual T operator [](const unsigned index) const
        {
            auto ml = (System::Collections::Generic::IList<MT>^)managedList;
            if (ml != null) return conv(ml[index]);
            auto mfl = (System::Collections::Generic::IList<System::Func<MT>^>^)managedFList;
            return conv(mfl[index]->Invoke());
        }
    };
}

