#include <vector>
#include <fstream>
#include <algorithm>
#include <typeinfo>
#include "generic_reader.h"
#include "enums.h"
#include "utils.h"
#include "logger.h"

using namespace std;
using namespace RTNet;

unsigned int _headings, _comments, _empties;
string _path;
vector<string> _lines, _linesOriginal;

void GenericReader::Init()
{
    _headings = 0;
    _comments = 0;
    _empties = 0;
    _lines = vector<string>();
    _linesOriginal = _lines;
}

void GenericReader::Init(const char* filename)
{
    _headings = 0;
    _comments = 0;
    _empties = 0;
    _lines = vector<string>();
    _path = string(filename);
    if(_path.empty())
        return;
    
    string dir = Utils::GetDirectoryName(_path);
    if(Utils::CreateDirIfNotExist(dir))
    {
        cout << "Created dir \"" << dir << "\"" << endl;
        Flush();
    }
    string line;
    ifstream input(filename);
    if(!input.is_open())
        LogWarning("Could not open \"%s\" - file could not be opened", _path.c_str());
    else
    {
        while(getline(input, line))
        {
            if(Utils::StartsWith(line, "["))
                line = "H_ = " + line.substr(1, line.size() - 2);
            if(Utils::StartsWith(line, "#"))
                line = "C_ = " + Utils::TrimStart(Utils::TrimEnd(line.substr(1)));
            else if(line.find("=") == string::npos)
                line = "E_";
            _lines.push_back(line);
        }
    }
    input.close();
    _linesOriginal = _lines;
}

string GenericReader::GetValue(string key, string defaultValue)
{
    for(unsigned int i = 0;i < _lines.size();i++)
        if(_lines[i].find(key) != string::npos)
            return Utils::Split(_lines[i], '=')[1].substr(1);
    return defaultValue;
}

void GenericReader::Write(string key, string value)
{
    string temp = key + " = " + value;
    _lines.push_back(temp);
}

template <class T> 
void GenericReader::Write(string key, T value)
{
    Write(key, Utils::ToString(value));
}

void GenericReader::Write(string key, int value)
{
    Write<int>(key, value);
}

void GenericReader::Write(string key, float value)
{
    Write(key, Utils::ToString(value));
}

void GenericReader::Write(string key, double value)
{
    Write<double>(key, value);
}

void GenericReader::Write(string key, bool value)
{
    Write(key, value ? "true" : "false");
}

void GenericReader::Comment(string comment)
{
    Write("C_", comment);
}

void GenericReader::Heading(string heading)
{
    Write("H_", heading);
}

void GenericReader::WriteLine()
{
    _lines.push_back("E_");
}

void GenericReader::Clear()
{
    _lines.clear();
}

string GenericReader::getPath()
{
    return _path;
}

void GenericReader::WriteToFile(const char* path)
{
    if(_lines == _linesOriginal) //no file change
        return;
    ofstream stream;
    stream.open(path);
    if(!stream.is_open())
    {
        LogError("Could not open file \"%s\"", path);
        return;
    }
    for(unsigned int i = 0;i < _lines.size();i++)
    {
        string line = _lines[i];
        if(Utils::StartsWith(line, "H_"))
            stream << "[" << line.substr(5) << "]" << endl;
        else  if(Utils::StartsWith(line, "C_"))
            stream << "# " << line.substr(5) << endl;
        else if(Utils::StartsWith(line, "E_"))
            stream << endl;
        else
            stream << line << endl;
    }
    LogDebug("Wrote %d lines to \"%s\"", _lines.size(), path);
    stream.close();
}

void GenericReader::Flush()
{
    if(!_path.empty())
        WriteToFile(_path.c_str());
    else
        cout << "Cannot flush when no path is set! Use WriteToFile instead." << endl;
}