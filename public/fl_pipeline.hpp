#pragma once
#ifndef _FL_PIPELINE_H
#define _FL_PIPELINE_H

#include <vulkan/vulkan_core.h>
#include <fl_swapchain.hpp>

#include <string>
#include <vector>

namespace fl {

// A Pipeline wraps around the generic rendering pipeline provided by Vulkan:
// -> Vertex & Index Buffer
// 1. Input Assembler
// 2. Vertex Shader
// 3. Rasterization
// 4. Fragment Shader
// 5. Color Blending
// <- Frame Buffer(Img)
class Pipeline {
public:
    Pipeline(const std::string &vert_path, const std::string &frag_path);
    ~Pipeline();

    Pipeline(Pipeline&) = delete;
    Pipeline& operator=(Pipeline&) = delete;

    bool init(VkDevice logical, Swapchain *swap_chain_ptr, VkRenderPass render_pass);

    VkPipeline get_raw_graphics_handle() const;
    
    VkViewport& get_viewport_ref();
    VkRect2D& get_scissor_ref();

private:
    // creates a graphics pipeline
    bool create_graphics(VkRenderPass render_pass,
                         const std::string &vert_path, const std::string &frag_path);
    bool create_shader_module(const std::vector<char> *shader_code_ptr, VkShaderModule *module_ptr);
    bool create_render_pass();

    std::vector<VkDynamicState> _dynamic_states = {
        VK_DYNAMIC_STATE_VIEWPORT,
        VK_DYNAMIC_STATE_SCISSOR
    };

    const std::string _vert_path;
    const std::string _frag_path;


    Swapchain *_swap_chain_ptr = nullptr;

    // layout of the uniformed values passed into the vertex and fragment shaders
    VkPipelineLayout _layout = VK_NULL_HANDLE;

    VkPipeline _graphics = VK_NULL_HANDLE;

    VkViewport _viewport;
    VkRect2D   _scissor;

    VkDevice _logical_device = VK_NULL_HANDLE;
};

} // namespace fl

#endif // _FL_PIPELINE_H
