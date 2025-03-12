#pragma once
#ifndef _FL_SWAPCHAIN_H
#define _FL_SWAPCHAIN_H

#include <vulkan/vulkan_core.h>

#include <vector>

namespace fl {


class Swapchain {
public:
    VkSwapchainKHR& get_raw_handle_ref();
    
    bool init(VkDevice device, VkSwapchainCreateInfoKHR *create_info_ptr,
              const VkAllocationCallbacks *alloc_callback);

    VkFormat get_img_format() const;
    VkExtent2D get_img_extent() const;

    uint32_t get_images(std::vector<VkImage> *images_ptr);

private:
    VkSwapchainKHR _handle = VK_NULL_HANDLE;
    VkDevice _logical_device = VK_NULL_HANDLE;

    VkFormat        _img_fmt;
    VkExtent2D      _img_extent;
    VkColorSpaceKHR _img_color_space;
};


} // namespace fl

#endif // _FL_SWAPCHAIN_H
