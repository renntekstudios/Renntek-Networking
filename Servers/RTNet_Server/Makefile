CC = g++

# Change to 'RELEASE' for actual releases
VERSION = DEBUG
DIALECT = -std=gnu++11 -m32 -D $(VERSION)
OPTIMIZATION = -O3

INCLUDES = -I headers

CFLAGS = $(DIALECT) $(INCLUDES) $(OPTIMIZATION) -c
CFLAGS_EXEC = $(DIALECT) $(INCLUDES) $(OPTIMIZATION)

OBJ = bin/obj

SOURCE = src/main.cpp src/logger.cpp src/generic_reader.cpp src/settings.cpp src/rtserver.cpp
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
	$(CC) -o bin/server/RTNet_Server.exe $(DEPS) $(CFLAGS_EXEC) -l ws2_32 -static-libgcc -static-libstdc++
else
ifeq '$(UNAME_S)' 'Linux'
		$(CC) -o bin/server/RTNet_Server $(DEPS) $(CFLAGS_EXEC) -pthread
		chmod a+x bin/server/RTNet_Server
endif
ifeq '$(UNAME_S)' 'Darwin'
		mkdir -p bin/server/RTNet_Server.app/MacOS/
		$(CC) -o bin/server/RTNet_Server.app/MacOS/RTNet_Server $(DEPS) $(CFLAGS_EXEC)
		chmod a+x bin/server/RTNet_Server.app
		mkdir -p bin/server/RTNet_Server.app/Contents/
		mkdir -p bin/server/RTNet_Server.app/Contents/Resources/
		cp resources/Info.plist bin/server/RTNet_Server.app/Contents/Info.plist
		cp resources/icon.icns bin/server/RTNet_Server.app/Contents/Resources/icon.icns
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