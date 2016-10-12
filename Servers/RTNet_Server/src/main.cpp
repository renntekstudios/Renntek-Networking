#include <string>
#include <algorithm>
#include "settings.h"
#include "logger.h"
#include "rtserver.h"

using namespace std;
using namespace RTNet;

#ifdef _WIN32
#define WIN32_LEAN_AND_MEAN
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
	Settings::Read(Settings::ResourceDir + "settings.ini");

	#if defined(_WIN32)
	if(!SetConsoleCtrlHandler((PHANDLER_ROUTINE)handle_exit, TRUE))
		LogWarning("Could not set console control handler");
	#elif defined(__linux__)
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
		else
			Log("\"%s\" is not a valid command", lower.c_str());
	}
	stop();

	#ifdef _WIN32
	cout << endl << endl << "Press any key to exit..." << endl;
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

#ifdef _WIN32
BOOL WINAPI handle_exit(DWORD event)
{
	/*
	switch(event)
	{
	case CTRL_C_EVENT: MessageBox(NULL, "CTRL+C", "Event Caught!", MB_OK); break;
	case CTRL_BREAK_EVENT: MessageBox(NULL, "CTRL+BREAK", "Event Caught!", MB_OK); break;
	case CTRL_CLOSE_EVENT: MessageBox(NULL, "CLOSE EVENT", "Event Caught!", MB_OK); break;
	case CTRL_LOGOFF_EVENT: MessageBox(NULL, "LOGOFF EVENT", "Event Caught!", MB_OK); break;
	case CTRL_SHUTDOWN_EVENT: MessageBox(NULL, "SHUTDOWN EVENT", "Event Caught!", MB_OK); break;
	default: MessageBox(NULL, "UNKNOWN EVENT", "Event Caught!", MB_OK); break;
	}
	*/
	stop(0);
	return true;
}
#else
void handle_exit(int signum)
{
	stop(signum);
}
#endif