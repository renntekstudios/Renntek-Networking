CC = g++

# Change to 'RELEASE' for actual releases
VERSION = DEBUG
DIALECT = -std=gnu++11 -m32 -D $(VERSION)
OPTIMIZATION = -O3

INCLUDES = -I headers

CFLAGS = $(DIALECT) $(INCLUDES) $(OPTIMIZATION) -c
CFLAGS_EXEC = $(DIALECT) $(INCLUDES) $(OPTIMIZATION)

OBJ = bin/obj

SOURCE = src/main.cpp src/logger.cpp src/generic_reader.cpp src/settings.cpp src/rtserver.cpp src/rt_compression.cpp src/rt_encryption.cpp
DEPS = ${SOURCE:%.cpp=$(OBJ)/%.o}

UNAME_S := $(shell uname -s)
ifeq '$(VERSION)' 'DEBUG'
	CFLAGS += -g
	CFLAGS_EXEC += -g
ifneq '$(UNAME_S)' 'Darwin'
	CFLAGS += -fmax-errors=3
	CFLAGS_EXEC += -fmax-errors=3
endif
endif

all: directories $(DEPS)
ifeq '$(OS)' 'Windows_NT'
	windres resources/windows/info.rc -O coff -o bin/obj/info.res
	$(CC) -o bin/server/RTNetServer.exe $(DEPS) $(CFLAGS_EXEC) -l ws2_32 -static-libgcc -static-libstdc++ bin/obj/info.res
else
ifeq '$(UNAME_S)' 'Linux'
		$(CC) -o bin/server/RTNetServer $(DEPS) $(CFLAGS_EXEC) -pthread
		chmod +x bin/server/RTNetServer
endif
ifeq '$(UNAME_S)' 'Darwin'
		$(CC) -o bin/server/RTNetServer $(DEPS) $(CFLAGS_EXEC)
		chmod +x bin/server/RTNetServer
endif
endif

$(OBJ)/%.o: %.cpp
	@mkdir -p "$(dir $@)"
	$(CC) $< -o $@ $(CFLAGS)

directories:
	@mkdir -p bin
	@mkdir -p bin/server
	@mkdir -p $(OBJ)

clean:
	@rm -rf bin

.PHONY: directories clean
