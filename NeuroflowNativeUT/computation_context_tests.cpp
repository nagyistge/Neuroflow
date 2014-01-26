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
            Assert::IsTrue(ctx->properties().empty());
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

                Assert::IsFalse(ctx->properties().empty());
                Assert::AreEqual(idx_t(4), ctx->properties().get<idx_t>(ocl_prop_max_connection_count));
                Assert::AreEqual(idx_t(4), ctx->properties().get<idx_t>(ocl_prop_max_layer_count));

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
    };
}