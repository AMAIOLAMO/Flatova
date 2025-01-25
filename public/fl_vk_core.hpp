#pragma once
#ifndef _FL_VK_MANAGER_H
#define _FL_VK_MANAGER_H

#include <vulkan/vulkan.h>

#include <string>
#include <optional>
#include <vector>

namespace fl {

struct QueueFamilyIdxs {
    std::optional<uint32_t> graphics;
};

class VkCore {
public:
    VkCore(bool enable_debug);
    ~VkCore();

    VkCore(VkCore&) = delete;
    VkCore& operator=(VkCore&) = delete;

    bool init(std::string app_name);

private:
    bool setup_instance(std::string app_name);

    void setup_debug_messenger();

    VkPhysicalDevice pick_physical_device();

    bool find_queue_families(QueueFamilyIdxs *idxs_ptr);

    void populate_debug_messenger_create_info(VkDebugUtilsMessengerCreateInfoEXT *info_ptr);

    std::vector<const char*> _validation_layers = {
        "VK_LAYER_KHRONOS_validation"
    };

    VkInstance _instance;

    bool _enable_debug;

    QueueFamilyIdxs _queue_family_idxs;


    VkDebugUtilsMessageSeverityFlagsEXT _debug_severity =
        VK_DEBUG_UTILS_MESSAGE_SEVERITY_VERBOSE_BIT_EXT |
        VK_DEBUG_UTILS_MESSAGE_SEVERITY_WARNING_BIT_EXT |
        VK_DEBUG_UTILS_MESSAGE_SEVERITY_ERROR_BIT_EXT;

    VkPhysicalDevice _physical_device = VK_NULL_HANDLE;

    VkDevice _logical_device = VK_NULL_HANDLE;

    VkDebugUtilsMessengerEXT _debug_messenger;
};

} // namespace fl

#endif // _FL_VK_MANAGER_H
