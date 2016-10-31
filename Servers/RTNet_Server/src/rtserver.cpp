#include <map>
#include <math.h>
#include <thread>
#include <stdio.h>
#include <algorithm>
#include <exception>
#include <sys/time.h>
#include "rtserver.h"
#include "settings.h"
#include "utils.h"

#define RECEIVE_TIMEOUT (1000 / 60) // 60Hz
#define PACKET_SIZE (sizeof(unsigned char) * 3)

using namespace std;
using namespace std::chrono;
using namespace RTNet;

bool running = false;
struct sockaddr_in si_server_udp;
struct sockaddr_in si_server_tcp;

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
thread* tcp_accept_thread;
vector<thread*> tcp_receive_threads;

map<rt_id, rt_client> clients;
vector<rt_byte> unhandled_bytes;
vector<rt_packet_id> sent_packet_ids;
vector<unhandled_packet_t> unhandled_packets;

void timer_loop();
int send_all(rt_byte data[], size_t length);
int send(rt_id client, rt_byte data[], size_t length);
int send_raw(rt_id client, rt_byte data[], size_t length);
int send(rt_client* client, rt_byte data[], size_t length);
int send_others(rt_id sender, rt_byte data[], size_t length);
int send_raw(rt_client* client, rt_byte data[], size_t length);
int send_others(rt_client* sender, rt_byte data[], size_t length);
void close_connection(rt_client* client, bool send_packet = true);
void handle_packet(rt_client* client, rt_byte* data, size_t length);

vector<rt_byte> int_to_bytes(int i)
{
	vector<rt_byte> b(4);
	for(int i = 0;i < 4;i++)
		b[3 - i] = (i >> (i * 8));
	return b;
}

int bytes_to_int(rt_byte* b) { return ((b[0] << 24) | (b[1] << 16) | (b[2] << 8) | b[3]); }
int bytes_to_int(rt_byte* b, int offset) { return ((b[offset] << 24) | (b[1 + offset] << 16) | (b[2 + offset] << 8) | b[3 + offset]); }

vector<rt_byte> short_to_bytes(short i)
{
	vector<rt_byte> b(2);
	b[1] = i >> 8;
	b[0] = i & 255;
	return b;
}

short bytes_to_short(rt_byte* b, int offset)
{
	if(b[offset] < 0)
		return (b[offset + 1] << 8) + b[offset] + 256;
	return (b[offset + 1] << 8) + b[offset];
}
short bytes_to_short(rt_byte* b) { return bytes_to_short(b, 0); }

rt_id GetID()
{
	rt_id id = 0;
	while(clients.find(id) != clients.end())
		id++; // continue looping until a client doesn't have the same id
	return id;
}

unsigned short GetPacketID()
{
	rt_packet_id id = 1;
	while(find(sent_packet_ids.begin(), sent_packet_ids.end(), id) != sent_packet_ids.end())
		id++;
	return id;
}

