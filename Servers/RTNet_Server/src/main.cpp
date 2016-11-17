#include <string>
#include <algorithm>
#include <selene.h>
#include "settings.h"
#include "logger.h"
#include "rtserver.h"

using namespace std;
using namespace RTNet;
using namespace sel;

#ifdef PLATFORM_WINDOWS
#include <windows.h>
BOOL WINAPI handle_exit(DWORD event);
#else
#include <signal.h>
void handle_exit(int signum);
#endif
void stop(int error_code = 0);

RTServer* server;

int main(int argc, char** argv)
{
	cout << "Reading settings" << endl;
	Settings::Read(Settings::ResourceDir + "settings.ini");

	#if defined(PLATFORM_WINDOWS)
	if(!SetConsoleCtrlHandler((PHANDLER_ROUTINE)handle_exit, TRUE))
		LogWarning("Could not set console control handler");
	#elif defined(PLATFORM_LINUX)
	signal(SIGHUP, handle_exit);
	#endif

	Log("Starting RTNet server v%s", Settings::Version.c_str());

	server = new RTServer();

	string line, lower;
	while(server->isRunning())
	{
		getline(cin, line);
		lower = line;
		transform(lower.begin(), lower.end(), lower.begin(), ::tolower);
		if(lower == "quit")
			server->Stop();
		else if(lower == "clear")
		{
			#ifdef _WIN32
			system("cls");
			#else
			cout << "\x1B[2J\x1B[H";
			#endif
		}
		else if(lower == "" || lower == " " || lower == "\n")
			continue;
		else
			Log("\"%s\" is not a valid command", lower.c_str());
	}
	stop();

	#ifdef PLATFORM_WINDOWS
	cout << endl << "Press any key to exit..." << endl;
	getchar();
	#endif
	return 0;
}

void stop(int error_code)
{
	server->Stop();
	delete server;
	Settings::Save();
}

#ifdef PLATFORM_WINDOWS
BOOL WINAPI handle_exit(DWORD event)
{
	stop(0);
	return true;
}
#else
void handle_exit(int signum)
{
	stop(signum);
}
#endif