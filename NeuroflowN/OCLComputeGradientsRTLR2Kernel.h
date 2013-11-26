#include "OCLTypedefs.h"
#include "OCLKernelBase.h"

namespace NeuroflowN
{
    class OCLComputeGradientsRTLR2Kernel : public OCLKernelBase
    {
        OCLProgramSPtrT program;

        void Build(const OCLVaultSPtrT& vault);
        std::string CreateCode();
    public:
        OCLComputeGradientsRTLR2Kernel(const OCLIntCtxSPtrT& ctx, const OCLVaultSPtrT& vault);
    };
}