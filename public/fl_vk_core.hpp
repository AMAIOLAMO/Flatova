#pragma once
#ifndef _FL_VK_MANAGER_H
#define _FL_VK_MANAGER_H

#include <fl_vk_device_manager.hpp>

#include <vulkan/vulkan.h>
#include <GLFW/glfw3.h>

#include <string>
#include <optional>
#include <vector>

namespace fl {

struct QueueFamilyIdxs {
    std::optional<uint32_t> graphics, present;
};


class VkCore {
public:
    VkCore(bool enable_debug);
    ~VkCore();

    VkCore(VkCore&) = delete;
    VkCore& operator=(VkCore&) = delete;

    bool init(std::string app_name, GLFWwindow *window_ptr);

private:
    bool setup_instance(std::string app_name);

    void setup_debug_messenger();
    
    bool setup_glfw_surface(GLFWwindow *window_ptr);

    bool check_device_extension_support(VkPhysicalDevice device);

    VkPhysicalDevice pick_physical_device();

    bool is_device_suitable(VkPhysicalDevice device);

    bool setup_logical_device();


    bool find_queue_families(QueueFamilyIdxs *idxs_ptr);

    void populate_debug_messenger_create_info(VkDebugUtilsMessengerCreateInfoEXT *info_ptr);



    VkInstance   _instance;
    VkSurfaceKHR _surface;

    VkDeviceManager *_device_manager_ptr;

    VkPhysicalDevice _physical_device = VK_NULL_HANDLE;
    VkDevice         _logical_device  = VK_NULL_HANDLE;

    QueueFamilyIdxs _queue_family_idxs;

    VkQueue _graphics_queue;
    VkQueue _present_queue;

    const std::vector<const char*> _device_req_extensions = {
        VK_KHR_SWAPCHAIN_EXTENSION_NAME
    };


    bool _enable_debug;

    const std::vector<const char*> _validation_layers = {
        "VK_LAYER_KHRONOS_validation"
    };

    VkDebugUtilsMessageSeverityFlagsEXT _debug_severity =
        VK_DEBUG_UTILS_MESSAGE_SEVERITY_VERBOSE_BIT_EXT |
        VK_DEBUG_UTILS_MESSAGE_SEVERITY_WARNING_BIT_EXT |
        VK_DEBUG_UTILS_MESSAGE_SEVERITY_ERROR_BIT_EXT;

    VkDebugUtilsMessengerEXT _debug_messenger;
};

} // namespace fl

#endif // _FL_VK_MANAGER_H
