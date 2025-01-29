#include <fl_application.hpp>

#include <cstring>
#include <GLFW/glfw3.h>

namespace fl {

#define app_info(...) do { printf("[Application] \033[0;37mINFO: " __VA_ARGS__); printf("\033[0m\n"); } while(0)
#define app_err(...) do { fprintf(stderr, "[Application] \033[0;31mERROR: " __VA_ARGS__); fprintf(stderr, "\033[0m\n"); } while(0)

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

    _vk_core.get_swap_chain_images(&_swap_chain_imgs);
    
    app_info("got %zu amount of swap chain images!", _swap_chain_imgs.size());

    if(setup_swap_chain_views())
        app_info("setup swap chain image views success!");
    else
        app_err("Failed setup swap chain image views!");

    VkDevice logical_device = _vk_core.get_device_manager_ptr()->get_logical();
    VkExtent2D swap_chain_extent = _vk_core.get_swap_chain_extent();
    
    if(_pipeline.init(logical_device, swap_chain_extent))
        app_info("Pipeline initialization complete");
    else
        app_err("Pipeline initialization failed");
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

bool Application::setup_swap_chain_views() {
    uint32_t img_count = static_cast<uint32_t>(_swap_chain_imgs.size());

    _swap_chain_views.resize(img_count);
    
    VkDeviceManager *device_manager_ptr = _vk_core.get_device_manager_ptr();

    for (size_t i = 0; i < img_count; i++) {
        VkDevice logical_device = device_manager_ptr->get_logical();
        VkImageViewCreateInfo create_info{};
        create_info.sType = VK_STRUCTURE_TYPE_IMAGE_VIEW_CREATE_INFO;
        create_info.image = _swap_chain_imgs[i];
        create_info.format = _vk_core.get_chosen_img_format();
        create_info.viewType = VK_IMAGE_VIEW_TYPE_2D;

        create_info.components.r = VK_COMPONENT_SWIZZLE_IDENTITY;
        create_info.components.g = VK_COMPONENT_SWIZZLE_IDENTITY;
        create_info.components.b = VK_COMPONENT_SWIZZLE_IDENTITY;
        create_info.components.a = VK_COMPONENT_SWIZZLE_IDENTITY;

        create_info.subresourceRange.aspectMask = VK_IMAGE_ASPECT_COLOR_BIT;
        create_info.subresourceRange.baseMipLevel = 0;
        create_info.subresourceRange.levelCount = 1;
        create_info.subresourceRange.baseArrayLayer = 0;
        create_info.subresourceRange.layerCount = 1;

        if(vkCreateImageView(logical_device, &create_info, nullptr, &_swap_chain_views[i]) != VK_SUCCESS)
            return false;
    }

    return true;
}

Application::~Application() {
    glfwDestroyWindow(_win_ptr);
    glfwTerminate();

    VkDeviceManager *device_manager_ptr = _vk_core.get_device_manager_ptr();

    for(auto view : _swap_chain_views)
        if(view != VK_NULL_HANDLE)
            vkDestroyImageView(device_manager_ptr->get_logical(), view, nullptr);

    app_info("Clean up");
}



} // namespace fl

