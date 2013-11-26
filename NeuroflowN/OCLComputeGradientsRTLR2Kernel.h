#include "OCLTypedefs.h"
#include "OCLKernelBase.h"
#include <string>

namespace NeuroflowN
{
    class OCLComputeGradientsRTLR2Kernel : public OCLKernelBase
    {
        OCLProgramSPtrT program;

        static OCLVectorKernelName name;

        void Build(const OCLVaultSPtrT& vault);
        std::string CreateCode();
        unsigned CalculateVectorSize(const RTLRLayerInfoVecVecT* infos) const;
        unsigned CalculateLocalSize(const DeviceArrayVecT* netValueDerivates) const;
        std::string DeclarePickMethod(const std::string& type, const std::string& name) const;
    public:
        OCLComputeGradientsRTLR2Kernel(const OCLIntCtxSPtrT& ctx, const OCLVaultSPtrT& vault);

        void Exec(NfObject* state, RTLRLayerInfoVecVecT* inputLayerInfos, DeviceArrayVecT* netValueDerivates, RTLRComputationData2* data, IDeviceArray2* pValuesOfWeights, IDeviceArray* outputs, IDeviceArray* desiredOutputs, SequenceMarker seqMark);
    };
}