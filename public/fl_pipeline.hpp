#pragma once
#ifndef _FL_PIPELINE_H
#define _FL_PIPELINE_H

#include <vulkan/vulkan_core.h>

#include <string>
#include <vector>

namespace fl {

// A Pipeline handles the generic linear rendering pipeline:
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

    bool init(VkDevice logical);

private:
    // creates a graphics pipeline
    bool create_graphics(const std::string &vert_path, const std::string &frag_path);
    bool create_shader_module(const std::vector<char> *shader_code_ptr, VkShaderModule *module_ptr);

    const std::string _vert_path;
    const std::string _frag_path;

    VkDevice _logical_device = VK_NULL_HANDLE;
};

} // namespace fl

#endif // _FL_PIPELINE_H
