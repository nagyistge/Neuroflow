#include "stdafx.h"
#include "OCLBuffer1.h"
#include "OCLIntCtx.h"
#include "OCLDeviceArrayPool.h"

using namespace std;
using namespace cl;
using namespace NeuroflowN;

cl::Buffer& OCLBuffer1::GetCLBuffer()
{
    if (buffer() == null)
    {
        assert(pool != null);
        buffer = pool->CreateSubBuffer(beginOffset, size);
    }
    return buffer;
}

void OCLBuffer1::Dump(const OCLIntCtxSPtrT& ctx, std::string title, bool toDebug) const
{
    struct DumpData
    {
        vector<float> values;
        bool toDebug;
        int id;
        string title;
    };

    auto dd = new DumpData();
    Event e;

    dd->values.resize(size);
    dd->toDebug = toDebug;
    dd->id = reinterpret_cast<int>(buffer());
    dd->title = title;

    ctx->GetQueue().enqueueReadBuffer(
        buffer,
        false,
        0,
        size * sizeof(float),
        &(dd->values)[0],
        nullptr,
        &e);

    e.setCallback(
        CL_COMPLETE,
        [](cl_event event, cl_int status, void* userData)
        {
            auto dumpData = (DumpData*)userData;
            bool first = true;
            wstringstream s;

            s << dumpData->title.c_str();
            s << " [";
            s << dumpData->id;
            s << "]: ";

            for (float v : dumpData->values)
            {
                if (first)
                {
                    first = false;
                }
                else
                {
                    s << " ";
                }
                s << v;
            }
            s << endl;

            if (dumpData->toDebug)
            {
                OutputDebugString(s.str().c_str());
            }
            else
            {
                wcout << s.str();
            }

            delete dumpData;
        },
        dd);   
}