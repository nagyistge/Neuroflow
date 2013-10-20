#pragma once

#include <sstream>
#include <exception>

#define throw_logic_error(msg) \
{ \
    std::stringstream str; \
	str << msg; \
	str << "\n at "; \
	str << __FILE__; \
	str << ": "; \
	str << __LINE__; \
	throw std::logic_error(str.str().c_str()); \
}

inline std::logic_error as_logic_error_helper(const std::exception& ex, const char* file, const int line)
{
    std::stringstream str; 
	str << ex.what();
    str << " (type: ";
    str << typeid(ex).name();
    str << ")";
	str << "\n at ";
	str << file;
	str << ": ";
	str << line;
	return std::logic_error(str.str().c_str()); 
}

#define throw_invalid_argument(msg) \
{ \
    std::stringstream str; \
	str << msg; \
	str << "\n at "; \
	str << __FILE__; \
	str << ": "; \
	str << __LINE__; \
	throw std::invalid_argument(str.str().c_str()); \
}

#define as_logic_error(ex) as_logic_error_helper(ex, __FILE__, __LINE__)

inline std::runtime_error as_runtime_error_helper(const std::exception& ex, const char* file, const int line)
{
    std::stringstream str; 
	str << ex.what();
    str << " (type: ";
    str << typeid(ex).name();
    str << ")";
	str << "\n at ";
	str << file;
	str << ": ";
	str << line;
	return std::runtime_error(str.str().c_str()); 
}

#define as_runtime_error(ex) as_runtime_error_helper(ex, __FILE__, __LINE__)

#define verify_arg(expr, argName) \
{ \
    if (!(expr)) \
    { \
        std::stringstream str; \
	    str << "Argument value of '"; \
		str << argName; \
		str << "' is invalid.\n at "; \
	    str << __FILE__; \
	    str << ": "; \
	    str << __LINE__; \
        throw std::invalid_argument(str.str().c_str()); \
    } \
}