#pragma once

#include "Typedefs.h"

namespace Neuroflow
{
    namespace Data
    {
        public ref class NativeDataArrayFactory : public Neuroflow::Data::DataArrayFactory
	    {
            NeuroflowN::DataArrayFactory* dataArrayFactory;

        public:
            NativeDataArrayFactory(NeuroflowN::DataArrayFactory* dataArrayFactory) :
                dataArrayFactory(dataArrayFactory)
            {
                assert(dataArrayFactory != nullptr);
            }
			
		protected:
            virtual DataArray^ DoCreate(int size, float fill) override;

            virtual DataArray^ DoCreate(array<float>^ array, int beginPos, int length) override;

            virtual DataArray^ DoCreateConst(array<float>^ array, int beginPos, int length) override;
	    };
    }
}
