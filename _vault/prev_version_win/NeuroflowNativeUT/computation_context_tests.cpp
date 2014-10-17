#include "stdafx.h"
#include "CppUnitTest.h"
#include "nf.h"

using namespace Microsoft::VisualStudio::CppUnitTestFramework;
using namespace std;
using namespace nf;

namespace nfut
{
    TEST_CLASS(computation_context_tests)
    {
    public:

        BEGIN_TEST_METHOD_ATTRIBUTE(get_nf_version)
            TEST_METHOD_ATTRIBUTE(L"Category", L"Misc")
        END_TEST_METHOD_ATTRIBUTE()
        TEST_METHOD(get_nf_version)
        {
            auto v = version();
            Logger::WriteMessage(v.c_str());
        }

        BEGIN_TEST_METHOD_ATTRIBUTE(cpp_get_devices)
            TEST_METHOD_ATTRIBUTE(L"Category", L"Computation Context")
            TEST_METHOD_ATTRIBUTE(L"Platform", L"CPP")
        END_TEST_METHOD_ATTRIBUTE()
        TEST_METHOD(cpp_get_devices)
        {
            computation_context_factory factory;
            auto devices = factory.get_available_devices(cpp_context);
            
            Assert::AreEqual(size_t(1), devices.size());
            Assert::AreEqual(L"cpp_st", devices.front().id().c_str());
            Assert::AreEqual(L"1.0", devices.front().version().c_str());
            Assert::AreEqual(L"C++ Single Threaded", devices.front().name().c_str());
            Assert::AreEqual(L"x86/x64", devices.front().platform().c_str());
        }

        BEGIN_TEST_METHOD_ATTRIBUTE(cpp_create_device)
            TEST_METHOD_ATTRIBUTE(L"Category", L"Computation Context")
            TEST_METHOD_ATTRIBUTE(L"Platform", L"CPP")
        END_TEST_METHOD_ATTRIBUTE()
        TEST_METHOD(cpp_create_device)
        {
            computation_context_factory factory;
            auto devices = factory.get_available_devices(cpp_context);
            
            Assert::AreEqual(size_t(1), devices.size());
            
            auto ctx = factory.create_context(cpp_context, devices.front().id());
            
            Assert::IsNotNull(ctx.get());
            Assert::AreEqual(devices.front().id().c_str(), ctx->device_info().id().c_str());
            Assert::AreEqual(devices.front().name().c_str(), ctx->device_info().name().c_str());
            Assert::AreEqual(devices.front().platform().c_str(), ctx->device_info().platform().c_str());
            Assert::IsNotNull(ctx->data_array_factory().get());
            Assert::IsNotNull(ctx->device_array_management().get());
            Assert::IsNotNull(ctx->utils().get());
        }

        BEGIN_TEST_METHOD_ATTRIBUTE(ocl_get_devices)
            TEST_METHOD_ATTRIBUTE(L"Category", L"Computation Context")
            TEST_METHOD_ATTRIBUTE(L"Platform", L"OCL")
        END_TEST_METHOD_ATTRIBUTE()
        TEST_METHOD(ocl_get_devices)
        {
            try
            {
                computation_context_factory factory;
                auto devices = factory.get_available_devices(ocl_context);

                Assert::AreNotEqual(size_t(0), devices.size());
            }
            catch (exception& ex)
            {
                Logger::WriteMessage(ex.what());
                throw;
            }            
        }

        BEGIN_TEST_METHOD_ATTRIBUTE(ocl_create_device)
            TEST_METHOD_ATTRIBUTE(L"Category", L"Computation Context")
            TEST_METHOD_ATTRIBUTE(L"Platform", L"OCL")
        END_TEST_METHOD_ATTRIBUTE()
        TEST_METHOD(ocl_create_device)
        {
            try
            {
                computation_context_factory factory;
                auto devices = factory.get_available_devices(ocl_context);

                Assert::AreNotEqual(size_t(0), devices.size());

                auto ctx = factory.create_context(ocl_context, devices.front().id());

                Assert::IsNotNull(ctx.get());
                Assert::AreEqual(devices.front().id().c_str(), ctx->device_info().id().c_str());
                Assert::AreEqual(devices.front().name().c_str(), ctx->device_info().name().c_str());
                Assert::AreEqual(devices.front().platform().c_str(), ctx->device_info().platform().c_str());

                Assert::IsNotNull(ctx->data_array_factory().get());
                Assert::IsNotNull(ctx->device_array_management().get());
                Assert::IsNotNull(ctx->utils().get());
            }
            catch (exception& ex)
            {
                Logger::WriteMessage(ex.what());
                throw;
            }
        }

        BEGIN_TEST_METHOD_ATTRIBUTE(ocl_rnd)
            TEST_METHOD_ATTRIBUTE(L"Category", L"Computation Context")
            TEST_METHOD_ATTRIBUTE(L"Platform", L"OCL")
        END_TEST_METHOD_ATTRIBUTE()
        TEST_METHOD(ocl_rnd)
        {
            try
            {
                test_rnd(ocl_context, L"cpu");
            }
            catch (exception& ex)
            {
                Logger::WriteMessage(ex.what());
                throw;
            }
        }

        BEGIN_TEST_METHOD_ATTRIBUTE(cpp_rnd)
            TEST_METHOD_ATTRIBUTE(L"Category", L"Computation Context")
            TEST_METHOD_ATTRIBUTE(L"Platform", L"CPP")
        END_TEST_METHOD_ATTRIBUTE()
        TEST_METHOD(cpp_rnd)
        {
            try
            {
                test_rnd(cpp_context, L"");
            }
            catch (exception& ex)
            {
                Logger::WriteMessage(ex.what());
                throw;
            }
        }

        void test_rnd(const wchar_t* typeId, wchar_t* hint = null)
        {
            computation_context_factory factory;

            // Random seed is random
            auto ctx = factory.create_context(typeId, hint);
            float v1 = ctx->rnd().next(0, 1);
            ctx = factory.create_context(typeId, hint);
            float v2 = ctx->rnd().next(0, 1);

            Assert::AreNotEqual(v1, v2);
            Assert::IsTrue(v1 >= 0 && v1 < 1);
            Assert::IsTrue(v2 >= 0 && v2 < 1);

            // Random seed is 0
            cc_init_pars props;
            props.random_seed = 0;
            ctx = factory.create_context(typeId, hint, &props);
            v1 = ctx->rnd().next(0, 1);
            ctx = factory.create_context(typeId, hint, &props);
            v2 = ctx->rnd().next(0, 1);

            Assert::AreEqual(v1, v2);
            Assert::IsTrue(v1 >= 0 && v1 < 1);
            Assert::IsTrue(v2 >= 0 && v2 < 1);

            // Random seed is 1
            cc_init_pars props2;
            props2.random_seed = 1;
            ctx = factory.create_context(typeId, hint, &props2);
            auto v3 = ctx->rnd().next(0, 1);
            ctx = factory.create_context(typeId, hint, &props2);
            auto v4 = ctx->rnd().next(0, 1);

            Assert::AreEqual(v3, v4);
            Assert::IsTrue(v3 >= 0 && v3 < 1);
            Assert::IsTrue(v4 >= 0 && v4 < 1);

            Assert::AreNotEqual(v1, v3);
        }
    };
}