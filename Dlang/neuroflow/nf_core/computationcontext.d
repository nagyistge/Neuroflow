import devicearraymanagement;
import dataarrayfactory;
import ccinitpars;
import utils;
import deviceinfo;
import neuralnetworkfactory;

wstring NativeContext = "NativeContext";

interface ComputationContext
{
    @property immutable(DeviceInfo) deviceInfo();

    @property DeviceArrayManagement deviceArrayManagement();

    @property DataArrayFactory dataArrayFactory();

    @property Utils utils();

	final @property NeuralNetworkFactory neuralNetworkFactory()
	{
		return NeuralNetworkFactory(this);
	}
}