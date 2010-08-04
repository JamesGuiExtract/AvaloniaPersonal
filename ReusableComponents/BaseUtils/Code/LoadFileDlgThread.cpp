#include "stdafx.h"
#include "LoadFileDlgThread.h"
#include "UCLIDException.h"

using namespace std;

//-------------------------------------------------------------------------------------------------
// ThreadFileDlg class
//-------------------------------------------------------------------------------------------------
ThreadFileDlg::ThreadFileDlg(CFileDialog* pFileDlg)
: m_pFileDlg(pFileDlg),
m_uiDlgResult(IDCANCEL)
{
}

//-------------------------------------------------------------------------------------------------
// Public methods
//-------------------------------------------------------------------------------------------------
UINT ThreadFileDlg::doModal()
{
	// Display the dialog in a separate thread to avoid the problem
	// of CFileDialog with CoInitializeEx(NULL, COINIT_MULTITHREADED)
	AfxBeginThread(LoadFileDlgThread, this);

	// Wait for the LoadFileDlgThread to finish
	m_threadEndedEvent.messageWait();

	// Return the button that was clicked
	return m_uiDlgResult;
}

//-------------------------------------------------------------------------------------------------
// thread proc
//-------------------------------------------------------------------------------------------------
UINT ThreadFileDlg::LoadFileDlgThread(void* pData)
{
	try
	{
		// Cast back to ThreadDataStruct pointer
		ThreadFileDlg* pTD = (ThreadFileDlg *) pData;
		ASSERT_ARGUMENT("ELI15482", pTD != NULL);

		// Get the file dialog pointer inside it
		CFileDialog* pFileDlg = pTD->m_pFileDlg;
		ASSERT_ARGUMENT("ELI15483", pFileDlg != NULL);

		// Set the flag to true if OK is clicked
		pTD->m_uiDlgResult = pFileDlg->DoModal();

		// Signal the event object
		pTD->m_threadEndedEvent.signal();
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI15484");

	return 0;
}
//-------------------------------------------------------------------------------------------------
