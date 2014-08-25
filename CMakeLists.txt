cmake_minimum_required (VERSION 2.8.6)
set (CMAKE_MODULE_PATH "${CMAKE_SOURCE_DIR}/CMake")
include(cotire)

project (Neuroflow)

set (Boost_USE_STATIC_LIBS TRUE)
find_package(Boost COMPONENTS system filesystem unit_test_framework thread chrono date_time REQUIRED)
find_package(OpenCL REQUIRED)

set_property(GLOBAL PROPERTY USE_FOLDERS ON)

include_directories(${OPENCL_INCLUDE_DIRS} ${Boost_INCLUDE_DIRS})

# GCC
# set (CMAKE_CXX_FLAGS "${CMAKE_CXX_FLAGS} -std=c++11")

# VS
set (CMAKE_CXX_FLAGS "${CMAKE_CXX_FLAGS} /EHa")

message ("cxx Flags: " ${CMAKE_CXX_FLAGS})

add_subdirectory(3rd_party)
add_subdirectory(neuroflow_core_slib)
add_subdirectory(neuroflow_core_tests)
