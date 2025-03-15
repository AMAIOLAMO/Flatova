#pragma once
#ifndef _FL_PIPELINE_H
#define _FL_PIPELINE_H

#include <vulkan/vulkan_core.h>
#include <glm/glm.hpp>

#include <fl_swapchain.hpp>

#include <string>
#include <vector>
#include <array>

namespace fl {

// A Pipeline wraps around the generic rendering pipeline provided by Vulkan:
// -> Vertex & Index Buffer
// 1. Input Assembler
// 2. Vertex Shader
// 3. Rasterization
// 4. Fragment Shader
// 5. Color Blending
// <- Frame Buffer(Img)

struct Vertex {
    glm::vec2 pos; 
    glm::vec3 color;

    // TODO: move this in an implementation file
    // this vertex binding describes at which rate to load data from memory
    // throughout the vertices. It specifies the number of bytes between data entries
    // and whether to move the next data entry after each vertex or after each instance. (vulkan-tutorial)
    static VkVertexInputBindingDescription get_binding_desc() {
        VkVertexInputBindingDescription desc{};

        desc.binding = 0;
        desc.stride = sizeof(Vertex);

        desc.inputRate = VK_VERTEX_INPUT_RATE_VERTEX; // send information per vertex

        return desc;
    }

    static std::array<VkVertexInputAttributeDescription, 2> get_attr_descs() {
        std::array<VkVertexInputAttributeDescription, 2> descs{};

        descs[0].binding  = 0;
        descs[0].location = 0;
        descs[0].format = VK_FORMAT_R32G32_SFLOAT;
        descs[0].offset = offsetof(Vertex, pos);

        descs[1].binding  = 0;
        descs[1].location = 1;
        descs[1].format = VK_FORMAT_R32G32B32_SFLOAT;
        descs[1].offset = offsetof(Vertex, color);

        return descs;
    }
};


class Pipeline {
public:
    Pipeline(const std::string &vert_path, const std::string &frag_path);
    ~Pipeline();

    Pipeline(Pipeline&) = delete;
    Pipeline& operator=(Pipeline&) = delete;

    bool init(VkDevice logical, Swapchain *swap_chain_ptr, VkRenderPass render_pass,
                    VkViewport *p_viewport, VkRect2D *p_scissor);

    VkPipeline get_raw_graphics_handle() const;

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

    VkViewport *_p_viewport;
    VkRect2D   *_p_scissor;

    VkDevice _logical_device = VK_NULL_HANDLE;
};

} // namespace fl

#endif // _FL_PIPELINE_H
