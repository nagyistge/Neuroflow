# CMAKE generated file: DO NOT EDIT!
# Generated by "NMake Makefiles" Generator, CMake Version 3.0

#=============================================================================
# Special targets provided by cmake.

# Disable implicit rules so canonical targets will work.
.SUFFIXES:

.SUFFIXES: .hpux_make_needs_suffix_list

# Suppress display of executed commands.
$(VERBOSE).SILENT:

# A target that is always out of date.
cmake_force:
.PHONY : cmake_force

#=============================================================================
# Set environment variables for the build.

!IF "$(OS)" == "Windows_NT"
NULL=
!ELSE
NULL=nul
!ENDIF
SHELL = cmd.exe

# The CMake executable.
CMAKE_COMMAND = "C:\Program Files (x86)\CMake\bin\cmake.exe"

# The command to remove a file.
RM = "C:\Program Files (x86)\CMake\bin\cmake.exe" -E remove -f

# Escaping for special characters.
EQUALS = =

# The top-level source directory on which CMake was run.
CMAKE_SOURCE_DIR = C:\GIT\Neuroflow

# The top-level build directory on which CMake was run.
CMAKE_BINARY_DIR = C:\GIT\Neuroflow\build

# Include any dependencies generated for this target.
include neuroflow_core_tests\CMakeFiles\neuroflow_core_tests_unity.dir\depend.make

# Include the progress variables for this target.
include neuroflow_core_tests\CMakeFiles\neuroflow_core_tests_unity.dir\progress.make

# Include the compile flags for this target's objects.
include neuroflow_core_tests\CMakeFiles\neuroflow_core_tests_unity.dir\flags.make

neuroflow_core_tests\cotire\neuroflow_core_tests_CXX_unity.cxx: neuroflow_core_tests\neuroflow_core_tests_CXX_cotire.cmake
	$(CMAKE_COMMAND) -E cmake_progress_report C:\GIT\Neuroflow\build\CMakeFiles $(CMAKE_PROGRESS_1)
	@$(CMAKE_COMMAND) -E cmake_echo_color --switch=$(COLOR) --blue --bold "Generating CXX unity source neuroflow_core_tests/cotire/neuroflow_core_tests_CXX_unity.cxx"
	cd C:\GIT\Neuroflow\neuroflow_core_tests
	echo >nul && "C:\Program Files (x86)\CMake\bin\cmake.exe" -DCOTIRE_BUILD_TYPE:STRING=Debug -DCOTIRE_VERBOSE:BOOL=$(VERBOSE) -P C:/GIT/Neuroflow/CMake/cotire.cmake unity C:/GIT/Neuroflow/build/neuroflow_core_tests/neuroflow_core_tests_CXX_cotire.cmake C:/GIT/Neuroflow/build/neuroflow_core_tests/cotire/neuroflow_core_tests_CXX_unity.cxx
	cd C:\GIT\Neuroflow\build

neuroflow_core_tests\CMakeFiles\neuroflow_core_tests_unity.dir\cotire\neuroflow_core_tests_CXX_unity.cxx.obj: neuroflow_core_tests\CMakeFiles\neuroflow_core_tests_unity.dir\flags.make
neuroflow_core_tests\CMakeFiles\neuroflow_core_tests_unity.dir\cotire\neuroflow_core_tests_CXX_unity.cxx.obj: neuroflow_core_tests\cotire\neuroflow_core_tests_CXX_unity.cxx
	$(CMAKE_COMMAND) -E cmake_progress_report C:\GIT\Neuroflow\build\CMakeFiles $(CMAKE_PROGRESS_2)
	@$(CMAKE_COMMAND) -E cmake_echo_color --switch=$(COLOR) --green "Building CXX object neuroflow_core_tests/CMakeFiles/neuroflow_core_tests_unity.dir/cotire/neuroflow_core_tests_CXX_unity.cxx.obj"
	cd C:\GIT\Neuroflow\build\neuroflow_core_tests
	C:\PROGRA~2\MICROS~2.0\VC\bin\amd64\cl.exe  @<<
 /nologo /TP $(CXX_FLAGS) /bigobj $(CXX_DEFINES) /FoCMakeFiles\neuroflow_core_tests_unity.dir\cotire\neuroflow_core_tests_CXX_unity.cxx.obj /FdCMakeFiles\neuroflow_core_tests_unity.dir/ /FS -c C:\GIT\Neuroflow\build\neuroflow_core_tests\cotire\neuroflow_core_tests_CXX_unity.cxx
