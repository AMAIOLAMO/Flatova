#include <fl_application.hpp>

#include <cstring>
#include <GLFW/glfw3.h>

namespace fl {

Application::Application(int width, int height, const std::string &name)
    : _width(width), _height(height), _name(name), _win(nullptr), _vk_core(_enable_validation_layers) {
    glfwInit();

    glfwWindowHint(GLFW_CLIENT_API, GLFW_NO_API);
    glfwWindowHint(GLFW_RESIZABLE, GLFW_FALSE);


    printf("[Application] Initialized\n");
}

void Application::init() {
    _vk_core.init(_name);
}

int Application::run() {
    _win = glfwCreateWindow(_width, _height, _name.c_str(), NULL, NULL);

    if(_win == nullptr)
        return EXIT_FAILURE;

    glfwMakeContextCurrent(_win);

    while(!glfwWindowShouldClose(_win)) {
        glClear(GL_COLOR_BUFFER_BIT);

        glfwSwapBuffers(_win);

        glfwPollEvents();
    }
    
    return EXIT_SUCCESS;
}

Application::~Application() {
    glfwDestroyWindow(_win);
    glfwTerminate();

    printf("[Application] Clean up\n");
}



} // namespace fl

