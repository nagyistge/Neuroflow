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
        struct t
        {
            int p1, p2;

            bool operator==(const t& other) const
            {
                return p1 == other.p1 && p2 == other.p2;
            }
        };

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
            std::for_each(begin(e), end(e),
            [&](int v)
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
                auto q = values | where(l::_1 % 2 == 0);
                auto cq = cvalues | where(l::_1 % 2 == 0);
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
                auto q = values | select([=](const int& v) { return to_string(v); });
                auto cq = values | select([=](const int& v) { return to_string(v); });
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
                for (auto v : values1 | concat(values2))
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
                const vector<int> values1 = { 1, 2, 3, 4, 5 };
                vector<int> values2;

                Assert::IsTrue(values1 | any());
                Assert::IsFalse(values2 | any());
                Assert::IsTrue(values1 | where([](int v) { return v == 1; }) | any());
                Assert::IsFalse(values2 | where([](int v) { return v == 1000; }) | any());
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
                for (auto& x : values1 | row_num())
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

        BEGIN_TEST_METHOD_ATTRIBUTE(first_test)
            TEST_METHOD_ATTRIBUTE(L"Category", L"Linqlike")
        END_TEST_METHOD_ATTRIBUTE()
        TEST_METHOD(first_test)
        {
            try
            {
                vector<int> values = { 0, 1, 2, 3, 4, 5 };

                int f = values | where([](int v) { return v % 2 != 0; }) | first();

                Assert::AreEqual(1, f);

                f = values | first([](int v) { return v > 3; });

                Assert::AreEqual(4, f);

                values.clear();

                try
                {
                    f = values | first();
                    Assert::Fail(L"Previous method should have failed.");
                }
                catch (runtime_error&)
                {
                }
            }
            catch (exception& ex)
            {
                Logger::WriteMessage(ex.what());
                throw;
            }
        }

        BEGIN_TEST_METHOD_ATTRIBUTE(first_or_default_test)
            TEST_METHOD_ATTRIBUTE(L"Category", L"Linqlike")
        END_TEST_METHOD_ATTRIBUTE()
        TEST_METHOD(first_or_default_test)
        {
            try
            {
                vector<int> values = { 2, 1, 2, 3, 4, 5 };

                int f = values | where([](int v) { return v % 2 != 0; }) | first_or_default();

                Assert::AreEqual(1, f);

                f = values | first_or_default([](int v) { return v > 3; });

                Assert::AreEqual(4, f);

                values.clear();

                f = values | first_or_default();
                Assert::AreEqual(0, f);
            }
            catch (exception& ex)
            {
                Logger::WriteMessage(ex.what());
                throw;
            }
        }

        BEGIN_TEST_METHOD_ATTRIBUTE(ordering_test)
            TEST_METHOD_ATTRIBUTE(L"Category", L"Linqlike")
        END_TEST_METHOD_ATTRIBUTE()
        TEST_METHOD(ordering_test)
        {
            try
            {
                vector<int> values1 = { 2, 1, 1, 1, 2, 7, -1, 5 };
                vector<int> values2;

                auto e = values1 | sort(dir::asc);
                values2.assign(e.begin(), e.end());

                Assert::AreEqual(values1.size(), values2.size());
                Assert::AreEqual(-1, values2.front());
                Assert::AreEqual(7, values2.back());

                values2.clear();
                e = values1 | sort(dir::desc);
                values2.assign(e.begin(), e.end());

                Assert::AreEqual(values1.size(), values2.size());
                Assert::AreEqual(-1, values2.back());
                Assert::AreEqual(7, values2.front());

                values2.clear();
                e = values1 | sort([](int v1, int v2) { return v2 < v1; });
                values2.assign(e.begin(), e.end());

                Assert::AreEqual(values1.size(), values2.size());
                Assert::AreEqual(-1, values2.back());
                Assert::AreEqual(7, values2.front());

                values2.clear();
                e = values1 | order_by([](int v) { return v; });
                values2.assign(e.begin(), e.end());

                Assert::AreEqual(values1.size(), values2.size());
                Assert::AreEqual(-1, values2.front());
                Assert::AreEqual(7, values2.back());

                values2.clear();
                e = values1 | order_by([](int v) { return v; }, dir::desc);
                values2.assign(e.begin(), e.end());

                Assert::AreEqual(values1.size(), values2.size());
                Assert::AreEqual(-1, values2.back());
                Assert::AreEqual(7, values2.front());

                vector<t> ts1 = { { 2, 1 }, { 1, 4 }, { 1, -1 }, { 2, 11 } };
                vector<t> ts2;

                auto e2 = ts1 | order_by([](t& v) { return v.p1; }, dir::asc, [](t& v) { return v.p2; }, dir::desc);
                ts2.assign(e2.begin(), e2.end());
                Assert::AreEqual(ts1.size(), ts2.size());
                Assert::AreEqual(1, ts2.front().p1);
                Assert::AreEqual(4, ts2.front().p2);
                Assert::AreEqual(2, ts2.back().p1);
                Assert::AreEqual(1, ts2.back().p2);

                e2 = ts1 | order_by([](t& v) { return v.p1; }, dir::desc, [](t& v) { return v.p2; }, dir::asc);
                ts2.assign(e2.begin(), e2.end());
                Assert::AreEqual(ts1.size(), ts2.size());
                Assert::AreEqual(2, ts2.front().p1);
                Assert::AreEqual(1, ts2.front().p2);
                Assert::AreEqual(1, ts2.back().p1);
                Assert::AreEqual(4, ts2.back().p2);
            }
            catch (exception& ex)
            {
                Logger::WriteMessage(ex.what());
                throw;
            }
        }

        BEGIN_TEST_METHOD_ATTRIBUTE(group_by_test)
            TEST_METHOD_ATTRIBUTE(L"Category", L"Linqlike")
        END_TEST_METHOD_ATTRIBUTE()
        TEST_METHOD(group_by_test)
        {
            try
            {
                vector<t> ts1 = { { 2, 1 }, { 1, 4 }, { 1, -1 }, { 2, 11 }, { 5, 11 }, { 1, 21 } };

                auto e = ts1 | group_by([](t& v) { return v.p1; });
                int count = 0, sum = 0;
                for (auto& g : e)
                {
                    count++;
                    sum += g.key();

                    int lcount = 0, lsum = 0;
                    for (auto& v : g.values())
                    {
                        lcount++;
                        lsum += v.p2;
                    }

                    switch (g.key())
                    {
                        case 1:
                            Assert::AreEqual(3, lcount);
                            Assert::AreEqual(4 + -1 + 21, lsum);
                            break;
                        case 2:
                            Assert::AreEqual(2, lcount);
                            Assert::AreEqual(1 + 11, lsum);
                            break;
                        case 5:
                            Assert::AreEqual(1, lcount);
                            Assert::AreEqual(11, lsum);
                            break;
                        default:
                            Assert::Fail(L"This ain't gonna happen.");
                    }
                }
                Assert::AreEqual(3, count);
                Assert::AreEqual(1 + 2 + 5, sum);

                auto e2 = ts1 | group_by([](t& v) { return v.p1; }, [](t& v) { return v.p2; });
                count = 0, sum = 0;
                for (auto& g : e2)
                {
                    count++;
                    sum += g.key();

                    int lcount = 0, lsum = 0;
                    for (int v : g.values())
                    {
                        lcount++;
                        lsum += v;
                    }

                    switch (g.key())
                    {
                        case 1:
                            Assert::AreEqual(3, lcount);
                            Assert::AreEqual(4 + -1 + 21, lsum);
                            break;
                        case 2:
                            Assert::AreEqual(2, lcount);
                            Assert::AreEqual(1 + 11, lsum);
                            break;
                        case 5:
                            Assert::AreEqual(1, lcount);
                            Assert::AreEqual(11, lsum);
                            break;
                        default:
                            Assert::Fail(L"This ain't gonna happen.");
                    }
                }
                Assert::AreEqual(3, count);
                Assert::AreEqual(1 + 2 + 5, sum);
            }
            catch (exception& ex)
            {
                Logger::WriteMessage(ex.what());
                throw;
            }
        }
	};
}