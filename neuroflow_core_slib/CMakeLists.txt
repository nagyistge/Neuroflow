cmake_minimum_required(VERSION 2.8)

include_directories ("${PROJECT_SOURCE_DIR}/3rd_party")

file(GLOB main_files *.h *.cpp)
file(GLOB cpp_files ./cpp/*.h ./cpp/*.cpp)
file(GLOB ocl_files ./ocl/*.h ./ocl/*.cpp)

add_library(neuroflow_core_slib STATIC ${main_files} ${cpp_files} ${ocl_files})
target_link_libraries(neuroflow_core_slib ${OPENCL_LIBRARIES} ${Boost_FILESYSTEM_LIBRARY} ${Boost_THREAD_LIBRARY})

#PCH
set_target_properties(neuroflow_core_slib PROPERTIES COTIRE_CXX_PREFIX_HEADER_INIT "stdafx.h")
cotire(neuroflow_core_slib)
