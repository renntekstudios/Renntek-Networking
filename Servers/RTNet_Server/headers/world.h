#ifndef _WORLD_H_
#define _WORLD_H_

namespace RTNet
{
	class World
	{
	public:
		World();
		~World();

		void SetTitle(const char* title);

	private:
		void InitOpenGL();
	};
}

#endif