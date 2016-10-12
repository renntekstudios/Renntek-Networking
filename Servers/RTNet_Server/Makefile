CC = g++

DIALECT = -std=gnu++11 -m32 -D$(TARGET)
OPTIMIZATION = -O3
TARGET = NONE
VERSION = RELEASE

INCLUDES = -I headers

CFLAGS = $(DIALECT) $(INCLUDES) $(OPTIMIZATION) -g -c -fmax-errors=3
CFLAGS_EXEC = $(DIALECT) $(INCLUDES) $(OPTIMIZATION) -g -fmax-errors=3

OBJ = bin/obj

SOURCE = src/main.cpp src/logger.cpp src/generic_reader.cpp src/settings.cpp src/rtserver.cpp
DEPS = ${SOURCE:%.cpp=$(OBJ)/%.o}

all: directories $(DEPS)
ifeq '$(OS)' 'Windows_NT'
	$(CC) -o bin/server/RTNet_Server.exe $(DEPS) $(CFLAGS_EXEC) -l ws2_32 -static-libgcc -static-libstdc++
else
	UNAME_S := $(shell uname -s)
ifeq '$(UNAME_S)' 'Linux'
		$(CC) -o bin/server/RTNet_Server $(DEPS) $(CFLAGS_EXEC) -pthread
		chmod 777 bin/server/RTNet_Server
endif
ifeq '$(UNAME_S)' 'Darwin'
		$(CC) -o bin/server/RTNet_Server.application $(DEPS) $(CFLAGS_EXEC)
endif
endif

$(OBJ)/%.o: %.cpp
	@mkdir -p "$(dir $@)"
	$(CC) $< -o $@ $(CFLAGS)

directories:
	mkdir -p bin
	mkdir -p bin/server
	mkdir -p $(OBJ)

clean:
	@rm -rf bin/obj

.PHONY: directories clean