void receive()
{
	int receive_length, result;
	struct sockaddr_in addr;
	socklen_t addrlen = sizeof(addr);
	char buffer[Settings::BufferSize];
	while(running)
	{		
		receive_length = recvfrom(udp_socket, buffer, Settings::BufferSize, 0, (struct sockaddr*)&addr, &addrlen);
		if(receive_length <= 0)
			continue;
		bytesInSec += receive_length;

		if(receive_length == 3 && buffer[0] == (char)17 && buffer[1] == (char)19 && buffer[2] == (char)RT_PACKET_DISCOVER)
		{
			// LogDebug("Got discover packet");
			sendto(udp_socket, buffer, receive_length, 0, (struct sockaddr*)&addr, sizeof(addr));
			continue;
		}
		// LogDebug("Got %d bytes", receive_length);

		rt_id client_id = -1;
		rt_client* client;
		char* address = inet_ntoa(addr.sin_addr);
		unsigned short port = ntohs(addr.sin_port);
		for(unsigned int i = 0;i < clients.size();i++)
		{
			if(inet_ntoa(clients[i].sock_addr.sin_addr) == address && clients[i].sock_addr.sin_port == addr.sin_port)
			{
				client_id = clients[i].id;
				client = &clients[i];
				break;
			}
		}

		//new (or unknown?) client
		if(client_id == -1)
		{
			client_id = GetID();
			clients[client_id] = { };
			clients[client_id].id = client_id;
			clients[client_id].sock_addr = addr;
			clients[client_id].connection_state = CONNECTING; // Wait for next packet to determine if actually connected or not
			clients[client_id].address = address;
			clients[client_id].port = port;
			Log("New connection from \"%s:%d\" (%d)", address, port, client_id);
			client = &clients[client_id];
		}

		rt_byte* data = new rt_byte[receive_length];
		memcpy(data, buffer, receive_length);

		// LogDebug("Received packet from \"%s:%d\" (%d)(%d bytes)", address, port, client_id, receive_length);
		if(receive_length == 3)
		{
			if(data[0] != 17 || data[1] != 19)
			{
				LogError("A client tried connecting with an unknown signature (%d)", client_id);
				delete[] data;
				continue;
			}

			client->signature = (RT_CLIENT_SIGNATURE)data[2];
			char temp[] = { 17, 19, (char)RT_SIGNATURE_SERVER };
			if((result = send_raw(client_id, temp, 3)) < 0)
				LogError("Could not send initial data to client \"%d\" (%d)", client_id, result);
			delete[] data;

			int auth_data_length = 4;
			char auth_data[auth_data_length];
			vector<char> temp_auth = short_to_bytes((short)RT_PACKET_AUTH);
			auth_data[0] = temp_auth[0];
			auth_data[1] = temp_auth[1];
			temp_auth = short_to_bytes(client_id);
			auth_data[2] = temp_auth[0];
			auth_data[3] = temp_auth[1];
			send(&clients[client_id], auth_data, auth_data_length);
			// LogDebug("Sent auth packet to \"%d\"", client_id);
		}
		else
			handle_packet(client, data, receive_length);
	}
}

void tcp_receive()
{

}

void tcp_accept()
{
	return;

	// CURRENTLY DOESN'T WORK
	while(running)
	{
		sockaddr addr;
		#ifdef _WIN32
		SOCKET a = accept(tcp_socket, &addr, NULL);
		if(a == INVALID_SOCKET)
		{
			LogError("Failed to accept socket - %d", WSAGetLastError());
			continue;
		}
		#else

		#endif
	}
}

#ifdef _WIN32
int create_socket(SOCKET* s, int protocol)
#else
int create_socket(int* s, int protocol)
#endif
{
	int result;
	if(protocol == 0) // udp
		result = (*s = socket(AF_INET, SOCK_DGRAM, IPPROTO_UDP));
	else if(protocol == 1) // tcp
		result = (*s = socket(AF_INET, SOCK_STREAM, 0));

	#ifdef _WIN32
	if(result == INVALID_SOCKET)
	{
		LogError("Could not create %s socket - %d", (protocol == 0 ? "UDP" : (protocol == 1 ? "TCP" : "UNKNOWN_PROTOCOL")), WSAGetLastError());
		return result;
	}
	DWORD nonBlocking = 1;
	if(ioctlsocket(*s, FIONBIO, &nonBlocking) != 0)
		LogWarning("Failed to set socket as non-blocking");
	#else
	if(result < 0)
	{
		LogError("Could not create %s socket - %d", (protocol == 0 ? "UDP" : (protocol == 1 ? "TCP" : "UNKNOWN_PROTOCOL")), s);
		return result;
	}
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
	tv.tv_usec = RECEIVE_TIMEOUT * 1000;
	if((result = setsockopt(*s, SOL_SOCKET, SO_RCVTIMEO, &tv, sizeof(tv))) != 0)
	#endif
		LogWarning("Could not set timeout for UDP socket (%d - %s)", result, Utils::GetErrorMessage(result).c_str());
}

