#pragma once
#ifndef _FL_APPLICATION_H
#define _FL_APPLICATION_H

#include <fl_pipeline.hpp>

#include <string>
#include <vector>

#include <vulkan/vulkan.h>

struct GLFWwindow;

namespace fl {

/// Application is an abstraction layer that handles the major loop and handles
/// both initializaztion and generation of the window, it is essentially the entire engine entry
class Application {
public:
    Application(int width, int height, const std::string &name);
    ~Application();

    Application(Application&) = delete;
    Application& operator=(const Application&) = delete;

    int run();

private:
    int _width, _height;
    std::string _name;

    /// creates the vulkan instance using the glfw windowing system
    bool create_glfw_vulkan_instance(VkInstance *instance_ptr);
    
    /// a helper function that returns the number of extension properties that's supported by your vulkan
    /// vendor.
    uint32_t get_vk_instance_extension_properties(std::vector<VkExtensionProperties> *properties_ptr);

    GLFWwindow *_win;

    Pipeline _pipeline {
        "vendor/shaders/demo_shader.vert.spv",
        "vendor/shaders/demo_shader.frag.spv"
    };

    VkInstance _instance;
};


} // namespace fl

#endif // _FL_APPLICATION_H
