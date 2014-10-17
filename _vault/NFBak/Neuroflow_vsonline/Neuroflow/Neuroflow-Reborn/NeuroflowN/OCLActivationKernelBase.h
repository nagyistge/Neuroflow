#pragma once

#include "OCLTypedefs.h"
#include "OCLVectorKernelName.h"
#include "NNMetadata.h"
#include "OCLKernelBase.h"

namespace NeuroflowN
{
    enum class ComputingUnit
    {
        CPU, GPU
    };

    template<const char* NameTemplate>
    class OCLActivationKernelBase : public OCLKernelBase
    {
        std::vector<std::pair<OCLVectorKernelName, OCLVectorKernelName>> cpuNames;
        std::vector<std::pair<OCLVectorKernelName, OCLVectorKernelName>> gpuNames;

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

        static std::pair<std::string, std::string> CreateNames(ComputingUnit unit, unsigned size)
        {
            using namespace std;

            const char* unitStr = unit == ComputingUnit::CPU ? "CPU" : "GPU";
            return move(make_pair(CreateName("Sigmoid", unitStr, size), CreateName("Linear", unitStr, size)));
        }

        inline const std::pair<OCLVectorKernelName, OCLVectorKernelName>& GetCPUNames(unsigned size) const
        {
            return cpuNames[size - 1];
        }

        inline const std::pair<OCLVectorKernelName, OCLVectorKernelName>& GetGPUNames(unsigned size) const
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

                cpuNames.emplace_back(OCLVectorKernelName(cpu.first), OCLVectorKernelName(cpu.second));
                gpuNames.emplace_back(OCLVectorKernelName(gpu.first), OCLVectorKernelName(gpu.second));
            }
        }
    };

}