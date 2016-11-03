#ifndef _SETTINGS_H_
#define _SETTINGS_H_
#include <string>
#include <vector>
#include "enums.h"
#include "generic_reader.h"

using namespace std;

namespace RTNet
{
	class Settings
	{
	public:
		static const string ResourceDir;
		static const string AccountDir;

		static const string Version;

		static int UDPPort;
		static int TCPPort;
		static int BufferSize;
		static int BacklogSize;
		static RT_UNKNOWN_BEHAVIOUR UnknownBehaviour;

		static bool DebugMode;

		static void Initialize(string path);
		static bool Read();
		static bool Read(string path);
		static void Save();
		static void Save(string path);
	private:
		static GenericReader _reader;
		static bool _initialized;

		static const vector<string> importantDirectories;

		static void Initialize();
		static void CheckDirectories();
	};
}
#endif