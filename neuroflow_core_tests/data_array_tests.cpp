#include <boost/test/unit_test.hpp>
#include "stdafx.h"
#include "nf.h"

using namespace nf;
using namespace std;

BOOST_AUTO_TEST_SUITE(computation_context_tests)

void test_copy_data(computation_context_ptr ctx)
{
    vector<float> values = { 0.0f, 1.1f, 2.2f, 3.3f, 4.4f };
    vector<float> target(2);
    auto valuesArray = ctx->data_array_factory()->create_const(&values[0], 1, 2);
    auto targetArray = ctx->data_array_factory()->create(2, 100.0f);

    BOOST_REQUIRE(valuesArray.get() != null);
    BOOST_REQUIRE_EQUAL(size_t(2), valuesArray->size());

    // Verify is target is filled:
    targetArray->read(0, 2, &target[0], 0).wait();
    for (float v : target) BOOST_REQUIRE_EQUAL(100.0f, v);

    ctx->device_array_management()->copy(valuesArray, 0, targetArray, 0, 2);

    targetArray->read(0, 2, &target[0], 0).wait();
    BOOST_REQUIRE_EQUAL(1.1f, target[0]);
    BOOST_REQUIRE_EQUAL(2.2f, target[1]);
}

void test_pooling(computation_context_ptr ctx)
{
    array<float, 100> values;

    auto pool = ctx->device_array_management()->create_pool(true);
    auto a1 = pool->create_array2(10, 10);
    auto a2 = pool->create_array(100);
    auto da = ctx->data_array_factory()->create(100, 9.9f);

    da->read(0, 100, &values[0], 0).wait();
    for (auto v : values) BOOST_REQUIRE_EQUAL(9.9f, v);

    ctx->device_array_management()->copy(a1, 0, da, 0, 100);
    da->read(0, 100, &values[0], 0).wait();
    for (auto v : values) BOOST_REQUIRE_EQUAL(0.0f, v);

    ctx->device_array_management()->copy(a2, 0, da, 0, 100);
    da->read(0, 100, &values[0], 0).wait();
    for (auto v : values) BOOST_REQUIRE_EQUAL(0.0f, v);

    fill(values.begin(), values.end(), 1.0f);
    da->write(&values[0], 0, 100, 0);
    ctx->device_array_management()->copy(da, 1, a1, 1, 99);
    ctx->utils()->zero(da);
    ctx->device_array_management()->copy(a1, 0, da, 0, 100);
    da->read(0, 100, &values[0], 0).wait();
    auto it = values.cbegin();
    BOOST_REQUIRE_EQUAL(0.0f, *it);
    it++;
    for (; it != values.cend(); it++) BOOST_REQUIRE_EQUAL(1.0f, *it);

    ctx->device_array_management()->copy(a1, 0, a2, 1, 2);
    ctx->device_array_management()->copy(a2, 0, da, 0, 100);
    da->read(0, 100, &values[0], 0).wait();
    it = values.cbegin();
    BOOST_REQUIRE_EQUAL(0.0f, *it);
    it++;
    BOOST_REQUIRE_EQUAL(0.0f, *it);
    it++;
    BOOST_REQUIRE_EQUAL(1.0f, *it);
    it++;
    for (; it != values.cend(); it++) BOOST_REQUIRE_EQUAL(0.0f, *it);

    pool->zero();

    ctx->device_array_management()->copy(a1, 0, da, 0, 100);
    da->read(0, 100, &values[0], 0).wait();
    for (auto v : values) BOOST_REQUIRE_EQUAL(0.0f, v);

    ctx->device_array_management()->copy(a2, 0, da, 0, 100);
    da->read(0, 100, &values[0], 0).wait();
    for (auto v : values) BOOST_REQUIRE_EQUAL(0.0f, v);
}

BOOST_AUTO_TEST_CASE(cpp_copy_data)
{
    auto ctx = computation_context_factory::instance().create_context(cpp_context);
    test_copy_data(ctx);
}

BOOST_AUTO_TEST_CASE(ocl_copy_data_cpu)
{
    auto ctx = computation_context_factory::instance().create_context(ocl_context, L"CPU");
    test_copy_data(ctx);
}

BOOST_AUTO_TEST_CASE(ocl_copy_data_gpu)
{
    auto ctx = computation_context_factory::instance().create_context(ocl_context, L"GPU");
    test_copy_data(ctx);
}

BOOST_AUTO_TEST_CASE(cpp_pooling)
{
    auto ctx = computation_context_factory::instance().create_context(cpp_context);
    test_pooling(ctx);
}

BOOST_AUTO_TEST_CASE(ocl_pooling_cpu)
{
    auto ctx = computation_context_factory::instance().create_context(ocl_context, L"CPU");
    test_pooling(ctx);
}

BOOST_AUTO_TEST_CASE(ocl_pooling_gpu)
{
    auto ctx = computation_context_factory::instance().create_context(ocl_context, L"gpu");
    test_pooling(ctx);
}

BOOST_AUTO_TEST_SUITE_END()