<<
	cd C:\GIT\Neuroflow\build

neuroflow_core_tests\CMakeFiles\neuroflow_core_tests_unity.dir\cotire\neuroflow_core_tests_CXX_unity.cxx.i: cmake_force
	@$(CMAKE_COMMAND) -E cmake_echo_color --switch=$(COLOR) --green "Preprocessing CXX source to CMakeFiles/neuroflow_core_tests_unity.dir/cotire/neuroflow_core_tests_CXX_unity.cxx.i"
	cd C:\GIT\Neuroflow\build\neuroflow_core_tests
	C:\PROGRA~2\MICROS~2.0\VC\bin\amd64\cl.exe  > CMakeFiles\neuroflow_core_tests_unity.dir\cotire\neuroflow_core_tests_CXX_unity.cxx.i @<<
 /nologo /TP $(CXX_FLAGS) /bigobj $(CXX_DEFINES) -E C:\GIT\Neuroflow\build\neuroflow_core_tests\cotire\neuroflow_core_tests_CXX_unity.cxx
<<
	cd C:\GIT\Neuroflow\build

neuroflow_core_tests\CMakeFiles\neuroflow_core_tests_unity.dir\cotire\neuroflow_core_tests_CXX_unity.cxx.s: cmake_force
	@$(CMAKE_COMMAND) -E cmake_echo_color --switch=$(COLOR) --green "Compiling CXX source to assembly CMakeFiles/neuroflow_core_tests_unity.dir/cotire/neuroflow_core_tests_CXX_unity.cxx.s"
	cd C:\GIT\Neuroflow\build\neuroflow_core_tests
	C:\PROGRA~2\MICROS~2.0\VC\bin\amd64\cl.exe  @<<
 /nologo /TP $(CXX_FLAGS) /bigobj $(CXX_DEFINES) /FoNUL /FAs /FaCMakeFiles\neuroflow_core_tests_unity.dir\cotire\neuroflow_core_tests_CXX_unity.cxx.s /c C:\GIT\Neuroflow\build\neuroflow_core_tests\cotire\neuroflow_core_tests_CXX_unity.cxx
<<
	cd C:\GIT\Neuroflow\build

neuroflow_core_tests\CMakeFiles\neuroflow_core_tests_unity.dir\cotire\neuroflow_core_tests_CXX_unity.cxx.obj.requires:
.PHONY : neuroflow_core_tests\CMakeFiles\neuroflow_core_tests_unity.dir\cotire\neuroflow_core_tests_CXX_unity.cxx.obj.requires

neuroflow_core_tests\CMakeFiles\neuroflow_core_tests_unity.dir\cotire\neuroflow_core_tests_CXX_unity.cxx.obj.provides: neuroflow_core_tests\CMakeFiles\neuroflow_core_tests_unity.dir\cotire\neuroflow_core_tests_CXX_unity.cxx.obj.requires
	$(MAKE) -f neuroflow_core_tests\CMakeFiles\neuroflow_core_tests_unity.dir\build.make /nologo -$(MAKEFLAGS) neuroflow_core_tests\CMakeFiles\neuroflow_core_tests_unity.dir\cotire\neuroflow_core_tests_CXX_unity.cxx.obj.provides.build
.PHONY : neuroflow_core_tests\CMakeFiles\neuroflow_core_tests_unity.dir\cotire\neuroflow_core_tests_CXX_unity.cxx.obj.provides

neuroflow_core_tests\CMakeFiles\neuroflow_core_tests_unity.dir\cotire\neuroflow_core_tests_CXX_unity.cxx.obj.provides.build: neuroflow_core_tests\CMakeFiles\neuroflow_core_tests_unity.dir\cotire\neuroflow_core_tests_CXX_unity.cxx.obj

