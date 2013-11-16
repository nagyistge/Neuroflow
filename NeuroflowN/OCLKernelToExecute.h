#pragma once

#include "OCL.h"
#include "OCLTypedefs.h"
#include "OCLIntCtx.h"
#include "OCLBuffer1.h"
#include "OCLBuffer2.h"
#include <array>
#include "NfObject.h"
#include "OCLError.h"
#include "OCLProgram.h"

namespace NeuroflowN
{
    class OCLKernelToExecute
    {
        struct Data
        {
            cl::Kernel kernel;
            std::string kernelName;
        };

        std::vector<Data> dataValues;

        bool isOutOfOrder = false;

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
            dataValues.resize(index + 1);
            return dataValues[index];
        }

    public:

        OCLKernelToExecute() { }

        OCLKernelToExecute(bool isOutOfOrder) : isOutOfOrder(isOutOfOrder) { }

        const std::string& GetKernelName(unsigned vectorSize)
        {
            return GetData(vectorSize).kernelName;
        }

        void Execute(const OCLProgramSPtrT& program, const std::string& kernelName, unsigned vectorSize, const std::function<void(cl::Kernel&)>& setupKernel, unsigned workItemSize = 1)
        {
            EnsureKernel(program, kernelName, vectorSize, setupKernel);
            DoExecute(program, vectorSize, cl::NullRange, workItemSize > 1 ? cl::NDRange(workItemSize) : cl::NullRange, cl::NullRange);
        }

        void Execute(const OCLProgramSPtrT& program, const std::string& kernelName, unsigned vectorSize, const std::function<void(cl::Kernel&)>& setupKernel, OCLBuffer1* extentBuffer)
        {
            EnsureKernel(program, kernelName, vectorSize, setupKernel);
            DoExecute(program, vectorSize, cl::NullRange, cl::NDRange(extentBuffer->GetSize()), cl::NullRange);
        }

        void Execute(const OCLProgramSPtrT& program, const std::string& kernelName, unsigned vectorSize, const std::function<void(cl::Kernel&)>& setupKernel, const cl::NDRange& workItemSizes, const cl::NDRange& localSizes)
        {
            EnsureKernel(program, kernelName, vectorSize, setupKernel);
            DoExecute(program, vectorSize, cl::NullRange, workItemSizes, localSizes);
        }

        void Execute(const OCLProgramSPtrT& program, const std::string& kernelName, unsigned vectorSize, const std::function<void(cl::Kernel&)>& setupKernel, const cl::NDRange& workItemOffsets, const cl::NDRange& workItemSizes, const cl::NDRange& localSizes)
        {
            EnsureKernel(program, kernelName, vectorSize, setupKernel);
            DoExecute(program, vectorSize, workItemOffsets, workItemSizes, localSizes);
        }

    private:
        void EnsureKernel(const OCLProgramSPtrT& program, const std::string& kernelName, unsigned vectorSize, const std::function<void(cl::Kernel&)>& setupKernel);
        void DoExecute(const OCLProgramSPtrT& program, unsigned vectorSize, const cl::NDRange& workItemOffsets, const cl::NDRange& workItemSizes, const cl::NDRange& localSizes);
    };
}