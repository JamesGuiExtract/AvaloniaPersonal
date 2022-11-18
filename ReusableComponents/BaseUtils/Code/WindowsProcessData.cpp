#include "stdafx.h"
#include "cpputil.h"
#include "WindowsProcessData.h"

template <typename Func, typename defaultType>
defaultType ReturnDefaultIfError(Func action, defaultType defaultValue)
{
	try
	{
		return action();
	}
	catch (...)
	{
		return defaultValue;
	}
}

WindowsProcessData::WindowsProcessData()
	: m_PID(0),
	m_strComputerName(""),
	m_strProcessName(""),
	m_strUserName(""),
	m_strVersion("")
{
	try
	{
		Initialize();
	}
	catch (...)
	{
		// Need to ignore errors and and let uninitialized values be the default;
	}
}

WindowsProcessData::WindowsProcessData(const WindowsProcessData& wpd)
{
	m_strProcessName = wpd.m_strProcessName;
	m_strComputerName = wpd.m_strComputerName;
	m_PID = wpd.m_PID;
	m_strVersion = wpd.m_strVersion;
	m_strUserName = wpd.m_strUserName;
}

void WindowsProcessData::Initialize()
{
	m_PID = (m_PID == 0) ? ReturnDefaultIfError(&GetCurrentProcessId, (DWORD)0) : 0;
	m_strProcessName = m_strProcessName.empty() ? getProcessName() : "";
	m_strComputerName = m_strComputerName.empty() ? ReturnDefaultIfError(&getComputerName, string("")) : "";
	m_strVersion = m_strVersion.empty() ? getVersion() : "";
	m_strUserName = m_strUserName.empty() ? ReturnDefaultIfError(&getCurrentUserName, string("")) : "";
}

std::string WindowsProcessData::getProcessName()
{
	return ReturnDefaultIfError([&]()
		{
			std::string ret = "Unknown";
	HANDLE handle = OpenProcess(
		PROCESS_QUERY_LIMITED_INFORMATION,
		FALSE,
		m_PID /* This is the PID, you can find one from windows task manager */
	);
	if (handle)
	{
		DWORD buffSize = 1024;
		CHAR buffer[1024];
		if (QueryFullProcessImageNameA(handle, 0, buffer, &buffSize))
		{
			ret = buffer;
		}
		CloseHandle(handle);
		ret = ::getFileNameFromFullPath(ret);
	}
	return ret;
		}, string(""));
}

std::string WindowsProcessData::getVersion()
{
	return ReturnDefaultIfError([&]()
		{
			HMODULE hModule = NULL;
	GetModuleHandleEx(
		GET_MODULE_HANDLE_EX_FLAG_FROM_ADDRESS,
		NULL,
		&hModule);

	// Get module path and filename
	char zFileName[MAX_PATH];
	::GetModuleFileName(hModule, zFileName, MAX_PATH);

	return getFileVersion(string(zFileName));

		}, string(""));
}

class MemoryReleaser
{
public:
	MemoryReleaser(LPVOID memoryPtr) : ptr(memoryPtr) {};
	~MemoryReleaser()
	{
		free(ptr);
	}

private:
	LPVOID ptr;

};

string WindowsProcessData::getFileVersion(string strFileFullName)
{
	string strVersion("");

	DWORD	dwHandle;
	UINT	uiDataSize;
	LPVOID	lpData;
	DWORD	dwSize;
	LPVOID	lpBuffer;
	LPTSTR	lpszImageName;
	char	Data[80] = { 0 }; // initialize array to empty

	lpszImageName = const_cast<char*>(strFileFullName.c_str());

	dwHandle = 0;
	lpData = (void*)(&Data);

	// Get the version information block size,
	// then use it to allocate a storage buffer.
	dwSize = ::GetFileVersionInfoSize(lpszImageName, &dwHandle);
	// check for error
	if (dwSize == 0)
	{
		// return empty string
		return string("");
	}

	lpBuffer = malloc(dwSize);
	MemoryReleaser memReleaser(lpBuffer);

	// Get the version information block
	if (::GetFileVersionInfo(lpszImageName, 0, dwSize, lpBuffer) == 0)
	{
		// free the lpBuffer and return empty string
		return string("");
	}

	// Use the Engligh and language neutral version information blocks to obtain the product name.
	if (VerQueryValue(lpBuffer, TEXT("\\StringFileInfo\\040904B0\\FileVersion"),
		&lpData, &uiDataSize) == 0 &&
		VerQueryValue(lpBuffer, TEXT("\\StringFileInfo\\000004B0\\FileVersion"),
			&lpData, &uiDataSize) == 0)
	{
		return string("");
	}

	// Replace commas with periods in version number substring
	strVersion = (char*)lpData;
	replaceVariable(strVersion, ", ", ".");

	return strVersion;
}


