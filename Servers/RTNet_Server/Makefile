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
	windres resources/windows/info.rc -O coff -o bin/obj/info.res
	$(CC) -o bin/server/RTNetServer.exe $(DEPS) $(CFLAGS_EXEC) -l ws2_32 -static-libgcc -static-libstdc++ bin/obj/info.res
else
ifeq '$(UNAME_S)' 'Linux'
		$(CC) -o bin/server/RTNetServer $(DEPS) $(CFLAGS_EXEC) -pthread
		chmod 777 bin/server/RTNetServer
endif
ifeq '$(UNAME_S)' 'Darwin'
		mkdir -p bin/server/RTNetServer.app/MacOS/
		$(CC) -o bin/server/RTNetServer.app/MacOS/RTNetServer $(DEPS) $(CFLAGS_EXEC)
		mkdir -p bin/server/RTNetServer.app/Contents/Resources/
		cp resources/mac/start_server.sh bin/server/RTNetServer.app/MacOS/start.sh
		cp resources/mac/Info.plist bin/server/RTNetServer.app/Contents/Info.plist
		cp resources/mac/icon.icns bin/server/RTNetServer.app/Contents/Resources/icon.icns
		chmod a+x bin/server/RTNetServer.app
		chmod +x bin/server/RTNetServer.app/MacOS/start.sh
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