RTServer::RTServer()
{
	Utils::SetTitle(string("RennTek Networking Server v") + Settings::Version);

	int result = 0;
	#ifdef _WIN32
	if(WSAStartup(MAKEWORD(2,2), &wsa) != 0)
	{
		LogError("Could not start RTServer - WSA_STARTUP_%d", WSAGetLastError());
		return;
	}
	#endif
	if(create_socket(&udp_socket, 0) == 0)
		LogDebug("Created UDP socket on port %d", Settings::UDPPort);
	else
		return;
	if(create_socket(&tcp_socket, 0) == 0)
		LogDebug("Created TCP socket on port %d", Settings::TCPPort);
	else
		return;
	
	si_server_udp = { };
	si_server_udp.sin_family = AF_INET;
	si_server_udp.sin_port = htons(Settings::UDPPort);
	si_server_udp.sin_addr.s_addr = htonl(INADDR_ANY);

	si_server_tcp = { };
	si_server_tcp.sin_family = AF_INET;
	si_server_tcp.sin_port = htons(Settings::TCPPort);
	si_server_tcp.sin_addr.s_addr = htonl(INADDR_ANY);

	if((result = bind(udp_socket, (const struct sockaddr*)&si_server_udp, (socklen_t)sizeof(si_server_udp))) != 0)
	{
		LogError("Could not bind to UDP port %d - %d", Settings::UDPPort, result);
		return;
	}
	if((result = bind(tcp_socket, (const struct sockaddr*)&si_server_tcp, (socklen_t)sizeof(si_server_tcp))) != 0)
	{
		LogError("Could not bind to TCP port %d - %d", Settings::TCPPort, result);
		return;
	}
	else
		listen(tcp_socket, 3);

	set_timeout(&udp_socket);
	// set_timeout(&tcp_socket);
	
	running = true;
	receive_thread = new thread(receive);
	timer_thread = new thread(timer_loop);
	tcp_accept_thread = new thread(tcp_accept);
}

void RTServer::Stop()
{
	if(!running)
		return;
	running = false;
	receive_thread->join();
	tcp_accept_thread->join();
	timer_thread->join();
	// delete receive_thread;
	#ifdef _WIN32
	closesocket(udp_socket);
	closesocket(tcp_socket);
	WSACleanup();
	#else
	close(udp_socket);
	close(tcp_socket);
	#endif
	Log("Server stopped");
}

RTServer::~RTServer()
{
	if(running)
		Stop();
}

int RTServer::Send(rt_id client, rt_byte data[], size_t length) { return send(client, data, length); }
int RTServer::Send(rt_client* client, rt_byte data[], size_t length) { return send(client, data, length); }

rt_client* get_client(rt_id client_id)
{
	for(int i = 0;i < clients.size();i++)
		if(clients[i].id == client_id)
			return &clients[i];
	return nullptr;
}

