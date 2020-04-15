#pragma once

#include "BaseUtils.h"

#include <afxmt.h>

#include <string>
#include <vector>

using namespace std;

class EXPORT_BaseUtils EnvironmentInfo
{
public:

	EnvironmentInfo();
	~EnvironmentInfo();

	string GetExtractVersion();
	// Will automatically initialize license data; An empty vector will be returned if licensing
	// is not currently available or in a bad state (exception will not be thrown).
	vector<unsigned long> GetLicensedComponents();
	// Will automatically initialize license data; An "[License error]" or "[Unlicensed]" will be
	// returned if licensing is not currently available or in a bad state (exception will not be thrown).
	vector<string> GetLicensedPackages(bool bIncludeExpriationDates = true);
	string GetOSInfo();
	vector<string> GetMachineInfo();
	void GetMemoryInfo(unsigned long long& rullVirtualMemTotal
		, unsigned long long& rullVirtualMemUsed
		, unsigned long long& rullPhysMemTotal
		, unsigned long long& rullPhysMemUsed);

	string ToString(bool bVersion = true
		, bool bLicensedPackages = true
		, bool bLicenseCodes = false
		, bool bOSInfo = true
		, bool bMachineInfo = true);

	static DWORD ms_dwOSMajorVersion;
	static DWORD ms_dwOSMinorVersion;
	static WORD ms_wOSServicePackMajor;
	static WORD ms_wOSServicePackMinor;
	static DWORD ms_dwOSBuildNumber;
	static string ms_strServicePackVersion;
	static DWORD ms_dwReleaseID;
	static DWORD ms_dwOSProductType;
	static bool ms_bIs64Bit;
	static bool ms_bIsServer;
	static string ms_strOSName;
	static string ms_strOSInfo;

	static string ms_strExtractVersion;
	static unsigned long ms_lNumLogicalCpus;

	static long ms_nLicenseExpiration;
	static vector<unsigned long> ms_vecLicensedComponents;
	static vector<DATE> ms_vecComponentExpirationDates;
	static vector<string> ms_vecLicensedPackageNames;
	static vector<DATE> ms_vecPackageExpirationDates;

	static void InitializeLicenseData();

private: 

	static CMutex ms_Mutex;
	static bool ms_staticsInitialized;
	static bool ms_licenseDataInitialized;
	static bool ms_bLicenseError;

	static void InitializeStatics();
};
