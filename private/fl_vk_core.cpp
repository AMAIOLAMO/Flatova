#include <fl_vk_core.hpp>

#include <fl_vulkan_utils.hpp>

#include <assert.h>
#include <cstring>

#include <GLFW/glfw3.h>

namespace fl {

VkCore::VkCore(bool enable_debug) : _enable_debug(enable_debug) {
}


VkCore::~VkCore() {
    if(_enable_debug) {
        auto destroy_debug_messenger_func = (PFN_vkDestroyDebugUtilsMessengerEXT)
            vkGetInstanceProcAddr(_instance, "vkDestroyDebugUtilsMessengerEXT");

        if(destroy_debug_messenger_func != nullptr)
            destroy_debug_messenger_func(_instance, _debug_messenger, nullptr);
        else
            fprintf(stderr, "[Vk Core] Cannot load debug messenger destroy function\n");
    }

    vkDestroyInstance(_instance, nullptr);
    printf("[VkManager] destroyed vulkan instance\n");
}

bool VkCore::init(std::string app_name) {
    if(setup_instance(app_name) == false)
        return false;

    // SETUP DEBUG STUFF
    if(_enable_debug)
        setup_debug_messenger();

    _physical_device = pick_physical_device();

    if(_physical_device != VK_NULL_HANDLE)
        printf("[Vk Core] Found Physical Device\n");
    else
        fprintf(stderr, "[Vk Core] Physical Device with graphics capabilities failed\n");

    if(find_queue_families(&_queue_family_idxs) == false)
        fprintf(stderr, "[Vk Core] Cannot find suitable queue families!\n");
    else
        printf("[Vk Core] Found suitable queue families!\n");


    return true;
}

void log_glfw_required_extensions_support() {
    std::vector<VkExtensionProperties> properties;
    uint32_t property_count = get_vk_instance_extension_properties(&properties);

    printf("[Vk Core] Found Vk Extensions properties: %u\n", property_count);

    for(auto property : properties)
        printf("\t%s\n", property.extensionName);


    uint32_t glfw_extensions_count = 0;
    const char **glfw_extensions = glfwGetRequiredInstanceExtensions(&glfw_extensions_count);

    printf("[Vk Core] Glfw required extensions for vulkan: %u\n", glfw_extensions_count);
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

std::vector<const char*> get_req_instance_extensions(bool enable_validation_layers) {
    uint32_t glfw_extensions_count = 0;
    const char **glfw_extensions = glfwGetRequiredInstanceExtensions(&glfw_extensions_count);

    std::vector<const char*> app_extensions{glfw_extensions, glfw_extensions + glfw_extensions_count};

    if(enable_validation_layers) {
        app_extensions.emplace_back(VK_EXT_DEBUG_UTILS_EXTENSION_NAME);
        printf("[Vk Core] Included DEBUG UTILS EXTENSION for validation layers\n");
    }

    return app_extensions;
}



bool VkCore::setup_instance(std::string app_name) {
    // APP INFO
    VkApplicationInfo app_info{};
    app_info.sType = VK_STRUCTURE_TYPE_APPLICATION_INFO;
    app_info.pApplicationName = app_name.c_str();
    app_info.applicationVersion = VK_MAKE_VERSION(0, 0, 1);
    app_info.pEngineName = "No Engine";
    app_info.engineVersion = VK_MAKE_VERSION(1, 0, 0);
    app_info.apiVersion = VK_API_VERSION_1_0;


    // CREATE INSTANCE INFO
    VkInstanceCreateInfo create_info{};
    create_info.sType = VK_STRUCTURE_TYPE_INSTANCE_CREATE_INFO;
    create_info.pApplicationInfo = &app_info;


    // GLFW REQUIRED VULKAN EXTENSIONS SUPPORT
    log_glfw_required_extensions_support();

    std::vector<const char*> required_extensions = get_req_instance_extensions(_enable_debug);

    create_info.enabledExtensionCount = required_extensions.size();
    create_info.ppEnabledExtensionNames = required_extensions.data();


    // VALIDATION VULKAN LAYERS SUPPORT
    create_info.enabledLayerCount = 0;

    bool validation_layers_available = are_layers_all_available(_validation_layers);

    printf("[Vk Core] Validation Layers fully Available: %s\n",
           validation_layers_available ? "True" : "False");

    if(_enable_debug) {

        if(validation_layers_available) {
            uint32_t layer_count = 0;
            vkEnumerateInstanceLayerProperties(&layer_count, nullptr);

            // used for global validation layers etc
            create_info.enabledLayerCount = layer_count;
            create_info.ppEnabledLayerNames = _validation_layers.data();

            printf("[Vk Core] Validation layers enabled!\n");
        }

        VkDebugUtilsMessengerCreateInfoEXT debug_utils_create_info{};
        populate_debug_messenger_create_info(&debug_utils_create_info);
        create_info.pNext = &debug_utils_create_info;

    }

    if(vkCreateInstance(&create_info, nullptr, &_instance) != VK_SUCCESS)
        return false;

    return true;
}

static VKAPI_ATTR VkBool32 VKAPI_CALL validation_error_callback(
    VkDebugUtilsMessageSeverityFlagBitsEXT severity,
    VkDebugUtilsMessageTypeFlagsEXT type,
    const VkDebugUtilsMessengerCallbackDataEXT *callback_data_ptr, void *user_data_ptr) {

    fprintf(stderr, "[Vk Core] Validation Layer: %s\n", callback_data_ptr->pMessage);

    return VK_FALSE;
}

void VkCore::populate_debug_messenger_create_info(VkDebugUtilsMessengerCreateInfoEXT *info_ptr) {
    assert(info_ptr != nullptr && "Cannot populate a null debug message create info_ptr");

    *info_ptr = {};

    info_ptr->sType = VK_STRUCTURE_TYPE_DEBUG_UTILS_MESSENGER_CREATE_INFO_EXT;
    info_ptr->messageSeverity = _debug_severity;
    info_ptr->messageType =
        VK_DEBUG_UTILS_MESSAGE_TYPE_GENERAL_BIT_EXT |
        VK_DEBUG_UTILS_MESSAGE_TYPE_VALIDATION_BIT_EXT |
        VK_DEBUG_UTILS_MESSAGE_TYPE_PERFORMANCE_BIT_EXT;

    info_ptr->pfnUserCallback = validation_error_callback;
    info_ptr->pUserData = nullptr;
}

void VkCore::setup_debug_messenger() {
    VkDebugUtilsMessengerCreateInfoEXT create_info{};
    populate_debug_messenger_create_info(&create_info);

    // since these are extension functions, they are not automatically loaded in memory
    auto create_func = (PFN_vkCreateDebugUtilsMessengerEXT)
        vkGetInstanceProcAddr(_instance, "vkCreateDebugUtilsMessengerEXT");

    if(create_func != nullptr) {
        create_func(_instance, &create_info, nullptr, &_debug_messenger);
        printf("[Vk Core] Debug Messenger Created successfully\n");
    }
    else {
        printf("[Vk Core] Debug Messenger Failed to create\n");
    }
}

VkPhysicalDevice VkCore::pick_physical_device() {
    uint32_t device_count = 0;
    vkEnumeratePhysicalDevices(_instance, &device_count, nullptr);

    std::vector<VkPhysicalDevice> devices{device_count};
    vkEnumeratePhysicalDevices(_instance, &device_count, devices.data());

    printf("[Vk Core] Found physical devices: %u \n", device_count);
    
    for(auto &device : devices) {
        VkPhysicalDeviceFeatures device_features;

        vkGetPhysicalDeviceFeatures(device, &device_features);

        if(device_features.geometryShader)
            return device;
    }

    return VK_NULL_HANDLE;
}

bool VkCore::find_queue_families(QueueFamilyIdxs *idxs_ptr) {
    uint32_t queue_family_count = 0;
    vkGetPhysicalDeviceQueueFamilyProperties(_physical_device, &queue_family_count, nullptr);

    std::vector<VkQueueFamilyProperties> queue_family_props{queue_family_count};
    vkGetPhysicalDeviceQueueFamilyProperties(_physical_device, &queue_family_count, queue_family_props.data());


    for(size_t i = 0; i < queue_family_props.size(); i++) {
        const auto &family_prop = queue_family_props[i];

        if(family_prop.queueFlags & VK_QUEUE_GRAPHICS_BIT) {
            idxs_ptr->graphics = i;
            return true;
        }
    }
    
    return false;
}

}; // namespace fl
