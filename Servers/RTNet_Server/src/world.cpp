#include "world.h"
#include "logger.h"
#include "settings.h"
#include "graphics/shader.h"

#ifdef PLATFORM_WINDOWS
#include "win_threading.h"
#else
#include <pthread.h>
#endif

#include <GL/glew.h>
#include <glm/glm.hpp>
#include <GLFW/glfw3.h>

using namespace glm;
using namespace RTNet;
using namespace RTNet::Graphics;

thread* displayThread;
GLFWwindow* window;
Shader* shader;

void Display();

World::World()
{
	LogDebug("InitOpenGL");
	InitOpenGL();
}

void World::InitOpenGL()
{
	LogDebug("Initializing GLFW");
	if(!glfwInit())
	{
		LogError("Could not initialize GLFW");
		return;
	}

	glfwWindowHint(GLFW_SAMPLES, 4);
	glfwWindowHint(GLFW_CONTEXT_VERSION_MAJOR, 3);
	glfwWindowHint(GLFW_CONTEXT_VERSION_MINOR, 3);
	glfwWindowHint(GLFW_OPENGL_FORWARD_COMPAT, GL_TRUE);
	glfwWindowHint(GLFW_OPENGL_PROFILE, GLFW_OPENGL_CORE_PROFILE);

	displayThread = new thread(Display);
}

World::~World()
{
	if(window != nullptr)
		glfwSetWindowShouldClose(window, GLFW_TRUE);
	delete displayThread;

	shader->Stop();
	glfwTerminate();

	delete shader;
}

void Display()
{
	LogDebug("Creating window");
	window = glfwCreateWindow(648, 480, "RTNet Server", NULL, NULL);
	if(!window)
	{
		LogError("Could not create GLFW window");
		glfwTerminate();
		return;
	}

	glfwMakeContextCurrent(window);
	glewExperimental = GL_TRUE;
	LogDebug("Initializing GLEW");
	if(glewInit() != GLEW_OK)
	{
		LogError("Failed to initialize GLEW");
		glfwTerminate();
		return;
	}

	/** SETUP TRIANGLE **/
	static const GLfloat vertex_buffer_data[] = 
	{
		-1.0f, -1.0f, 0.0f,
		1.0f, -1.0f, 0.0f,
		0.0f,  1.0f, 0.0f,
	};

	GLuint vao, vbo;
	glGenVertexArrays(1, &vao);
	glBindVertexArray(vao);
	glGenBuffers(1, &vbo);
	glBindBuffer(GL_ARRAY_BUFFER, vbo);
	glBufferData(GL_ARRAY_BUFFER, sizeof(vertex_buffer_data), vertex_buffer_data, GL_STATIC_DRAW);

	LogDebug("Creating shader");
	shader = new Shader(Settings::ShaderDir + "default.vs");

	/** DRAWING LOOP **/
	glfwSetInputMode(window, GLFW_STICKY_KEYS, GL_TRUE);
	do
	{
		glClear(GL_COLOR_BUFFER_BIT | GL_DEPTH_BUFFER_BIT);

		shader->Start();

		glEnableVertexAttribArray(0);
		glBindBuffer(GL_ARRAY_BUFFER, vbo);
		glVertexAttribPointer(0, 3, GL_FLOAT, GL_FALSE, 0, (void*)0);
		glDrawArrays(GL_TRIANGLES, 0, 3);
		glDisableVertexAttribArray(0);

		shader->Stop();

		glfwSwapBuffers(window);
		glfwPollEvents();
	} while(glfwGetKey(window, GLFW_KEY_ESCAPE) != GLFW_PRESS && glfwWindowShouldClose(window) == 0);
	glfwDestroyWindow(window);
	window = nullptr;
	LogDebug("Destroyed window");
}

void World::SetTitle(const char* title)
{
	if(window != nullptr)
		glfwSetWindowTitle(window, title);
}