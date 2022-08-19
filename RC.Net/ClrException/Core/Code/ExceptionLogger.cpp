#include "stdafx.h"
#include "ExceptionLogger.h"

#include <msclr\marshal_cppstd.h>
#include <msclr\marshal_windows.h>

using namespace System;
using namespace System::Configuration;
using namespace System::IO;
using namespace System::Security::Principal;
using namespace Extract::ErrorHandling;
using namespace msclr::interop;

bool ExceptionLogger::m_sbConfigurationLoaded = false;
bool ExceptionLogger::m_sbUseNetLogging = false;

ExceptionLogger::ExceptionLogger()
{
	auto commonAppData = Environment::GetFolderPath(Environment::SpecialFolder::CommonApplicationData);
	String^ logPath = Path::Combine(commonAppData, "Extract Systems\\LogFiles\\ExtractException.Uex");
	LogFileFullPath = marshal_as<string>(logPath);
}

ExceptionLogger::ExceptionLogger(const string& logFileFullPath)
{
	LogFileFullPath = logFileFullPath;
}
ExceptionLogger::ExceptionLogger(const char* logFileFullPath)
{
	LogFileFullPath = (logFileFullPath == nullptr) ? "" : logFileFullPath;
}

void ExceptionLogger::Log(string stringizedException )
{
	ExtractException^ ee = ExtractException::LoadFromByteStream(marshal_as<String^>(stringizedException));
	ee->Log(marshal_as<String^>(LogFileFullPath), false);
}

bool ExceptionLogger::UseNetLogging()
{
	try
	{
		if (m_sbConfigurationLoaded) return m_sbUseNetLogging;

		ExeConfigurationFileMap^ configMap = gcnew ExeConfigurationFileMap();

		auto commonAppData = Environment::GetFolderPath(Environment::SpecialFolder::CommonApplicationData);
		String^ configPath = Path::Combine(commonAppData, "Extract Systems\\Configuration\\ExceptionSettings.config");
		if (!File::Exists(configPath))
		{
			return false;
		}
		configMap->ExeConfigFilename = configPath;
		auto config = ConfigurationManager::OpenMappedExeConfiguration(configMap, ConfigurationUserLevel::None);
		auto UseNetLogging = config->AppSettings->Settings["UseNetLogging"]->Value;

		m_sbUseNetLogging = ((String^)UseNetLogging)->Equals("1");
		m_sbConfigurationLoaded = true;
		return m_sbUseNetLogging;
	}
	catch (Exception^ netex)
	{
		ExtractException^ ee = gcnew ExtractException("ELI53550", "Unable to get ExceptionConfiguration.", 
			ExceptionExtensionMethods::AsExtractException(netex, "ELI53551"));
		ee->Log();
	}
	return false;
}
