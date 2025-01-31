#include <fl_vk_device_manager.hpp>

#include <cstring>
#include <cassert>

namespace fl {

VkDeviceManager::VkDeviceManager(VkPhysicalDevice physical, VkDevice logical)
    : _physical(physical), _logical(logical) {

}

VkDeviceManager::~VkDeviceManager() {

}

void VkDeviceManager::get_queue(uint32_t queue_family_idx, VkQueue *queue_ptr, uint32_t queue_idx) {
    vkGetDeviceQueue(
        _logical, queue_family_idx,
        queue_idx, queue_ptr
    );
}

bool VkDeviceManager::get_swap_chain_support(VkSurfaceKHR surface, SwapChainSupportInfo *info_ptr) {
    return get_physical_swap_chain_support(_physical, surface, info_ptr);
}

const VkPhysicalDevice VkDeviceManager::get_physical() {
    return _physical;
}

const VkDevice VkDeviceManager::get_logical() {
    return _logical;
}

/*bool VkDeviceManager::create_swap_chain(const VkSwapchainCreateInfoKHR *create_info_ptr,*/
/*                       const VkAllocationCallbacks *alloc_callback, Swapchain *swap_chain_ptr) {*/
/*    return vkCreateSwapchainKHR(_logical, create_info_ptr, alloc_callback, swap_chain_ptr) == VK_SUCCESS;*/
/*}*/

uint32_t VkDeviceManager::get_swap_chain_images(Swapchain *swap_chain_ptr, std::vector<VkImage> *imgs_ptr) {
    VkSwapchainKHR raw_handle = swap_chain_ptr->get_raw_handle();

    uint32_t image_count = 0;
    vkGetSwapchainImagesKHR(_logical, raw_handle, &image_count, nullptr);

    imgs_ptr->resize(image_count);
    vkGetSwapchainImagesKHR(_logical, raw_handle, &image_count, imgs_ptr->data());

    return image_count;
}


} // namespace fl
