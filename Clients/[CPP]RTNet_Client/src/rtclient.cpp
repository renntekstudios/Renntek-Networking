#include "rtnetclient.h"
#include "rt_logger.h"
#include <vector>
#include <sstream>
#include <iostream>

#ifdef _WIN32
#define WIN32_LEAN_AND_MEAN
#include <winsock2.h>
#include "rt_threading.h"
typedef int socklen_t;
#else
#include <thread>
#include <sys/socket.h>
#include <netinet/in.h>
#endif

#define RECEIVE_TIMEOUT (1000 / 60) // 60Hz
#define PACKET_SIZE (sizeof(unsigned char) * 3)

typedef char rt_byte;
typedef signed short rt_id;
typedef unsigned char rt_packet_id;

using namespace std;
using namespace RTNet;

enum RT_SIGNATURE : char { RT_SIGNATURE_SERVER = 99, RT_SIGNATURE_CS = 1, RT_SIGNATURE_CPP = 2 };

int bufferSize;
bool debugMode;
bool running = false;
bool shouldTimeout;
struct sockaddr_in si_server_udp;
struct sockaddr_in si_server_tcp;
enum RT_CONNECTION_STATUS connectionStatus;

short _id;
char* _address;
unsigned short _port;

unsigned int bytesInSec;
unsigned int bytesOutSec;
unsigned int bytesInSecFinal;
unsigned int bytesOutSecFinal;

#ifdef _WIN32
SOCKET udp_socket;
SOCKET tcp_socket;
WSADATA wsa;
#else
int udp_socket;
int tcp_socket;
#endif

thread* timer_thread;
thread* receive_thread;
thread* tcp_receive_thread;

vector<rt_byte> unhandled_bytes;

RTEvent* event_log;
RTEvent* event_logDebug;
RTEvent* event_logWarning;
RTEvent* event_logError;

void stop();
void timer_loop();
void udp_receive();
void tcp_receive();
short bytes_to_short(rt_byte* b);
void disconnect(bool send_packet);
vector<rt_byte> short_to_bytes(short i);
short bytes_to_short(rt_byte* b, int offset);
void handle_packet(rt_byte* data, size_t length);
void Connect(char* address, unsigned short port, unsigned short tcpPort);
int send(short packetID, rt_byte data[], size_t length);

void RTNetClient::Disconnect(bool send_packet) { disconnect(send_packet); }
enum RT_CONNECTION_STATUS RTNetClient::status() { return connectionStatus; }
void RTNetClient::Connect(char* address, unsigned short port, unsigned short tcpPort) { Connect(address, port, tcpPort); }

#ifdef _WIN32
void set_timeout(SOCKET* s);
int create_socket(SOCKET* s, int protocol);
#else
void set_timeout(int* s);
int create_socket(int* s, int protocol);
#endif

RTNetClient::RTNetClient()
{
	_id = -1;
	_port = 0;
	bufferSize = 512;
	debugMode = false;
	_address = (char*)"";
	shouldTimeout = true;
	connectionStatus = RT_STATUS_DISCONNECTED;

	int result = 0;
	#ifdef _WIN32
	if(WSAStartup(MAKEWORD(2, 2), &wsa) != 0)
	{
		LogError("Could not start RTNetClient - WSA_STARTUP_%d", WSAGetLastError());
		return;
	}
	#endif
}

RTNetClient::~RTNetClient()
{
	if(!running)
		return;
	running = false;
	#ifdef _WIN32
	closesocket(udp_socket);
	closesocket(tcp_socket);
	WSACleanup();
	#else
	close(udp_socket);
	close(tcp_socket);
	#endif
}

void Connect(char* address, unsigned short port, unsigned short tcpPort)
{
	if(connectionStatus != RT_STATUS_DISCONNECTED)
		return;
	_address = address;
	_port = port;
	running = true;

	if(create_socket(&udp_socket, 0) != 0)
		return;
	if(create_socket(&tcp_socket, 1) != 0)
		return;

	si_server_udp = { };
	si_server_udp.sin_family = AF_INET;
	si_server_udp.sin_port = htons(port);
	si_server_udp.sin_addr.s_addr = inet_addr(address);

	si_server_tcp = { };
	si_server_tcp.sin_family = AF_INET;
	si_server_tcp.sin_port = htons(tcpPort == 0 ? port + 1 : tcpPort);
	si_server_tcp.sin_addr.s_addr = inet_addr(address);

	connectionStatus = RT_STATUS_CONNECTING;

	if(connect(tcp_socket, (struct sockaddr*)&si_server_tcp, sizeof(si_server_tcp)) < 0)
	{
		LogError("Could not connect to server (TCP)");
		return;
	}

	rt_byte signature[] = { 17, 19, RT_SIGNATURE_CPP };
	if(sendto(udp_socket, signature, 3, 0, (struct sockaddr*)&si_server_udp, sizeof(si_server_udp)) == -1)
	{
		LogError("Could not connect to server (UDP)");
		return;
	}

	receive_thread = new thread(udp_receive);
	tcp_receive_thread = new thread(tcp_receive);
	timer_thread = new thread(timer_loop);
}

void Disconnect(bool send_packet = true)
{

}

short RTNetClient::id() { return _id; }
char* RTNetClient::address() { return _address; }
unsigned short RTNetClient::port() { return _port; }
bool RTNetClient::connected() { return connectionStatus == RT_STATUS_CONNECTED; }
unsigned int RTNetClient::networkIn() { return bytesInSecFinal; }
unsigned int RTNetClient::networkOut() { return bytesOutSecFinal; }

bool RTNetClient::SetBufferSize(unsigned int size)
{
	if(connectionStatus != RT_STATUS_DISCONNECTED)
		return false;
	bufferSize = size;
	return true;
}

