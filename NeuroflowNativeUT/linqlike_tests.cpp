#include "stdafx.h"
#include "CppUnitTest.h"
#include "linqlike.h"
#include <boost/lambda/lambda.hpp>

using namespace Microsoft::VisualStudio::CppUnitTestFramework;
using namespace linqlike;
using namespace std;
namespace l = boost::lambda;

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

            for (int v : from(values))
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

            e = from(values);
            std::for_each(begin(e), end(e),
                [&](int v)
            {
                ss << to_string(v) << " ";
            });

            ss << "\n";

            Logger::WriteMessage(ss.str().c_str());

            vector<const int> cvalues = { 1, 2, 3, 4, 5 };

            stringstream css;
            auto ce = from_iterators(cvalues.begin(), cvalues.end());
            for (const int& v : ce)
            {
                css << to_string(v) << " ";
            }

            css << "\n";

            for (const int& v : from(values))
            {
                css << to_string(v) << " ";
            }

            css << "\n";

            std::for_each(ce.begin(), ce.end(),
                [&](int v)
            {
                css << to_string(v) << " ";
            });

            css << "\n";

            ce = from(values);
            std::for_each(ce.begin(), ce.end(),
                [&](int v)
            {
                css << to_string(v) << " ";
            });

            Logger::WriteMessage(css.str().c_str());
		}

        BEGIN_TEST_METHOD_ATTRIBUTE(where_test)
            TEST_METHOD_ATTRIBUTE(L"Category", L"Linqlike")
        END_TEST_METHOD_ATTRIBUTE()
        TEST_METHOD(where_test)
        {
            try
            {
                vector<int> values = { 1, 2, 3, 4, 5 };
                vector<const int> cvalues = { 1, 2, 3, 4, 5 };
                auto q = from(values) | where(l::_1 % 2 == 0);
                auto cq = from(cvalues) >> where(l::_1 % 2 == 0);
                stringstream ss;
                for (auto& v : q)
                {
                    ss << to_string(v) << " ";
                }
                for (auto v : q)
                {
                    ss << to_string(v) << " ";
                }
                for (auto& v : cq)
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
                const vector<int> values = { 1, 2, 3, 4, 5 };
                auto q = from(values) | select([=](const int& v) { return to_string(v * v); });
                auto cq = from(values) >> select([=](const int& v) { return to_string(v * v); });
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
        }

	};
}