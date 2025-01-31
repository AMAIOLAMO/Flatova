#pragma once
#ifndef _FL_VK_INSTANCE_H
#define _FL_VK_INSTANCE_H

#include <vulkan/vulkan_core.h>
#include <vector>

namespace fl {

class Instance {
public:
    Instance();
    ~Instance();

    bool init(const VkInstanceCreateInfo *create_info_ptr,
             const VkAllocationCallbacks *allocator_ptr);

    void destroy_surface(VkSurfaceKHR surface, const VkAllocationCallbacks *allocator_ptr);

    PFN_vkVoidFunction get_instance_proc_addr(const char *proc_str);
    uint32_t get_physical_devices(std::vector<VkPhysicalDevice> *devices_ptr);

    VkInstance get_raw_handle();

private:
    VkInstance _handle = VK_NULL_HANDLE;
};

} // namespace fl

#endif // _FL_VK_INSTANCE_H
