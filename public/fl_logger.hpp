#pragma once
#ifndef _FL_LOGGER_H
#define _FL_LOGGER_H

#include <string>

namespace fl {

class Logger {
public:
    Logger(std::string prefix);

private:
    std::string _prefix;
};

} // namespace fl

#endif // _FL_LOGGER_H
