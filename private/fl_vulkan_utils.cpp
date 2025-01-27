#include <fl_vulkan_utils.hpp>

#include <vulkan/vulkan_core.h>

#include <cstdio>
#include <cstring>
#include <vector>
#include <cassert>

#include <GLFW/glfw3.h>

namespace fl {

uint32_t get_physical_queue_family_props(VkPhysicalDevice device, std::vector<VkQueueFamilyProperties> *props_ptr) {
    uint32_t queue_family_count = 0;
    vkGetPhysicalDeviceQueueFamilyProperties(device, &queue_family_count, nullptr);

    props_ptr->resize(queue_family_count);
    vkGetPhysicalDeviceQueueFamilyProperties(device, &queue_family_count, props_ptr->data());

    return queue_family_count;
}

bool is_physical_surface_supported(VkPhysicalDevice device, uint32_t queue_family_idx, VkSurfaceKHR surface) {
    VkBool32 surface_present_support = false;
    assert(vkGetPhysicalDeviceSurfaceSupportKHR(device, queue_family_idx, surface, &surface_present_support) == VK_SUCCESS);

    return surface_present_support;
}

bool get_physical_swap_chain_support(VkPhysicalDevice device, VkSurfaceKHR surface, SwapChainSupportInfo *info_ptr) {
    if(vkGetPhysicalDeviceSurfaceCapabilitiesKHR(device, surface, &info_ptr->capabilities) != VK_SUCCESS)
        return false;

    uint32_t format_count = 0;
    vkGetPhysicalDeviceSurfaceFormatsKHR(device, surface, &format_count, nullptr);

    if(format_count == 0)
        return false;
    // else

    info_ptr->formats.resize(format_count);
    vkGetPhysicalDeviceSurfaceFormatsKHR(device, surface, &format_count, info_ptr->formats.data());


    uint32_t present_modes_count = 0;
    vkGetPhysicalDeviceSurfacePresentModesKHR(device, surface, &present_modes_count, nullptr);

    info_ptr->present_modes.resize(present_modes_count);
    vkGetPhysicalDeviceSurfacePresentModesKHR(device, surface, &present_modes_count, info_ptr->present_modes.data());

    return true;
}

uint32_t get_vk_instance_extension_properties(std::vector<VkExtensionProperties> *properties_ptr) {
    uint32_t property_count = 0;
    vkEnumerateInstanceExtensionProperties(nullptr, &property_count, nullptr);

    properties_ptr->resize(property_count);
    vkEnumerateInstanceExtensionProperties(nullptr, &property_count, properties_ptr->data());

    return property_count;
}

bool are_layers_all_available(const std::vector<const char*> &layer_names) {
    uint32_t layer_count = 0;
    vkEnumerateInstanceLayerProperties(&layer_count, nullptr);

    std::vector<VkLayerProperties> available_layers{layer_count};

    vkEnumerateInstanceLayerProperties(&layer_count, available_layers.data());

    for(const char *layer_name : layer_names) {
        bool found = false;

        for(size_t i = 0; i < layer_count; i++) {
            auto layer_property = available_layers[i];
            
            if(strcmp(layer_name, layer_property.layerName) == 0) {
                found = true;
                break;
            }
        }

        // searched through, but still not found, then not all validation layers are available
        if(!found)
            return false;
    }

    return true;
}

uint32_t get_physical_devices(VkInstance instance, std::vector<VkPhysicalDevice> *devices_ptr) {
    uint32_t device_count = 0;
    vkEnumeratePhysicalDevices(instance, &device_count, nullptr);

    devices_ptr->resize(device_count);
    vkEnumeratePhysicalDevices(instance, &device_count, devices_ptr->data());

    return device_count;
}

uint32_t get_physical_device_extension_props(VkPhysicalDevice device, const char *layer_name,
                                             std::vector<VkExtensionProperties> *props_ptr) {
    uint32_t prop_count = 0;
    vkEnumerateDeviceExtensionProperties(device, layer_name, &prop_count, nullptr);

    props_ptr->resize(prop_count);
    vkEnumerateDeviceExtensionProperties(device, layer_name, &prop_count, props_ptr->data());

    return prop_count;
}

bool physical_device_extension_exists(VkPhysicalDevice device, const char *layer_name, const char *extension) {
    std::vector<VkExtensionProperties> device_properties{};
    get_physical_device_extension_props(device, layer_name, &device_properties);

    // linear search through the device properties, and see if they exist
    for(auto ext_prop : device_properties) {
        if(strcmp(extension, ext_prop.extensionName) == 0)
            return true;
    }

    return false;
}

} // namespace fl

