#define WIN32_LEAN_AND_MEAN
#include <Windows.h>
#include <iostream>
#include <rtnetclient.h>

using namespace std;
using namespace RTNet;

void log(void* output)
{
	char* m = (char*)output;
	cout << m << endl;
}

void main(int argc, char** argv)
{
	RTNetClient client;

	client.addLog(log);
	client.addDebug(log);
	client.addWarning(log);
	client.addError(log); 

	client.Connect("127.0.0.1", 4434);
}