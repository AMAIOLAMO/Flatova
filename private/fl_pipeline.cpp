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
    vkDestroyRenderPass(_logical_device, _render_pass, nullptr);
}

bool Pipeline::init(VkDevice logical, Swapchain *swap_chain_ptr) {
    _logical_device = logical;
    _swap_chain_ptr = swap_chain_ptr;

    VkPipelineLayoutCreateInfo layout_create_info{};
    layout_create_info.sType = VK_STRUCTURE_TYPE_PIPELINE_LAYOUT_CREATE_INFO;

    if(vkCreatePipelineLayout(logical, &layout_create_info, nullptr, &_layout) != VK_SUCCESS) {
        fprintf(stderr, "[Pipeline] failed to create pipeline layout\n");
        return false;
    }

    if(create_render_pass() == false) {
        fprintf(stderr, "[Pipeline] failed to create render pass\n");
        return false;
    }

    if(create_graphics(_vert_path, _frag_path) == false) {
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

    
    // PROGRAMMABLE FUNCTION STAGES
    VkPipelineVertexInputStateCreateInfo vert_input_info{};
    vert_input_info.sType = VK_STRUCTURE_TYPE_PIPELINE_VERTEX_INPUT_STATE_CREATE_INFO;
    vert_input_info.vertexBindingDescriptionCount = 0; // dont pass any info into vertex yet
    vert_input_info.vertexAttributeDescriptionCount = 0;

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

bool Pipeline::create_render_pass() {
    // define color attachment for swapchain rendering
    VkAttachmentDescription color_attach{};

    color_attach.format  = _swap_chain_ptr->get_img_format();
    color_attach.samples = VK_SAMPLE_COUNT_1_BIT;

    color_attach.loadOp  = VK_ATTACHMENT_LOAD_OP_CLEAR; // before rendering
    color_attach.storeOp = VK_ATTACHMENT_STORE_OP_STORE; // after rendering

    color_attach.stencilLoadOp  = VK_ATTACHMENT_LOAD_OP_DONT_CARE;
    color_attach.stencilStoreOp = VK_ATTACHMENT_STORE_OP_DONT_CARE;

    // before render pass
    color_attach.initialLayout = VK_IMAGE_LAYOUT_UNDEFINED;
    // after render pass
    color_attach.finalLayout   = VK_IMAGE_LAYOUT_PRESENT_SRC_KHR; // images need to be transitioned into specific layouts


    VkAttachmentReference color_attach_ref{};

    color_attach_ref.attachment = 0; // index in the attachment description array
    color_attach_ref.layout = VK_IMAGE_LAYOUT_COLOR_ATTACHMENT_OPTIMAL;

    // create basic triangle subpass
    VkSubpassDescription sub_pass{};

    sub_pass.pipelineBindPoint = VK_PIPELINE_BIND_POINT_GRAPHICS;
    sub_pass.colorAttachmentCount = 1;
    sub_pass.pColorAttachments = &color_attach_ref; // direct reference of layout(location = 0) out vec4 outColor fragment shader!

    VkRenderPassCreateInfo render_pass_info{};

    render_pass_info.sType = VK_STRUCTURE_TYPE_RENDER_PASS_CREATE_INFO;
    render_pass_info.subpassCount = 1;
    render_pass_info.pSubpasses = &sub_pass;

    render_pass_info.attachmentCount = 1;
    render_pass_info.pAttachments = &color_attach;

    return vkCreateRenderPass(_logical_device, &render_pass_info, nullptr, &_render_pass) == VK_SUCCESS;
}

} // namespace fl
