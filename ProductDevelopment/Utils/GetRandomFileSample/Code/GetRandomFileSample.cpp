

#pragma warning(disable:4786)

#define VC_EXTRALEAN		// Exclude rarely-used stuff from Windows headers

#include "CommonToExtractProducts.h"

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
#include <StringTokenizer.h>
#include <Misc.h>
#include <VectorOperations.h>

static const string gstrRAND = "<RAND>";

void printUsage()
{
	cout <<	"Usage: GetRandomFileSample <DirectoryOrFileName> <FileExtension> <NumFiles> [OPTIONS]" << endl;
	cout << "\tOPTIONS:" << endl;
	cout << "\t\t/CopyTo <dirname> - copies the files in the sample to the selected directory" << endl;
	cout << "\t\t\tdirname - the name of the directory to which files shall be copied" << endl;
	cout << "\t\t\t          if dirname contains the tag <RAND> a random number will be" << endl;
	cout << "\t\t\t          inserted in its place" << endl;
	cout << "\t\t/b          Treat <DirectoryOrFileName> as a batch file name where the file" << endl;
	cout << "\t\t\t          <DirectoryOrFileName> contains a list of folders (one per line) that" << endl;
	cout << "\t\t\t          the sample should be taken from.  This is in lieu of the default " << endl;
	cout << "\t\t\t          behavior where  <DirectoryOrFileName> is treated as a directory that" << endl;
	cout << "\t\t\t          the sample should be taken from." << endl;
}

// syntax
int main(int argc, char** argv)
{

	if(argc < 4)
	{
		printUsage();
		return 0;
	}
	string strRootDir = argv[1];
	string strExtension = argv[2];
	unsigned long nNumFiles = atoi(argv[3]);
	
	string strCopyDir = "";
	bool bCopyTo = false;

	string strBatchFileName = "";
	bool bFromBatchFile = false;

	int i;
	for(i = 4; i < argc; i++)
	{
		string arg = argv[i];
		if(arg == "/CopyTo")
		{
			if(argc < i+2)
			{
				printUsage();
				return 0;
			}
			strCopyDir = argv[i+1];
			i++;
			bCopyTo = true;
		}
		else if(arg == "/b")
		{
			bFromBatchFile = true;
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

			int s = GetTickCount();
			srand(s);

			vector<string> vecSampleFiles;
			vector<string> vecFiles;

			if(!bFromBatchFile)
			{
				FileDirectorySearcher fsd;
				string strFull = strRootDir + "\\" + strExtension;
				vecFiles = fsd.searchFiles(strFull, true);
			}
			else
			{
				FileDirectorySearcher fsd;
				vector<string> vecFolders = convertFileToLines(strRootDir);
				unsigned int i;
				for(i = 0; i < vecFolders.size(); i++)
				{
					string strFull = vecFolders[i] + "\\" + strExtension;
					vector<string> vecNewFiles = fsd.searchFiles(strFull, true);
					addVectors(vecFiles, vecNewFiles);
				}
			}


			if(vecFiles.size() < nNumFiles)
			{
				cerr << "Not enough files" << endl;
				return 0;
			}
			long nNumFilesLeft = vecFiles.size();
			unsigned int i;
			for(i = 0; i < nNumFiles; i++)
			{
				int r = rand();
				int index = r % nNumFilesLeft;
				vecSampleFiles.push_back(vecFiles[index]);
				vecFiles.erase(vecFiles.begin() + index);
				nNumFilesLeft--;
			}

			std::sort(vecSampleFiles.begin(), vecSampleFiles.end());

			for(i = 0; i < vecSampleFiles.size(); i++)
			{
				cout << vecSampleFiles[i].c_str() << endl;
			}

			if(bCopyTo)
			{
				vector<string> vecPathTokens;
				StringTokenizer::sGetTokens(strCopyDir, "\\", vecPathTokens);
				if(vecPathTokens.size() == 0)
				{
					THROW_LOGIC_ERROR_EXCEPTION("ELI09807");
				}
				// Now create the directory structure if it does not exist
				string strCurrDir = "";
				unsigned int i;
				for(i = 0; i < vecPathTokens.size(); i++)
				{
					string strLocal = vecPathTokens[i];

					long nTagStart = strLocal.find(gstrRAND);
					if(nTagStart != string::npos)
					{
						long nRand = rand();
						strLocal = strLocal.substr(0, nTagStart) + 
							asString(nRand) + strLocal.substr(nTagStart + 
							gstrRAND.length());

					}

					strCurrDir += strLocal + "\\";
					if(!isFileOrFolderValid(strCurrDir))
					{
						createDirectory(strCurrDir);
					}
				}
				for(i = 0; i < vecSampleFiles.size(); i++)
				{
					// Copy the original file
					string strFile = vecSampleFiles[i];
					copyFileToNewPath(strFile, strCurrDir);

					// Append .uss for corresponding USS file
					strFile += ".uss";
					if ( isFileOrFolderValid ( strFile ) )
					{
						copyFileToNewPath(strFile, strCurrDir);
					}
				}
			}
		}
		CATCH_ALL_AND_RETHROW_AS_UCLID_EXCEPTION("ELI09725");
	}
	catch(UCLIDException ue)
	{
		string str;
		ue.asString(str);
		cout << str.c_str() << endl;
	}
	cout << "Finished";
	return 0;
}