#include <fl_vk_core.hpp>

#include <fl_vulkan_utils.hpp>


#include <GLFW/glfw3.h>

#include <assert.h>
#include <cstring>
#include <set>

namespace fl {

#define core_info(...) do { printf("[Vk Core] \033[0;37mINFO: " __VA_ARGS__); printf("\033[0m\n"); } while(0)
#define core_err(...) do { fprintf(stderr, "[Vk Core] \033[0;31mERROR: " __VA_ARGS__); fprintf(stderr, "\033[0m\n"); } while(0)

VkCore::VkCore(bool enable_debug) : _enable_debug(enable_debug) {
}


VkCore::~VkCore() {
    if(_enable_debug) {
        auto destroy_debug_messenger_func = (PFN_vkDestroyDebugUtilsMessengerEXT)
            vkGetInstanceProcAddr(_instance, "vkDestroyDebugUtilsMessengerEXT");

        if(destroy_debug_messenger_func != nullptr)
            destroy_debug_messenger_func(_instance, _debug_messenger, nullptr);
        else
            core_err("Cannot load debug messenger destroy function");
    }

    vkDestroySurfaceKHR(_instance, _surface, nullptr);
    vkDestroyDevice(_logical_device, nullptr);
    vkDestroyInstance(_instance, nullptr);

    if(_device_manager_ptr)
        delete _device_manager_ptr;

    core_info("destroyed vk core");
}

#define action_check(FUNC, MSG) do { \
    if((FUNC) == false) { \
        core_err("ACTION \"" MSG "\" FAILED"); \
        return false; \
    } \
    core_info("ACTION \"" MSG "\" SUCCESS"); \
} while(0)

bool VkCore::init(std::string app_name, GLFWwindow *window_ptr) {
    action_check(setup_instance(app_name), "setup instance");

    // SETUP DEBUG STUFF
    if(_enable_debug)
        setup_debug_messenger();

    action_check(setup_glfw_surface(window_ptr), "setup glfw surface");

    _physical_device = pick_physical_device();

    if(_physical_device == VK_NULL_HANDLE) {
        core_err("Physical Device with graphics capabilities failed");
        return false;
    }
    else
        core_info("Find Suitable Physical Device Success");

    action_check(find_queue_families(&_queue_family_idxs), "find suitable queue families");
    action_check(setup_logical_device(), "setup logical device");

    _device_manager_ptr = new VkDeviceManager {
        _physical_device, _logical_device
    };

    _device_manager_ptr->get_queue(_queue_family_idxs.graphics.value(), &_graphics_queue);

    core_info("grabbed graphics queue");

    _device_manager_ptr->get_queue(_queue_family_idxs.present.value(), &_present_queue);

    core_info("grabbed present queue");

    SwapChainSupportInfo swap_chain_support{};

    action_check(
        get_physical_swap_chain_support(_physical_device, _surface, &swap_chain_support),
        "get swap chain support"
    );


    return true;
}

void log_glfw_required_extensions_support() {
    std::vector<VkExtensionProperties> properties;
    uint32_t property_count = get_vk_instance_extension_properties(&properties);

    core_info("Found Vk Extensions properties: %u", property_count);

    for(auto property : properties)
        core_info("\t%s", property.extensionName);


    uint32_t glfw_extensions_count = 0;
    const char **glfw_extensions = glfwGetRequiredInstanceExtensions(&glfw_extensions_count);

    core_info("Glfw required extensions for vulkan: %u", glfw_extensions_count);
    for(size_t i = 0; i < glfw_extensions_count; i++) {
        bool found = false;
        
        for(auto property : properties)
            if(strcmp(property.extensionName, glfw_extensions[i])) {
                found = true;
                break;
            }

        if(found)
            core_info("\t%zu: Supported extension: %s", i + 1, glfw_extensions[i]);
        else
            core_info("\t%zu: Unsupported extension: %s ", i + 1, glfw_extensions[i]);
    }
}

