#include <fl_vk_instance.hpp>
#include <fl_vulkan_utils.hpp>

namespace fl {

Instance::Instance() {
}

Instance::~Instance() {
    vkDestroyInstance(_handle, nullptr);
}

bool Instance::init(const VkInstanceCreateInfo *create_info_ptr,
                    const VkAllocationCallbacks *allocator_ptr) {
    return vkCreateInstance(create_info_ptr, allocator_ptr, &_handle) == VK_SUCCESS;
}

void Instance::destroy_surface(VkSurfaceKHR surface, const VkAllocationCallbacks *allocator_ptr) {
    vkDestroySurfaceKHR(_handle, surface, allocator_ptr);
}

PFN_vkVoidFunction Instance::get_instance_proc_addr(const char *proc_str) {
    return vkGetInstanceProcAddr(_handle, proc_str);
}

uint32_t Instance::get_physical_devices(std::vector<VkPhysicalDevice> *devices_ptr) {
    // TODO: let's not use this kind of weird global reference to avoid shadowing, maybe add utils in the namespace?
    return ::fl::get_physical_devices(_handle, devices_ptr);
}

VkInstance Instance::get_raw_handle() {
    return _handle;
}

} // namespace fl
