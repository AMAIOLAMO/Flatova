#pragma once
#include <vector>
#ifndef _FL_VK_MANAGER_H
#define _FL_VK_MANAGER_H

#include <vulkan/vulkan.h>
#include <string>

namespace fl {

class VkManager {
public:
    VkManager();
    ~VkManager();

    VkManager(VkManager&) = delete;
    VkManager& operator=(VkManager&) = delete;

    bool init(std::string app_name, bool enable_validation_layers);

private:
    std::vector<const char*> _validation_layers = {
        "VK_LAYER_KHRONOS_validation"
    };

    VkInstance _instance;
};

} // namespace fl

#endif // _FL_VK_MANAGER_H
