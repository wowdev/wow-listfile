CXXFLAGS += -I $(PWD) --std=c++11 -O3

all: check_files hash_lines

check_files: check_files.o lookup3.o
	$(CXX) $^ -o $@

hash_lines: hash_lines.o lookup3.o
	$(CXX) $^ -o $@
