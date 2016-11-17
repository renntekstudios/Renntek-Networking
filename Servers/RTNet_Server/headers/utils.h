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

#ifdef PLATFORM_WINDOWS
#include <windows.h>
#include <winsock2.h>
#else
#include <sys/stat.h>
#include <sys/types.h>
#endif

#include <GL/glew.h>
#include <GLFW/glfw3.h>

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
                #ifdef PLATFORM_WINDOWS
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

        // reference to http://stackoverflow.com/questions/306533/how-do-i-get-a-list-of-files-in-a-directory-in-c
        static vector<string> GetFilesInDir(string directory)
        {
            vector<string> files;
            if(directory.empty())
                return files;
            #ifdef PLATFORM_WINDOWS
            HANDLE dir;
            WIN32_FIND_DATA file_data;

            if((dir = FindFirstFile((directory + "/*").c_str(), &file_data)) == INVALID_HANDLE_VALUE)
                return files;

            do
            {
                const string file_name = file_data.cFileName;
                const bool is_directory = (file_data.dwFileAttributes & FILE_ATTRIBUTE_DIRECTORY) != 0;

                if(file_name[0] == '.' || is_directory)
                    continue;
                files.push_back(file_name);
            } while (FindNextFile(dir, &file_data));

            FindClose(dir);
            #else
            DIR* dir;
            class dirent* ent;
            class stat st;

            dir = opendir(directory);
            while((ent = readdir(dir)) != NULL)
            {
                const string file_name = ent->d_name;

                if(file_name[0] == '.' || stat(file_name.c_str(), &st) == -1)
                    continue;
                const bool is_directory = (st.st_mode & S_IFDIR) != 0;

                if(is_directory)
                    continue;
                files.push_back(file_name);
            }
            closedir(dir);
            #endif
            return files;
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
                #ifdef PLATFORM_WINDOWS
                LPSTR error_string = NULL;
                ss << error_string;
                LocalFree(error_string);
                #else
                ss << "Error messages not implemented yet";
                #endif
            }
            ss << "\n";
            return ss.str();
        }

        static void SetTitle(string title)
        {
            #ifdef PLATFORM_WINDOWS
            SetConsoleTitle(TEXT(title.c_str()));
            #elif PLATFORM_MAC
            
            #else
            /*
            char esc_start[] = { 0x1b, ']', '0', ';', 0 };
            char esc_end[] = { 0x07, 0 };
            cout << esc_start << title << esc_end;
            */
            cout << "\033]0;" << title << "\007";
            #endif
        }
    };
}

extern bool _internal_GLError(const char* file, int line);
#define GLError() _internal_GLError(__FILE__, __LINE__)
#endif