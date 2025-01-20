#pragma once
#ifndef _FL_PIPELINE_H
#define _FL_PIPELINE_H

#include <string>

namespace fl {

// A Pipeline handles the generic linear rendering pipeline:
// -> Vertex & Index Buffer
// 1. Input Assembler
// 2. Vertex Shader
// 3. Rasterization
// 4. Fragment Shader
// 5. Color Blending
// <- Frame Buffer(Img)
class Pipeline {
public:
    Pipeline(const std::string &vert_path, const std::string &frag_path);
    ~Pipeline();

    Pipeline(Pipeline&) = delete;
    Pipeline& operator=(Pipeline&) = delete;

private:
    // creates a graphics pipeline
    bool create_graphics(const std::string &vert_path, const std::string &frag_path);
};

} // namespace fl

#endif // _FL_PIPELINE_H
