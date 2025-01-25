#include <fl_application.hpp>

#include <cstring>
#include <GLFW/glfw3.h>

namespace fl {

Application::Application(int width, int height, const std::string &name)
    : _width(width), _height(height), _name(name), _win_ptr(nullptr), _vk_core(_enable_validation_layers) {
    glfwInit();

    glfwWindowHint(GLFW_CLIENT_API, GLFW_NO_API);
    glfwWindowHint(GLFW_RESIZABLE, GLFW_FALSE);


    printf("[Application] Initialized\n");
}

void Application::init() {
    init_glfw_window();

    _vk_core.init(_name, _win_ptr);
}

int Application::init_glfw_window() {
    _win_ptr = glfwCreateWindow(_width, _height, _name.c_str(), NULL, NULL);
    
    if(_win_ptr == nullptr)
        return EXIT_FAILURE;

    return EXIT_SUCCESS;
}

int Application::run() {
    glfwMakeContextCurrent(_win_ptr);

    while(!glfwWindowShouldClose(_win_ptr)) {
        glClear(GL_COLOR_BUFFER_BIT);

        glfwSwapBuffers(_win_ptr);

        glfwPollEvents();
    }
    
    return EXIT_SUCCESS;
}

Application::~Application() {
    glfwDestroyWindow(_win_ptr);
    glfwTerminate();

    printf("[Application] Clean up\n");
}



} // namespace fl

