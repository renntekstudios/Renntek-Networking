#ifndef _RT_EVENT_H_
#define _RT_EVENT_H_
#include <vector>
#include <algorithm>

using namespace std;

namespace RTNet
{
	class RTEvent
	{
	friend class RTNetClient;
	private:
		vector<void (*)(void*)> functions;
		void fire(void* args) { for(unsigned int i = 0; i < functions.size(); i++) functions[i](args); }
	public:
		RTEvent();
		~RTEvent();

		void add(void (*function)(void*)) { functions.push_back(function); }
		void remove(void (*function)(void*))
		{
			vector<void (*)(void*)>::iterator it = find(functions.begin(), functions.end(), function);
			if(it != functions.end())
				functions.erase(it);
		}
		void clear() { functions.clear(); }
	};
}

#endif