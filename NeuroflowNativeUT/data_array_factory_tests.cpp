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
		
        BEGIN_TEST_METHOD_ATTRIBUTE(copy_cpp_data)
            TEST_METHOD_ATTRIBUTE(L"Category", L"Data Array")
            TEST_METHOD_ATTRIBUTE(L"Platform", L"CPP")
        END_TEST_METHOD_ATTRIBUTE()
        TEST_METHOD(copy_cpp_data)
		{
            auto ctx = computation_context_factory::default().create_context(cpp_context);
            test_copy_data(ctx);
		}

    private:

        void test_copy_data(computation_context_ptr& ctx)
        {
            vector<float> values = { 0.0f, 1.1f, 2.2f, 3.3f, 4.4f };
            auto valuesArray = ctx->data_array_factory()->create_const(&values[0], 1, 2);

            Assert::IsNotNull(valuesArray.get());
            Assert::AreEqual(size_t(2), valuesArray->size());
        }

	};
}