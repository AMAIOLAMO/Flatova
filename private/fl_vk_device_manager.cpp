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


} // namespace fl
