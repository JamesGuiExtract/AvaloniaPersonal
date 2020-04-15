
#include "stdafx.h"
#include "BaseUtils.h"
#include "EnvironmentInfo.h"
#include "cpputil.h"
#include "SafeArrayAccessGuard.h"
#include "COMUtils.h"
#include "UCLIDException.h"

#include <VersionHelpers.h>

#include <codecvt>

// Registry hive containing info about the current Windows installation.
#define REGKEY_WINDOWS_CURRENT_VERSION "SOFTWARE\\Microsoft\\Windows NT\\CurrentVersion" 

CMutex EnvironmentInfo::ms_Mutex;
bool EnvironmentInfo::ms_staticsInitialized = false;
bool EnvironmentInfo::ms_licenseDataInitialized = false;
bool EnvironmentInfo::ms_bLicenseError = false;

DWORD EnvironmentInfo::ms_dwOSMajorVersion = 0;
DWORD EnvironmentInfo::ms_dwOSMinorVersion = 0;
WORD EnvironmentInfo::ms_wOSServicePackMajor = 0;
WORD EnvironmentInfo::ms_wOSServicePackMinor = 0;
DWORD EnvironmentInfo::ms_dwOSBuildNumber = 0;
string EnvironmentInfo::ms_strServicePackVersion = "";
DWORD  EnvironmentInfo::ms_dwReleaseID = 0;
DWORD EnvironmentInfo::ms_dwOSProductType = 0;
bool EnvironmentInfo::ms_bIsServer = false;
bool EnvironmentInfo::ms_bIs64Bit = false;
string EnvironmentInfo::ms_strOSName = "";
string EnvironmentInfo::ms_strOSInfo = "";
string EnvironmentInfo::ms_strExtractVersion = "";
unsigned long EnvironmentInfo::ms_lNumLogicalCpus = 0;

long EnvironmentInfo::ms_nLicenseExpiration = 0;
vector<unsigned long> EnvironmentInfo::ms_vecLicensedComponents;
vector<DATE> EnvironmentInfo::ms_vecComponentExpirationDates;
vector<string> EnvironmentInfo::ms_vecLicensedPackageNames;
vector<DATE> EnvironmentInfo::ms_vecPackageExpirationDates;

