#pragma once

#include "Typedefs.h"
#include "DataArray.h"

namespace Neuroflow
{
    namespace Data
    {
        ref class NativeDataArray : public Neuroflow::Data::DataArray
        {
            NeuroflowN::DataArray* dataArray;
        
        public:
            NativeDataArray(NeuroflowN::DataArray* dataArray) :
                dataArray(dataArray)
            {
                assert(dataArray != nullptr);
            }

            virtual property bool IsConst
            {
                bool get() override
                {
                    return dataArray->GetIsConst();
                }
            }

            virtual property int Size
            {
                int get() override
                {
                    return (int)dataArray->GetSize();
                }
            }

        internal:
            property NeuroflowN::DataArray* PDataArray
            {
                NeuroflowN::DataArray* get()
                {
                    return dataArray;
                }
            }

        protected:

            virtual void ReadAsync(int sourceBeginIndex, int count, float* targetPtr, int targetBeginIndex, DataArrayCompletedCallback^ doneFunc) override;

            virtual void WriteAsync(float* sourcePtr, int sourceBeginIndex, int count, int targetBeginIndex, DataArrayCompletedCallback^ doneFunc) override;

            virtual void CleanupNativeResources() override
            {
                delete dataArray;
                dataArray = nullptr;
            }
        };
    }
}