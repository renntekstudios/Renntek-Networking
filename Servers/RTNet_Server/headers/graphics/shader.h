#ifndef _SHADER_H_
#define _SHADER_H_
#include <string.h>
#include "logger.h"

using namespace std;

namespace RTNet
{
	namespace Graphics
	{
		class Shader
		{
		public:
			Shader(string vertexShader);
			Shader(string vertexShader, string fragmentShader);
			~Shader();

			void Start();
			void Stop();

		private:
			string vertexPath, fragmentPath;
		};
	}
}

#endif