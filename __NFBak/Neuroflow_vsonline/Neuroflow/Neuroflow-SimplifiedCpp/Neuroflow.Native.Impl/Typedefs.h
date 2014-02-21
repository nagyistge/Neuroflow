#pragma once

#include <vector>
#include <memory>

class IntRange;

class LayerForwardCompute;

class CPPAmpValueBuffer;

typedef std::unique_ptr<LayerForwardCompute> UPLayerForwardCompute;

typedef std::vector<UPLayerForwardCompute> LayerForwardComputes;

typedef std::vector<LayerForwardComputes> LayerForwardComputeGroups;

typedef std::tuple<int, int, int> VectorHandle;