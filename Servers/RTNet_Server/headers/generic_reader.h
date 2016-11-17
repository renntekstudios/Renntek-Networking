#ifndef _GENERIC_READER_H_
#define _GENERIC_READER_H_
#include <string>
#include <iostream>
#include <algorithm>
#include <exception>
#include "logger.h"

using namespace std;

namespace RTNet
{
	class GenericReader
	{
	public:
		void Init();
		void Init(const char* path);
		
		string GetValue(string key, string defaultValue = "");

		string Read(string key, string defaultValue = "") { return GetValue(key, defaultValue); }
		int Read(string key, int defaultValue) { string value = GetValue(key); if(value.empty()) return defaultValue; return stoi(value); }
		float Read(string key, float defaultValue) { string value = GetValue(key); if(value.empty()) return defaultValue; return stof(value); }
		double Read(string key, double defaultValue) { string value = GetValue(key); if(value.empty()) return defaultValue; return stod(value); }
		bool Read(string key, bool defaultValue)
		{
			string value = GetValue(key);
			if(value.empty())
				return defaultValue;
			transform(value.begin(), value.end(), value.begin(), ::tolower);
			if(value == "true" || value == "1")
				return true;
			else if(value == "false" || value == "0")
				return false;
			return defaultValue;
		}

		void Write(string key, string value);
		void Write(string key, int value);
		void Write(string key, float value);
		void Write(string key, double value);
		void Write(string key, bool value);
		
		void Comment(string comment);
		void Heading(string heading);
		void WriteLine();
		void Clear();
					
		void WriteToFile(const char* path);
		void Flush();
		
		string getPath();
	private:
		template<class T> 
		void Write(string key, T value);
	};
}
#endif