#include <fl_vulkan_utils.hpp>

#include <vulkan/vulkan_core.h>

#include <cstdio>
#include <cstring>
#include <vector>

#include <GLFW/glfw3.h>

namespace fl {

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

} // namespace fl

