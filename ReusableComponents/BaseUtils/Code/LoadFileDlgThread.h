#pragma once

#include "BaseUtils.h"
#include "Win32Event.h"

// Class that wrap a separate thread which will load
// the CFileDialogEx object. This CFileDialogEx class have 
// some problem when working with CoInitializeEx(NULL, COINIT_MULTITHREADED)
// Read the following for details
// http://www.kbalertz.com/Q287087/Calling.Shell.Functions.Interfaces.Multithreaded.Apartment.aspx
// Thread file dialog class
class EXPORT_BaseUtils ThreadFileDlg
{
public:
	// Constructor
	ThreadFileDlg(CFileDialog* pFileDlg);

	// The method that will call DoModal of CFileDialog
	// in an separate thread
	UINT doModal();

private:
	// A pointer to the open file dialog
	CFileDialog * m_pFileDlg;

	// the result from the DoModal of the file dialog
	UINT m_uiDlgResult;
	
	// Event to signal the dialog has been closed
	Win32Event m_threadEndedEvent;

	// Thread to load open file dialog
	static UINT LoadFileDlgThread(void* pData);
};