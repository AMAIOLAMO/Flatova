#include <fl_pipeline.hpp>
#include <fl_shader_utils.hpp>

#include <stdio.h>

namespace fl {

Pipeline::Pipeline(const std::string &vert_path, const std::string &frag_path)
    : _vert_path(vert_path), _frag_path(frag_path) {
}

Pipeline::~Pipeline() {
    
}

bool Pipeline::init(VkDevice logical) {
    _logical_device = logical;

    if(create_graphics(_vert_path, _frag_path) == false) {
        fprintf(stderr, "[Pipeline] failed create graphics pipeline\n");
    }

    return true;
}

bool Pipeline::create_graphics(const std::string &vert_path, const std::string &frag_path) {
    std::vector<char> vert_shader{};
    std::vector<char> frag_shader{};

    if(read_compiled_shader(vert_path, &vert_shader) == false)
        return false;

    if(read_compiled_shader(frag_path, &frag_shader) == false)
        return false;

    printf("[Pipeline] Vertex Shader Code Size: %zu\n", vert_shader.size());
    printf("[Pipeline] Fragment Shader Code Size: %zu\n", frag_shader.size());

    VkShaderModule vert_module = VK_NULL_HANDLE;
    if(create_shader_module(&vert_shader, &vert_module) == false)
        return false;
    printf("[Pipeline] Vertex Shader Module created\n");

    VkShaderModule frag_module = VK_NULL_HANDLE;
    if(create_shader_module(&frag_shader, &frag_module) == false)
        return false;
    printf("[Pipeline] Fragment Shader Module created\n");


    VkPipelineShaderStageCreateInfo vert_shader_stage_info{};
    vert_shader_stage_info.sType = VK_STRUCTURE_TYPE_PIPELINE_SHADER_STAGE_CREATE_INFO;
    vert_shader_stage_info.stage = VK_SHADER_STAGE_VERTEX_BIT;
    vert_shader_stage_info.module = vert_module;
    vert_shader_stage_info.pName = "main"; // main entry name
    

    VkPipelineShaderStageCreateInfo frag_shader_stage_info{};
    frag_shader_stage_info.sType = VK_STRUCTURE_TYPE_PIPELINE_SHADER_STAGE_CREATE_INFO;
    frag_shader_stage_info.stage = VK_SHADER_STAGE_FRAGMENT_BIT;
    frag_shader_stage_info.module = frag_module;
    frag_shader_stage_info.pName = "main"; // main entry name




    vkDestroyShaderModule(_logical_device, vert_module, nullptr);
    vkDestroyShaderModule(_logical_device, frag_module, nullptr);

    return true;
}

bool Pipeline::create_shader_module(const std::vector<char> *shader_code_ptr, VkShaderModule *module_ptr) {
    VkShaderModuleCreateInfo create_info{};
    create_info.sType = VK_STRUCTURE_TYPE_SHADER_MODULE_CREATE_INFO;
    create_info.codeSize = shader_code_ptr->size();
    create_info.pCode = reinterpret_cast<const uint32_t*>(shader_code_ptr->data());

    return vkCreateShaderModule(_logical_device, &create_info, nullptr, module_ptr) == VK_SUCCESS;
}

} // namespace fl
