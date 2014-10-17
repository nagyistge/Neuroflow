#include "stdafx.h"
#include "CppUnitTest.h"
#include "nf.h"

using namespace Microsoft::VisualStudio::CppUnitTestFramework;
using namespace std;
using namespace nf;

namespace nfut
{
	TEST_CLASS(data_array_factory_tests)
	{
	public:
		
        BEGIN_TEST_METHOD_ATTRIBUTE(cpp_copy_data)
            TEST_METHOD_ATTRIBUTE(L"Category", L"Array")
            TEST_METHOD_ATTRIBUTE(L"Platform", L"CPP")
        END_TEST_METHOD_ATTRIBUTE()
        TEST_METHOD(cpp_copy_data)
		{
            try
            {
                auto ctx = computation_context_factory::default().create_context(cpp_context);
                test_copy_data(ctx);
            }
            catch (exception& ex)
            {
                Logger::WriteMessage(ex.what());
                throw;
            }
		}

        BEGIN_TEST_METHOD_ATTRIBUTE(ocl_copy_data_cpu)
            TEST_METHOD_ATTRIBUTE(L"Category", L"Array")
            TEST_METHOD_ATTRIBUTE(L"Platform", L"OCL")
            TEST_METHOD_ATTRIBUTE(L"Device", L"OCL CPU")
        END_TEST_METHOD_ATTRIBUTE()
        TEST_METHOD(ocl_copy_data_cpu)
        {
            try
            {
                auto ctx = computation_context_factory::default().create_context(ocl_context, L"CPU");
                test_copy_data(ctx);
            }
            catch (exception& ex)
            {
                Logger::WriteMessage(ex.what());
                throw;
            }
        }

        BEGIN_TEST_METHOD_ATTRIBUTE(ocl_copy_data_gpu)
            TEST_METHOD_ATTRIBUTE(L"Category", L"Array")
            TEST_METHOD_ATTRIBUTE(L"Platform", L"OCL")
            TEST_METHOD_ATTRIBUTE(L"Device", L"OCL GPU")
        END_TEST_METHOD_ATTRIBUTE()
        TEST_METHOD(ocl_copy_data_gpu)
        {
            try
            {
                auto ctx = computation_context_factory::default().create_context(ocl_context, L"GPU");
                test_copy_data(ctx);
            }
            catch (exception& ex)
            {
                Logger::WriteMessage(ex.what());
                throw;
            }
        }

        BEGIN_TEST_METHOD_ATTRIBUTE(cpp_pooling)
            TEST_METHOD_ATTRIBUTE(L"Category", L"Array")
            TEST_METHOD_ATTRIBUTE(L"Platform", L"CPP")
        END_TEST_METHOD_ATTRIBUTE()
        TEST_METHOD(cpp_pooling)
        {
            try
            {
                auto ctx = computation_context_factory::default().create_context(cpp_context);
                test_pooling(ctx);
            }
            catch (exception& ex)
            {
                Logger::WriteMessage(ex.what());
                throw;
            }
        }

        BEGIN_TEST_METHOD_ATTRIBUTE(ocl_pooling_cpu)
            TEST_METHOD_ATTRIBUTE(L"Category", L"Array")
            TEST_METHOD_ATTRIBUTE(L"Platform", L"OCL")
            TEST_METHOD_ATTRIBUTE(L"Device", L"OCL CPU")
        END_TEST_METHOD_ATTRIBUTE()
        TEST_METHOD(ocl_pooling_cpu)
        {
            try
            {
                auto ctx = computation_context_factory::default().create_context(ocl_context, L"CPU");
                test_pooling(ctx);
            }
            catch (exception& ex)
            {
                Logger::WriteMessage(ex.what());
                throw;
            }
        }

        BEGIN_TEST_METHOD_ATTRIBUTE(ocl_pooling_gpu)
            TEST_METHOD_ATTRIBUTE(L"Category", L"Array")
            TEST_METHOD_ATTRIBUTE(L"Platform", L"OCL")
            TEST_METHOD_ATTRIBUTE(L"Device", L"OCL GPU")
        END_TEST_METHOD_ATTRIBUTE()
        TEST_METHOD(ocl_pooling_gpu)
        {
            try
            {
                auto ctx = computation_context_factory::default().create_context(ocl_context, L"gpu");
                test_pooling(ctx);
            }
            catch (exception& ex)
            {
                Logger::WriteMessage(ex.what());
                throw;
            }
        }

    private:

        void test_copy_data(computation_context_ptr ctx)
        {
            vector<float> values = { 0.0f, 1.1f, 2.2f, 3.3f, 4.4f };
            vector<float> target(2);
            auto valuesArray = ctx->data_array_factory()->create_const(&values[0], 1, 2);
            auto targetArray = ctx->data_array_factory()->create(2, 100.0f);

            Assert::IsNotNull(valuesArray.get());
            Assert::AreEqual(size_t(2), valuesArray->size());

            // Verify is target is filled:
            targetArray->read(0, 2, &target[0], 0).wait();
            for (float v : target) Assert::AreEqual(100.0f, v);

            ctx->device_array_management()->copy(valuesArray, 0, targetArray, 0, 2);

            targetArray->read(0, 2, &target[0], 0).wait();
            Assert::AreEqual(1.1f, target[0]);
            Assert::AreEqual(2.2f, target[1]);
        }

        void test_pooling(computation_context_ptr ctx)
        {
            array<float, 100> values;
            
            auto pool = ctx->device_array_management()->create_pool(true);
            auto a1 = pool->create_array2(10, 10);
            auto a2 = pool->create_array(100);
            auto da = ctx->data_array_factory()->create(100, 9.9f);

            da->read(0, 100, &values[0], 0).wait();
            for (auto v : values) Assert::AreEqual(9.9f, v);

            ctx->device_array_management()->copy(a1, 0, da, 0, 100);
            da->read(0, 100, &values[0], 0).wait();
            for (auto v : values) Assert::AreEqual(0.0f, v);

            ctx->device_array_management()->copy(a2, 0, da, 0, 100);
            da->read(0, 100, &values[0], 0).wait();
            for (auto v : values) Assert::AreEqual(0.0f, v);

            fill(values.begin(), values.end(), 1.0f);
            da->write(&values[0], 0, 100, 0);
            ctx->device_array_management()->copy(da, 1, a1, 1, 99);
            ctx->utils()->zero(da);
            ctx->device_array_management()->copy(a1, 0, da, 0, 100);
            da->read(0, 100, &values[0], 0).wait();
            auto it = values.cbegin();
            Assert::AreEqual(0.0f, *it);
            it++;
            for (; it != values.cend(); it++) Assert::AreEqual(1.0f, *it);

            ctx->device_array_management()->copy(a1, 0, a2, 1, 2);
            ctx->device_array_management()->copy(a2, 0, da, 0, 100);
            da->read(0, 100, &values[0], 0).wait();
            it = values.cbegin();
            Assert::AreEqual(0.0f, *it);
            it++;
            Assert::AreEqual(0.0f, *it);
            it++;
            Assert::AreEqual(1.0f, *it);
            it++;
            for (; it != values.cend(); it++) Assert::AreEqual(0.0f, *it);

            pool->zero();

            ctx->device_array_management()->copy(a1, 0, da, 0, 100);
            da->read(0, 100, &values[0], 0).wait();
            for (auto v : values) Assert::AreEqual(0.0f, v);

            ctx->device_array_management()->copy(a2, 0, da, 0, 100);
            da->read(0, 100, &values[0], 0).wait();
            for (auto v : values) Assert::AreEqual(0.0f, v);
        }
	};
}