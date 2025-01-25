#pragma once
#ifndef _FL_APPLICATION_H
#define _FL_APPLICATION_H

#include <fl_pipeline.hpp>
#include <fl_vk_core.hpp>

#include <string>

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

    void init();

    int run();


private:
    int _width, _height;
    std::string _name;

    GLFWwindow *_win;

    Pipeline _pipeline {
        "vendor/shaders/demo_shader.vert.spv",
        "vendor/shaders/demo_shader.frag.spv"
    };


    #ifdef NDEBUG
        const bool _enable_validation_layers = false;
    #else
        const bool _enable_validation_layers = true;
    #endif


    VkCore _vk_core;
};


} // namespace fl

#endif // _FL_APPLICATION_H
