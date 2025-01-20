#include <fl_pipeline.hpp>
#include <fl_shader_utils.hpp>

#include <stdio.h>

namespace fl {

Pipeline::Pipeline(const std::string &vert_path, const std::string &frag_path) {
    if(create_graphics(vert_path, frag_path) == false) {
        fprintf(stderr, "[Pipeline] failed create graphics pipeline\n");
    }
}

Pipeline::~Pipeline() {
    
}

bool Pipeline::create_graphics(const std::string &vert_path, const std::string &frag_path) {
    std::vector<char> vert_shader{};
    std::vector<char> frag_shader{};

    if(read_shader_compiled(vert_path, &vert_shader) == false)
        return false;

    if(read_shader_compiled(frag_path, &frag_shader) == false)
        return false;

    printf("[Pipeline] Vertex Shader Code Size: %zu\n", vert_shader.size());
    printf("[Pipeline] Fragment Shader Code Size: %zu\n", frag_shader.size());

    return true;
}

} // namespace fl
