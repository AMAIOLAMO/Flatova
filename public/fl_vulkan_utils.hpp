#pragma once
#include <vulkan/vulkan_core.h>
#ifndef _FL_VULKAN_UTILS_H
#define _FL_VULKAN_UTILS_H

#include <cstdint>
#include <vector>

struct VkExtensionProperties;

namespace fl {

struct SwapChainSupportInfo {
    VkSurfaceCapabilitiesKHR capabilities;
    std::vector<VkSurfaceFormatKHR> formats;
    std::vector<VkPresentModeKHR> present_modes;
};

/// a helper function that returns the number of extension properties that's supported by your vulkan
/// vendor.
uint32_t get_vk_instance_extension_properties(std::vector<VkExtensionProperties> *properties_ptr);

bool are_layers_all_available(const std::vector<const char*> &layer_names);

uint32_t get_physical_devices(VkInstance instance, std::vector<VkPhysicalDevice> *devices_ptr);

uint32_t get_physical_device_extension_props(VkPhysicalDevice device, const char *layer_name,
                                             std::vector<VkExtensionProperties> *props_ptr);

bool physical_device_extension_exists(VkPhysicalDevice device, const char *layer_name, const char *extension);

bool get_physical_swap_chain_support(VkPhysicalDevice device, VkSurfaceKHR surface, SwapChainSupportInfo *info_ptr);

bool is_physical_surface_supported(VkPhysicalDevice device, uint32_t queue_idx, VkSurfaceKHR surface);

uint32_t get_physical_queue_family_props(VkPhysicalDevice device, std::vector<VkQueueFamilyProperties> *props_ptr);

} // namespace fl

#endif // _FL_VULKAN_UTILS_H
