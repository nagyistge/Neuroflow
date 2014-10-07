public import nfhelpers;
import std.math;

float sigmoid(float value, float alpha) nothrow
{
	//return (2.0f / (1.0f + exp(-alpha * value))) - 1.0f; // Logistics
	//return tanh(value * alpha); // Tanh
	return (value * alpha) / (1.0f + fabs(value * alpha)); // Elliot
}

float sigmoidDeriv(float value, float alpha) nothrow
{
	//return alpha * (1.0f - value * value) / 2.0f; // Logistics
	//return alpha * (1.0f - (value * value)); // Tanh
	return alpha * 1.0f / ((1.0f + fabs(value * alpha)) * (1.0f + fabs(value * alpha))); // Elliot
}

float linear(float value, float alpha) nothrow
{
	return fmin(fmax(value * alpha, -alpha), alpha);
}