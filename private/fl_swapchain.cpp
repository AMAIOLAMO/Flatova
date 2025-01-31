#include <fl_swapchain.hpp>

namespace fl {

VkSwapchainKHR Swapchain::get_raw_handle() {
    return _handle;
}

bool Swapchain::init(VkDevice device, VkSwapchainCreateInfoKHR *create_info_ptr,
                     const VkAllocationCallbacks *alloc_callback) {
    return vkCreateSwapchainKHR(device, create_info_ptr, alloc_callback, &_handle) == VK_SUCCESS;
}

} // namespace fl
