#include "stdafx.h"
#include "CppUnitTest.h"
#include "linqlike2.h"

using namespace Microsoft::VisualStudio::CppUnitTestFramework;
using namespace linqlike2;
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

            stringstream ss;
            auto e = from_iterators(values.begin(), values.end());
            for (int v : e)
            {
                ss << to_string(v) << " ";
            }

            ss << "\n";

            std::for_each(begin(e), end(e),
            [&](int v)
            {
                ss << to_string(v) << " ";
            });

            ss << "\n";

            Logger::WriteMessage(ss.str().c_str());

            vector<const int> cvalues = { 1, 2, 3, 4, 5 };

            stringstream css;
            auto ce = from_iterators(cvalues.cbegin(), cvalues.cend());
            for (const int& v : ce)
            {
                css << to_string(v) << " ";
            }

            css << "\n";

            std::for_each(ce.cbegin(), ce.cend(),
                [&](int v)
            {
                css << to_string(v) << " ";
            });

            Logger::WriteMessage(css.str().c_str());

            /*auto e = from_iterators(values.begin(), values.end());
            auto ce = from_const_iterators(values.cbegin(), values.cend());
            auto ev = from(values);
            auto cev = from_const(values);

            stringstream ss;

            std::for_each(e.begin(), e.end(),
            [&](int& v)
            {
                ss << to_string(v) << " ";
            });

            ss << "\n";

            std::for_each(e.cbegin(), e.cend(),
            [&](int v)
            {
                ss << to_string(v) << " ";
            });

            ss << "\n";
            
            for (auto v : e)
            {
                ss << to_string(v) << " ";
            }

            ss << "\n";

            std::for_each(ce.cbegin(), ce.cend(),
            [&](const int v)
            {
                ss << to_string(v) << " ";
            });

            ss << "\n";

            for (const auto v : ce)
            {
                ss << to_string(v) << " ";
            }

            ss << "\n";

            for (auto& v : ev)
            {
                ss << to_string(v) << " ";
            }

            ss << "\n";

            for (const int& v : cev)
            {
                ss << to_string(v) << " ";
            }

            Logger::WriteMessage(ss.str().c_str());*/
		}

        /*BEGIN_TEST_METHOD_ATTRIBUTE(where_test)
            TEST_METHOD_ATTRIBUTE(L"Category", L"Linqlike")
        END_TEST_METHOD_ATTRIBUTE()
        TEST_METHOD(where_test)
        {
            try
            {
                vector<int> values = { 1, 2, 3, 4, 5 };
                auto q = from(values).where([](int v) { return v % 2 != 0; });
                auto cq = from_const(values).where([](const int v) { return v % 2 != 0; });
                stringstream ss;
                for (auto v : q)
                {
                    ss << to_string(v) << " ";
                }
                for (auto v : cq)
                {
                    ss << to_string(v) << " ";
                }
                Logger::WriteMessage(ss.str().c_str());
            }
            catch (exception& ex)
            {
                Logger::WriteMessage(ex.what());
                throw;
            }
        }

        BEGIN_TEST_METHOD_ATTRIBUTE(select_test)
            TEST_METHOD_ATTRIBUTE(L"Category", L"Linqlike")
        END_TEST_METHOD_ATTRIBUTE()
        TEST_METHOD(select_test)
        {
            try
            {
                vector<int> values = { 1, 2, 3, 4, 5 };
                auto q = from(values).select([=](int v) { return to_string(v * v); });
                auto cq = from_const(values).select([=](const int v) { return to_string(v * v); });
                stringstream ss;
                for (auto v : q)
                {
                    ss << v << " ";
                }
                for (auto v : cq)
                {
                    ss << v << " ";
                }
                Logger::WriteMessage(ss.str().c_str());
            }
            catch (exception& ex)
            {
                Logger::WriteMessage(ex.what());
                throw;
            }
        }*/

	};
}