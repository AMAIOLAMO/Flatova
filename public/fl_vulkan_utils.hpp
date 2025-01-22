#pragma once
#ifndef _FL_VULKAN_UTILS_H
#define _FL_VULKAN_UTILS_H

#include <cstdint>
#include <vector>

struct VkExtensionProperties;

namespace fl {

/// a helper function that returns the number of extension properties that's supported by your vulkan
/// vendor.
uint32_t get_vk_instance_extension_properties(std::vector<VkExtensionProperties> *properties_ptr);

bool are_layers_all_available(const std::vector<const char*> &layer_names);

} // namespace fl

#endif // _FL_VULKAN_UTILS_H
