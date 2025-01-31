#pragma once
#ifndef _FL_VK_DEVICE_MANAGER_H
#define _FL_VK_DEVICE_MANAGER_H

#include <vulkan/vulkan.h>
#include <fl_vulkan_utils.hpp>
#include <fl_swapchain.hpp>

namespace fl {

class VkDeviceManager {
public:
    VkDeviceManager(VkPhysicalDevice physical, VkDevice logical);
    ~VkDeviceManager();

    void get_queue(uint32_t queue_family_idx, VkQueue *queue_ptr, uint32_t queue_idx = 0);

    bool get_swap_chain_support(VkSurfaceKHR surface, SwapChainSupportInfo *info_ptr);

    const VkPhysicalDevice get_physical();

    const VkDevice get_logical();

private:
    VkPhysicalDevice _physical;
    VkDevice _logical;
};

} // namespace fl

#endif // _FL_VK_DEVICE_MANAGER_H
