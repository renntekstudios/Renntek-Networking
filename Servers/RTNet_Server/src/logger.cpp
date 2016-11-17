#include <iostream>
#include <cstdarg>
#include <sstream>
#include "logger.h"
#include "settings.h"

using namespace std;
using namespace RTNet;

void _internal_log(const char* file, int line, const string& message)
{
	stringstream ss;
	if(Settings::Timestamp)
		ss << "[" << __TIME__ << "]";
	if(Settings::DebugLine)
		ss << "(\"" << file << "\", line " << line << ") ";
	ss << message;
	cout << ss.str() << endl;
}

void _internal_log_debug(const char* file, int line, const string& message)
{
	if(!Settings::DebugMode)
		return;
	stringstream ss;
	if(Settings::Timestamp)
		ss << "[" << __TIME__ << "]";
	if(Settings::DebugLine)
		ss << "(\"" << file << "\", line " << line << ")";
	ss << "[DEBUG] " << message;
	cout << ss.str() << endl;
}

void _internal_log_error(const char* file, int line, const string& message)
{
	stringstream ss;
	if(Settings::Timestamp)
		ss << "[" << __TIME__ << "]";
	if(Settings::DebugLine)
		ss << "(\"" << file << "\", line " << line << ")";
	ss << "[ERROR] " << message;
	cerr << ss.str() << endl;
}

void _internal_log_warning(const char* file, int line, const string& message)
{
	stringstream ss;
	if(Settings::Timestamp)
		ss << "[" << __TIME__ << "]";
	if(Settings::DebugLine)
		ss << "(\"" << file << "\", line " << line << ")";
	ss << "[WARNING] " << message;
	cout << ss.str() << endl;
}

string _internal_string_vsprintf(const char *format, va_list args)
{
	va_list tmp_args;
	va_copy(tmp_args, args);
	const int required_len = vsnprintf(nullptr, 0, format, tmp_args) + 1;
	va_end(tmp_args);

	string buf(required_len, '\0');
	if(vsnprintf(&buf[0], buf.size(), format, args) < 0)
		_internal_log_error(__FILE__, __LINE__, "Encoding error!");
	return buf;
}

string _internal_format(const char *format, ...)
{
	va_list args;
	va_start(args, format);
	string str { _internal_string_vsprintf(format, args) };
	va_end(args);
	return str;
}