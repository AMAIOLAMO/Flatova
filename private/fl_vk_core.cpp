#include <fl_vk_core.hpp>

#include <fl_vulkan_utils.hpp>

#include <GLFW/glfw3.h>

#include <assert.h>

#include <cstring>
#include <limits>
#include <set>
#include <algorithm>

namespace fl {

#define core_info(...) do { printf("[Vk Core] \033[0;37mINFO: " __VA_ARGS__); printf("\033[0m\n"); } while(0)
#define core_err(...) do { fprintf(stderr, "[Vk Core] \033[0;31mERROR: " __VA_ARGS__); fprintf(stderr, "\033[0m\n"); } while(0)

VkCore::VkCore(bool enable_debug) : _enable_debug(enable_debug) {
}


VkCore::~VkCore() {
    if(_enable_debug) {
        auto destroy_debug_messenger_func = (PFN_vkDestroyDebugUtilsMessengerEXT)
            _instance.get_instance_proc_addr("vkDestroyDebugUtilsMessengerEXT");

        if(destroy_debug_messenger_func != nullptr)
            destroy_debug_messenger_func(_instance.get_raw_handle(), _debug_messenger, nullptr);
        else
            core_err("Cannot load debug messenger destroy function");
    }

    // TODO: HACK: the swap chain should handle this itself
    vkDestroySwapchainKHR(_logical_device, _swap_chain.get_raw_handle(), nullptr);
    _instance.destroy_surface(_surface, nullptr);
    vkDestroyDevice(_logical_device, nullptr);

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

    VkPhysicalDevice physical_device = pick_physical_device();

    if(physical_device == VK_NULL_HANDLE) {
        core_err("Physical Device with graphics capabilities failed");
        return false;
    }
    else
        core_info("Find Suitable Physical Device Success");

    action_check(find_queue_families(physical_device, &_queue_family_idxs), "find suitable queue families");

    VkDevice logical_device = VK_NULL_HANDLE;
    action_check(setup_logical_device(physical_device, &logical_device), "Setup Logical Device");

    _device_manager_ptr = new VkDeviceManager {
        physical_device, logical_device
    };

    _logical_device = logical_device;

    _device_manager_ptr->get_queue(_queue_family_idxs.graphics.value(), &_graphics_queue);
    if(_graphics_queue == VK_NULL_HANDLE)
        core_err("graphics queue is null");
    else
        core_info("grabbed graphics queue");

    _device_manager_ptr->get_queue(_queue_family_idxs.present.value(), &_present_queue);
    if(_graphics_queue == VK_NULL_HANDLE)
        core_err("present queue is null");
    else
        core_info("grabbed present queue");


    action_check(create_swap_chain(window_ptr), "create swap chain");

    return true;
}


VkDeviceManager* VkCore::get_device_manager_ptr() {
    return _device_manager_ptr;
}

Swapchain* VkCore::get_swap_chain_ptr() {
    return &_swap_chain;
}


VkExtent2D VkCore::get_swap_chain_extent() const {
    return _chosen_extent;
}
const QueueFamilyIdxs* VkCore::get_queue_family_idxs_ptr() const {
    return &_queue_family_idxs;
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

    return _instance.init(&create_info, nullptr);
}

bool VkCore::setup_glfw_surface(GLFWwindow *window_ptr) {
    return glfwCreateWindowSurface(_instance.get_raw_handle(), window_ptr, nullptr, &_surface) == VK_SUCCESS;
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
        _instance.get_instance_proc_addr("vkCreateDebugUtilsMessengerEXT");

    if(create_func != nullptr) {
        create_func(_instance.get_raw_handle(), &create_info, nullptr, &_debug_messenger);
        core_info("Debug Messenger Created successfully");
    }
    else
        core_err("Debug Messenger Failed to create");
}

VkPhysicalDevice VkCore::pick_physical_device() {
    std::vector<VkPhysicalDevice> devices{};
    _instance.get_physical_devices(&devices);

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

    if(check_device_extension_support(device) == false)
        return false;
    // else extensions supported

    SwapChainSupportInfo support_info{};
    get_physical_swap_chain_support(device, _surface, &support_info);

    bool swap_chain_support = !support_info.formats.empty() && !support_info.present_modes.empty();

    if(swap_chain_support && device_features.geometryShader)
        return true;

    return false;
}


bool VkCore::find_queue_families(VkPhysicalDevice physical_device, QueueFamilyIdxs *idxs_ptr) {
    std::vector<VkQueueFamilyProperties> queue_family_props{};
    get_physical_queue_family_props(physical_device, &queue_family_props);

    for(size_t i = 0; i < queue_family_props.size(); i++) {
        const auto &family_prop = queue_family_props[i];

        if(family_prop.queueFlags & VK_QUEUE_GRAPHICS_BIT)
            idxs_ptr->graphics = i;

        if(is_physical_surface_supported(physical_device, i, _surface))
            idxs_ptr->present = i;

        if(idxs_ptr->graphics.has_value() && idxs_ptr->present.has_value())
            return true;
    }
    
    return false;
}

bool VkCore::setup_logical_device(VkPhysicalDevice physical_device, VkDevice *logical_device_ptr) {
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
    vkGetPhysicalDeviceFeatures(physical_device, &device_features);

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

    return vkCreateDevice(physical_device, &device_create_info, nullptr, logical_device_ptr) == VK_SUCCESS;
}

VkSurfaceFormatKHR get_best_swap_surface_format(const std::vector<VkSurfaceFormatKHR> *surface_formats_ptr) {
    for(const auto &surface_format : *surface_formats_ptr) {
        // best format hardcoded
        if(surface_format.format == VK_FORMAT_R8G8B8A8_SRGB &&
            surface_format.colorSpace == VK_COLOR_SPACE_SRGB_NONLINEAR_KHR)
            return surface_format;
    }

    assert(surface_formats_ptr->empty() == false && "Surface format vector cannot be empty!");
    return (*surface_formats_ptr)[0];
}

VkPresentModeKHR get_best_swap_present_mode(const std::vector<VkPresentModeKHR> *present_mode_ptr) {
    for(const auto &present_mode : *present_mode_ptr) {
        // best format hardcoded
        if(present_mode == VK_PRESENT_MODE_MAILBOX_KHR)
            return present_mode;
    }

    return VK_PRESENT_MODE_FIFO_KHR;
}

VkExtent2D get_glfw_best_swap_extent(GLFWwindow *window_ptr, const VkSurfaceCapabilitiesKHR *capabilities_ptr) {
    if(capabilities_ptr->currentExtent.width != std::numeric_limits<uint32_t>::max())
        return capabilities_ptr->currentExtent;
    // else

    int width, height;
    glfwGetFramebufferSize(window_ptr, &width, &height);
    
    VkExtent2D min_extent = capabilities_ptr->minImageExtent;
    VkExtent2D max_extent = capabilities_ptr->maxImageExtent;

    VkExtent2D extent {
        std::clamp(static_cast<uint32_t>(width), min_extent.width, max_extent.width),
        std::clamp(static_cast<uint32_t>(height), min_extent.height, max_extent.height)
    };

    return extent;
}

bool VkCore::create_swap_chain(GLFWwindow *window_ptr) {
    SwapChainSupportInfo support_info{};

    if(_device_manager_ptr->get_swap_chain_support(_surface, &support_info) == false)
        return false;
    // else
    
    VkSurfaceCapabilitiesKHR &capabilities = support_info.capabilities;

    VkSurfaceFormatKHR surface_format = get_best_swap_surface_format(&support_info.formats);
    VkPresentModeKHR   present_mode   = get_best_swap_present_mode(&support_info.present_modes);
    VkExtent2D         extent         = get_glfw_best_swap_extent(window_ptr, &support_info.capabilities);

    _chosen_img_format = surface_format.format;
    _chosen_extent = extent;

    uint32_t image_count = capabilities.minImageCount + 1;
    if(capabilities.maxImageCount > 0 && image_count > capabilities.maxImageCount)
        image_count = capabilities.maxImageCount;
    
    VkSwapchainCreateInfoKHR create_info{};
    create_info.sType = VK_STRUCTURE_TYPE_SWAPCHAIN_CREATE_INFO_KHR;
    create_info.surface = _surface;
    create_info.minImageCount = capabilities.minImageCount;
    create_info.imageFormat = surface_format.format;
    create_info.imageColorSpace = surface_format.colorSpace;
    create_info.imageExtent = extent;

    create_info.imageArrayLayers = 1;
    create_info.imageUsage = VK_IMAGE_USAGE_COLOR_ATTACHMENT_BIT;

    QueueFamilyIdxs idxs{};
    if(find_queue_families(_device_manager_ptr->get_physical(), &idxs) == false)
        return false;

    uint32_t queue_family_idxs[] = {idxs.graphics.value(), idxs.present.value()};

    if(idxs.graphics != idxs.present) {
        create_info.imageSharingMode = VK_SHARING_MODE_CONCURRENT;
        create_info.queueFamilyIndexCount = sizeof(queue_family_idxs) / sizeof(queue_family_idxs[0]);
        create_info.pQueueFamilyIndices = queue_family_idxs;
    }
    else
        create_info.imageSharingMode = VK_SHARING_MODE_EXCLUSIVE;

    create_info.preTransform = capabilities.currentTransform;

    create_info.compositeAlpha = VK_COMPOSITE_ALPHA_OPAQUE_BIT_KHR;

    create_info.presentMode = present_mode;
    create_info.clipped = VK_TRUE;

    // when window resize create new swap chain(not supported yet)
    create_info.oldSwapchain = VK_NULL_HANDLE;


    return _swap_chain.init(_device_manager_ptr->get_logical(), &create_info, nullptr);
}

VkFormat VkCore::get_chosen_img_format() const {
    return _chosen_img_format;
}

}; // namespace fl
