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
            int target = 1 + 2 + 3 + 4 + 5;
            int result = 0;

            auto e = from_iterators(values.begin(), values.end());
            for (int v : e)
            {
                result += v;
            }
            Assert::AreEqual(target, result);

            result = 0;
            for (int v : from(values))
            {
                result += v;
            }
            Assert::AreEqual(target, result);

            result = 0;
            std::for_each(begin(e), end(e),
            [&](int v)
            {
                result += v;
            });
            Assert::AreEqual(target, result);

            result = 0;
            e = from(values);
            std::for_each(begin(e), end(e), [&](int v)
            {
                result += v;
            });
            Assert::AreEqual(target, result);

            vector<const int> cvalues = { 1, 2, 3, 4, 5 };

            result = 0;
            auto ce = from_iterators(cvalues.begin(), cvalues.end());
            for (const int& v : ce)
            {
                result += v;
            }
            Assert::AreEqual(target, result);

            result = 0; 
            for (const int& v : from(values))
            {
                result += v;
            }
            Assert::AreEqual(target, result);

            result = 0;
            std::for_each(ce.begin(), ce.end(), [&](int v)
            {
                result += v;
            });
            Assert::AreEqual(target, result);

            result = 0;
            ce = from(values);
            std::for_each(ce.begin(), ce.end(), [&](int v)
            {
                result += v;
            });
            Assert::AreEqual(target, result);
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
                int target = 2 + 4;
                int result = 0;
                auto q = from(values) | where(l::_1 % 2 == 0);
                auto cq = from(cvalues) >> where(l::_1 % 2 == 0);
                for (auto& v : q)
                {
                    result += v;
                }
                Assert::AreEqual(target, result);
                result = 0;
                for (auto v : q)
                {
                    result += v;
                }
                Assert::AreEqual(target, result);
                result = 0;
                for (auto& v : cq)
                {
                    result += v;
                }
                Assert::AreEqual(target, result);
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
                string target("12345");
                string result;
                auto q = from(values) | select([=](const int& v) { return to_string(v); });
                auto cq = from(values) >> select([=](const int& v) { return to_string(v); });
                result = "";
                for (auto v : q)
                {
                    result += v;
                }
                Assert::AreEqual(target, result);
                result = "";
                for (auto v : cq)
                {
                    result += v;
                }
                Assert::AreEqual(target, result);
            }
            catch (exception& ex)
            {
                Logger::WriteMessage(ex.what());
                throw;
            }
        }

        BEGIN_TEST_METHOD_ATTRIBUTE(concat_test)
            TEST_METHOD_ATTRIBUTE(L"Category", L"Linqlike")
        END_TEST_METHOD_ATTRIBUTE()
        TEST_METHOD(concat_test)
        {
            try
            {
                vector<int> values1 = { 1, 2, 3, 4, 5 };
                vector<int> values2 = { 6, 7, 8, 9, 10 };
                    
                vector<int> result;
                for (auto v : from(values1) >> concat(values2))
                {
                    result.push_back(v);
                }

                Assert::AreEqual(10, (int)result.size());
                Assert::AreEqual(1, result[0]);
                Assert::AreEqual(2, result[1]);
                Assert::AreEqual(9, result[8]);
                Assert::AreEqual(10, result[9]);
            }
            catch (exception& ex)
            {
                Logger::WriteMessage(ex.what());
                throw;
            }
        }

        BEGIN_TEST_METHOD_ATTRIBUTE(any_test)
            TEST_METHOD_ATTRIBUTE(L"Category", L"Linqlike")
        END_TEST_METHOD_ATTRIBUTE()
        TEST_METHOD(any_test)
        {
            try
            {
                vector<int> values1 = { 1, 2, 3, 4, 5 };
                vector<int> values2;

                Assert::IsTrue(from(values1) >> any());
                Assert::IsFalse(from(values2) >> any());
                Assert::IsTrue(from(values1) >> where([](int v) { return v == 1; }) >> any());
                Assert::IsFalse(from(values1) >> where([](int v) { return v == 1000; }) >> any());
            }
            catch (exception& ex)
            {
                Logger::WriteMessage(ex.what());
                throw;
            }
        }

        BEGIN_TEST_METHOD_ATTRIBUTE(row_num_test)
            TEST_METHOD_ATTRIBUTE(L"Category", L"Linqlike")
        END_TEST_METHOD_ATTRIBUTE()
        TEST_METHOD(row_num_test)
        {
            try
            {
                vector<int> values1 = { 0, 1, 2, 3, 4, 5 };
                
                ::size_t sum1 = 0, sum2 = 0;
                for (auto& x : from(values1) | row_num())
                {
                    sum1 += x.row_num();
                    sum2 += x.value();
                }

                Assert::AreEqual(sum1, sum2);
                Assert::AreEqual(0 + 1 + 2 + 3 + 4 + 5, (int)sum1);
            }
            catch (exception& ex)
            {
                Logger::WriteMessage(ex.what());
                throw;
            }
        }
	};
}