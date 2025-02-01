#include <fl_application.hpp>

#include <cstring>
#include <GLFW/glfw3.h>

namespace fl {

#define app_info(...) do { printf("[Application] \033[0;37mINFO: " __VA_ARGS__); printf("\033[0m\n"); } while(0)
#define app_err(...) do { fprintf(stderr, "[Application] \033[0;31mERROR: " __VA_ARGS__); fprintf(stderr, "\033[0m\n"); } while(0)

#define action_check(FUNC, MSG) do { \
    if((FUNC) == false) { \
        app_err("ACTION \"" MSG "\" FAILED"); \
        return false; \
    } \
    app_info("ACTION \"" MSG "\" SUCCESS"); \
} while(0)

Application::Application(int width, int height, const std::string &name)
    : _width(width), _height(height), _name(name), _win_ptr(nullptr), _vk_core(_enable_validation_layers) {
    glfwInit();

    glfwWindowHint(GLFW_CLIENT_API, GLFW_NO_API);
    glfwWindowHint(GLFW_RESIZABLE, GLFW_FALSE);

    app_info("glfw initialized");
}

Application::~Application() {
    VkDeviceManager *device_manager_ptr = _vk_core.get_device_manager_ptr();
    VkDevice logical = device_manager_ptr->get_logical();

    for(size_t i = 0; i < _swpchn_imgs.size(); i++) {
        VkImageView img_view = _swpchn_views[i];
        vkDestroyImageView(device_manager_ptr->get_logical(), img_view, nullptr);

        VkFramebuffer frame_buffer = _swpchn_frame_buffers[i];
        vkDestroyFramebuffer(device_manager_ptr->get_logical(), frame_buffer, nullptr);
    }

    vkDestroyRenderPass(logical, _render_pass, nullptr);
    vkDestroyCommandPool(logical, _cmd_pool, nullptr);
    glfwDestroyWindow(_win_ptr);
    glfwTerminate();

    app_info("Clean up");
}

void Application::init() {
    init_glfw_window();

    _vk_core.init(_name, _win_ptr);
    _vk_core.get_swap_chain_ptr()->get_images(&_swpchn_imgs);
    
    app_info("got %zu amount of swap chain images!", _swpchn_imgs.size());

    if(setup_swap_chain_views())
        app_info("setup swap chain image views success!");
    else
        app_err("Failed setup swap chain image views!");

    Swapchain *swpchn_ptr = _vk_core.get_swap_chain_ptr();
    VkDevice logical_device = _vk_core.get_device_manager_ptr()->get_logical();

    if(setup_render_pass(swpchn_ptr, logical_device))
        app_info("Create render pass success!");
    else
        app_err("create render pass failed!");

    
    if(_pipeline.init(logical_device, swpchn_ptr, _render_pass))
        app_info("Pipeline initialization complete");
    else
        app_err("Pipeline initialization failed");

    _swpchn_frame_buffers.resize(_swpchn_views.size());

    for(size_t i = 0; i < _swpchn_views.size(); i++) {
        VkFramebufferCreateInfo fb_create_info{};
        fb_create_info.sType = VK_STRUCTURE_TYPE_FRAMEBUFFER_CREATE_INFO;
        fb_create_info.renderPass = _render_pass;
        fb_create_info.attachmentCount = 1;
        fb_create_info.pAttachments = &_swpchn_views[i];

        VkExtent2D extent = swpchn_ptr->get_img_extent();

        fb_create_info.width = extent.width;
        fb_create_info.height = extent.height;
        fb_create_info.layers = 1; // single layered images

        if(vkCreateFramebuffer(logical_device, &fb_create_info, nullptr, &_swpchn_frame_buffers[i]) != VK_SUCCESS)
            app_err("creating frame buffer for frame view at index %zu failed!", i);
    }

    if(setup_command_pool())
        app_info("Create command pool success!");
    else
        app_err("create command pool failed!");

    if(setup_command_buffer())
        app_info("Create command buffer success!");
    else
        app_err("create command buffer failed!");
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
    uint32_t img_count = static_cast<uint32_t>(_swpchn_imgs.size());

    _swpchn_views.resize(img_count);
    
    VkDeviceManager *device_manager_ptr = _vk_core.get_device_manager_ptr();

    for (size_t i = 0; i < img_count; i++) {
        VkDevice logical_device = device_manager_ptr->get_logical();
        VkImageViewCreateInfo create_info{};
        create_info.sType = VK_STRUCTURE_TYPE_IMAGE_VIEW_CREATE_INFO;
        create_info.image = _swpchn_imgs[i];
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

        if(vkCreateImageView(logical_device, &create_info, nullptr, &_swpchn_views[i]) != VK_SUCCESS)
            return false;
    }

    return true;
}

bool Application::setup_render_pass(Swapchain *swap_chain_ptr, VkDevice device) {
    // define color attachment for swapchain rendering
    VkAttachmentDescription color_attach{};
    color_attach.format  = swap_chain_ptr->get_img_format();
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

    return vkCreateRenderPass(device, &render_pass_info, nullptr, &_render_pass) == VK_SUCCESS;
}

bool Application::setup_command_pool() {
    const QueueFamilyIdxs *idxs_ptr = _vk_core.get_queue_family_idxs_ptr();
    VkDevice logical_device = _vk_core.get_device_manager_ptr()->get_logical();

    VkCommandPoolCreateInfo create_info{};
    create_info.sType = VK_STRUCTURE_TYPE_COMMAND_POOL_CREATE_INFO;
    create_info.flags = VK_COMMAND_POOL_CREATE_RESET_COMMAND_BUFFER_BIT; // we want to render many times, so we will clear and rerecord over it
    create_info.queueFamilyIndex = idxs_ptr->graphics.value();

    return vkCreateCommandPool(logical_device, &create_info, nullptr, &_cmd_pool) == VK_SUCCESS;
}


bool Application::setup_command_buffer() {
    VkCommandBufferAllocateInfo alloc_info{};
    VkDevice logical_device = _vk_core.get_device_manager_ptr()->get_logical();

    alloc_info.sType = VK_STRUCTURE_TYPE_COMMAND_BUFFER_ALLOCATE_INFO;

    alloc_info.commandPool = _cmd_pool;
    alloc_info.commandBufferCount = 1;
    alloc_info.level = VK_COMMAND_BUFFER_LEVEL_PRIMARY;

    return vkAllocateCommandBuffers(logical_device, &alloc_info, &_cmd_buffer) == VK_SUCCESS;
}


} // namespace fl

