// RVUIThread.cpp : implementation file
//

#include "stdafx.h"
#include "resource.h"
#include "RVUIThread.h"

#include <UCLIDException.h>
#include <cpputil.h>

#ifdef _VERIFICATION_LOGGING
#include <ThreadSafeLogFile.h>
#endif

#ifdef _DEBUG
#define new DEBUG_NEW
#undef THIS_FILE
static char THIS_FILE[] = __FILE__;
#endif

//-------------------------------------------------------------------------------------------------
// RVUIThread
//-------------------------------------------------------------------------------------------------
IMPLEMENT_DYNCREATE(RVUIThread, CWinThread)
//-------------------------------------------------------------------------------------------------
RVUIThread::RVUIThread()
: m_threadFileCompleteEvent(false),
  m_threadInitCompleteEvent(false),
  m_bExceptionThrown(false),
  m_bTerminationRequested(false),
  m_bNewFileReceived(false),
  m_ipFAMDB(NULL),
  m_ipFAMTagManager(NULL)
{
}
//-------------------------------------------------------------------------------------------------
RVUIThread::~RVUIThread()
{
	try
	{
	}
	CATCH_AND_LOG_ALL_EXCEPTIONS("ELI16484");
}
//-------------------------------------------------------------------------------------------------
BOOL RVUIThread::InitInstance()
{
	BOOL bReturn = TRUE;

	try
	{
		try
		{
			//CoInitializeEx(NULL, COINIT_MULTITHREADED);
			// This is being used instead of multithreaded version because
			// This app uses the Spot Recognition Window that uses an OCX
			// that will not work with the multithreaded option
			CoInitializeEx(NULL, COINIT_APARTMENTTHREADED);

			// Create an instance of the Data Area dialog
			if (m_apDlg.get() == NULL)
			{
				m_apDlg = auto_ptr<CDataAreaDlg>(new CDataAreaDlg( m_UISettings ));
				ASSERT_RESOURCE_ALLOCATION( "ELI11274", m_apDlg.get() != NULL);

				if (m_apDlg->IsValidINI())
				{
					m_apDlg->Create( CDataAreaDlg::IDD, NULL );
				}
				else
				{
					UCLIDException exception( "ELI12060", "Unable to find INI file." );
					exception.addDebugInfo( "INI Path", m_apDlg->getINIPath() );
					throw exception;
				}

				if (!m_apDlg->isDialogInitialized())
				{
					UCLIDException ue("ELI20385", "Failed to initialize the verification dialog!");
					throw ue;
				}
			}
		}
		CATCH_ALL_AND_RETHROW_AS_UCLID_EXCEPTION("ELI11385")
	}
	catch(UCLIDException &ue)
	{
		// Store exception information
		m_ue = ue;
		m_bExceptionThrown = true;

		bReturn = FALSE;
	}

	// Inform File Processor that thread and dialog now ready for files
	m_threadInitCompleteEvent.signal();

	return bReturn;
}
//-------------------------------------------------------------------------------------------------
int RVUIThread::ExitInstance()
{
	try
	{
		// Clear the FAMTagManager pointer
		m_ipFAMTagManager = NULL;

		// Clear the FAMDB pointer
		m_ipFAMDB = NULL;

		// Clean up dialog
		m_apDlg.reset(NULL);
	}
	CATCH_AND_LOG_ALL_EXCEPTIONS("ELI17990");

	// Default return value to error value (non-zero) in case of exception during CWinThread::ExitInstance
	int iRes = 1; 

	try
	{
		CoUninitialize();

		iRes = CWinThread::ExitInstance();
	}
	CATCH_AND_LOG_ALL_EXCEPTIONS("ELI17989");

	return iRes;
}
//-------------------------------------------------------------------------------------------------
BEGIN_MESSAGE_MAP(RVUIThread, CWinThread)
	//{{AFX_MSG_MAP(RVUIThread)
		// NOTE - the ClassWizard will add and remove mapping macros here.
	//}}AFX_MSG_MAP
END_MESSAGE_MAP()

