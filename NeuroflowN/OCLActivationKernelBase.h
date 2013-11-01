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

    template <typename T, ::size_t NameCount>
    class OCLActivationKernelName
    {
        template<const char* NameTemplate, ::size_t NameCount>
        friend class OCLActivationKernelBase;

        std::vector<T> values;

        void PushName(T&& value)
        {
            values.emplace_back(value);
        }

    public:
        OCLActivationKernelName() { }
        OCLActivationKernelName(const OCLActivationKernelName& other) :
            values(other.values)
        {
        }
        OCLActivationKernelName(OCLActivationKernelName&& other) :
            values(other.values)
        {
        }

        const T& GetName(::size_t index) const
        {
            return values[index];
        }
    };

    template<const char* NameTemplate, ::size_t NameCount>
    class OCLActivationKernelBase : public OCLKernelBase
    {
        std::vector<OCLActivationKernelName<OCLVectorKernelName, NameCount>> cpuNames;
        std::vector<OCLActivationKernelName<OCLVectorKernelName, NameCount>> gpuNames;

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

        static OCLActivationKernelName<std::string, NameCount> CreateNames(ComputingUnit unit, unsigned size)
        {
            using namespace std;

            const char* unitStr = unit == ComputingUnit::CPU ? "CPU" : "GPU";
            OCLActivationKernelName<std::string, NameCount> result;
            for (::size_t i = 0; i < NameCount; i++)
            {
                stringstream n;
                n << "Version";
                n << i;
                result.PushName(CreateName(n.str().c_str(), unitStr, size));
            }
            return result;
        }

        inline const OCLActivationKernelName<OCLVectorKernelName, NameCount>& GetCPUNames(unsigned size) const
        {
            return cpuNames[size - 1];
        }

        inline const OCLActivationKernelName<OCLVectorKernelName, NameCount>& GetGPUNames(unsigned size) const
        {
            return gpuNames[size - 1];
        }

    public:
        OCLActivationKernelBase(const OCLIntCtxSPtrT& ctx) :
            OCLKernelBase(ctx)
        {
            for (unsigned size = 1; size <= ctx->GetMaxConnectionCount(); size++)
            {
                auto cpu = CreateNames(ComputingUnit::CPU, size);
                auto gpu = CreateNames(ComputingUnit::GPU, size);
                cpuNames.emplace_back();
                gpuNames.emplace_back();

                for (::size_t i = 0; i < NameCount; i++)
                {
                    auto& cpuN = cpu.GetName(i);
                    cpuNames.back().PushName(OCLVectorKernelName(cpuN));
                    auto& gpuN = gpu.GetName(i);
                    gpuNames.back().PushName(OCLVectorKernelName(gpuN));
                }
            }
        }
    };

}