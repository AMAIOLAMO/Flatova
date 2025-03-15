#pragma once
#ifndef _FL_APPLICATION_H
#define _FL_APPLICATION_H

#include <fl_pipeline.hpp>
#include <fl_vk_core.hpp>

#include <string>

#include <vulkan/vulkan.h>

struct GLFWwindow;

namespace fl {

const int MAX_FRAMES_IN_FLIGHT = 2;

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
    int init_glfw_window();

    bool recreate_swap_chain_and_views();

    void set_viewport_extents_scissors(VkExtent2D extent);

    bool setup_swap_chain_views();

    bool setup_swap_chain_frame_buffers();

    bool setup_render_pass(Swapchain *swap_chain_ptr, VkDevice device);

    bool setup_command_pool();
    bool setup_command_buffers();

    bool record_command_buffer(VkCommandBuffer cmd_buf, uint32_t img_idx);

    bool setup_synchronize_objs();

    bool setup_vertex_buffer();

    bool alloc_bind_vertex_buffer_mem();

    bool draw_frame();
    
    void destroy_views_and_frame_buffers();

    bool find_mem_type(uint32_t type_filter, VkMemoryPropertyFlags props, uint32_t *mem_type_ptr);


    VkViewport _viewport;
    VkRect2D   _scissor;

    std::vector<VkSemaphore> _img_avail_semas;
    std::vector<VkSemaphore> _render_fin_semas;
    std::vector<VkFence>     _rendering_fences;

    VkRenderPass _render_pass = VK_NULL_HANDLE;

    std::vector<VkImageView>   _swpchn_views{};
    std::vector<VkImage>       _swpchn_imgs{};
    std::vector<VkFramebuffer> _swpchn_frame_buffers{};

    VkCommandPool _cmd_pool = VK_NULL_HANDLE;
    std::vector<VkCommandBuffer> _cmd_buffers;

    const std::vector<Vertex> _verticies {
        { {-.5f, -.5f}, {1.f, .0f, .0f} },
        { {.5f,  -.5f}, {1.f, 1.f, 1.f} },
        { {-.5f,  .5f}, {0.f, 1.f, .0f} },

        { {.5f, -.5f}, {1.f, 1.f, 1.f} },
        { {.5f,  .5f}, {0.f, 0.f, 1.f} },
        { {-.5f, .5f}, {0.f, 1.f, .0f} }
    };

    VkMemoryRequirements _vert_buf_mem_req;


    VkBuffer _vertex_buf = VK_NULL_HANDLE;
    VkDeviceMemory _vertex_buf_mem;
    void *_vertex_buf_mapped;

    size_t _current_frame = 0;

    #ifdef NDEBUG
        const bool _enable_validation_layers = false;
    #else
        const bool _enable_validation_layers = true;
    #endif

    int _width, _height;
    std::string _name;

    GLFWwindow *_win_ptr = nullptr;

    VkCore _vk_core;

    Pipeline _pipeline {
        "vendor/shaders/demo_shader.vert.spv",
        "vendor/shaders/demo_shader.frag.spv"
    };
};




} // namespace fl

#endif // _FL_APPLICATION_H
