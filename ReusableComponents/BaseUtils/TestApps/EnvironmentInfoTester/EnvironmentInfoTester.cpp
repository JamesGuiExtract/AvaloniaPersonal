// EnvironmentTester.cpp : This file contains the 'main' function. Program execution begins and ends there.
#include "stdafx.h"

#include <cpputil.h>
#include <EnvironmentInfo.h>

#include <iostream>
#include <string>

using namespace std;

int main()
{
    CoInitializeEx(NULL, COINIT_MULTITHREADED);

    EnvironmentInfo envInfo;

    cout << "--- Individual OS/Machine info values ---" << endl;
    cout << "OSMajorVersion:\t\t" << asString(envInfo.ms_dwOSMajorVersion) << endl;
    cout << "OSMinorVersion:\t\t" << asString(envInfo.ms_dwOSMinorVersion) << endl;
    cout << "OSServicePackMajor:\t" << asString(envInfo.ms_wOSServicePackMajor) << endl;
    cout << "OSServicePackMinor:\t" << asString(envInfo.ms_wOSServicePackMinor) << endl;
    cout << "OSBuildNumber:\t\t" << asString(envInfo.ms_dwOSBuildNumber) << endl;
    cout << "ServicePackVersion:\t" << envInfo.ms_strServicePackVersion << endl;
    cout << "OSProductType:\t\t" << asString(envInfo.ms_dwOSProductType) << endl;
    cout << "ReleaseID:\t\t" << asString(envInfo.ms_dwReleaseID) << endl;
    cout << "IsServer:\t\t" << asString(envInfo.ms_bIsServer) << endl;
    cout << "Is64Bit:\t\t" << asString(envInfo.ms_bIs64Bit) << endl;
    cout << "OSName:\t\t\t" << envInfo.ms_strOSName << endl;
    cout << "ExtractVersion:\t\t" << envInfo.ms_strExtractVersion << endl;
    cout << "NumLogicalCpus:\t\t" << asString(envInfo.ms_lNumLogicalCpus) << endl;
    cout << endl;

    unsigned long long ullVirtualMemTotal, ullVirtualMemUsed, ullPhysMemTotal, ullPhysMemUsed = 0;
    envInfo.GetMemoryInfo(ullVirtualMemTotal, ullVirtualMemUsed, ullPhysMemTotal, ullPhysMemUsed);

    cout << "--- Individual memory info values ---" << endl;
    cout << "VirtualMemTotal:\t" << asString(ullVirtualMemTotal / (1024 * 1024)) << endl;
    cout << "VirtualMemUsed:\t\t" << asString(ullVirtualMemUsed / (1024 * 1024)) << endl;
    cout << "PhysMemTotal:\t\t" << asString(ullPhysMemTotal / (1024 * 1024)) << endl;
    cout << "PhysMemUsed:\t\t" << asString(ullPhysMemUsed / (1024 * 1024)) << endl;
    cout << endl;

    cout << "--- ToString output ---" << endl;
    cout << envInfo.ToString();
    cout << endl << endl;
    
    system("pause");

    CoUninitialize();
}
