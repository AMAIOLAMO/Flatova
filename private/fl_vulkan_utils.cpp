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


void log_glfw_required_extensions_support() {
    std::vector<VkExtensionProperties> properties;
    uint32_t property_count = get_vk_instance_extension_properties(&properties);

    printf("[Extensions] Found Vk Extensions properties: %u\n", property_count);

    for(auto property : properties)
        printf("\t%s\n", property.extensionName);


    uint32_t glfw_extensions_count = 0;
    const char **glfw_extensions = glfwGetRequiredInstanceExtensions(&glfw_extensions_count);

    printf("[Extensions] Glfw required extensions for vulkan: %u\n", glfw_extensions_count);
    for(size_t i = 0; i < glfw_extensions_count; i++) {
        bool found = false;
        
        for(auto property : properties)
            if(strcmp(property.extensionName, glfw_extensions[i])) {
                found = true;
                break;
            }

        printf("\t%zu: ", i + 1);

        if(found)
            printf("Found supported extension: %s\n", glfw_extensions[i]);
        else
            printf("Extension: %s is unsupported\n", glfw_extensions[i]);
    }
}

} // namespace fl

