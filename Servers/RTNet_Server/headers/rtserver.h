#include <vector>
#include "enums.h"
#include "logger.h"
#include "packet.h"

#ifdef _WIN32
#define WIN32_LEAN_AND_MEAN
typedef int socklen_t;
#include <winsock2.h>
#include "win_threading.h"
#elif defined(__linux__) | defined(__APPLE__)
#include <unistd.h>
#include <pthread.h>
#include <arpa/inet.h>
#include <sys/socket.h>

#include <sys/un.h>
#include <sys/types.h>
#include <netinet/in.h>
#include <netinet/tcp.h>
#include <fcntl.h>
#include <netdb.h>
#include <errno.h>
#endif

using namespace std;

namespace RTNet
{
	struct rt_client
	{
		rt_id id;
		struct sockaddr_in sock_addr;
		vector<unsigned int> unhandled_packet_ids;
		vector<unhandled_packet_t> unhandled_packets;
		RT_CONNECTION_STATE connection_state;
		RT_CLIENT_SIGNATURE signature;

		char* address;
		unsigned short port;
	};

	class RTServer
	{
	public:
		RTServer();
		~RTServer();

		void Stop();
		bool isRunning();

		int Send(rt_id client, rt_byte data[], size_t length);
		int Send(rt_client* client, rt_byte data[], size_t length);
		int SendOthers(rt_id sender, rt_byte data[], size_t length);
		int SendOthers(rt_client* sender, rt_byte data[], size_t length);
		int SendAll(rt_byte data[], size_t length);
	};
}