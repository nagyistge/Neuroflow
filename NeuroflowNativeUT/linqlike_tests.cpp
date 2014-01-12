#include "stdafx.h"
#include "CppUnitTest.h"
#include "linqlike.h"

using namespace Microsoft::VisualStudio::CppUnitTestFramework;
using namespace linqlike;
using namespace std;

namespace NeuroflowNativeUT
{
	TEST_CLASS(linqlike_tests)
	{
	public:
		
        BEGIN_TEST_METHOD_ATTRIBUTE(enumerable_iterators_test)
            TEST_METHOD_ATTRIBUTE(L"Category", L"Linqlike")
        END_TEST_METHOD_ATTRIBUTE()
        TEST_METHOD(enumerable_iterators_test)
		{
            vector<int> values = { 1, 2, 3, 4, 5 };
            auto impl = [&]() { return  make_shared<enumerable_iterators<vector<int>::iterator>>(values.begin(), values.end()); };
            auto e = enumerable<int>(impl);

            stringstream ss;

            std::for_each(e.begin(), e.end(),
            [&](int& v)
            {
                ss << to_string(v) << " ";
            });

            ss << "\n";

            std::for_each(e.cbegin(), e.cend(),
            [&](const int& v)
            {
                ss << to_string(v) << " ";
            });

            ss << "\n";
            
            for (auto v : e)
            {
                ss << to_string(v) << " ";
            }
            Logger::WriteMessage(ss.str().c_str());
		}

	};
}