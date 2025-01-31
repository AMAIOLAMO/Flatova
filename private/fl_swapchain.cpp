#include <fl_swapchain.hpp>

namespace fl {

VkSwapchainKHR Swapchain::get_raw_handle() {
    return _handle;
}

bool Swapchain::init(VkDevice device, VkSwapchainCreateInfoKHR *create_info_ptr,
                     const VkAllocationCallbacks *alloc_callback) {

    if(vkCreateSwapchainKHR(device, create_info_ptr, alloc_callback, &_handle) != VK_SUCCESS)
        return false;
    // else
    _logical_device  = device;
    _img_fmt         = create_info_ptr->imageFormat;
    _img_extent      = create_info_ptr->imageExtent;
    _img_color_space = create_info_ptr->imageColorSpace;

    return true;
}

VkFormat Swapchain::get_img_format() const {
    return _img_fmt;
}

VkExtent2D Swapchain::get_img_extent() const {
    return _img_extent;
}

uint32_t Swapchain::get_images(std::vector<VkImage> *imgs_ptr) {
    uint32_t image_count = 0;
    vkGetSwapchainImagesKHR(_logical_device, _handle, &image_count, nullptr);

    imgs_ptr->resize(image_count);
    vkGetSwapchainImagesKHR(_logical_device, _handle, &image_count, imgs_ptr->data());

    return image_count;
}
} // namespace fl
