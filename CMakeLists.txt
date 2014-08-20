cmake_minimum_required (VERSION 2.8)

project (Neuroflow)

set_property(GLOBAL PROPERTY USE_FOLDERS ON)

set (CMAKE_CXX_FLAGS "${CMAKE_CXX_FLAGS} -std=c++11")

message ("cxx Flags: " ${CMAKE_CXX_FLAGS})

add_subdirectory(3rd_party)
add_subdirectory(neuroflow_core_slib)
