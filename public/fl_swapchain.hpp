#pragma once
#ifndef _FL_SWAPCHAIN_H
#define _FL_SWAPCHAIN_H

#include <vulkan/vulkan_core.h>

namespace fl {


class Swapchain {
public:
    VkSwapchainKHR get_raw_handle();
    
    bool init(VkDevice device, VkSwapchainCreateInfoKHR *create_info_ptr,
              const VkAllocationCallbacks *alloc_callback);

private:
    VkSwapchainKHR _handle = VK_NULL_HANDLE;
};


} // namespace fl

#endif // _FL_SWAPCHAIN_H
