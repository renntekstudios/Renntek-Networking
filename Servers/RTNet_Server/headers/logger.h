#ifndef _LOGGER_H_
#define _LOGGER_H_
#include <string>
#include <cstdarg>

using namespace std;

extern void _internal_log(const char* file, int line, const string& message);
extern void _internal_log_debug(const char* file, int line, const string& message);
extern void _internal_log_error(const char* file, int line, const string& message);
extern void _internal_log_warning(const char* file, int line, const string& message);
extern string _internal_string_vsprintf(const char *format, va_list args);
extern string _internal_format(const char *format, ...);

#define Log(message, ...) _internal_log(__FILE__, __LINE__, _internal_format(message, ## __VA_ARGS__))
#define LogDebug(message, ...) _internal_log_debug(__FILE__, __LINE__, _internal_format(message, ## __VA_ARGS__))
#define LogError(message, ...) _internal_log_error(__FILE__, __LINE__, _internal_format(message, ## __VA_ARGS__))
#define LogWarning(message, ...) _internal_log_warning(__FILE__, __LINE__, _internal_format(message, ## __VA_ARGS__))

#define LogErrorRaw(message, file, line, ...) _internal_log_error(file, line, _internal_format(message, ## __VA_ARGS__))
#endif