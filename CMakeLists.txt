cmake_minimum_required (VERSION 2.8.6)
set (CMAKE_MODULE_PATH "${CMAKE_SOURCE_DIR}/CMake")
include(cotire)

project (Neuroflow)

find_package(Boost COMPONENTS system filesystem unit_test_framework REQUIRED)
find_package(OpenCL REQUIRED )

set_property(GLOBAL PROPERTY USE_FOLDERS ON)

include_directories( ${OPENCL_INCLUDE_DIRS} )

set (CMAKE_CXX_FLAGS "${CMAKE_CXX_FLAGS} -std=c++11")

message ("cxx Flags: " ${CMAKE_CXX_FLAGS})

add_subdirectory(3rd_party)
add_subdirectory(neuroflow_core_slib)
add_subdirectory(neuroflow_core_tests)
