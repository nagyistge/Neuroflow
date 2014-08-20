cmake_minimum_required(VERSION 2.8)

file(GLOB main_files *.h* *.c*)

add_library(3rd_party STATIC ${main_files})
set_target_properties(3rd_party PROPERTIES LINKER_LANGUAGE CXX)
