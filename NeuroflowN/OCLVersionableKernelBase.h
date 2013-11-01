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
    class OCLKernelVersionBag
    {
        friend class OCLVersionableKernelBase;

        std::vector<std::unique_ptr<T>> values;

        template <typename... Args>
        void EmplaceVersion(::size_t version, Args... args)
        {
            values.resize(version + 1);
            if (values[version] == null) values.emplace(values.cbegin() + version, std::make_unique<T>(args...));
        }

    public:
        OCLKernelVersionBag() { }
        OCLKernelVersionBag(const OCLKernelVersionBag& other) = delete;
        OCLKernelVersionBag(OCLKernelVersionBag&& other) :
            values(move(other.values))
        {
        }
        
        const T& GetVersion(::size_t version) const
        {
            return *(values[version].get());
        }
    };

    class OCLVersionableKernelBase : public OCLKernelBase
    {
        std::string namePrefix;
        std::vector<OCLKernelVersionBag<OCLVectorKernelName>> cpuNames;
        std::vector<OCLKernelVersionBag<OCLVectorKernelName>> gpuNames;

        std::string CreateName(const char* ver, const char* unit, unsigned size)
        {
            using namespace std;

            stringstream name;
            name << namePrefix;
            name << "_";
            name << ver;
            name << "_";
            if (size > 1)
            {
                name << size;
                name << "_";
            }
            name << unit;

            return name.str();
        }

    protected:

        OCLKernelVersionBag<std::string> CreateNames(const std::initializer_list<::size_t>& versions, ComputingUnit unit, unsigned size)
        {
            using namespace std;

            const char* unitStr = unit == ComputingUnit::CPU ? "CPU" : "GPU";
            OCLKernelVersionBag<std::string> result;
            for (auto i : versions)
            {
                stringstream n;
                n << "V";
                n << i;
                result.EmplaceVersion(i, CreateName(n.str().c_str(), unitStr, size));
            }
            return result;
        }

        inline const OCLKernelVersionBag<OCLVectorKernelName>& GetCPUNames(unsigned size = 1) const
        {
            return cpuNames[size - 1];
        }

        inline const OCLKernelVersionBag<OCLVectorKernelName>& GetGPUNames(unsigned size = 1) const
        {
            return gpuNames[size - 1];
        }

    public:
        OCLVersionableKernelBase(const OCLIntCtxSPtrT& ctx, const std::string& namePrefix, const std::initializer_list<::size_t>& versions, unsigned maxConnectionCount = 1) :
            OCLKernelBase(ctx),
            namePrefix(namePrefix)
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