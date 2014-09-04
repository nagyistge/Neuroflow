import dataarray;

interface DataArrayFactory
{
	DataArray create(in size_t size, in float fill = 0.0f);

	DataArray create(in float* values, in size_t beginPos, in size_t size);

	DataArray createConst(in float* values, in size_t beginPos, in size_t size);
}
