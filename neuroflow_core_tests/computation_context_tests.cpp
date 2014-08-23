#include <boost/test/unit_test.hpp>
#include "stdafx.h"
#include "nf.h"

using namespace nf;

BOOST_AUTO_TEST_SUITE(computation_context_tests)

BOOST_AUTO_TEST_CASE(get_nf_version)
{
    auto v = version();
    BOOST_CHECK(v.size() > 0);
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

BOOST_AUTO_TEST_SUITE_END()