//--------------------------------------------------------------------------------------------------
// EnvironmentInfo class
//--------------------------------------------------------------------------------------------------
EnvironmentInfo::EnvironmentInfo()
{
	if (!ms_staticsInitialized)
	{
		InitializeStatics();
	}
}
//--------------------------------------------------------------------------------------------------
EnvironmentInfo::~EnvironmentInfo()
{
	try
	{
		
	}
	CATCH_AND_LOG_ALL_EXCEPTIONS("ELI49758");
}
//--------------------------------------------------------------------------------------------------
void EnvironmentInfo::GetMemoryInfo(unsigned long long& rullVirtualMemTotal
	, unsigned long long& rullVirtualMemUsed
	, unsigned long long& rullPhysMemTotal
	, unsigned long long& rullPhysMemUsed)
{
	try
	{
		MEMORYSTATUSEX memInfo = { 0 };
		memInfo.dwLength = sizeof(MEMORYSTATUSEX);
		GlobalMemoryStatusEx(&memInfo);

		rullVirtualMemTotal = memInfo.ullTotalPageFile;
		rullVirtualMemUsed = memInfo.ullTotalPageFile - memInfo.ullAvailPageFile;
		rullPhysMemTotal = memInfo.ullTotalPhys;
		rullPhysMemUsed = memInfo.ullTotalPhys - memInfo.ullAvailPhys;
	}
	catch (...)
	{
		rullVirtualMemTotal = 0;
		rullVirtualMemUsed = 0;
		rullPhysMemTotal = 0;
		rullPhysMemUsed = 0;
	}
}
//--------------------------------------------------------------------------------------------------
string EnvironmentInfo::ToString(bool bVersion/*= true*/
	, bool bLicensedPackages /*= true*/
	, bool bLicenseCodes /*= true*/
	, bool bOSInfo /*= true*/
	, bool bMachineInfo /*= true*/)
{
	vector<string> vecEnvInfoLines;

	if (bVersion)
	{
		vecEnvInfoLines.push_back("Extract Version: " + ms_strExtractVersion);
	}

	if (bLicensedPackages)
	{
		vector<string> vecLicensedPackages = GetLicensedPackages();
		vecEnvInfoLines.insert(vecEnvInfoLines.end(), vecLicensedPackages.begin(), vecLicensedPackages.end());
	}

	if (bLicenseCodes)
	{
		vector<unsigned long> vecLicensedComponents = GetLicensedComponents();
		string strLicensedCodes;
		for each (unsigned long nCode in vecLicensedComponents)
		{
			if (!strLicensedCodes.empty())
			{
				strLicensedCodes += ", ";
			}

			strLicensedCodes += asString(nCode);
		}

		vecEnvInfoLines.push_back("License codes: " + strLicensedCodes);
	}

	if (bOSInfo)
	{
		vecEnvInfoLines.push_back(ms_strOSInfo);
	}

	if (bMachineInfo)
	{
		vector<string> vecMachineInfo = GetMachineInfo();
		vecEnvInfoLines.insert(vecEnvInfoLines.end(), vecMachineInfo.begin(), vecMachineInfo.end());
	}

	return asString(vecEnvInfoLines, false, "\r\n");
}
//--------------------------------------------------------------------------------------------------
string EnvironmentInfo::GetExtractVersion()
{
	return ms_strExtractVersion;
}
//--------------------------------------------------------------------------------------------------
vector<string> EnvironmentInfo::GetLicensedPackages(bool bIncludeExpriationDates /*= true*/)
{
	if (!ms_licenseDataInitialized)
	{
		InitializeLicenseData();

		if (!ms_licenseDataInitialized)
		{
			vector<string> vecUninitialized;
			if (ms_bLicenseError)
			{
				vecUninitialized.push_back("[License error]");
			}
			else
			{
				vecUninitialized.push_back("[Unlicensed]");
			}

			return vecUninitialized;
		}
	}

	if (bIncludeExpriationDates)
	{
		vector<string> vecPackagesWithExpiration;
		for (size_t i = 0; i < ms_vecLicensedPackageNames.size(); i++)
		{
			string strPackage = ms_vecLicensedPackageNames[i];
			if (ms_vecPackageExpirationDates[i] > 0)
			{
				COleDateTime date(ms_vecPackageExpirationDates[i]);
				CString szExpriation = date.Format("%m/%d/%y");
				strPackage = string("*") + strPackage + string(" *Expires: ") + (LPCTSTR)szExpriation;
			}

			vecPackagesWithExpiration.push_back(strPackage);
		}

		return vecPackagesWithExpiration;
	}
	else
	{
		return ms_vecLicensedPackageNames;
	}
}
//--------------------------------------------------------------------------------------------------
vector<unsigned long> EnvironmentInfo::GetLicensedComponents()
{
	if (!ms_licenseDataInitialized)
	{
		InitializeLicenseData();
	}

	return ms_vecLicensedComponents;
}
//--------------------------------------------------------------------------------------------------
vector<string> EnvironmentInfo::GetMachineInfo()
{
	unsigned long long ullVirtualMemTotal, ullVirtualMemUsed, ullPhysMemTotal, ullPhysMemUsed = 0;
	GetMemoryInfo(ullVirtualMemTotal, ullVirtualMemUsed, ullPhysMemTotal, ullPhysMemUsed);

	vector<string> vecMachineInfo;
	vecMachineInfo.push_back(string("Logical CPUs: ") + asString(ms_lNumLogicalCpus));
	vecMachineInfo.push_back(
		+"Memory Used: Physical " + asString(ullPhysMemUsed / (1024 * 1024))
		+ "/" + asString(ullPhysMemTotal / (1024 * 1024)) + " MB"
		+ ", Virtual " + asString(ullVirtualMemUsed / (1024 * 1024))
		+ "/" + asString(ullVirtualMemTotal / (1024 * 1024)) + " MB");

	return vecMachineInfo;
}
//--------------------------------------------------------------------------------------------------
void EnvironmentInfo::InitializeStatics()
{
	CSingleLock(&ms_Mutex, TRUE);

	if (!ms_staticsInitialized)
	{
		RegistryPersistenceMgr registryManager(HKEY_LOCAL_MACHINE, REGKEY_WINDOWS_CURRENT_VERSION);

		SYSTEM_INFO sysInfo = { 0 };
		GetNativeSystemInfo(&sysInfo);
		ms_bIs64Bit = (sysInfo.wProcessorArchitecture == PROCESSOR_ARCHITECTURE_AMD64);
		ms_lNumLogicalCpus = sysInfo.dwNumberOfProcessors;

		// Calls such as GetVersion depend on an application being manifested for the most recent version
		// of Windows. This method should return the actual Windows version regardless of which application
		// is calling and whether that app is properly manifiested; Using RtlGetVersion from ntdll can do
		// that.
		NTSTATUS(WINAPI * RtlGetVersion)(LPOSVERSIONINFOEXW);
		HMODULE hNTdll = GetModuleHandle("ntdll");
		*(FARPROC*)&RtlGetVersion = GetProcAddress(hNTdll, "RtlGetVersion");

		if (RtlGetVersion != __nullptr)
		{
			OSVERSIONINFOEXW osvi = { 0 };
			osvi.dwOSVersionInfoSize = sizeof(OSVERSIONINFOEXW);
			RtlGetVersion((LPOSVERSIONINFOEXW)&osvi);

			ms_dwOSMajorVersion = osvi.dwMajorVersion;
			ms_dwOSMinorVersion = osvi.dwMinorVersion;
			ms_wOSServicePackMajor = osvi.wServicePackMajor;
			ms_wOSServicePackMinor = osvi.wServicePackMinor;

			wstring_convert<std::codecvt_utf8_utf16<wchar_t>> wcharConverter;
			ms_strServicePackVersion = wcharConverter.to_bytes(osvi.szCSDVersion);
			ms_dwOSBuildNumber = osvi.dwBuildNumber;
			ms_bIsServer = (osvi.wProductType != VER_NT_WORKSTATION);

			DWORD dwOSProductType;
			if (asCppBool(GetProductInfo(
				osvi.dwMajorVersion, osvi.dwMinorVersion, osvi.wServicePackMajor, osvi.wServicePackMinor, &dwOSProductType)))
			{
				ms_dwOSProductType = dwOSProductType;
			}
		}

		ms_dwReleaseID = asUnsignedLong(registryManager.getKeyValue("", "ReleaseId", "0"));

		ms_strOSName = registryManager.getKeyValue("", "ProductName", "(Unknown)");
		if (ms_dwOSBuildNumber != asUnsignedLong(registryManager.getKeyValue("", "CurrentBuildNumber", "0")))
		{
			ms_strOSName += " (Unverified)";
		}

		char szFileName[MAX_PATH] = { 0 };
		HMODULE handle = ::GetModuleHandle("BaseUtils.dll");
		int ret = ::GetModuleFileName(handle, szFileName, MAX_PATH);
		if (ret != 0)
		{
			ms_strExtractVersion = getFileVersion(szFileName);
		}

		ms_strOSInfo = ms_strOSName
			+ (ms_bIs64Bit ? " 64-bit" : " 32-bit")
			+ " (" + asString(ms_dwOSMajorVersion) + "." + asString(ms_dwOSMinorVersion);

		if (!ms_strServicePackVersion.empty())
		{
			ms_strOSInfo += ", " + ms_strServicePackVersion;
		}
		else if (ms_wOSServicePackMajor > 0)
		{
			ms_strOSInfo += ", SP " + asString(ms_wOSServicePackMajor) + "." + asString(ms_wOSServicePackMinor);
		}
		
		if (ms_dwReleaseID > 0)
		{
			ms_strOSInfo += ", Release " + asString(ms_dwReleaseID);
		}

		ms_strOSInfo += ", Build " + asString(ms_dwOSBuildNumber);
		ms_strOSInfo += ")";

		ms_staticsInitialized = true;
	}
}
//--------------------------------------------------------------------------------------------------
void EnvironmentInfo::InitializeLicenseData()
{
	try
	{
		CSingleLock(&ms_Mutex, TRUE);

		if (!ms_licenseDataInitialized)
		{
			ms_vecLicensedComponents.clear();
			ms_vecLicensedPackageNames.clear();

			ILicenseInfoPtr licenseInfo;
			CLSID clsidCOMLM = { 0xE129D8F2, 0xE327, 0x4FC1, { 0x8C, 0xBC, 0x97, 0x7F, 0xA8, 0x76, 0xEF, 0xC2} };
			licenseInfo.CreateInstance(clsidCOMLM);

			LPSAFEARRAY pComponentExpirationDates = NULL;
			LPSAFEARRAY psaComponents = licenseInfo->GetLicensedComponents(&pComponentExpirationDates);

			CComSafeArray<unsigned long> saComponents;
			saComponents.Attach(psaComponents);

			CComSafeArray<DATE> saComponentExpriationDates;
			saComponentExpriationDates.Attach(pComponentExpirationDates);

			saComponents.GetCount();

			long nCount = saComponents.GetCount();
			for (long i = 0; i < nCount; i++)
			{
				ms_vecLicensedComponents.push_back(saComponents[i]);
				ms_vecComponentExpirationDates.push_back(saComponentExpriationDates[i]);
			}

			LPSAFEARRAY pPackageExpirationDates = NULL;
			LPSAFEARRAY psaPackageNames = licenseInfo->GetLicensedPackageNames(&pPackageExpirationDates);

			CComSafeArray<BSTR> saPackageNames;
			saPackageNames.Attach(psaPackageNames);

			CComSafeArray<DATE> saPackageExpirationDates;
			saPackageExpirationDates.Attach(pPackageExpirationDates);

			nCount = saPackageNames.GetCount();
			for (long i = 0; i < nCount; i++)
			{
				ms_vecLicensedPackageNames.push_back(asString(saPackageNames[i]));
				ms_vecPackageExpirationDates.push_back(saPackageExpirationDates[i]);
			}

			ms_licenseDataInitialized = true;
		}
	}
	catch (...) 
	{ 
		ms_bLicenseError = true;
	}
}