#define BOOST_TEST_MODULE neuroflow_code
#include "stdafx.h"
#include <boost/test/unit_test.hpp>

int add(int i, int j)
{
    return i + j;
}

BOOST_AUTO_TEST_CASE(universeInOrder)
{
    BOOST_CHECK(add(2, 2) == 4);
    BOOST_CHECK(add(2, 2) == 5);
    BOOST_CHECK(add(2, 2) == 4);
}
