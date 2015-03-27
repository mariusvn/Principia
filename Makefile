CPP_SOURCES=ksp_plugin/plugin.cpp ksp_plugin/interface.cpp ksp_plugin/physics_bubble.cpp 
CC_SOURCES=$(wildcard serialization/*.cc)
#CPP_SOURCES=$(wildcard testing_utilities/*.cpp) 
#$(wildcard quantities/*.cpp) $(wildcard base/*.cpp) $(wildcard integrators/*.cpp) $(wildcard ksp_plugin/*.cpp) $(wildcard geometry/*.cpp) $(wildcard physics/*.cpp) $(wildcard benchmarks/*.cpp)
PROTO_SOURCES=$(wildcard */*.proto)

OBJECTS=$(CPP_SOURCES:.cpp=.o) $(CC_SOURCES:.cc=.o)
VERSION_HEADER=base/version.hpp
PROTO_HEADERS=$(PROTO_SOURCES:.proto=.pb.h)

LIB_DIR=lib
LIB=$(LIB_DIR)/principia.so


INCLUDE=-I. -I../glog/src -I../protobuf-2.6.1/src -I../benchmark/include -I../gmock-1.7.0/gtest/include -I../gmock-1.7.0/include

CPPC=clang++
SHARED_ARGS=-std=c++1y -stdlib=libc++ -O3 -g -ggdb -m64 -fPIC -fexceptions -ferror-limit=0 # -Wall -Wpedantic 
COMPILE_ARGS=-c $(SHARED_ARGS) $(INCLUDE)
LINK_ARGS=$(SHARED_ARGS) 
LD_LIBRARY_PATH=/opt/principa/glog/.libs:/opt/principa/benchmark/src/:/opt/principa/protobuf/src/.libs:/opt/principia/gmock-1.7.0/lib/.libs/
LIBS=-l:libc++.a -l:libprotobuf.a -l:libglog.a -lpthread

all: $(LIB) run_tests

$(LIB): $(VERSION_HEADER) $(PROTO_HEADERS) $(OBJECTS) Makefile $(LIB_DIR)
	$(CPPC) -shared $(LINK_ARGS) $(OBJECTS) $(INCLUDE) -o $(LIB) $(LIBS) 

$(LIB_DIR):
	mkdir -p $(LIB_DIR)

$(VERSION_HEADER): .git
	./generate_version_header.sh

%.pb.h: %.proto
	../protobuf/src/protoc $< --cpp_out=.

%.o: %.cpp Makefile
	$(CPPC) $(COMPILE_ARGS) $< -o $@ 

%.o: %.cc Makefile
	$(CPPC) $(COMPILE_ARGS) $< -o $@ 

clean:
	rm $(LIB) $(PROTO_HEADERS) $(OBJECTS); true

##### TESTS #####

TEST_BIN=hex_test
TEST_SOURCE=base/hexadecimal_test.cpp $(wildcard ../gmock-1.7.0/fused-src/*.cc)
TEST_OBJS=$(TEST_SOURCE:.cpp=.o)

$(TEST_BIN): $(TEST_OBJS) Makefile
	@echo $(LD_LIBRARY_PATH)
	$(CPPC) $(LINK_ARGS) $(TEST_OBJS) $(INCLUDE) -o $(TEST_BIN) $(LIBS)

run_tests: $(TEST_BIN)
	./$(TEST_BIN)
