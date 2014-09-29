import devicearraymanagement;
import dataarrayfactory;
import ccinitpars;
import utils;
import deviceinfo;
import neuralnetworkfactory;
import computeactivation;
import learningimplfactory;

wstring NativeContext = "NativeContext";

interface ComputationContext
{
    @property immutable(DeviceInfo) deviceInfo();

    @property DeviceArrayManagement deviceArrayManagement();

    @property DataArrayFactory dataArrayFactory();

    @property Utils utils();

	@property ComputeActivation computeActivation();

	@property LearningImplFactory learningImplFactory();

	final @property NeuralNetworkFactory neuralNetworkFactory()
	{
		return NeuralNetworkFactory(this);
	}
}