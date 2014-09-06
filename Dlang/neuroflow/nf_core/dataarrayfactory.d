import dataarray;

interface DataArrayFactory
{
	DataArray create(size_t size, float fill = 0.0f);

	DataArray create(in float* values, size_t beginPos, size_t size);

	DataArray createConst(in float* values, size_t beginPos, size_t size);
}