int send(rt_client* client, rt_byte data[], size_t length)
{
	try
	{
		size_t data_length = length;
		if(client == nullptr)
		{
			LogWarning("Could not send data to client - client is null");
			return -2;
		}
		sockaddr_in sock_addr = client->sock_addr;
		socklen_t socket_length = sizeof(sock_addr);
		
		vector<rt_byte> tosend;
		rt_packet_id packet_id = GetPacketID();
		char index = 0;
		sent_packet_ids.push_back(packet_id);

		char* address = inet_ntoa(sock_addr.sin_addr);
		unsigned short port = ntohs(sock_addr.sin_port);

		vector<rt_byte> temp;
		if(data_length > Settings::BufferSize - PACKET_SIZE)
		{
			while(data_length > Settings::BufferSize - PACKET_SIZE)
			{
				tosend.push_back(1);
				tosend.push_back(packet_id);
				tosend.push_back(index++);
				tosend.insert(tosend.end(), data, data + Settings::BufferSize - PACKET_SIZE);
				
				if(sendto(udp_socket, tosend.data(), tosend.size(), 0, (struct sockaddr*)&sock_addr, socket_length) == -1)
					LogWarning("Could not send data (%d bytes)", tosend.size());
				// else
				//	LogDebug("Sent %d bytes", tosend.size());
				bytesOutSec += tosend.size();

				data_length -= Settings::BufferSize - PACKET_SIZE;
				rt_byte* temp_data = new rt_byte[data_length];
				memcpy(temp_data, data, sizeof(rt_byte) * data_length);
				data = temp_data;
				delete[] temp_data;
				tosend.clear();
			}
			tosend.push_back(2);
			tosend.push_back(packet_id);
			tosend.push_back(index++);
			tosend.insert(tosend.end(), data, data + data_length);
		}
		else
		{
			tosend.push_back(3);
			tosend.push_back(packet_id);
			tosend.push_back(index++);
			for(unsigned int i = 0; i < data_length; i++)
				tosend.push_back(data[i]);
			// tosend.insert(tosend.end(), data, data + data_length);
		}

		int result = 0;
		if((result = sendto(udp_socket, tosend.data(), tosend.size(), 0, (struct sockaddr*)&sock_addr, socket_length)) <= 0)
			LogWarning("Could not send data (%d bytes)", tosend.size());
		else
		{
			result = 0;
			// LogDebug("Sent %d bytes", tosend.size());
		}
		bytesOutSec += tosend.size();
		sent_packet_ids.erase(remove(sent_packet_ids.begin(), sent_packet_ids.end(), packet_id), sent_packet_ids.end());
		return result;
	}
	catch(exception e)
	{
		LogError("Couldn't send data to (%d) - %s", client->id, e.what());
		return -1;
	}
}

int send_raw(rt_client* client, rt_byte data[], size_t length)
{
	return sendto(udp_socket, data, length, 0, (struct sockaddr*)&client->sock_addr, sizeof(client->sock_addr));
}

int send(rt_id client_id, rt_byte data[], size_t length)
{
	rt_client* client = get_client(client_id);
	if(client == nullptr)
		return -1;
	else
		return send(client, data, length);
}

int send_raw(rt_id client_id, rt_byte data[], size_t length)
{
	int index = -1;
	for(int i = 0;i < clients.size();i++)
	{
		if(clients[i].id == client_id)
			return send_raw(&clients[i], data, length);
	}
	return -1;
}

int send_others(rt_id sender, rt_byte data[], size_t length)
{
	int result;
	for(unsigned int i = 0;i < clients.size();i++)
	{
		if(clients[i].id != sender)
			result = send(&clients[i], data, length);
	}
	return result;
}

int send_others(rt_client* sender, rt_byte data[], size_t length)
{
	int result;
	for(unsigned int i = 0;i < clients.size();i++)
	{
		if(clients[i].id != sender->id)
			result = send(&clients[i], data, length);
	}
	return result;
}

int send_all(rt_byte data[], size_t length)
{
	int result;
	for(unsigned int i = 0;i < clients.size();i++)
		result = send(&clients[i], data, length);
	return result;
}

int SendOthers(rt_id sender, rt_byte data[], size_t length) { return send_others(sender, data, length); }
int SendOthers(rt_client* sender, rt_byte data[], size_t length) { return send_others(sender, data, length); }
int SendAll(rt_byte data[], size_t length) { return send_all(data, length); }

bool RTServer::isRunning() { return running; }

