#pragma once
#ifndef _FL_VK_DEVICE_MANAGER_H
#define _FL_VK_DEVICE_MANAGER_H

#include <vulkan/vulkan.h>

#include <vector>

namespace fl {

class VkDeviceManager {
public:
    VkDeviceManager(VkInstance instance);
    ~VkDeviceManager();

    /// resizes and lists down the physical devices the given vulkan instance has
    uint32_t enumerate_physical(std::vector<VkPhysicalDevice> *devices_ptr);
    
    /// enumerates all the extension properties of a given physical device
    uint32_t enumerate_extension_props(VkPhysicalDevice device, const char *layer_name,
                                       std::vector<VkExtensionProperties> *props_ptr);

    uint32_t get_physical_queue_family_props(VkPhysicalDevice device,
                                             std::vector<VkQueueFamilyProperties> *queue_family_props_ptr);

    /// checks if a device extension exists
    bool extension_exists(VkPhysicalDevice device, const char *layer_name, const char *extension);



private:
    VkInstance _instance;
};

} // namespace fl

#endif // _FL_VK_DEVICE_MANAGER_H
