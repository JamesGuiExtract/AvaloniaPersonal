#include "stdafx.h"
#include "cpputil.h"
#include "WindowsProcessData.h"
    
WindowsProcessData::WindowsProcessData()
{
	m_PID = GetCurrentProcessId();
    m_strProcessName = getProcessName();
    m_strComputerName = getComputerName();
    m_strVersion = getVersion();
    m_strUserName = getCurrentUserName();
}

WindowsProcessData::WindowsProcessData(const WindowsProcessData& wpd)
{
    m_strProcessName = wpd.m_strProcessName;
    m_strComputerName = wpd.m_strComputerName;
    m_PID = wpd.m_PID;
    m_strVersion = wpd.m_strVersion;
    m_strUserName = wpd.m_strUserName;
}

std::string WindowsProcessData::getProcessName()
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
}

std::string WindowsProcessData::getVersion()
{
    HMODULE hModule = NULL;
    GetModuleHandleEx(
        GET_MODULE_HANDLE_EX_FLAG_FROM_ADDRESS,
        NULL,
        &hModule);

    // Get module path and filename
    char zFileName[MAX_PATH];
    ::GetModuleFileName(hModule, zFileName, MAX_PATH);

    return ::getFileVersion(string(zFileName));
}
