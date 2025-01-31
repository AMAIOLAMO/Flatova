#define GLFW_INCLUDE_VULKAN
#include <GLFW/glfw3.h>

// flatova
#include <fl_application.hpp>

int main(void) {
    fl::Application app {
        1000, 800, "Flatova"
    };

    app.init();

    return app.run();
}
