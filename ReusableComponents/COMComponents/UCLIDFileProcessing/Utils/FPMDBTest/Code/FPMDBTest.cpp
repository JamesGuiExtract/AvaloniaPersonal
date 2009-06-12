// FPMDBTest.cpp : Defines the entry point for the console application.
//

#include "stdafx.h"
#include "FPMDBTest.h"

#include <UCLIDExceptionDlg.h>
#include <UCLIDException.hpp>
#include <LicenseMgmt.h>
#include "FileDirectorySearcher.hpp"
#include "StopWatch.h"


#ifdef _DEBUG
#define new DEBUG_NEW
#endif


// The one and only application object

CWinApp theApp;

using namespace std;


class DBFileSearcher : public FileDirectorySearcherBase
{
public:
	DBFileSearcher ( IFileProcessingDBPtr ipDB );
protected:
	virtual void addFile ( const std::string &strFile );
private:
	IFileProcessingDBPtr m_ipDB;
};

DBFileSearcher::DBFileSearcher(IFileProcessingDBPtr ipDB)
: m_ipDB(ipDB)
{
}

void DBFileSearcher::addFile( const std::string &strFile )
{
	VARIANT_BOOL bAlreadyExists;
	EActionStatus easPrev;
	EActionStatus easNew = kActionPending;
	m_ipDB->AddFile( strFile.c_str(), "TestAction", "Adding Files", VARIANT_TRUE, 
		VARIANT_FALSE, easNew, &bAlreadyExists, &easPrev);
}

int main(int argc, char *argv[])
{
	int nRetCode = 0;

	// initialize MFC and print and error on failure
	if (!AfxWinInit(::GetModuleHandle(NULL), NULL, ::GetCommandLine(), 0))
	{
		// TODO: change error code to suit your needs
		_tprintf(_T("Fatal Error: MFC initialization failed\n"));
		nRetCode = 1;
	}
	else
	{
		
		if ( argc != 2 )
		{
			cout << "Syntax: FPMDBProto <FileSpec>" << endl;
			return 0;
		}
		string strFileSpec = argv[1];
		//string strOutputName = argv[2];


		CoInitializeEx(NULL, COINIT_MULTITHREADED);

		try
		{

			// Set up the exception handling aspect.
			static UCLIDExceptionDlg exceptionDlg;
			UCLIDException::setExceptionHandler( &exceptionDlg );


			// initialize license
			LicenseManagement::sGetInstance().loadLicenseFilesFromFolder();

			// Create action to add files
			
			IFileProcessingDBPtr ipFPDB(CLSID_FileProcessingDB);
			ASSERT_RESOURCE_ALLOCATION("ELITODO", ipFPDB != NULL );

			
			try
			{
				long newID = ipFPDB->DefineNewAction( "TestAction");
				cout << newID << endl;
			}
			catch (...)
			{
			}
			
			DBFileSearcher dbFileSearch(ipFPDB);
			dbFileSearch.findFiles(strFileSpec, true); 
		
		}
		CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELITODO");
		LicenseManagement::sGetInstance().terminate();
		CoUninitialize();
	}

	return nRetCode;
}
