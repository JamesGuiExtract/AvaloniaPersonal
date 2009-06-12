// DisplayCOMDllLocation.cpp : Defines the entry point for the console application.
//

#include "stdafx.h"

#include <afxwin.h>
#include <string>
#include <iostream>
#include <fstream>
#include <map>
#include <vector>
#include <io.h>

using namespace std;

struct DLLInfo
{
	DLLInfo()
	:m_bExistsInRegistry(false), m_bExistsInLocation(false)
	{
	}

	string m_strDLLFileName;
	string m_strProgID;
	bool m_bExistsInRegistry;
	bool m_bExistsInLocation;
};


string getKeyValue(const string& strKeyName, const string& strKeyValueName)
{
	HKEY hKey;
	// open the key first 
	LONG ret = ::RegOpenKeyEx(HKEY_CLASSES_ROOT,         // handle to open key
							  TEXT(strKeyName.c_str()),					  // subkey name
							  0,							// reserved
							  KEY_QUERY_VALUE,					// security access mask
							  &hKey);				 // handle to open key

	// set max length to be 500
	TCHAR szValue[500];
	DWORD dwBufLen = 500;

	// if key doesn't exist, return empty
	if (ret != ERROR_SUCCESS)
	{
		::RegCloseKey(hKey);
		return "";
	}
	
	ret = ::RegQueryValueEx(hKey,
							TEXT(strKeyValueName.c_str()),
							NULL,
							NULL,
							(LPBYTE)szValue,
							&dwBufLen);
	
	if (ret != ERROR_SUCCESS)
	{
		::RegCloseKey(hKey);
		string strMsg("Failed to get the value of ");
		strMsg += strKeyName + strKeyValueName;
		cout << strMsg << endl;
		return "";
	}
	
	::RegCloseKey(hKey);

	return string(szValue);
}

string findCLSID(const string& strProgID)
{
	string strKey(strProgID + "\\CLSID");
	return getKeyValue(strKey, "");
}

string findDllLocation(const string& strCLSID)
{
	string strKey("CLSID\\");
	strKey += strCLSID;
	strKey += "\\InprocServer32";
	return getKeyValue(strKey, "");
}

void trim(string& strText)
{
	// trim off the leading and trailing spaces
	CString zText(strText.c_str());
	zText.TrimLeft(" \t");
	zText.TrimRight(" \t");
	strText = (LPCTSTR)zText;
}

bool fileExists(const string& strFullFileName)
{
	if (_access(strFullFileName.c_str(), 00) == 0)
	{
		return true;
	}

	return false;
}

void printLocation(const string& strDllListFile)
{
	map<string, vector<DLLInfo> > mapLocationToDLLInfo;

	string strDllLocation("");
	string strFound("Y");
	string strNotFound("N");
	string strNotFoundInfo(" can not be found in registry.");

	ifstream ifs(strDllListFile.c_str());
	string strText("");
	while(ifs)
	{
		getline(ifs, strText);
		trim(strText);
		if (!strText.empty())
		{
			DLLInfo dllInfo;
			dllInfo.m_strProgID = strText;

			// get the clsid from the prog id key
			string strCLSID(findCLSID(strText));
			if (!strCLSID.empty())
			{
				dllInfo.m_bExistsInRegistry = true;

				// get the dll location according to the clsid
				string strDllLocation(findDllLocation(strCLSID));
				if (!strDllLocation.empty())
				{
					char pszDLLDrive[_MAX_DRIVE], pszDLLFolder[_MAX_DIR], pszDLLFileName[_MAX_FNAME], pszDLLExtension[_MAX_EXT];

					// check whether or not the file is actually exists
					if (fileExists(strDllLocation))
					{
						dllInfo.m_bExistsInLocation = true;
						_splitpath_s(strDllLocation.c_str(), pszDLLDrive, pszDLLFolder, pszDLLFileName, pszDLLExtension);
						dllInfo.m_strDLLFileName = string(pszDLLFileName) + string(pszDLLExtension);
						string strDLLFolder = string(pszDLLDrive) + string(pszDLLFolder);
						mapLocationToDLLInfo[strDLLFolder].push_back(dllInfo);	
						continue;
					}

					static string strNotFoundInLocation = "Not found in location";
					dllInfo.m_strDLLFileName = strDllLocation;
					mapLocationToDLLInfo[strNotFoundInLocation].push_back(dllInfo);
					continue;
				}
			}

			static string strNotFoundInRegistry = "Not found in registry";
			mapLocationToDLLInfo[strNotFoundInRegistry].push_back(dllInfo);
		}
	}

	// print all the entries in the map
	map<string, vector<DLLInfo> >::const_iterator iter;
	for (iter = mapLocationToDLLInfo.begin(); iter != mapLocationToDLLInfo.end(); iter++)
	{
		cout << iter->first << endl;
		
		const vector<DLLInfo>& vecDLLInfo = iter->second;
		vector<DLLInfo>::const_iterator iter2;
		for (iter2 = vecDLLInfo.begin(); iter2 != vecDLLInfo.end(); iter2++)
		{
			cout << "   ";

			if (!iter2->m_bExistsInRegistry)
			{
				cout << iter2->m_strProgID << endl;
			}
			else
			{
				cout << "ProgId = " << iter2->m_strProgID << ", File = " << iter2->m_strDLLFileName << endl;
			}
		}
	}
}

int main(int argc, char* argv[])
{
	if (argc == 2)
	{
		// first parameter is reserved for dll list file
		string strDllListFile(argv[1]);

		// read the file and look in the registry, then print the result out
		printLocation(strDllListFile);
	}
	else
	{
		cout << "\nExpecting one parameter";
		cout << "\nUsage:";
		cout << "\n\targ1 - Fully qualified file name.";
		cout << endl;

		return 1;
	}

	return 0;
}
