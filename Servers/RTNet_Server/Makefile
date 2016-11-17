CC = g++
OBJ = bin/obj

# Change to 'RELEASE' for actual releases
VERSION = DEBUG
DIALECT = -std=gnu++11 -m32 -D$(VERSION)
OPTIMIZATION = -O3

PREPROCESSOR = -D GLFW_DLL -D GLEW_STATIC
INCLUDES = -I headers -I include
LIBRARIES = -L libs -l glew32s -l glfw3dll -l glu32 -l opengl32

ifeq '$(OS)' 'Windows_NT'
LIBRARIES += -l win32_lua53
else
LIBRARIES += -l lua53 -l glfw3
endif

CFLAGS = $(DIALECT) $(INCLUDES) $(OPTIMIZATION) $(PREPROCESSOR) -c
CFLAGS_EXEC = $(DIALECT) $(INCLUDES) $(OPTIMIZATION)

<<<<<<< HEAD
SOURCE = src/main.cpp src/logger.cpp src/generic_reader.cpp src/settings.cpp src/rtserver.cpp src/rt_compression.cpp src/rt_encryption.cpp
=======
SOURCE = src/main.cpp src/logger.cpp src/generic_reader.cpp src/settings.cpp src/rtserver.cpp src/world.cpp src/graphics/shader.cpp
>>>>>>> Made changes to server
DEPS = ${SOURCE:%.cpp=$(OBJ)/%.o}

UNAME_S := $(shell uname -s)
ifeq '$(VERSION)' 'DEBUG'
	CFLAGS += -g -Wall
	CFLAGS_EXEC += -g
ifneq '$(UNAME_S)' 'Darwin'
	CFLAGS += -fmax-errors=3
	CFLAGS_EXEC += -fmax-errors=3
endif
endif

ifeq '$(OS)' 'Windows_NT'
CFLAGS += -D PLATFORM_WINDOWS -D WIN32_LEAN_AND_MEAN
else
ifeq '$(UNAME_S)' 'Linux'
CFLAGS += -D PLATFORM_LINUX
endif
ifeq '$(UNAME_S)' 'Darwin'
CFLAGS += -D PLATFORM_MAC
endif
endif

all: directories $(DEPS)
ifeq '$(OS)' 'Windows_NT'
	windres resources/windows/info.rc -O coff -o bin/obj/info.res
	$(CC) -o bin/server/RTNetServer.exe $(DEPS) $(CFLAGS_EXEC) -static-libgcc -static-libstdc++ bin/obj/info.res -l ws2_32 $(LIBRARIES)
	@cp libs/glfw3.dll bin/server/glfw3.dll
else
ifeq '$(UNAME_S)' 'Linux'
	$(CC) -o bin/server/RTNetServer $(DEPS) $(CFLAGS_EXEC) -pthread $(LIBRARIES)
	chmod +x bin/server/RTNetServer
endif
ifeq '$(UNAME_S)' 'Darwin'
	$(CC) -o bin/server/RTNetServer $(DEPS) $(CFLAGS_EXEC) $(LIBRARIES)
	chmod +x bin/server/RTNetServer
endif
endif
	@mkdir -p bin/server/Shaders/
	@mkdir -p bin/server/Plugins/
	@cp resources/Shaders/* bin/server/Shaders/
	@cp resources/TemplatePlugin.lua bin/server/Plugins/TemplatePlugin.lua

$(OBJ)/%.o: %.cpp
	@mkdir -p "$(dir $@)"
	$(CC) $< -o $@ $(CFLAGS)

directories:
	@mkdir -p bin/server
	@mkdir -p $(OBJ)

clean:
	@rm -rf bin

.PHONY: directories clean
