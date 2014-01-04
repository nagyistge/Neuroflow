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
            TEST_METHOD_ATTRIBUTE(L"Category", L"Data Array")
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
            TEST_METHOD_ATTRIBUTE(L"Category", L"Data Array")
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
            TEST_METHOD_ATTRIBUTE(L"Category", L"Data Array")
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

	};
}