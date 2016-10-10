#ifndef _PACKET_H_
#define _PACKET_H_
#include <vector>
#include <map>
#include "enums.h"
#include "logger.h"

using namespace std;

namespace RTNet
{
	typedef unsigned short rt_packet_id;
	
	struct unhandled_packet_t
	{
		rt_packet_id packet_id;
		unsigned short expected;
		map<int, vector<rt_byte>> bytes;

		void get_final_buffer(rt_byte* output)
		{
			if(bytes.empty())
			{
				output = nullptr;
				return;
			}
			vector<rt_byte> handled;
			for(unsigned int i = 0;i < expected;i++)
			{
				if(bytes.find(i) != bytes.end())
					handled.insert(bytes[i].begin(), bytes[i].end(), bytes[i].begin());
				else
					break;
			}
			if(bytes.size() != expected)
				LogWarning("Didn't get the expected amount of packets! (%d)", packet_id);
			bytes.clear();
			output = handled.data();
		}
	};
}

#endif