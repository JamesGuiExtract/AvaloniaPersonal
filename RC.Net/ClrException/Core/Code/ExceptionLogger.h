#pragma once

#include "ClrException.h"
#include <string>


using namespace std;


class EXPORT_ClrException ExceptionLogger
{
public:
	ExceptionLogger();
	ExceptionLogger(const string& logFileFullPath);
	ExceptionLogger(const char* logFileFullPath);

	void Log(string stringizedException);

	static bool UseNetLogging;

private:
	string LogFileFullPath;
};