void handle_packet(rt_client* client, rt_byte* buffer, size_t length)
{
	if(length <= 3)
		return;
	if(client == nullptr)
		return;

	char packet_status = (char)buffer[0];
	char packet_internal_id = (char)buffer[1];
	char packet_index = (char)buffer[2];

	// LogDebug("STATUS: %d; INTERNAL_ID: %d; INDEX: %d", packet_status, packet_internal_id, packet_index);

	unsigned int data_length = length - PACKET_SIZE;
	rt_byte* data = new rt_byte[data_length];
	vector<rt_byte> v_data(data_length);
	for(int i = 0;i < data_length;i++)
		v_data[i] = data[i] = buffer[i + PACKET_SIZE];

	int index = -1;
	for(unsigned int i = 0; i < client->unhandled_packets.size(); i++)
	{
		if(client->unhandled_packets[i]->packet_id == packet_internal_id)
		{
			index = i;
			break;
		}
	}
	if(packet_status == -1)
	{
		if(index >= 0)
		{
			client->unhandled_packets[index]->bytes[packet_index] = v_data; // .insert(client->unhandled_packets[index]->bytes.begin() + (int)packet_index, pair<int, vector<rt_byte>>(packet_index, v_data));
			if(client->unhandled_packets[index]->expected > 0 && client->unhandled_packets[index]->bytes.size() == client->unhandled_packets[index]->expected)
			{
				client->unhandled_packets[index]->get_final_buffer(&data, &data_length);
				if(data == nullptr)
				{
					delete[] data;
					return;
				}
				// LogDebug("Got final packet! (%d bytes)", data_length);
				client->unhandled_packets.erase(client->unhandled_packets.begin() + index);
			}
			else
			{
				// LogWarning("Don't have all packets, returning");
				delete[] data;
				return;
			}
		}
		else
		{
			unhandled_packet_t* packet = new unhandled_packet_t();
			packet->packet_id = packet_internal_id;
			packet->bytes[packet_index] = v_data;
			client->unhandled_packets.push_back(packet);
			// LogDebug("Added new unhandled packet with ID \"%d\"", packet_internal_id);
			delete[] data;
			return;
		}
	}
	else if(packet_status == -2)
	{
		if(index >= 0)
		{
			client->unhandled_packets[index]->bytes[packet_index] = v_data; // .insert(client->unhandled_packets[index]->bytes.begin() + (int)packet_index, pair<int, vector<rt_byte>>(packet_index, v_data));
			int expected = client->unhandled_packets[index]->expected = packet_index + 1;
			if(expected > 0 && client->unhandled_packets[index]->bytes.size() == expected)
			{
				client->unhandled_packets[index]->get_final_buffer(&data, &data_length);
				if(data == nullptr)
				{
					delete[] data;
					return;
				}
				// LogDebug("Got final packet! (%d bytes)", data_length);
				client->unhandled_packets.erase(client->unhandled_packets.begin() + index);
			}
			else
			{
				// LogWarning("Don't have all packets, returning");
				delete[] data;
				return;
			}
		}
		else
		{
			unhandled_packet_t* packet = new unhandled_packet_t();
			packet->packet_id = packet_internal_id;
			packet->bytes[packet_index] = v_data;
			client->unhandled_packets.push_back(packet);
			// LogDebug("Added new unhandled packet with ID \"%d\"", packet_internal_id);
			delete[] data;
			return;
		}
	}

	short packet_id = bytes_to_short(data);
	// Log("Got \"%d\" packet from (%d)", packet_id, client->id);
	delete[] buffer;

	// LogDebug("Got packet with ID \"%d\" (%d bytes)", packet_id, data_length);

	switch((RT_PACKET_ID)packet_id)
	{
	case RT_PACKET_DISCONNECT:
		close_connection(client, false);
		break;
	default:
		switch(Settings::UnknownBehaviour)
		{
		case RT_BEHAVIOUR_ALL: send_all(data, data_length); break;
		case RT_BEHAVIOUR_SELF: send(client, data, data_length); break;
		default: case RT_BEHAVIOUR_OTHERS: send_others(client, data, data_length); break;
		}
		break;
	}
	delete[] data;
}

void close_connection(rt_client* client, bool send_packet)
{
	if(client == nullptr)
		return;
	if(send_packet)
		send(client, short_to_bytes((short)RT_PACKET_DISCONNECT).data(), 2);
	
	Log("(%d) \"%s:%d\" disconnected", client->id, client->address, client->port);
	clients.erase(clients.find(client->id));
	client = nullptr;
}

unsigned int RTServer::BytesInSec() { return bytesInSecFinal; }
unsigned int RTServer::BytesOutSec() { return bytesOutSecFinal; }

void timer_loop()
{
	stringstream ss;
	short milliseconds = 0;
	while(running)
	{
		if(milliseconds == 1000)
		{
			bytesInSecFinal = bytesInSec;
			bytesOutSecFinal = bytesOutSec;
			bytesInSec = 0;
			bytesOutSec = 0;

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