//-------------------------------------------------------------------------------------------------
// Public methods
//-------------------------------------------------------------------------------------------------
bool RVUIThread::WaitForFileComplete(unsigned long ulWaitTimeMilliseconds)
{
	// Stop if the user chose to cancel
	if (m_bTerminationRequested)
	{
		return false;
	}

	// Wait until Dialog completion
	int iWaitCount = 0;
	int iWaitLength = ulWaitTimeMilliseconds;
	while (m_threadFileCompleteEvent.wait( iWaitLength ) != WAIT_OBJECT_0)
	{
		// Stop if the user chose to cancel
		if (m_bTerminationRequested)
		{
			return false;
		}

#ifdef _VERIFICATION_LOGGING
		// Log a timeout message, not necessarily an error
		ThreadSafeLogFile tslf;
		string strText = "Timeout waiting for File Complete, previous wait count = ";
		strText += asString( iWaitCount );
		tslf.writeLine( strText );
#endif

		// Check for no file ready yet
		if (!m_bNewFileReceived)
		{
#ifdef _VERIFICATION_LOGGING
			// Log an error and return false
			tslf.writeLine( "Unable to WaitForFileComplete() without new file" );
#endif

			return false;
		}
		else
		{
			// New file has been received, keep waiting indefinitely
			iWaitLength = INFINITE;
			iWaitCount++;
		}
	}

	return true;
}
//-------------------------------------------------------------------------------------------------
void RVUIThread::WaitForThreadInitialized()
{
	// Wait until Thread and Dialog are ready
	m_threadInitCompleteEvent.wait();
}
//-------------------------------------------------------------------------------------------------
void RVUIThread::RequestTermination()
{
	PostThreadMessage(WM_CLOSE_VERIFICATION_DLG, 0, 0); 
}

//-------------------------------------------------------------------------------------------------
// RVUIThread message handlers
//-------------------------------------------------------------------------------------------------
BOOL RVUIThread::PreTranslateMessage(MSG* pMsg) 
{
	// Keep track of whether we are handling a user message
	bool bUserMessage = false;

	try
	{
		try
		{
			// Check for message from File Processor
			if (pMsg->message == WM_NEW_FILE_READY)
			{
				bUserMessage = true;

#ifdef _VERIFICATION_LOGGING
				// Add entry to default log file
				ThreadSafeLogFile tslf;
				tslf.writeLine( "WM_NEW_FILE_READY received in RVUIThread::PreTranslate" );
				tslf.writeLine( m_strFileName );
#endif

				// Make sure exception flag is false
				m_bExceptionThrown = false;

				// Set the FAM Tag Manager pointer
				m_apDlg->InitFAMTagManager(m_ipFAMTagManager);
				
				// Set the FAMDB
				m_apDlg->SetFAMDB(m_ipFAMDB);				

				// Provide file to Dialog
				m_apDlg->SetInputFile( m_strFileName );

				// Set flag
				m_bNewFileReceived = true;
				return TRUE;
			}
			else if (pMsg->message == WM_FILE_COMPLETE)
			{
				bUserMessage = true;

#ifdef _VERIFICATION_LOGGING
				// Add entry to default log file
				ThreadSafeLogFile tslf;
				tslf.writeLine( "Ready to m_threadFileCompleteEvent.signal()" );
#endif

				// Signal the event that file review is complete
				m_threadFileCompleteEvent.signal();

				// Clear flag
				m_bNewFileReceived = false;
				return TRUE;
			}
			else if (pMsg->message == WM_CLOSE_VERIFICATION_DLG)
			{
				// Set the flag to indicate that verification is being cancelled
				m_bTerminationRequested = true;

				// Signal the event that file review is complete
				m_threadFileCompleteEvent.signal();

				// Clear flag
				m_bNewFileReceived = false;
				return TRUE;
			}
			else if (pMsg->message == WM_FILE_FAILED)
			{
				// Get the exception string from the wParam
				char* pszException = (char*) pMsg->wParam;
				if (pszException != NULL)
				{
					// Create a new UCLIDException from the stringized byte stream
					m_ue.createFromString("ELI25341", pszException);

					// Free the memory
					delete [] pszException;
				}
				else
				{
					// Unknown exception
					m_ue = UCLIDException("ELI25342", "File failed in verification!");
				}

				// Set the exception thrown flag to true
				m_bExceptionThrown = true;

				// Signal the file complete event
				m_threadFileCompleteEvent.signal();
				m_bNewFileReceived = false;
				return TRUE;
			}
		}
		CATCH_ALL_AND_RETHROW_AS_UCLID_EXCEPTION("ELI11387")
	}
	catch(UCLIDException &ue)
	{
		// Store exception information
		m_ue = ue;
		m_bExceptionThrown = true;

		// Signal the event that file review is complete
		m_threadFileCompleteEvent.signal();

		// Clear flag
		m_bNewFileReceived = false;

		// In case we caught an exception while handling a user message, return true here to 
		// indicate the message has been handled.  Not returning true in this case can leave 
		// the thread in a bad state
		if (bUserMessage)
			return TRUE;
	}

	return CWinThread::PreTranslateMessage(pMsg);
}
//-------------------------------------------------------------------------------------------------
