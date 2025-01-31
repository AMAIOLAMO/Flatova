#include <fl_pipeline.hpp>
#include <fl_shader_utils.hpp>
#include <fl_swapchain.hpp>

#include <stdio.h>

namespace fl {

Pipeline::Pipeline(const std::string &vert_path, const std::string &frag_path)
    : _vert_path(vert_path), _frag_path(frag_path) {
}

Pipeline::~Pipeline() {
    vkDestroyPipelineLayout(_logical_device, _layout, nullptr);
    vkDestroyPipeline(_logical_device, _graphics, nullptr);
}

bool Pipeline::init(VkDevice logical, Swapchain *swap_chain_ptr, VkRenderPass render_pass) {
    _logical_device = logical;
    _swap_chain_ptr = swap_chain_ptr;

    VkPipelineLayoutCreateInfo layout_create_info{};
    layout_create_info.sType = VK_STRUCTURE_TYPE_PIPELINE_LAYOUT_CREATE_INFO;

    if(vkCreatePipelineLayout(logical, &layout_create_info, nullptr, &_layout) != VK_SUCCESS) {
        fprintf(stderr, "[Pipeline] failed to create pipeline layout\n");
        return false;
    }

    if(create_graphics(render_pass, _vert_path, _frag_path) == false) {
        fprintf(stderr, "[Pipeline] failed create graphics pipeline\n");
        return false;
    }

    VkExtent2D extent = _swap_chain_ptr->get_img_extent();

    _viewport.x = 0.0f;
    _viewport.y = 0.0f;
    _viewport.width = (float) extent.width;
    _viewport.height = (float) extent.height;
    _viewport.minDepth = 0.0f;
    _viewport.maxDepth = 1.0f;

    _scissor.extent = extent;
    _scissor.offset = {0, 0};

    return true;
}

bool Pipeline::create_graphics(VkRenderPass render_pass,
                               const std::string &vert_path, const std::string &frag_path) {
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

    
    // PROGRAMMABLE FUNCTION STAGES
    VkPipelineVertexInputStateCreateInfo vert_input_state{};
    vert_input_state.sType = VK_STRUCTURE_TYPE_PIPELINE_VERTEX_INPUT_STATE_CREATE_INFO;
    vert_input_state.vertexBindingDescriptionCount = 0; // dont pass any info into vertex yet
    vert_input_state.vertexAttributeDescriptionCount = 0;

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
    

    // FIXED FUNCTION STAGES
    VkPipelineDynamicStateCreateInfo dynamic_state{};
    dynamic_state.sType = VK_STRUCTURE_TYPE_PIPELINE_DYNAMIC_STATE_CREATE_INFO;
    // defines what kind of dynamic states within the pipeline we want
    dynamic_state.dynamicStateCount = static_cast<uint32_t>(_dynamic_states.size());
    dynamic_state.pDynamicStates = _dynamic_states.data();

    VkPipelineInputAssemblyStateCreateInfo in_assembly_state{};
    in_assembly_state.sType = VK_STRUCTURE_TYPE_PIPELINE_INPUT_ASSEMBLY_STATE_CREATE_INFO;
    in_assembly_state.topology = VK_PRIMITIVE_TOPOLOGY_TRIANGLE_LIST; // load without reuse of the vertex data
    in_assembly_state.primitiveRestartEnable = VK_FALSE; // we dont need to restart the primitive topology using special values

    VkPipelineViewportStateCreateInfo viewport_state{};
    viewport_state.sType = VK_STRUCTURE_TYPE_PIPELINE_VIEWPORT_STATE_CREATE_INFO;
    viewport_state.viewportCount = 1;
    viewport_state.scissorCount = 1;
    viewport_state.pViewports = &_viewport;
    viewport_state.pScissors = &_scissor;

    VkPipelineRasterizationStateCreateInfo raster_state{};
    raster_state.sType = VK_STRUCTURE_TYPE_PIPELINE_RASTERIZATION_STATE_CREATE_INFO;
    raster_state.depthClampEnable = VK_FALSE; // clamp far or near fragments that is considered irrelevant
    raster_state.rasterizerDiscardEnable = VK_FALSE; // whether or not to disable any geometry passthrough (which ignores everything)
    raster_state.polygonMode = VK_POLYGON_MODE_FILL;
    raster_state.lineWidth = 1.0f; // default line width line thickness based on number of fragments
    raster_state.cullMode = VK_CULL_MODE_NONE; // TODO: disable this when finished rendering basic things, just for debug reasons
    raster_state.frontFace = VK_FRONT_FACE_CLOCKWISE;
    raster_state.depthBiasEnable = VK_FALSE;

    VkPipelineMultisampleStateCreateInfo multisample_state{}; // for now no need of multi sampling(Anti Aliasing)
    multisample_state.sType = VK_STRUCTURE_TYPE_PIPELINE_MULTISAMPLE_STATE_CREATE_INFO;
    multisample_state.sampleShadingEnable = VK_FALSE;
    multisample_state.rasterizationSamples = VK_SAMPLE_COUNT_1_BIT;


    VkPipelineColorBlendAttachmentState color_blend_attachment{};
    color_blend_attachment.colorWriteMask =
        VK_COLOR_COMPONENT_R_BIT | VK_COLOR_COMPONENT_G_BIT |
        VK_COLOR_COMPONENT_B_BIT | VK_COLOR_COMPONENT_A_BIT;
    color_blend_attachment.blendEnable = VK_FALSE; // disable color blending for now
    

    VkPipelineColorBlendStateCreateInfo color_blend_state{};
    color_blend_state.sType = VK_STRUCTURE_TYPE_PIPELINE_COLOR_BLEND_STATE_CREATE_INFO;
    color_blend_state.logicOpEnable = VK_FALSE; // since color blending is disabled for now, this doesnt need to be enabled
    color_blend_state.attachmentCount = 1;
    color_blend_state.pAttachments = &color_blend_attachment;

    VkPipelineShaderStageCreateInfo shader_stages[] = {
        vert_shader_stage_info, frag_shader_stage_info
    };

    VkGraphicsPipelineCreateInfo pipeline_info{};
    pipeline_info.sType = VK_STRUCTURE_TYPE_GRAPHICS_PIPELINE_CREATE_INFO;

    // programmable functions
    pipeline_info.pVertexInputState = &vert_input_state;
    pipeline_info.stageCount = 2;
    pipeline_info.pStages = shader_stages;

    // fixed functions states
    pipeline_info.pDynamicState = &dynamic_state;
    pipeline_info.pViewportState = &viewport_state;

    pipeline_info.pInputAssemblyState = &in_assembly_state;

    pipeline_info.pDepthStencilState = nullptr;
    pipeline_info.pRasterizationState = &raster_state;
    pipeline_info.pMultisampleState = &multisample_state;

    pipeline_info.pColorBlendState = &color_blend_state;

    pipeline_info.layout = _layout;

    pipeline_info.renderPass = render_pass;
    pipeline_info.subpass = 0;


    pipeline_info.basePipelineHandle = nullptr;
    pipeline_info.basePipelineIndex = -1;

    if(vkCreateGraphicsPipelines(_logical_device, VK_NULL_HANDLE,
                                 1, &pipeline_info, nullptr, &_graphics) != VK_SUCCESS) {
        vkDestroyShaderModule(_logical_device, vert_module, nullptr);
        vkDestroyShaderModule(_logical_device, frag_module, nullptr);
        return false;
    }

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
