#pragma once
#include <vector>
#ifndef _FL_VK_MANAGER_H
#define _FL_VK_MANAGER_H

#include <vulkan/vulkan.h>
#include <string>

namespace fl {

class VkManager {
public:
    VkManager(bool enable_debug);
    ~VkManager();

    VkManager(VkManager&) = delete;
    VkManager& operator=(VkManager&) = delete;

    bool init(std::string app_name);

private:
    bool setup_instance(std::string app_name);

    void setup_debug_messenger();

    void populate_debug_messenger_create_info(VkDebugUtilsMessengerCreateInfoEXT *info_ptr);

    std::vector<const char*> _validation_layers = {
        "VK_LAYER_KHRONOS_validation"
    };

    VkInstance _instance;

    bool _enable_debug;
    VkDebugUtilsMessageSeverityFlagsEXT _debug_severity =
        VK_DEBUG_UTILS_MESSAGE_SEVERITY_VERBOSE_BIT_EXT |
        VK_DEBUG_UTILS_MESSAGE_SEVERITY_WARNING_BIT_EXT |
        VK_DEBUG_UTILS_MESSAGE_SEVERITY_ERROR_BIT_EXT;


    VkDebugUtilsMessengerEXT _debug_messenger;
};

} // namespace fl

#endif // _FL_VK_MANAGER_H
