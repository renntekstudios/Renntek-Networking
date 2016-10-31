#ifndef _PACKET_H_
#define _PACKET_H_
#include <vector>
#include <map>
#include "enums.h"
#include "logger.h"

using namespace std;

namespace RTNet
{
	typedef unsigned char rt_packet_id;
	
	struct unhandled_packet_t
	{
		rt_packet_id packet_id;
		unsigned short expected;
		map<int, vector<rt_byte>> bytes;

		void get_final_buffer(rt_byte** output, unsigned int* size)
		{
			if(bytes.empty())
			{
				*output = nullptr;
				*size = 0;
				return;
			}
			vector<rt_byte>* handled = new vector<rt_byte>();
			unsigned int i;
			for(i = 0; i < expected; i++)
			{
				if(bytes.find(i) == bytes.end())
				{
					LogDebug("[WARNING] Couldn't find %d", i);
					break;
				}
				handled->reserve(bytes[i].size());
				handled->insert(handled->end(), bytes[i].begin(), bytes[i].end());
			}
			if(i != expected)
				LogWarning("Didn't get the expected amount of packets! Expected %d but got %d", expected, i);
			bytes.clear();
			*output = handled->data();
			*size = handled->size();
		}
	};
}

#endif