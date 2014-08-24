#include <boost/test/unit_test.hpp>
#include "stdafx.h"
#include "nf.h"

using namespace nf;
using namespace std;

BOOST_AUTO_TEST_SUITE(computation_context_tests)

BOOST_AUTO_TEST_CASE(get_nf_version)
{
    auto v = version();
    BOOST_CHECK(v.size() > 0);
    wstringstream s;
    s << L"Neuroflow Version: ";
    s << v;
    auto str = s.str();
    BOOST_TEST_MESSAGE(string(str.begin(), str.end()));
}

BOOST_AUTO_TEST_CASE(cpp_get_devices)
{
    computation_context_factory factory;
    auto devices = factory.get_available_devices(cpp_context);

    BOOST_REQUIRE_EQUAL(size_t(1), devices.size());
    BOOST_REQUIRE_EQUAL(L"cpp_st", devices.front().id().c_str());
    BOOST_REQUIRE_EQUAL(L"1.0", devices.front().version().c_str());
    BOOST_REQUIRE_EQUAL(L"C++ Single Threaded", devices.front().name().c_str());
    BOOST_REQUIRE_EQUAL(L"x86/x64", devices.front().platform().c_str());
}

BOOST_AUTO_TEST_CASE(cpp_create_device)
{
    computation_context_factory factory;
    auto devices = factory.get_available_devices(cpp_context);

    BOOST_REQUIRE_EQUAL(size_t(1), devices.size());

    auto ctx = factory.create_context(cpp_context, devices.front().id());

    BOOST_REQUIRE(ctx.get() != null);
    BOOST_REQUIRE_EQUAL(devices.front().id().c_str(), ctx->device_info().id().c_str());
    BOOST_REQUIRE_EQUAL(devices.front().name().c_str(), ctx->device_info().name().c_str());
    BOOST_REQUIRE_EQUAL(devices.front().platform().c_str(), ctx->device_info().platform().c_str());
    BOOST_REQUIRE(ctx->data_array_factory().get() != null);
    BOOST_REQUIRE(ctx->device_array_management().get() != null);
    BOOST_REQUIRE(ctx->utils().get() != null);
}

BOOST_AUTO_TEST_CASE(ocl_get_devices)
{
    computation_context_factory factory;
    auto devices = factory.get_available_devices(ocl_context);

    BOOST_REQUIRE(devices.size() > size_t(0));

    BOOST_TEST_MESSAGE("OCL Devices:");
    for (auto device: devices)
    {
        auto dstr = to_wstring(device);
        BOOST_TEST_MESSAGE(string(dstr.begin(), dstr.end()));
    }
}

BOOST_AUTO_TEST_CASE(ocl_create_device)
{
    computation_context_factory factory;
    auto devices = factory.get_available_devices(ocl_context);

    BOOST_REQUIRE(devices.size() > size_t(0));

    auto ctx = factory.create_context(ocl_context, devices.front().id());

    BOOST_REQUIRE(ctx.get() != null);
    BOOST_REQUIRE_EQUAL(devices.front().id().c_str(), ctx->device_info().id().c_str());
    BOOST_REQUIRE_EQUAL(devices.front().name().c_str(), ctx->device_info().name().c_str());
    BOOST_REQUIRE_EQUAL(devices.front().platform().c_str(), ctx->device_info().platform().c_str());

    BOOST_REQUIRE(ctx->data_array_factory().get() != null);
    BOOST_REQUIRE(ctx->device_array_management().get() != null);
    BOOST_REQUIRE(ctx->utils().get() != null);
}

void test_rnd(const wchar_t* typeId, std::wstring hint)
{
    computation_context_factory factory;

    // Random seed is random
    auto ctx = factory.create_context(typeId, hint);
    float v1 = ctx->rnd().next(0, 1);
    ctx = factory.create_context(typeId, hint);
    float v2 = ctx->rnd().next(0, 1);

    BOOST_REQUIRE(v1 != v2);
    BOOST_REQUIRE(v1 >= 0 && v1 < 1);
    BOOST_REQUIRE(v2 >= 0 && v2 < 1);

    // Random seed is 0
    cc_init_pars props;
    props.random_seed = 0;
    ctx = factory.create_context(typeId, hint, &props);
    v1 = ctx->rnd().next(0, 1);
    ctx = factory.create_context(typeId, hint, &props);
    v2 = ctx->rnd().next(0, 1);

    BOOST_REQUIRE_EQUAL(v1, v2);
    BOOST_REQUIRE(v1 >= 0 && v1 < 1);
    BOOST_REQUIRE(v2 >= 0 && v2 < 1);

    // Random seed is 1
    cc_init_pars props2;
    props2.random_seed = 1;
    ctx = factory.create_context(typeId, hint, &props2);
    auto v3 = ctx->rnd().next(0, 1);
    ctx = factory.create_context(typeId, hint, &props2);
    auto v4 = ctx->rnd().next(0, 1);

    BOOST_REQUIRE_EQUAL(v3, v4);
    BOOST_REQUIRE(v3 >= 0 && v3 < 1);
    BOOST_REQUIRE(v4 >= 0 && v4 < 1);

    BOOST_REQUIRE(v1 != v3);
}

BOOST_AUTO_TEST_CASE(ocl_rnd)
{
    test_rnd(ocl_context, L"cpu");
}

BOOST_AUTO_TEST_CASE(cpp_rnd)
{
    test_rnd(cpp_context, L"");
}

BOOST_AUTO_TEST_SUITE_END()
