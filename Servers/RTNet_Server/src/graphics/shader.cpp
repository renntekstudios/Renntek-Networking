#include <fstream>
#include "logger.h"
#include "settings.h"
#include "graphics/shader.h"

#include <GL/glew.h>
#include <glm/glm.hpp>
#include <GLFW/glfw3.h>

using namespace RTNet;
using namespace RTNet::Graphics;

string defaultFragmentShader;
string defaultFragmentShaderSource;

GLuint programID;

string GetShaderSource(string& source);
bool CompileShader(string* stringSource, GLuint* shaderID);

Shader::Shader(string vertexShader) : Shader(vertexShader, defaultFragmentShader) { }
Shader::Shader(string vertexShader, string fragmentShader)
{	
	if(defaultFragmentShader.empty())
	{
		defaultFragmentShader = Settings::ShaderDir + "default.fs";
		defaultFragmentShaderSource = GetShaderSource(defaultFragmentShader);
	}

	vertexPath = vertexShader;
	fragmentPath = fragmentShader;

	GLuint vertexShaderID = glCreateShader(GL_VERTEX_SHADER);
	GLuint fragmentShaderID = glCreateShader(GL_FRAGMENT_SHADER);

	string vertexSource = GetShaderSource(vertexShader);
	string fragmentSource = GetShaderSource(fragmentShader);
	if(vertexSource.empty())
		return;
	if(fragmentSource.empty())
	{
		if(defaultFragmentShaderSource.empty())
			return;
		LogWarning("Could not get fragment shader source, falling back to '%s'", (fragmentShader = defaultFragmentShader).c_str());
		fragmentSource = defaultFragmentShaderSource;
	}

	LogDebug("Compiling \"%s\"", vertexShader.c_str());
	if(!CompileShader(&vertexSource, &vertexShaderID))
		return;
	LogDebug("Compiling \"%s\"", fragmentShader.c_str());
	if(!CompileShader(&fragmentSource, &fragmentShaderID))
		return;

	LogDebug("Linking shader program");
	programID = glCreateProgram();
	glAttachShader(programID, vertexShaderID);
	glAttachShader(programID, fragmentShaderID);
	glLinkProgram(programID);

	int infoLogLength;
	glGetProgramiv(programID, GL_INFO_LOG_LENGTH, &infoLogLength);
	if(infoLogLength > 0)
	{
		vector<char> errorMessage(infoLogLength + 1);
		glGetProgramInfoLog(programID, infoLogLength, NULL, &errorMessage[0]);
		LogWarning("Could not link shader program - %s", &errorMessage[0]);
		return;
	}

	glDetachShader(programID, vertexShaderID);
	glDetachShader(programID, fragmentShaderID);
	glDeleteShader(vertexShaderID);
	glDeleteShader(fragmentShaderID);
}

Shader::~Shader()
{

}

void Shader::Start() { glUseProgram(programID); }
void Shader::Stop() { glUseProgram(0); }

string GetShaderSource(string& source)
{
	string shaderCode;
	ifstream shaderStream(source, ios::in);
	if(shaderStream.is_open())
	{
		string line = "";
		while(getline(shaderStream, line))
			shaderCode += "\n" + line;
		shaderStream.close();
	}
	else
	{
		LogWarning("Could not open \"%s\" - file could not be opened", source.c_str());
		return "";
	}
	return shaderCode;
}

bool CompileShader(string* stringSource, GLuint* shaderID)
{
	const char* source = stringSource->c_str();
	glShaderSource(*shaderID, 1, &source, NULL);
	glCompileShader(*shaderID);

	int infoLogLength;
	glGetShaderiv(*shaderID, GL_INFO_LOG_LENGTH, &infoLogLength);
	if(infoLogLength > 0)
	{
		vector<char> errorMessage(infoLogLength + 1);
		glGetShaderInfoLog(*shaderID, infoLogLength, NULL, &errorMessage[0]);
		LogWarning("Could not compile shader - \"%s\"", &errorMessage[0]);
		return false;
	}
	return true;
}