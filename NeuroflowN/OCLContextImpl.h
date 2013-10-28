#pragma once
#include "nfobject.h"
#include <vector>
#include "OCLTypedefs.h"
#include "DeviceInfo.h"
#include "OCLVault.h"

namespace NeuroflowN
{
    class OCLIntCtx;

    class OCLContextImpl : public NfObject
    {
        std::shared_ptr<OCLDataArrayFactory> dataArrayFactory;
        std::shared_ptr<OCLVectorUtils> vectorUtils;
        std::shared_ptr<OCLMultilayerPerceptronAdapter> multilayerPerceptronAdapter;
        std::shared_ptr<OCLDeviceArrayManagement> deviceArrayManagement;
        std::string version;
        DeviceInfo deviceInfo;
        OCLIntCtxSPtrT ctx;
        OCLVaultSPtrT vault;

    public:
        OCLContextImpl(const std::string& deviceID, const std::string& version);

        void Free();

        static DeviceInfoVecT GetAvailableDevices();

        const OCLIntCtxSPtrT& GetIntCtx() const 
        {
            return ctx;
        }

        const DeviceInfo& GetDevice() const;

        DataArrayFactory* GetDataArrayFactoryPtr() const;

        IVectorUtils* GetVectorUtilsPtr() const;

        IMultilayerPerceptronAdapter* GetMultilayerPerceptronAdapterPtr() const;

        IDeviceArrayManagement* GetDeviceArrayManagementPtr() const;

    private:
        void Initialize(const DeviceInfo& deviceInfo, const cl::Device& device);
    };
}
