#include <boost/test/unit_test.hpp>
#include "stdafx.h"
#include "nf.h"

using namespace nf;
using namespace std;

BOOST_AUTO_TEST_SUITE(computation_context_tests)

// Impl:

void test_zero(computation_context_ptr ctx)
{
    vector<float> values = { 0.1f, 1.1f, 2.2f, 3.3f, 4.4f };
    auto valuesArray = ctx->data_array_factory()->create(&values[0], 0, values.size());

    valuesArray->read(0, values.size(), &values[0], 0).wait();
    for (float v : values)
    {
        BOOST_REQUIRE(0.0f != v);
    }

    ctx->utils()->zero(valuesArray);

    valuesArray->read(0, values.size(), &values[0], 0).wait();
    for (float v : values)
    {
        BOOST_REQUIRE_EQUAL(0.0f, v);
    }
}

float calc_mse(const vector<float>& desired, const vector<float>& current)
{
    float mse = 0.0f;
    for (size_t i = 0; i < desired.size(); i++)
    {
        float v = (desired[i] - current[i]) * 0.5f;
        mse += v * v;
    }
    return mse / (float)desired.size();
}

float calc_mse(const vector<vector<vector<float>>>& desired, const vector<vector<vector<float>>>& current)
{
    BOOST_REQUIRE_EQUAL(desired.size(), current.size());

    float count = 0.0f;
    float mse = 0.0f;
    for (size_t i1 = 0; i1 < desired.size(); i1++)
    {
        auto& d1 = desired[i1];
        auto& c1 = current[i1];
        BOOST_REQUIRE_EQUAL(d1.size(), c1.size());
        for (size_t i2 = 0; i2 < d1.size(); i2++)
        {
            auto& d2 = d1[i2];
            auto& c2 = c1[i2];
            BOOST_REQUIRE_EQUAL(d2.size(), c2.size());

            mse += calc_mse(d2, c2);
            count++;
        }
    }

    return mse / count;
}

void test_calculate_mse(computation_context_ptr ctx)
{
    vector<vector<vector<float>>> desired =
    {
        {
            { 0.0345436f, 0.1345345f, 0.234346f },
            { 0.2784f, 0.6376768f, 0.9465477f }
        },
        {
            { 1.0f, 0.26576765f, 0.7376888f },
            { 0.183675457f, 0.437677f, 0.633776376357f }
        }
    };

    vector<vector<vector<float>>> current =
    {
        {
            { 0.1f, 0.5f, 0.8f },
            { 0.9f, 0.6f, 0.3f }
        },
        {
            { 1.0f, 0.2f, 0.5f },
            { 0.3f, 0.1f, 0.3f }
        }
    };

    float mse = calc_mse(desired, current);

    supervised_batch batch;
    auto resultValues = ctx->data_array_factory()->create(15, 8.0f);

    BOOST_REQUIRE_EQUAL(desired.size(), current.size());
    for (size_t i1 = 0; i1 < desired.size(); i1++)
    {
        auto& d1 = desired[i1];
        auto& c1 = current[i1];
        BOOST_REQUIRE_EQUAL(d1.size(), c1.size());
        auto& sample = batch.new_back();
        for (size_t i2 = 0; i2 < d1.size(); i2++)
        {
            auto& d2 = d1[i2];
            auto& c2 = c1[i2];
            BOOST_REQUIRE_EQUAL(d2.size(), c2.size());
            auto da = ctx->data_array_factory()->create_const(&d2[0], 0, d2.size());
            auto ca = ctx->data_array_factory()->create_const(&c2[0], 0, c2.size());
            sample.push_back(da, da, ca);
        }
    }

    vector<float> result(15);

    ctx->utils()->calculate_mse(batch, resultValues, 1);

    resultValues->read(0, result.size(), &result[0], 0).wait();

    BOOST_REQUIRE_EQUAL(8.0f, result[0]);
    BOOST_REQUIRE(abs(mse - result[1]) < 0.000001f);
    BOOST_REQUIRE_EQUAL(8.0f, result[2]);
    BOOST_REQUIRE_EQUAL(8.0f, result[14]);
}

BOOST_AUTO_TEST_CASE(cpp_zero)
{
    auto ctx = computation_context_factory::instance().create_context(cpp_context);
    test_zero(ctx);
}

BOOST_AUTO_TEST_CASE(ocl_zero_cpu)
{
    auto ctx = computation_context_factory::instance().create_context(ocl_context, L"CPU");
    test_zero(ctx);
}

BOOST_AUTO_TEST_CASE(ocl_zero_gpu)
{
    auto ctx = computation_context_factory::instance().create_context(ocl_context, L"gPU");
    test_zero(ctx);
}

BOOST_AUTO_TEST_CASE(cpp_calculate_mse)
{
    auto ctx = computation_context_factory::instance().create_context(cpp_context);
    test_calculate_mse(ctx);
}

BOOST_AUTO_TEST_CASE(ocl_calculate_mse_cpu)
{
    auto ctx = computation_context_factory::instance().create_context(ocl_context, L"CPU");
    test_calculate_mse(ctx);
}

BOOST_AUTO_TEST_CASE(ocl_calculate_mse_gpu)
{
    auto ctx = computation_context_factory::instance().create_context(ocl_context, L"gpu");
    test_calculate_mse(ctx);
}

BOOST_AUTO_TEST_SUITE_END()
