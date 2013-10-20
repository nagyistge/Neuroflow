#pragma once

#include "Error.h"
#include <boost/mpl/assert.hpp>

namespace NeuroflowN
{
	template <typename T>
	struct GetSize
	{
		BOOST_MPL_ASSERT_MSG(false, SPECIALIZATION_EXPECTED, (T));

		inline static unsigned Get(T obj)
		{
			return 0;
		}
	};

	template <>
	struct GetSize<int>
	{
		inline static unsigned Get(int size)
		{
			return (unsigned)size;
		}
	};

	template <>
	struct GetSize<unsigned>
	{
		inline static unsigned Get(unsigned size)
		{
			return (unsigned)size;
		}
	};
}