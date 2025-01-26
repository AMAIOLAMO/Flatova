#include <fl_vk_device_manager.hpp>

#include <cstring>

namespace fl {

VkDeviceManager::VkDeviceManager(VkInstance instance) : _instance(instance) {

}

VkDeviceManager::~VkDeviceManager() {

}

uint32_t VkDeviceManager::enumerate_extension_props(VkPhysicalDevice device, const char *layer_name,
                                                    std::vector<VkExtensionProperties> *props_ptr) {
    uint32_t prop_count = 0;
    vkEnumerateDeviceExtensionProperties(device, nullptr, &prop_count, nullptr);

    props_ptr->resize(prop_count);
    vkEnumerateDeviceExtensionProperties(device, nullptr, &prop_count, props_ptr->data());

    return prop_count;
}

uint32_t VkDeviceManager::enumerate_physical(std::vector<VkPhysicalDevice> *devices_ptr) {
    uint32_t device_count = 0;
    vkEnumeratePhysicalDevices(_instance, &device_count, nullptr);

    devices_ptr->resize(device_count);
    vkEnumeratePhysicalDevices(_instance, &device_count, devices_ptr->data());

    return device_count;
}

bool VkDeviceManager::extension_exists(VkPhysicalDevice device, const char *layer_name, const char *extension) {
    std::vector<VkExtensionProperties> device_properties{};
    enumerate_extension_props(device, layer_name, &device_properties);

    // linear search through the device properties, and see if they exist
    for(auto ext_prop : device_properties) {
        if(strcmp(extension, ext_prop.extensionName) == 0)
            return true;
    }

    return false;
}
uint32_t VkDeviceManager::get_physical_queue_family_props(VkPhysicalDevice device,
                                         std::vector<VkQueueFamilyProperties> *queue_family_props_ptr) {
    uint32_t queue_family_count = 0;
    vkGetPhysicalDeviceQueueFamilyProperties(device, &queue_family_count, nullptr);

    queue_family_props_ptr->resize(queue_family_count);
    vkGetPhysicalDeviceQueueFamilyProperties(device, &queue_family_count, queue_family_props_ptr->data());

    return queue_family_count;
}


} // namespace fl
