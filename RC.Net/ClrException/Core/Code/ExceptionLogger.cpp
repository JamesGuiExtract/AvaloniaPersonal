#include "stdafx.h"
#include "ExceptionLogger.h"

#include <msclr\marshal_cppstd.h>
#include <msclr\marshal_windows.h>

using namespace System;
using namespace System::Configuration;
using namespace System::IO;
using namespace System::Reflection;
using namespace System::Security::Principal;
using namespace Extract::ErrorHandling;
using namespace msclr::interop;

bool ExceptionLogger::UseNetLogging = false;

ExceptionLogger::ExceptionLogger()
{
	auto commonAppData = Environment::GetFolderPath(Environment::SpecialFolder::CommonApplicationData);
	String^ logPath = Path::Combine(commonAppData, "Extract Systems\\LogFiles\\ExtractException.Uex");
	LogFileFullPath = marshal_as<string>(logPath);
	UseNetLogging = ExtractException::UseNetLogging;
}

ExceptionLogger::ExceptionLogger(const string& logFileFullPath)
{
	LogFileFullPath = logFileFullPath;
	UseNetLogging = ExtractException::UseNetLogging;
}
ExceptionLogger::ExceptionLogger(const char* logFileFullPath)
{
	LogFileFullPath = (logFileFullPath == nullptr) ? "" : logFileFullPath;
	UseNetLogging = ExtractException::UseNetLogging;
}

void ExceptionLogger::Log(string stringizedException )
{
	ExtractException^ ee = ExtractException::LoadFromByteStream(marshal_as<String^>(stringizedException));
	ee->Log(marshal_as<String^>(LogFileFullPath));
}

void ExceptionLogger::Display(string stringizedException)
{
	ExtractException^ ee = ExtractException::LoadFromByteStream(marshal_as<String^>(stringizedException));
	ee->Display();

}

