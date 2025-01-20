#include <fl_shader_utils.hpp>

#include <fstream>

namespace fl {

bool read_shader_compiled(const std::string &path, std::vector<char> *res_ptr) {
    std::ifstream file{
        path,
        std::ios::ate | std::ios::binary
    };

    if(file.is_open() == false)
        return false;
    
    // std::ios::ate seeks at end, hence tellg gives the position of the seek pointer
    // returns the length
    size_t file_size = static_cast<size_t>(file.tellg());

    res_ptr->resize(file_size);
    
    file.seekg(0);
    file.read(res_ptr->data(), file_size);

    file.close();

    return true;
}

} // namespace fl