bool RTNetClient::SetShouldTimeout(bool timeout)
{
	if(connectionStatus != RT_STATUS_DISCONNECTED)
		return false;
	shouldTimeout = timeout;
	return true;
}

void RTNetClient::SetDebugMode(bool debug)
{
	debugMode = debug;
}

void udp_receive()
{
	rt_byte buffer[bufferSize];
	socklen_t length = sizeof(si_server_udp);
	while(running)
	{
		recvfrom(udp_socket, buffer, bufferSize, 0, (struct sockaddr*)&si_server_udp, &length);
	}
}

void tcp_receive()
{

}

void timer_loop()
{
	// stringstream ss;
	short milliseconds = 0;
	while(running)
	{
		if(milliseconds == 1000)
		{
			bytesInSecFinal = bytesInSec;
			bytesOutSecFinal = bytesOutSec;
			bytesInSec = 0;
			bytesOutSec = 0;

			/*
			ss << "RennTek Networking Server v" << Settings::Version;
			if(bytesInSecFinal > 0 || bytesOutSecFinal > 0)
			{
				ss << " [";
				if(bytesInSecFinal > 1024)
					ss << roundf(bytesInSecFinal / 10.24f) / 100 << "kb/s in : ";
				else
					ss << bytesInSecFinal << "b/s in : ";
				if(bytesOutSecFinal > 1024)
					ss << roundf(bytesOutSecFinal / 10.24f) / 100 << "kb/s out]";
				else
					ss << bytesOutSecFinal << "b/s out]";
			}
			Utils::SetTitle(ss.str());
			
			ss.clear();
			ss.str(string());
			*/
			milliseconds = 0;
		}

		#ifdef _WIN32
		Sleep((DWORD)100);
		#else
		sleep(100);
		#endif
		milliseconds += 100;
	}
}

#ifdef _WIN32
int create_socket(SOCKET* s, int protocol)
#else
int create_socket(int* s, int protocol)
#endif
{
	int result;
	if(protocol == 0)
		result = (*s = socket(AF_INET, SOCK_DGRAM, IPPROTO_UDP));
	else
		result = (*s = socket(AF_INET, SOCK_STREAM, IPPROTO_TCP));

	#ifdef _WIN32
	if(result == INVALID_SOCKET)
	{
		LogError("Could not create %s socket - %d", (protocol == 0 ? "UDP" : (protocol == 1 ? "TCP" : "UNKNOWN_PROTOCOL")), WSAGetLastError());
		return result;
	}
	#else
	if(result < 0)
	{
		LogError("Could not create %s socket - %d", (protocol == 0 ? "UDP" : (protocol == 1 ? "TCP" : "UNKNOWN_PROTOCOL")), s);
		return result;
	}
	#endif

	#ifdef _WIN32
	DWORD nonBlocking = 1;
	if(ioctlsocket(*s, FIONBIO, &nonBlocking) != 0)
		LogWarning("Failed to set socket as non-blocking - %d", WSAGetLastError());
	#else
	int nonBlocking = 1;
	if(fcntl(*s, F_SETFL, O_NONBLOCK, nonBlocking) == -1)
		LogWarning("Failed to set socket as non-blocking");
	#endif
	return 0;
}

#ifdef _WIN32
void set_timeout(SOCKET* s)
#else
void set_timeout(int* s)
#endif
{
	int result;
	#ifdef _WIN32
	int timeout = RECEIVE_TIMEOUT;
	if((result = setsockopt(*s, SOL_SOCKET, SO_RCVTIMEO, (char*)&timeout, sizeof(timeout))) != 0)
	#else
	struct timeval tv;
	tv.tv_sec = 0;
	tv.tv_used = RECEIVE_TIMEOUT * 1000;
	if((result = setsockopt(*s, SOL_SOCKET, SO_RCVTIMEO, &tv, sizeof(tv))) != 0)
	#endif
		LogWarning("Could not set timeout for socket (%d)", result);
}


/** LOGGING **/
void RTNetClient::_internal_log(const char* file, int line, const string& message)
{
	stringstream ss;
	ss << "[" << __TIME__ << "](\"" << file << "\", line " << line << ") " << message << endl;
	event_log->fire((void*)ss.str().c_str());
}

void RTNetClient::_internal_log_debug(const char* file, int line, const string& message)
{
	if(debugMode)
	{
		stringstream ss;
		ss << "[" << __TIME__ << "](\"" << file << "\", line " << line << ")[DEBUG] " << message << endl;
		event_logDebug->fire((void*)ss.str().c_str());
	}
}

void RTNetClient::_internal_log_error(const char* file, int line, const string& message)
{
	stringstream ss;
	ss << "[" << __TIME__ << "](\"" << file << "\", line " << line << ")[ERROR] " << message << endl;
	event_logError->fire((void*)ss.str().c_str());
}

void RTNetClient::_internal_log_warning(const char* file, int line, const string& message)
{
	stringstream ss;
	ss << "[" << __TIME__ << "](\"" << file << "\", line " << line << ")[WARNING] " << message << endl;
	event_logWarning->fire((void*)ss.str().c_str());
}

string RTNetClient::_internal_string_vsprintf(const char *format, va_list args)
{
	va_list tmp_args;
	va_copy(tmp_args, args);
	const int required_len = vsnprintf(nullptr, 0, format, tmp_args) + 1;
	va_end(tmp_args);

	string buf(required_len, '\0');
	if(vsnprintf(&buf[0], buf.size(), format, args) < 0)
		_internal_log_error(__FILE__, __LINE__, "Encoding error!");
	return buf;
}

string RTNetClient::_internal_format(const char *format, ...)
{
	va_list args;
	va_start(args, format);
	string str { _internal_string_vsprintf(format, args) };
	va_end(args);
	return str;
}