#include "stdafx.h"
#include "ClipboardManager.h"
#include "UCLIDException.h"
#include "Win32Util.h"

//--------------------------------------------------------------------------------------------------
// ClipboardManager
//--------------------------------------------------------------------------------------------------
ClipboardManager::ClipboardManager(CWnd* pWnd)
: m_pWnd(NULL)
{
	if (pWnd == NULL)
	{
		UCLIDException ue("ELI11850", "Cannot accept NULL window!");
		throw ue;
	}
	m_pWnd = pWnd;
}
//--------------------------------------------------------------------------------------------------
ClipboardManager::~ClipboardManager()
{
	try
	{
	}
	CATCH_AND_LOG_ALL_EXCEPTIONS("ELI16377");
}
//--------------------------------------------------------------------------------------------------
void ClipboardManager::writeText(const std::string& strText)
{

	// Open the clipboard
	ClipboardOpenerCloser clipboardOpener(m_pWnd);

	// clear the clipboard
	BOOL bRet = EmptyClipboard();
	if(bRet == FALSE)
	{
		UCLIDException ue("ELI11845", "Unable to clear the clipboard contents for writing text.");
		ue.addDebugInfo("Text", strText);
		throw ue;
	}

	// Allocate global memory to store the text
	HGLOBAL hglbString = GlobalAlloc(GMEM_MOVEABLE, (strText.size() + 1) * sizeof(TCHAR)); 
	if (hglbString == NULL) 
	{ 
		UCLIDException ue("ELI11847", "Unable to allocate Global memory for storing to the clipboard.");
		ue.addDebugInfo("Text", strText);
		throw ue;
	} 
	
	// Lock the global memory
	GlobalMemoryHandler memoryHandler(hglbString);

	// copy the text to the global memory (including the '\0')
	TCHAR* lptstrCopy = static_cast<TCHAR*>(memoryHandler.getData()); 
	memcpy(lptstrCopy, strText.c_str(), (strText.size() + 1) * sizeof(TCHAR)); 

	// Set the clipboard data to the global memory
	HANDLE hData = SetClipboardData(CF_TEXT, memoryHandler); 
	if (hData == NULL)
	{
		UCLIDException ue("ELI11846", "Unable to copy text to the clipboard.");
		ue.addDebugInfo("Text", strText);
		throw ue;
	}
}
//--------------------------------------------------------------------------------------------------
bool ClipboardManager::readText(std::string& strText) const
{
	// Open the clipboard
	ClipboardOpenerCloser clipboardOpener(m_pWnd);

	// retun false if the cliboard data is not text
	if (!IsClipboardFormatAvailable(CF_TEXT)) 
	{
		return false;
	}
	
	// Get a global handle to the clipboard text
	HGLOBAL hglbString = GetClipboardData(CF_TEXT);
	if (hglbString == NULL)
	{
		UCLIDException ue("ELI19399", "Unable to get text from the clipboard.");
		throw ue;
	}

	// Lock the handle
	GlobalMemoryHandler memoryHandler(hglbString);

	// get the text from the global memory
	TCHAR* lptstr = static_cast<TCHAR*>(memoryHandler.getData());
	// return the string
	strText = lptstr;

	return true;
}
//--------------------------------------------------------------------------------------------------