std::vector<const char*> get_req_instance_extensions(bool enable_validation_layers) {
    uint32_t glfw_extensions_count = 0;
    const char **glfw_extensions = glfwGetRequiredInstanceExtensions(&glfw_extensions_count);

    std::vector<const char*> app_extensions{glfw_extensions, glfw_extensions + glfw_extensions_count};

    if(enable_validation_layers) {
        app_extensions.emplace_back(VK_EXT_DEBUG_UTILS_EXTENSION_NAME);
        core_info("Included DEBUG UTILS EXTENSION for validation layers");
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

    core_info("Validation Layers fully Available: %s",
           validation_layers_available ? "True" : "False");

    if(_enable_debug) {

        if(validation_layers_available) {
            uint32_t layer_count = 0;
            vkEnumerateInstanceLayerProperties(&layer_count, nullptr);

            // used for global validation layers etc
            create_info.enabledLayerCount = layer_count;
            create_info.ppEnabledLayerNames = _validation_layers.data();

            core_info("Validation layers enabled!");
        }

        VkDebugUtilsMessengerCreateInfoEXT debug_utils_create_info{};
        populate_debug_messenger_create_info(&debug_utils_create_info);
        create_info.pNext = &debug_utils_create_info;

    }

    if(vkCreateInstance(&create_info, nullptr, &_instance) != VK_SUCCESS)
        return false;

    return true;
}

bool VkCore::setup_glfw_surface(GLFWwindow *window_ptr) {
    return glfwCreateWindowSurface(_instance, window_ptr, nullptr, &_surface) == VK_SUCCESS;
}

static VKAPI_ATTR VkBool32 VKAPI_CALL validation_error_callback(
    VkDebugUtilsMessageSeverityFlagBitsEXT severity,
    VkDebugUtilsMessageTypeFlagsEXT type,
    const VkDebugUtilsMessengerCallbackDataEXT *callback_data_ptr, void *user_data_ptr) {

    if(severity & (VK_DEBUG_UTILS_MESSAGE_SEVERITY_INFO_BIT_EXT | VK_DEBUG_UTILS_MESSAGE_SEVERITY_VERBOSE_BIT_EXT))
        core_info("Validation Layer -> %s", callback_data_ptr->pMessage);

    if(severity & VK_DEBUG_UTILS_MESSAGE_SEVERITY_ERROR_BIT_EXT)
        core_err("Validation Layer -> %s", callback_data_ptr->pMessage);

    return VK_FALSE;
}

void VkCore::populate_debug_messenger_create_info(VkDebugUtilsMessengerCreateInfoEXT *info_ptr) {
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
        core_info("Debug Messenger Created successfully");
    }
    else
        core_err("Debug Messenger Failed to create");
}

VkPhysicalDevice VkCore::pick_physical_device() {
    std::vector<VkPhysicalDevice> devices{};
    get_physical_devices(_instance, &devices);

    core_info("physical devices count: %zu", devices.size());
    
    for(auto &device : devices)
        if(is_device_suitable(device))
            return device;

    return VK_NULL_HANDLE;
}

bool VkCore::check_device_extension_support(VkPhysicalDevice device) {
    for(auto req_ext : _device_req_extensions) {
        if(physical_device_extension_exists(device, nullptr, req_ext) == false)
            return false;
    }

    return true;
}

bool VkCore::is_device_suitable(VkPhysicalDevice device) {
    VkPhysicalDeviceFeatures device_features;
    vkGetPhysicalDeviceFeatures(device, &device_features);

    if(check_device_extension_support(device) && device_features.geometryShader)
        return true;

    return false;
}


bool VkCore::find_queue_families(QueueFamilyIdxs *idxs_ptr) {
    std::vector<VkQueueFamilyProperties> queue_family_props{};
    get_physical_queue_family_props(_physical_device, &queue_family_props);

    for(size_t i = 0; i < queue_family_props.size(); i++) {
        const auto &family_prop = queue_family_props[i];

        if(family_prop.queueFlags & VK_QUEUE_GRAPHICS_BIT)
            idxs_ptr->graphics = i;

        if(is_physical_surface_supported(_physical_device, i, _surface))
            idxs_ptr->present = i;

        if(idxs_ptr->graphics.has_value() && idxs_ptr->present.has_value())
            return true;
    }
    
    return false;
}

bool VkCore::setup_logical_device() {
    std::vector<VkDeviceQueueCreateInfo> queue_create_infos{};

    std::set<uint32_t> unique_queue_families_idxs {
        _queue_family_idxs.graphics.value(), _queue_family_idxs.present.value()
    };

    float queue_priority = 1.0f;

    for(auto queue_family_idx : unique_queue_families_idxs) {
        VkDeviceQueueCreateInfo queue_info{};
        queue_info.sType = VK_STRUCTURE_TYPE_DEVICE_QUEUE_CREATE_INFO;
        queue_info.queueFamilyIndex = queue_family_idx;
        queue_info.queueCount = 1;
        queue_info.pQueuePriorities = &queue_priority;

        queue_create_infos.push_back(queue_info);
    }

    VkPhysicalDeviceFeatures device_features{};
    vkGetPhysicalDeviceFeatures(_physical_device, &device_features);

    VkDeviceCreateInfo device_create_info{};
    device_create_info.sType = VK_STRUCTURE_TYPE_DEVICE_CREATE_INFO;

    device_create_info.pQueueCreateInfos = queue_create_infos.data();
    device_create_info.queueCreateInfoCount = static_cast<uint32_t>(queue_create_infos.size());

    device_create_info.ppEnabledExtensionNames = _device_req_extensions.data();
    device_create_info.enabledExtensionCount = static_cast<uint32_t>(_device_req_extensions.size());

    device_create_info.pEnabledFeatures = &device_features;

    device_create_info.enabledLayerCount = 0;

    if(_enable_debug) {
        device_create_info.enabledLayerCount = static_cast<uint32_t>(_validation_layers.size());
        device_create_info.ppEnabledLayerNames = _validation_layers.data();
    }

    return vkCreateDevice(
        _physical_device, &device_create_info,
        nullptr, &_logical_device
    ) == VK_SUCCESS;
}

}; // namespace fl
