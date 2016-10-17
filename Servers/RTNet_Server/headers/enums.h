#ifndef _ENUMS_H_
#define _ENUMS_H_
#include <map>

using namespace std;

namespace RTNet
{
	typedef char rt_byte;
	typedef signed short rt_id;

	enum RT_PACKET_ID : short
	{
		RT_PACKET_DISCONNECT = 1,
		RT_PACKET_AUTH = 2,
		RT_PACKET_DISCOVER = 3,
	};

	enum ERROR_CODES : short
	{
		RT_ERROR_NONE = 0,
		RT_UNKNOWN_ERROR = -99,
		RT_ERROR_INVALID_ARGS = -1,
	};

	enum RT_CONNECTION_STATE 
	{
		DISCONNECTED = -1,
		CONNECTING = 0,
		CONNECTED = 1
	};

	enum RT_CLIENT_SIGNATURE : short
	{
		RT_SIGNATURE_UNKNOWN = 0,
		RT_SIGNATURE_SERVER = 99,
		RT_SIGNATURE_CSHARP = 1,
	};

	enum RT_UNKNOWN_BEHAVIOUR
	{
		RT_BEHAVIOUR_OTHERS = 0,
		RT_BEHAVIOUR_ALL = 1,
		RT_BEHAVIOUR_SELF = 2
	};
}
#endif