#ifndef _RT_CLIENT_H_
#define _RT_CLIENT_H_
#include "rt_event.h"
#include <stdarg.h>

namespace RTNet
{
	enum RT_CONNECTION_STATUS { RT_STATUS_DISCONNECTED, RT_STATUS_CONNECTING, RT_STATUS_CONNECTED };
	enum RT_PACKET_ID : short { RT_PACKET_LANDISCOVERY };
	enum RT_PACKETRELIABLE_ID : short { };

	class RTNetClient
	{
	public:
		RTNetClient();
		~RTNetClient();

		bool SetShouldTimeout(bool timeout);
		void SetDebugMode(bool debug = true);
		bool SetBufferSize(unsigned int size);

		short id();
		char* address();
		bool connected();
		unsigned short port();
		unsigned int networkIn();
		unsigned int networkOut();
		enum RT_CONNECTION_STATUS status();

		void Connect(char* address, unsigned short port, unsigned short tcpPort = 0);
		void Disconnect(bool send_packet = true);

		void addLog(void (*)(void*));
		void addDebug(void (*)(void*));
		void addWarning(void (*)(void*));
		void addError(void (*)(void*));

	private:
		void _internal_log(const char* file, int line, const string& message);
		void _internal_log_debug(const char* file, int line, const string& message);
		void _internal_log_error(const char* file, int line, const string& message);
		void _internal_log_warning(const char* file, int line, const string& message);
		string _internal_string_vsprintf(const char *format, va_list args);
		string _internal_format(const char *format, ...);
	};
}

#endif