#pragma once

#include "OCLTypedefs.h"
#include "OCLVectorKernelName.h"
#include "NNMetadata.h"
#include "OCLKernelBase.h"
#include <tuple>

namespace NeuroflowN
{
    enum class ComputingUnit
    {
        CPU, GPU
    };

    template <typename T>
    class OCLActivationKernelVersion
    {
        template<const char* NameTemplate>
        friend class OCLActivationKernelBase;

        std::vector<std::unique_ptr<T>> values;

        template <typename... Args>
        void EmplaceVersion(::size_t version, Args... args)
        {
            values.resize(version + 1);
            if (values[version] == null) values.emplace(values.cbegin() + version, std::make_unique<T>(args...));
        }

    public:
        OCLActivationKernelVersion() { }
        OCLActivationKernelVersion(const OCLActivationKernelVersion& other) = delete;
        OCLActivationKernelVersion(OCLActivationKernelVersion&& other) :
            values(move(other.values))
        {
        }
        
        const T& GetVersion(::size_t version) const
        {
            return *(values[version].get());
        }
    };

    template<const char* NameTemplate>
    class OCLActivationKernelBase : public OCLKernelBase
    {
        std::vector<OCLActivationKernelVersion<OCLVectorKernelName>> cpuNames;
        std::vector<OCLActivationKernelVersion<OCLVectorKernelName>> gpuNames;

        static std::string CreateName(const char* type, const char* unit, unsigned size)
        {
            using namespace std;

            string result = string(NameTemplate);
            boost::replace_all(result, "{0}", type);
            boost::replace_all(result, "{1}", to_string(size));
            boost::replace_all(result, "{2}", unit);
            return move(result);
        }

    protected:

        OCLActivationKernelVersion<std::string> CreateNames(const std::initializer_list<::size_t>& versions, ComputingUnit unit, unsigned size)
        {
            using namespace std;

            const char* unitStr = unit == ComputingUnit::CPU ? "CPU" : "GPU";
            OCLActivationKernelVersion<std::string> result;
            for (auto i : versions)
            {
                stringstream n;
                n << "Version";
                n << i;
                result.EmplaceVersion(i, CreateName(n.str().c_str(), unitStr, size));
            }
            return result;
        }

        inline const OCLActivationKernelVersion<OCLVectorKernelName>& GetCPUNames(unsigned size) const
        {
            return cpuNames[size - 1];
        }

        inline const OCLActivationKernelVersion<OCLVectorKernelName>& GetGPUNames(unsigned size) const
        {
            return gpuNames[size - 1];
        }

    public:
        OCLActivationKernelBase(const OCLIntCtxSPtrT& ctx, const std::initializer_list<::size_t>& versions, unsigned maxConnectionCount) :
            OCLKernelBase(ctx)
        {
            for (unsigned size = 1; size <= maxConnectionCount; size++)
            {
                auto cpu = CreateNames(versions, ComputingUnit::CPU, size);
                auto gpu = CreateNames(versions, ComputingUnit::GPU, size);
                cpuNames.emplace_back();
                gpuNames.emplace_back();

                for (auto i : versions)
                {
                    auto& cpuN = cpu.GetVersion(i);
                    cpuNames.back().EmplaceVersion(i, cpuN);
                    auto& gpuN = gpu.GetVersion(i);
                    gpuNames.back().EmplaceVersion(i, gpuN);
                }
            }
        }
    };

}