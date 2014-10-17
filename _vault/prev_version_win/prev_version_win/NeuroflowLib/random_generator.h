#pragma once
#include "nfdev.h"

namespace nf
{
    struct random_generator
    {
        random_generator(const boost::optional<unsigned long>& seed);

        float next(float min, float max);

    private:
        std::mt19937 _generator;
    };
}