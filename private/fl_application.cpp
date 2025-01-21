#include "fl_vulkan_utils.hpp"
#include <cstring>
#include <fl_application.hpp>

#include <GLFW/glfw3.h>

namespace fl {

Application::Application(int width, int height, const std::string &name)
    : _width(width), _height(height), _name(name), _win(nullptr) {
    glfwInit();

    glfwWindowHint(GLFW_CLIENT_API, GLFW_NO_API);
    glfwWindowHint(GLFW_RESIZABLE, GLFW_FALSE);

    if(create_glfw_vulkan_instance(&_instance))
        printf("[Application] Vulkan instance created\n");
    else
        printf("[Application] Vulkan instance create failed\n");

    printf("[Application] Initialized\n");
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
    vkDestroyInstance(_instance, nullptr);

    glfwDestroyWindow(_win);
    glfwTerminate();

    printf("[Application] Clean up\n");
}

bool Application::create_glfw_vulkan_instance(VkInstance *instance_ptr) {
    log_glfw_required_extensions_support();

    VkApplicationInfo app_info{};
    app_info.sType = VK_STRUCTURE_TYPE_APPLICATION_INFO;
    app_info.pApplicationName = _name.c_str();
    app_info.applicationVersion = VK_MAKE_VERSION(0, 0, 1);
    app_info.pEngineName = "No Engine";
    app_info.engineVersion = VK_MAKE_VERSION(1, 0, 0);
    app_info.apiVersion = VK_API_VERSION_1_0;


    VkInstanceCreateInfo create_info{};
    create_info.sType = VK_STRUCTURE_TYPE_INSTANCE_CREATE_INFO;
    create_info.pApplicationInfo = &app_info;

    uint32_t glfw_extensions_count = 0;
    const char **glfw_extensions = glfwGetRequiredInstanceExtensions(&glfw_extensions_count);


    create_info.enabledExtensionCount = glfw_extensions_count;
    create_info.ppEnabledExtensionNames = glfw_extensions;

    // used for global validation layers etc
    create_info.enabledLayerCount = 0;

    return vkCreateInstance(&create_info, nullptr, instance_ptr) == VK_SUCCESS;
}


} // namespace fl

