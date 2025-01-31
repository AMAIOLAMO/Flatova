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

} // namespace fl
