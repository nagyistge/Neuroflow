import layer;
import std.typecons;
import std.range;
import std.typecons;

class LayerOrderComparer
{
    bool less(Layer layer1, Layer layer2)
	{
		return compare(layer1, layer2) < 0;
	}

    private int[Tuple!(Layer, Layer)] _results;

	private int compare(Layer layer1, Layer layer2)
	{
		auto key = tuple(layer1, layer2);

		int result = _results.get(key, -1);
		if (result != -1) return result;

		auto rKey = tuple(layer2, layer1);

		result = _results.get(rKey, -1);
		if (result != -1) return -result;

		int ret;
		if (isBelow(layer1, layer2)) ret = -1;
		else if (isBelow(layer2, layer1)) ret = 1;
		else ret = 0;

		_results[key] = ret;
		return ret;
	}

	private bool isBelow(Layer layer1, Layer layer2, int r = 0)
	{
		if (r > 100) return false;

		foreach (output; 
				 chain(layer1.outputConnections.connectedLayers(FlowDirection.oneWay),
					   layer1.outputConnections.connectedLayers(FlowDirection.twoWay)))
		{
			if (output == layer2) return true;
		}

		foreach (output; 
				 chain(layer1.outputConnections.connectedLayers(FlowDirection.oneWay),
					   layer1.outputConnections.connectedLayers(FlowDirection.twoWay)))
		{
			if (isBelow(output, layer2, r + 1)) return true;
		}

		return false;
	}
}

