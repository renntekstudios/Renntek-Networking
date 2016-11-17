CC = g++
AR = ar

# Change to 'RELEASE' for actual releases
VERSION = DEBUG
DIALECT = -std=gnu++11 -m32 -D $(VERSION)
OPTIMIZATION = -O3

INCLUDES = -I includes

CFLAGS = $(DIALECT) $(INCLUDES) $(OPTIMIZATION) -c

OBJ = bin/obj

SOURCE = src/rtclient.cpp
DEPS = ${SOURCE:%.cpp=$(OBJ)/%.o}

UNAME_S := $(shell uname -s)
ifeq '$(VERSION)' 'DEBUG'
	CFLAGS += -g
ifneq '$(UNAME_S)' 'Darwin'
	CFLAGS += -fmax-errors=3
endif
endif

all: directories $(DEPS)
ifeq '$(OS)' 'Windows_NT'
	$(AR) rvs bin/RTNet_Client.a $(DEPS)
else
	$(AR) rvs bin/RTNet_Client.a $(DEPS)
endif

$(OBJ)/%.o: %.cpp
	@mkdir -p "$(dir $@)"
	$(CC) $< -o $@ $(CFLAGS)

directories:
	@mkdir -p bin
	@mkdir -p $(OBJ)

clean:
	@rm -rf bin

.PHONY: directories clean
