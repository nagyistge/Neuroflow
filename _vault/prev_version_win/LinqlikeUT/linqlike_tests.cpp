#include "stdafx.h"
#include "CppUnitTest.h"
#include "utility.h"
#include <linqlike.h>

using namespace Microsoft::VisualStudio::CppUnitTestFramework;
using namespace linqlike;
using namespace std;

namespace LinqlikeUT
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
                auto q = from(values) | where([](int v) { return v % 2 == 0; });
                auto cq = from(cvalues) | where([](int v) { return v % 2 == 0; });
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
                auto cq = from(values) | select([=](const int& v) { return to_string(v); });
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
                for (auto v : from(values1) | concat(from(values2)))
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

        BEGIN_TEST_METHOD_ATTRIBUTE(any_test)
            TEST_METHOD_ATTRIBUTE(L"Category", L"Linqlike")
        END_TEST_METHOD_ATTRIBUTE()
        TEST_METHOD(any_test)
        {
            try
            {
                const vector<int> values1 = { 1, 2, 3, 4, 5 };
                vector<int> values2;

                Assert::IsTrue(from(values1) | any());
                Assert::IsFalse(from(values2) | any());
                Assert::IsTrue(from(values1) | where([](int v) { return v == 1; }) | any());
                Assert::IsFalse(from(values2) | where([](int v) { return v == 1000; }) | any());
                Assert::IsTrue(from(values1) | any([](int v) { return v == 1; }));
                Assert::IsFalse(from(values2) | any([](int v) { return v == 1000; }));
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

                int f = from(values) | where([](int v) { return v % 2 != 0; }) | first();

                Assert::AreEqual(1, f);

                f = from(values) | first([](int v) { return v > 3; });

                Assert::AreEqual(4, f);

                values.clear();

                try
                {
                    f = from(values) | first();
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

                int f = from(values) | where([](int v) { return v % 2 != 0; }) | first_or_default();

                Assert::AreEqual(1, f);

                f = from(values) | first_or_default([](int v) { return v > 3; });

                Assert::AreEqual(4, f);

                values.clear();

                f = from(values) | first_or_default();
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

                auto e = from(values1) | sort(dir::asc);
                values2.assign(e.begin(), e.end());

                Assert::AreEqual(values1.size(), values2.size());
                Assert::AreEqual(-1, values2.front());
                Assert::AreEqual(7, values2.back());

                values2.clear();
                e = from(values1) | sort(dir::desc);
                values2.assign(e.begin(), e.end());

                Assert::AreEqual(values1.size(), values2.size());
                Assert::AreEqual(-1, values2.back());
                Assert::AreEqual(7, values2.front());

                values2.clear();
                e = from(values1) | sort([](int v1, int v2) { return v2 < v1; });
                values2.assign(e.begin(), e.end());

                Assert::AreEqual(values1.size(), values2.size());
                Assert::AreEqual(-1, values2.back());
                Assert::AreEqual(7, values2.front());

                values2.clear();
                e = from(values1) | order_by([](int v) { return v; });
                values2.assign(e.begin(), e.end());

                Assert::AreEqual(values1.size(), values2.size());
                Assert::AreEqual(-1, values2.front());
                Assert::AreEqual(7, values2.back());

                values2.clear();
                e = from(values1) | order_by([](int v) { return v; }, dir::desc);
                values2.assign(e.begin(), e.end());

                Assert::AreEqual(values1.size(), values2.size());
                Assert::AreEqual(-1, values2.back());
                Assert::AreEqual(7, values2.front());

                vector<t> ts1 = { { 2, 1 }, { 1, 4 }, { 1, -1 }, { 2, 11 } };
                vector<t> ts2;

                auto e2 = from(ts1) | order_by([](t& v) { return v.p1; }, dir::asc, [](t& v) { return v.p2; }, dir::desc);
                ts2.assign(e2.begin(), e2.end());
                Assert::AreEqual(ts1.size(), ts2.size());
                Assert::AreEqual(1, ts2.front().p1);
                Assert::AreEqual(4, ts2.front().p2);
                Assert::AreEqual(2, ts2.back().p1);
                Assert::AreEqual(1, ts2.back().p2);

                e2 = from(ts1) | order_by([](t& v) { return v.p1; }, dir::desc, [](t& v) { return v.p2; }, dir::asc);
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

                auto e = from(ts1) | group_by([](t& v) { return v.p1; });
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

                auto e2 = from(ts1) | group_by([](t& v) { return v.p1; }, [](t& v) { return v.p2; });
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

        BEGIN_TEST_METHOD_ATTRIBUTE(cast_test)
            TEST_METHOD_ATTRIBUTE(L"Category", L"Linqlike")
        END_TEST_METHOD_ATTRIBUTE()
        TEST_METHOD(cast_test)
        {
            try
            {
                vector<int> values = { 2, 1, 2, 3, 4, 5 };

                short f = from(values) | where([](int v) { return v % 2 != 0; }) | scast<short>() | first_or_default();

                Assert::AreEqual((short)1, f);

                vector<shared_ptr<t>> ts;
                ts.push_back(make_shared<t2>(11, 12, 13));
                ts.push_back(make_shared<t2>(1, 2, 3));
                ts.push_back(make_shared<t2>(4, 5, 6));
                
                int count = 0;
                for (auto v : from(ts) | dcast<t2>())
                {
                    count++;
                }
                Assert::AreEqual(3, count);

                ts.clear();

                ts.push_back(make_shared<t>(1, 2));
                ts.push_back(make_shared<t2>(11, 12, 13));
                ts.push_back(make_shared<t2>(1, 2, 3));
                ts.push_back(make_shared<t2>(4, 5, 6));

                auto t = from(ts) | of_type<t2>() | first();

                Assert::IsNotNull(t.get());
                Assert::AreEqual(11, t->p1);
            }
            catch (exception& ex)
            {
                Logger::WriteMessage(ex.what());
                throw;
            }
        }

        BEGIN_TEST_METHOD_ATTRIBUTE(to_map_test)
            TEST_METHOD_ATTRIBUTE(L"Category", L"Linqlike")
        END_TEST_METHOD_ATTRIBUTE()
        TEST_METHOD(to_map_test)
        {
            try
            {
                vector<t> ts = { { 2, 1 }, { 1, 4 }, { 1, -1 }, { 2, 11 }, { 5, 11 }, { 1, 21 } };

                auto map1 = from(ts) | to_map([](t& v) { return v.p1; });

                Assert::AreEqual((::size_t)3, map1.size());
                int sum = 0;
                for (auto& p : map1)
                {
                    sum += p.first;
                }
                Assert::AreEqual(1 + 2 + 5, sum);

                auto map2 = from(ts) | to_map([](t& v) { return v.p2; }, [](t& v) { return v.p1; });
                Assert::AreEqual((::size_t)5, map2.size());
                for (auto& p : map2)
                {
                    Assert::IsTrue(p.second == 1 || p.second == 2 || p.second == 5);
                }
            }
            catch (exception& ex)
            {
                Logger::WriteMessage(ex.what());
                throw;
            }
        }

        BEGIN_TEST_METHOD_ATTRIBUTE(size_test)
            TEST_METHOD_ATTRIBUTE(L"Category", L"Linqlike")
        END_TEST_METHOD_ATTRIBUTE()
        TEST_METHOD(size_test)
        {
                try
                {
                    vector<t> ts = { { 2, 1 }, { 1, 4 }, { 1, -1 }, { 2, 11 }, { 5, 11 }, { 1, 21 } };

                    Assert::AreEqual(ts.size(), from(ts) | size());
                    Assert::AreEqual(ts.size() - 1, from(ts) | size([](t& v) { return v.p2 != 21; }));

                    ts.clear();

                    Assert::AreEqual(ts.size(), from(ts) | size());
                }
                catch (exception& ex)
                {
                    Logger::WriteMessage(ex.what());
                    throw;
                }
        }

        BEGIN_TEST_METHOD_ATTRIBUTE(sum_test)
            TEST_METHOD_ATTRIBUTE(L"Category", L"Linqlike")
        END_TEST_METHOD_ATTRIBUTE()
        TEST_METHOD(sum_test)
        {
            try
            {
                vector<int> values = { 2, 1, 2, 3, 4, 5 };
                Assert::AreEqual(2 + 1 + 2 + 3 + 4 + 5, from(values) | sum());
            }
            catch (exception& ex)
            {
                Logger::WriteMessage(ex.what());
                throw;
            }
        }

        BEGIN_TEST_METHOD_ATTRIBUTE(distinct_test)
            TEST_METHOD_ATTRIBUTE(L"Category", L"Linqlike")
        END_TEST_METHOD_ATTRIBUTE()
        TEST_METHOD(distinct_test)
        {
            try
            {
                vector<int> values = { 2, 1, 2, 3, 4, 5, 5, 1, 4, 3, 10 };
                vector<int> desiredValues = { 2, 1, 3, 4, 5, 10 };
                    
                vector<int> distinctValues = from(values) | distinct() | to_vector();

                Assert::AreEqual(desiredValues.size(), distinctValues.size());

                for (size_t i = 0; i < desiredValues.size(); i++)
                {
                    Assert::AreEqual(desiredValues[i], distinctValues[i]);
                }

                vector<t> ts = { { 2, 1 }, { 1, 4 }, { 1, -1 }, { 2, 11 }, { 5, 11 }, { 1, 21 } };
                vector<t> desiredTs = { { 2, 1 }, { 1, 4 }, { 5, 11 } };

                vector<t> distinctTs = from(ts) | distinct([](const t& t1, const t& t2) { return t1.p1 == t2.p1; }) | to_vector();

                Assert::AreEqual(desiredTs.size(), distinctTs.size());

                for (size_t i = 0; i < desiredTs.size(); i++)
                {
                    Assert::IsTrue(desiredTs[i] == distinctTs[i]);
                }
            }
            catch (exception& ex)
            {
                Logger::WriteMessage(ex.what());
                throw;
            }
        }

        BEGIN_TEST_METHOD_ATTRIBUTE(cross_join_test)
            TEST_METHOD_ATTRIBUTE(L"Category", L"Linqlike")
        END_TEST_METHOD_ATTRIBUTE()
        TEST_METHOD(cross_join_test)
        {
            try
            {
                vector<int> values1 = { 2, 1, 2, 3, 4, 5 };
                vector<int> values2 = { 10, 11 };
                size_t count = 0, s = 0;
                for (auto& v : from(values1) | cross_join(from(values2)))
                {
                    count++;
                    s += v.first + v.second;
                }
                Assert::AreEqual(values1.size() * values2.size(), count);
                Assert::AreEqual(size_t((from(values1) | sum()) * 2 + (from(values2) | sum()) * 6), s);
            }
            catch (exception& ex)
            {
                Logger::WriteMessage(ex.what());
                throw;
            }
        }

        BEGIN_TEST_METHOD_ATTRIBUTE(select_many_test)
            TEST_METHOD_ATTRIBUTE(L"Category", L"Linqlike")
        END_TEST_METHOD_ATTRIBUTE()
        TEST_METHOD(select_many_test)
        {
            try
            {
                vector<pair<int, vector<int>>> stuff;
                stuff.push_back(make_pair(0, vector<int>({ 1, 2, 3, 4 })));
                stuff.push_back(make_pair(1, vector<int>({ 5, 6, 7, 8 })));
                vector<int> values = from(stuff) | select_many([](const pair<int, vector<int>>& s) { return from(s.second); }) | to_vector();
                
                Assert::AreEqual(size_t(8), values.size());
                Assert::AreEqual(1 + 2 + 3 + 4 + 5 + 6 + 7 + 8, from(values) | sum());
            }
            catch (exception& ex)
            {
                Logger::WriteMessage(ex.what());
                throw;
            }
        }
	};
}