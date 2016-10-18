#ifndef _UTILS_H_
#define _UTILS_H_
#include <string>
#include <vector>
#include <algorithm>
#include <iostream>
#include <sys/stat.h>
#include <sstream>
#include <cstring>
#include <typeinfo>
#include "enums.h"
#include "logger.h"

#ifdef _WIN32
#define WIN32_LEAN_AND_MEAN
#include <windows.h>
#include <winsock2.h>
#else
#include <sys/stat.h>
#include <sys/types.h>
#endif

#define TYPE_STRING "s"
#define TYPE_FLOAT "f"
#define TYPE_INT "i"
#define TYPE_BOOL "b"
#define TYPE_DOUBLE "d"

using namespace std;

namespace RTNet
{
    class Utils
    {
    public:
        static bool CreateDirIfNotExist(string dir)
        {
            if(dir.empty())
                return false;
            struct stat st;
            if(stat(dir.c_str(), &st) != 0)
            {
                #ifdef _WIN32
                int result = CreateDirectory(dir.c_str(), NULL) ? 0 : -1;
                #else
                int result = mkdir(dir.c_str(), 777);
                #endif
                if(result == 0)
                    return true;
                else
                    return false;
            }
            return false;
        }

        static int ArrayLength(float* array)
        {
            return sizeof(array) / sizeof(float);
        }

        static bool StartsWith(string a, string contains)
        {
            if(a.size() < contains.size())
                return false;
            if(a.find(contains) == 0)
                return true;
            return false;
        }

        static bool Contains(string a, string contains)
        {
            if(a.size() < contains.size())
                return false;
            if(a.find(contains) != string::npos)
                return true;
            return false;
        }

        static void Split(string line, char delim, vector<string>& elems)
        {
            stringstream ss(line);
            string item;
            while(getline(ss, item, delim))
                elems.push_back(item);
        }

        static vector<string> Split(string line, char delim)
        {
            vector<string> elems;
            Split(line, delim, elems);
            return elems;
        }

        static string GetDirectoryName(string path)
        {
            return path.substr(0, path.find_last_of("/\\") + 1);
        }

        static string GetFileName(string path)
        {
            return path.substr(path.find_last_of("/\\") + 1);
        }

        static string GetFileExtension(string path)
        {
            return path.substr(path.find_last_of(".") + 1);
        }

        static bool FileExists(string path)
        {
            struct stat buffer;
            return (stat(path.c_str(), &buffer) == 0);
        }

        static string Replace(const string& original, const char a, const char b)
        {
            string temp = original;
            replace(temp.begin(), temp.end(), a, b);
            return temp;
        }

        template<class T>
        static string ToString(T value)
        {
            #ifdef __APPLE__
            stringstream s(stringstream::in | stringstream::out);
            s << value;
            return s.str();
            #else // use hax
            return static_cast<stringstream*>(&(stringstream(stringstream::in | stringstream::out) << value))->str();
            #endif
        }

        static string TrimStart(const string& s)
        {
            string temp = s;
            while(temp[0] == ' ' || temp[0] == '\0')
                temp = temp.substr(1);
            return temp;
        }

        static string TrimEnd(const string& s)
        {
            string temp = s;
            while(temp[temp.size() - 1] == ' ' || temp[temp.size() - 1] == '\0')
                temp = temp.substr(0, temp.size() - 1);
            return temp;
        }

        static string GetErrorMessage(int error, bool os_specific = true)
        {
            stringstream ss(stringstream::in | stringstream::out);
            if(os_specific)
            {
                #ifdef _WIN32
                LPSTR error_string = NULL;
                int size = FormatMessage(FORMAT_MESSAGE_ALLOCATE_BUFFER | FORMAT_MESSAGE_FROM_SYSTEM, 0, error, 0, (LPSTR)&error_string, 0, 0);
                ss << error_string;
                LocalFree(error_string);
                #else
                ss << "Error messages not implemented yet";
                #endif
            }
            ss << "\n";
            return ss.str();
        }
    };
}

extern bool _internal_GLError(const char* file, int line);
#define GLError() _internal_GLError(__FILE__, __LINE__)
#endif