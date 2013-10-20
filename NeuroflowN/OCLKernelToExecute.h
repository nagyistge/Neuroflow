#pragma once

#include "OCL.h"
#include "OCLTypedefs.h"
#include "OCLIntCtx.h"
#include "OCLBuffer1.h"
#include "OCLBuffer2.h"
#include <array>
#include "NfObject.h"
#include "OCLError.h"

namespace NeuroflowN
{
    class OCLKernelToExecute : public NfObject
    {
        struct Data
        {
            cl::Kernel kernel;
            std::string kernelName;
        };

        std::vector<Data> dataValues;

        unsigned VectorSizeToIndex(unsigned vectorSize)
        {
            switch (vectorSize)
            {
                case 1:
                    return 0;
                case 2:
                    return 1;
                case 4:
                    return 2;
                case 8:
                    return 3;
                case 16:
                    return 4;
                default:
                    throw_invalid_argument("Ivalid vectorSize argument!");
            }
        }

        Data& GetData(unsigned vectorSize)
        {
            unsigned index = VectorSizeToIndex(vectorSize);
            if (dataValues.size() < index + 1) dataValues.resize(index + 1);
            return dataValues[index];
        }

    public:
        OCLKernelToExecute()
        {
        }

        const std::string& GetKernelName(unsigned vectorSize)
        {
            return GetData(vectorSize).kernelName;
        }

        void Execute(const OCLIntCtxSPtrT& ctx, const std::string& kernelName, unsigned vectorSize, const std::function<void(cl::Kernel&)>& setupKernel, unsigned workItemSize = 1)
        {
            EnsureKernel(ctx, kernelName, vectorSize, setupKernel);
            DoExecute(ctx, vectorSize, cl::NullRange, workItemSize > 1 ? cl::NDRange(workItemSize) : cl::NullRange, cl::NullRange);
        }

        void Execute(const OCLIntCtxSPtrT& ctx, const std::string& kernelName, unsigned vectorSize, const std::function<void(cl::Kernel&)>& setupKernel, const OCLBuffer1& extentBuffer)
        {
            EnsureKernel(ctx, kernelName, vectorSize, setupKernel);
            DoExecute(ctx, vectorSize, cl::NullRange, cl::NDRange(extentBuffer.GetSize()), cl::NullRange);
        }

        void Execute(const OCLIntCtxSPtrT& ctx, const std::string& kernelName, unsigned vectorSize, const std::function<void(cl::Kernel&)>& setupKernel, const cl::NDRange& workItemSizes, const cl::NDRange& localSizes)
        {
            EnsureKernel(ctx, kernelName, vectorSize, setupKernel);
            DoExecute(ctx, vectorSize, cl::NullRange, workItemSizes, localSizes);
        }

        void Execute(const OCLIntCtxSPtrT& ctx, const std::string& kernelName, unsigned vectorSize, const std::function<void(cl::Kernel&)>& setupKernel, const cl::NDRange& workItemOffsets, const cl::NDRange& workItemSizes, const cl::NDRange& localSizes)
        {
            EnsureKernel(ctx, kernelName, vectorSize, setupKernel);
            DoExecute(ctx, vectorSize, workItemOffsets, workItemSizes, localSizes);
        }

    private:
        inline void EnsureKernel(const OCLIntCtxSPtrT& ctx, const std::string& kernelName, unsigned vectorSize, const std::function<void(cl::Kernel&)>& setupKernel)
        {
            auto& data = GetData(vectorSize);
            if (data.kernelName.size() == 0) data.kernelName = kernelName;
            if (data.kernel() == nullptr) data.kernel = cl::Kernel(ctx->GetProgram(), kernelName.c_str());
            setupKernel(data.kernel);
        }

        inline void DoExecute(const OCLIntCtxSPtrT& ctx, unsigned vectorSize, const cl::NDRange& workItemOffsets, const cl::NDRange& workItemSizes, const cl::NDRange& localSizes)
        {
            auto& data = GetData(vectorSize);
            if (workItemSizes.dimensions() == 0)
            {
                ctx->GetQueue().enqueueTask(data.kernel);
            }
            else
            {
                ctx->GetQueue().enqueueNDRangeKernel(
                    data.kernel,
                    workItemOffsets,
                    workItemSizes,
                    localSizes);
            }
        }
    };
}