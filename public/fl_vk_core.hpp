#pragma once
#ifndef _FL_VK_MANAGER_H
#define _FL_VK_MANAGER_H

#include <fl_vk_device_manager.hpp>
#include <fl_vk_instance.hpp>
#include <fl_swapchain.hpp>

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

    VkDeviceManager* get_device_manager_ptr();
    Swapchain* get_swap_chain_ptr();

    VkFormat get_chosen_img_format() const;
    VkExtent2D get_swap_chain_extent() const;
    const QueueFamilyIdxs* get_queue_family_idxs_ptr() const;

private:
    bool setup_instance(std::string app_name);

    void setup_debug_messenger();
    
    bool setup_glfw_surface(GLFWwindow *window_ptr);

    bool check_device_extension_support(VkPhysicalDevice device);

    VkPhysicalDevice pick_physical_device();

    bool is_device_suitable(VkPhysicalDevice device);

    bool setup_logical_device(VkPhysicalDevice physical_device, VkDevice *logical_device_ptr);

    bool find_queue_families(VkPhysicalDevice physical_device, QueueFamilyIdxs *idxs_ptr);

    bool create_swap_chain(GLFWwindow *window_ptr);

    void populate_debug_messenger_create_info(VkDebugUtilsMessengerCreateInfoEXT *info_ptr);

    Instance _instance;

    VkSurfaceKHR _surface;
    Swapchain _swap_chain;

    VkFormat _chosen_img_format;
    VkExtent2D _chosen_extent;

    VkDeviceManager *_device_manager_ptr;

    VkDevice _logical_device  = VK_NULL_HANDLE;

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
