#pragma warning(disable:4786)

#define VC_EXTRALEAN		// Exclude rarely-used stuff from Windows headers

#include <CommonToExtractProducts.h>

#include <afxwin.h>         // MFC core and standard components
#include <afxext.h>         // MFC extensions

#ifndef _AFX_NO_OLE_SUPPORT
#include <afxole.h>         // MFC OLE classes
#include <afxodlgs.h>       // MFC OLE dialog classes
#include <afxdisp.h>        // MFC Automation classes
#endif // _AFX_NO_OLE_SUPPORT


#ifndef _AFX_NO_DB_SUPPORT
#include <afxdb.h>			// MFC ODBC database classes
#endif // _AFX_NO_DB_SUPPORT

#ifndef _AFX_NO_DAO_SUPPORT
#include <afxdao.h>			// MFC DAO database classes
#endif // _AFX_NO_DAO_SUPPORT

#include <afxdtctl.h>		// MFC support for Internet Explorer 4 Common Controls
#ifndef _AFX_NO_AFXCMN_SUPPORT
#include <afxcmn.h>			// MFC support for Windows Common Controls
#endif // _AFX_NO_AFXCMN_SUPPORT

#include <iostream>
#include <string>
#include <vector>
#include <algorithm>
#include <map>
using namespace std;

#include <cpputil.h>
#include <FileDirectorySearcher.h>
#include <UCLIDException.h>

void printUsage()
{
	cout <<	" Usage: FindFileDistribution <DirectoryName> <FileExtension> [OPTIONS]" << endl;
	cout <<	" OPTIONS:" << endl; 
	cout <<	"	/l - Get the distribution based on the lowest level directory name" << endl; 
}

bool bUseLocalDirName = false;

class DirCountPair
{
public:
	DirCountPair() : m_nFileCount(0), m_strDirname(""){}
	long m_nFileCount;
	string m_strDirname;

	bool operator<(const DirCountPair& pair)
	{
		if(m_nFileCount < pair.m_nFileCount)
		{
			return true;
		}
		return false;
	}

	// sort in ascending order
	bool operator()(DirCountPair& rpStart, DirCountPair& rpEnd)
	{
	  return rpStart.m_nFileCount > rpEnd.m_nFileCount;
	}
};

void countDirectory(const string& strFullDirPath, const string& strExt, long& nTotalNumFiles, map<string, DirCountPair>& mapDirs)
{
	
	FileDirectorySearcher fsd;

	string strLocalDirName = getFileNameFromFullPath(strFullDirPath, false);

	string strFull = strFullDirPath + "\\" + strExt;
	vector<string> vecFiles = fsd.searchFiles(strFull, false);
	if(vecFiles.size() > 0)
	{
		mapDirs[strLocalDirName].m_strDirname = strLocalDirName;
		mapDirs[strLocalDirName].m_nFileCount += vecFiles.size();
		nTotalNumFiles += vecFiles.size();
	}

	vector<string> vecTmpDirs;
	vecTmpDirs = fsd.getSubDirectories(strFullDirPath, false);
	
	for(unsigned int i = 0; i < vecTmpDirs.size(); i++)
	{
		countDirectory(vecTmpDirs[i], strExt, nTotalNumFiles, mapDirs);
	}
}

// syntax
int main(int argc, char** argv)
{

	if(argc < 3)
	{
		printUsage();
		return 0;
	}
	string strRootDir = argv[1];
	string strExtension = argv[2];
	
	int i;
	for(i = 3; i < argc; i++)
	{
		string arg = argv[i];
		if( arg == "/l")
		{
			bUseLocalDirName = true;
		}
		else
		{
			printUsage();
			return 0;
		}
	}

	try
	{
		try
		{
			long nTotalNumFiles = 0;
			vector<DirCountPair> vecDirs;
			if(!bUseLocalDirName)
			{
				vector<string> vecTmpDirs;
				FileDirectorySearcher fsd;
				vecTmpDirs = fsd.getSubDirectories(strRootDir, false);
				
				for(unsigned int i = 0; i < vecTmpDirs.size(); i++)
				{
					DirCountPair dp;
					dp.m_strDirname = vecTmpDirs[i];
					vecDirs.push_back(dp);
				}

				for(unsigned int i = 0; i < vecDirs.size(); i++)
				{
					string strFull = vecDirs[i].m_strDirname + "\\" + strExtension;
					vector<string> vecFiles = fsd.searchFiles(strFull, true);
					vecDirs[i].m_nFileCount = vecFiles.size();
					nTotalNumFiles += vecDirs[i].m_nFileCount;
				}
			}
			else
			{
				map<string, DirCountPair> mapDirs;
				countDirectory(strRootDir, strExtension, nTotalNumFiles, mapDirs);
				map<string, DirCountPair>::iterator it;
				for(it = mapDirs.begin(); it != mapDirs.end(); it++)
				{
					//cout << it->second.
					vecDirs.push_back(it->second);
				}
			}
			cout.precision(3);

			std::sort(vecDirs.begin(), vecDirs.end(), DirCountPair());

			for(unsigned int i = 0; i < vecDirs.size(); i++)
			{

				string strRelDir = vecDirs[i].m_strDirname;
				if(!bUseLocalDirName)
				{
					strRelDir = vecDirs[i].m_strDirname.substr(strRootDir.size(), string::npos);
				}
				cout << strRelDir.c_str() << "," << vecDirs[i].m_nFileCount << "," << double(vecDirs[i].m_nFileCount * 100) / (double)nTotalNumFiles << "%" << endl;
			}
		}
		CATCH_ALL_AND_RETHROW_AS_UCLID_EXCEPTION("ELI09512");
	}
	catch(UCLIDException ue)
	{
		string str;
		ue.asString(str);
		cout << str.c_str() << endl;
	}
	return 0;
}