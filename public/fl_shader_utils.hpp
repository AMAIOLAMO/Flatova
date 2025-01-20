#pragma once
#ifndef _FL_SHADER_UTILS_H
#define _FL_SHADER_UTILS_H

#include <string>
#include <vector>

namespace fl {

bool read_shader_compiled(const std::string &path, std::vector<char> *res_ptr);

} // namespace fl

#endif // _FL_SHADER_UTILS_H
