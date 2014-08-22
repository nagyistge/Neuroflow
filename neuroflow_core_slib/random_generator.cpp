#include "stdafx.h"
#include "random_generator.h"
#include "nfdev.h"

USING

random_generator::random_generator(const boost::optional<unsigned long>& seed) :
_generator(!seed.is_initialized() ? (((std::random_device())() << 16) | (std::random_device())()) : *seed)
{
}

float random_generator::next(float min, float max)
{
    return uniform_real_distribution<float>(min, max)(_generator);
}
