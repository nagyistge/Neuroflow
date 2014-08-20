using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Neuroflow.Data
{
    public abstract class DataArrayFactory
    {
        public DataArray Create(int size)
        {
            return Create(size, 0.0f);
        }

        public DataArray Create(float[] array)
        {
            return Create(array, 0, array.Length);
        }

        public DataArray CreateConst(float[] array)
        {
            return CreateConst(array, 0, array.Length);
        }

        public DataArray Create(int size, float fill)
        {
            Args.Requires(() => size, () => size > 0);

            return DoCreate(size, fill);
        }

        public DataArray Create(float[] array, int beginPos, int length)
        {
            Args.Requires(() => array, () => array != null && array.Length > 0);
            Args.Requires(() => beginPos, () => beginPos >= 0 && beginPos < array.Length);
            Args.Requires(() => length, () => length <= array.Length - beginPos);

            return DoCreate(array, beginPos, length);
        }

        public DataArray CreateConst(float[] array, int beginPos, int length)
        {
            Args.Requires(() => array, () => array != null && array.Length > 0);
            Args.Requires(() => beginPos, () => beginPos >= 0 && beginPos < array.Length);
            Args.Requires(() => length, () => length <= array.Length - beginPos);

            return DoCreateConst(array, beginPos, length);
        }

        protected abstract DataArray DoCreate(int size, float fill);

        protected abstract DataArray DoCreate(float[] array, int beginPos, int length);

        protected abstract DataArray DoCreateConst(float[] array, int beginPos, int length);
    }
}
