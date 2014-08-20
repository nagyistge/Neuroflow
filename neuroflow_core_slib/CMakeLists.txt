cmake_minimum_required(VERSION 2.8)

include_directories ("${PROJECT_SOURCE_DIR}/3rd_party")

file(GLOB main_files *.h *.cpp)
file(GLOB cpp_files ./cpp/*.h ./cpp/*.cpp)
file(GLOB ocl_files ./ocl/*.h ./ocl/*.cpp)

add_library(neuroflow_core_slib STATIC ${main_files} ${cpp_files} ${ocl_files})
