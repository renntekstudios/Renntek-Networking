#include <iostream>
#include <algorithm>
#include "settings.h"
#include "enums.h"
#include "utils.h"
#include "logger.h"

#define DEFAULT_UDP_PORT 4434
#define DEFAULT_TCP_PORT 4435
#define DEFAULT_BUFFERSIZE 512

using namespace std;
using namespace RTNet;

template<class T>
extern T Read(string key, T defaultValue);

string Replace(const string& original, const char a, const char b);

const string Settings::ResourceDir = "./";
const string Settings::Version = "0.1";

int Settings::UDPPort = DEFAULT_UDP_PORT;
int Settings::TCPPort = DEFAULT_TCP_PORT;
int Settings::BufferSize = DEFAULT_BUFFERSIZE;
bool Settings::DebugMode = false;
RT_UNKNOWN_BEHAVIOUR Settings::UnknownBehaviour = RT_BEHAVIOUR_ALL;

const vector<string> Settings::importantDirectories = {  };

bool Settings::_initialized = false;
GenericReader Settings::_reader;

void Settings::Initialize()
{
	CheckDirectories();
	_reader.Init("");
	_initialized = true;
}

void Settings::Initialize(string path)
{
	CheckDirectories();
	_reader.Init(path.c_str());
	_initialized = true;
}

bool Settings::Read()
{
	if(!_initialized)
		return false;
	return true;
}

bool Settings::Read(string path)
{
	if(!_initialized)
		Initialize(path);

	// variable = GenericReaader.Read("KEY", default_value);
	UDPPort = _reader.Read("UDP Port", DEFAULT_UDP_PORT);
	TCPPort = _reader.Read("TCP Port", DEFAULT_TCP_PORT);
	BufferSize = _reader.Read("Buffer Size", 512);
	DebugMode = _reader.Read("Debug Mode", false);
	UnknownBehaviour = (RT_UNKNOWN_BEHAVIOUR)_reader.Read("Unknown Behaviour", (int)RT_BEHAVIOUR_ALL);

	if(UDPPort < 0 || UDPPort > 65535)
		UDPPort = DEFAULT_UDP_PORT;
	if(TCPPort < 0 || TCPPort > 65535)
		TCPPort = DEFAULT_TCP_PORT;
	if(BufferSize <= 0 || BufferSize > 65535)
		BufferSize = DEFAULT_BUFFERSIZE;

	LogDebug("RennTek Networking Server v%s", Version.c_str());
	LogDebug("UDP Port: %d", UDPPort);
	LogDebug("TCP Port: %d", TCPPort);
	LogDebug("Buffer Size: %d", BufferSize);
	LogDebug("Unknown Behaviour: %s", (UnknownBehaviour == (int)RT_BEHAVIOUR_OTHERS ? "others" : (UnknownBehaviour == (int)RT_BEHAVIOUR_ALL ? "all" : "unknown")));
	return true;
}

void Settings::Save()
{
	if(!_initialized)
		Initialize();
	string path = _reader.getPath();
	if(path.empty())
	{
		Log("No path was set for settings, using defalt of \"Resources/settings.ini\"");
		path = ResourceDir + "settings.ini";
	}
	Save(path);
}

void Settings::Save(string path)
{
	if(!_initialized)
		Initialize(path);
	_reader.Clear();

	_reader.Heading("Server Settings");
	_reader.Write("UDP Port", UDPPort);
	_reader.Write("TCP Port", TCPPort);
	_reader.Write("Buffer Size", BufferSize);

	_reader.WriteLine();
	_reader.Comment("The behaviour used when an unknown packet is received");
	_reader.Comment("\t0 = Others (data is sent to all other clients)");
	_reader.Comment("\t1 = All (data is sent back, but to all clients)");
	_reader.Comment("\t2 = Self (data is sent back to the sender)");
	_reader.Write("Unknown Behaviour", (int)UnknownBehaviour);

	_reader.WriteLine();

	_reader.Heading("Debugging");
	_reader.Write("Debug Mode", DebugMode);

	_reader.WriteLine();
	_reader.WriteLine();
	_reader.Comment("Developed by RennTek Studios™ (2014-2016)");

	_reader.WriteToFile(path.c_str());
	LogDebug("Saved settings to \"%s\"", path.c_str());
}

void Settings::CheckDirectories()
{
	if(Utils::CreateDirIfNotExist(ResourceDir))
		Log("Created dir \"%s\"", ResourceDir.c_str());
	for(unsigned int i = 0;i < importantDirectories.size();i++)
	{
		string dir = Settings::ResourceDir + importantDirectories[i];
		dir = Utils::Replace(dir, '\\', '/');
		if(Utils::CreateDirIfNotExist(dir))
			Log("Created dir \"%s\"", dir.c_str());
	}
}