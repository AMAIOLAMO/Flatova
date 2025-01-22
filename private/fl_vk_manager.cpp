#include <GLFW/glfw3.h>
#include <cstring>
#include <fl_vk_manager.hpp>

#include <fl_vulkan_utils.hpp>

namespace fl {

VkManager::VkManager() {
}


VkManager::~VkManager() {
    vkDestroyInstance(_instance, nullptr);
    printf("[VkManager] destroyed vulkan instance\n");
}

void log_glfw_required_extensions_support() {
    std::vector<VkExtensionProperties> properties;
    uint32_t property_count = get_vk_instance_extension_properties(&properties);

    printf("[Vk Manager] Found Vk Extensions properties: %u\n", property_count);

    for(auto property : properties)
        printf("\t%s\n", property.extensionName);


    uint32_t glfw_extensions_count = 0;
    const char **glfw_extensions = glfwGetRequiredInstanceExtensions(&glfw_extensions_count);

    printf("[Vk Manager] Glfw required extensions for vulkan: %u\n", glfw_extensions_count);
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

bool VkManager::init(std::string app_name, bool enable_validation_layers) {
    VkApplicationInfo app_info{};
    app_info.sType = VK_STRUCTURE_TYPE_APPLICATION_INFO;
    app_info.pApplicationName = app_name.c_str();
    app_info.applicationVersion = VK_MAKE_VERSION(0, 0, 1);
    app_info.pEngineName = "No Engine";
    app_info.engineVersion = VK_MAKE_VERSION(1, 0, 0);
    app_info.apiVersion = VK_API_VERSION_1_0;


    VkInstanceCreateInfo create_info{};
    create_info.sType = VK_STRUCTURE_TYPE_INSTANCE_CREATE_INFO;
    create_info.pApplicationInfo = &app_info;

    log_glfw_required_extensions_support();

    uint32_t glfw_extensions_count = 0;
    const char **glfw_extensions = glfwGetRequiredInstanceExtensions(&glfw_extensions_count);


    create_info.enabledExtensionCount = glfw_extensions_count;
    create_info.ppEnabledExtensionNames = glfw_extensions;

    create_info.enabledLayerCount = 0;

    bool validation_layers_available = are_layers_all_available(_validation_layers);

    printf("[Vk Manager] Validation Layers fully Available: %s\n",
           validation_layers_available ? "True" : "False");

    if(enable_validation_layers && validation_layers_available) {
        uint32_t layer_count = 0;
        vkEnumerateInstanceLayerProperties(&layer_count, nullptr);
        
        // used for global validation layers etc
        create_info.enabledLayerCount = layer_count;
        create_info.ppEnabledLayerNames = _validation_layers.data();
    }

    return vkCreateInstance(&create_info, nullptr, &_instance) == VK_SUCCESS;
}

}; // namespace fl
