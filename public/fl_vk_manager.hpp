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
    void setup_debug_messenger(VkDebugUtilsMessageSeverityFlagsEXT severity);

    std::vector<const char*> _validation_layers = {
        "VK_LAYER_KHRONOS_validation"
    };

    VkInstance _instance;

    bool _enable_debug;

    VkDebugUtilsMessengerEXT _debug_messenger;
};

} // namespace fl

#endif // _FL_VK_MANAGER_H