# Object files for target neuroflow_core_tests_unity
neuroflow_core_tests_unity_OBJECTS = \
"CMakeFiles\neuroflow_core_tests_unity.dir\cotire\neuroflow_core_tests_CXX_unity.cxx.obj"

# External object files for target neuroflow_core_tests_unity
neuroflow_core_tests_unity_EXTERNAL_OBJECTS =

neuroflow_core_tests\neuroflow_core_tests.exe: neuroflow_core_tests\CMakeFiles\neuroflow_core_tests_unity.dir\cotire\neuroflow_core_tests_CXX_unity.cxx.obj
neuroflow_core_tests\neuroflow_core_tests.exe: neuroflow_core_tests\CMakeFiles\neuroflow_core_tests_unity.dir\build.make
neuroflow_core_tests\neuroflow_core_tests.exe: neuroflow_core_tests\CMakeFiles\neuroflow_core_tests_unity.dir\objects1.rsp
	@$(CMAKE_COMMAND) -E cmake_echo_color --switch=$(COLOR) --red --bold "Linking CXX executable neuroflow_core_tests.exe"
	cd C:\GIT\Neuroflow\build\neuroflow_core_tests
	"C:\Program Files (x86)\CMake\bin\cmake.exe" -E vs_link_exe C:\PROGRA~2\MICROS~2.0\VC\bin\amd64\link.exe /nologo @CMakeFiles\neuroflow_core_tests_unity.dir\objects1.rsp @<<
 /out:neuroflow_core_tests.exe /implib:neuroflow_core_tests.lib /pdb:C:\GIT\Neuroflow\build\neuroflow_core_tests\neuroflow_core_tests.pdb /version:0.0   /machine:x64  /debug /INCREMENTAL /subsystem:console  kernel32.lib user32.lib gdi32.lib winspool.lib shell32.lib ole32.lib oleaut32.lib uuid.lib comdlg32.lib advapi32.lib 
<<
	cd C:\GIT\Neuroflow\build

# Rule to build all files generated by this target.
neuroflow_core_tests\CMakeFiles\neuroflow_core_tests_unity.dir\build: neuroflow_core_tests\neuroflow_core_tests.exe
.PHONY : neuroflow_core_tests\CMakeFiles\neuroflow_core_tests_unity.dir\build

neuroflow_core_tests\CMakeFiles\neuroflow_core_tests_unity.dir\requires: neuroflow_core_tests\CMakeFiles\neuroflow_core_tests_unity.dir\cotire\neuroflow_core_tests_CXX_unity.cxx.obj.requires
.PHONY : neuroflow_core_tests\CMakeFiles\neuroflow_core_tests_unity.dir\requires

neuroflow_core_tests\CMakeFiles\neuroflow_core_tests_unity.dir\clean:
	cd C:\GIT\Neuroflow\build\neuroflow_core_tests
	$(CMAKE_COMMAND) -P CMakeFiles\neuroflow_core_tests_unity.dir\cmake_clean.cmake
	cd C:\GIT\Neuroflow\build
.PHONY : neuroflow_core_tests\CMakeFiles\neuroflow_core_tests_unity.dir\clean

neuroflow_core_tests\CMakeFiles\neuroflow_core_tests_unity.dir\depend: neuroflow_core_tests\cotire\neuroflow_core_tests_CXX_unity.cxx
	$(CMAKE_COMMAND) -E cmake_depends "NMake Makefiles" C:\GIT\Neuroflow C:\GIT\Neuroflow\neuroflow_core_tests C:\GIT\Neuroflow\build C:\GIT\Neuroflow\build\neuroflow_core_tests C:\GIT\Neuroflow\build\neuroflow_core_tests\CMakeFiles\neuroflow_core_tests_unity.dir\DependInfo.cmake --color=$(COLOR)
.PHONY : neuroflow_core_tests\CMakeFiles\neuroflow_core_tests_unity.dir\depend
