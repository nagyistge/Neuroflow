add_definitions (-DBOOST_TEST_DYN_LINK)
include_directories ("${PROJECT_SOURCE_DIR}/3rd_party" "${PROJECT_SOURCE_DIR}/neuroflow_core_slib")

file(GLOB main_files *.h* *.c*)

add_executable(neuroflow_core_tests ${main_files})

target_link_libraries (neuroflow_core_tests neuroflow_core_slib ${Boost_SYSTEM_LIBRARY} ${Boost_UNIT_TEST_FRAMEWORK_LIBRARY} ${Boost_CHRONO_LIBRARY})

#PCH
set_target_properties(neuroflow_core_tests PROPERTIES COTIRE_CXX_PREFIX_HEADER_INIT "stdafx.h")
cotire(neuroflow_core_tests)
