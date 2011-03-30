
#include "stdafx.h"
#include "ClipboardManagerWnd.h"

#include <UCLIDException.h>
#include <COMUtils.h>
#include <Win32Util.h>

#ifdef _DEBUG
#undef THIS_FILE
static char THIS_FILE[]=__FILE__;
#define new DEBUG_NEW
#endif

using namespace std;

unsigned int g_nFormat = 0;

//-------------------------------------------------------------------------------------------------
ClipboardManagerWnd::ClipboardManagerWnd()
:/*m_hNextClipboardViewer(NULL),*/
 m_ipObj(NULL)
{
	try
	{
		// create a hidden window
		if (!CreateEx(NULL, AfxRegisterWndClass(NULL), "", NULL, 0, 0, 0, 0, NULL, NULL))
		{
			throw UCLIDException("ELI05532", "Unable to create window!");
		}

		// register a clipboard format for our objects
		if (g_nFormat == 0)
		{
			g_nFormat = RegisterClipboardFormat("UCLID Object");
			if (!g_nFormat)
			{
				throw UCLIDException("ELI05537", "Unable to register clipboard format for UCLID objects!");
			}
		}

		// set this object as one of the clipboard viewers
//		m_hNextClipboardViewer = SetClipboardViewer();
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI05533")
}
//-------------------------------------------------------------------------------------------------
ClipboardManagerWnd::~ClipboardManagerWnd()
{
	try
	{
		// remove this window from the clipboard chain
		//	ChangeClipboardChain(m_hNextClipboardViewer);
	}
	CATCH_AND_LOG_ALL_EXCEPTIONS("ELI16532");
}
//-------------------------------------------------------------------------------------------------
BEGIN_MESSAGE_MAP(ClipboardManagerWnd, CWnd)
	//{{AFX_MSG_MAP(ClipboardManagerWnd)
//	ON_MESSAGE(WM_DRAWCLIPBOARD, OnClipChange)  // clipboard change notification
//	ON_MESSAGE(WM_CHANGECBCHAIN, OnChangeCbChain) // cb chain change notification
	//}}AFX_MSG_MAP
END_MESSAGE_MAP()

//-------------------------------------------------------------------------------------------------
// Public methods
//-------------------------------------------------------------------------------------------------
void ClipboardManagerWnd::clear()
{
	// empty the clipboard
	EmptyClipboard();
}
//-------------------------------------------------------------------------------------------------
void ClipboardManagerWnd::copyObjectToClipboard(IUnknownPtr ipObj)
{
	// ensure first that the object supports persistence
	IPersistStreamPtr ipPersistObj = ipObj;
	if (ipPersistObj == __nullptr)
	{
		throw UCLIDException("ELI05539", "Object cannot be copied to clipboard "
			"because it does not support persistence!");
	}

	// create a temporary IStream object
	IStreamPtr ipStream;
	if (FAILED(CreateStreamOnHGlobal(NULL, TRUE, &ipStream)))
	{
		throw UCLIDException("ELI05540", "Unable to create stream object!");
	}

	// stream the object into the IStream
	writeObjectToStream(ipPersistObj, ipStream, "ELI09933", FALSE);

	// find the size of the stream
	LARGE_INTEGER zeroOffset;
	zeroOffset.QuadPart = 0;
	ULARGE_INTEGER length;
	ipStream->Seek(zeroOffset, STREAM_SEEK_END, &length);

	// copy the data in the stream to the buffer
	GlobalMemoryHandler clipbuffer = GlobalAlloc(GMEM_DDESHARE, length.LowPart);
	char *pszBuffer = (char *) clipbuffer.getData();
	ipStream->Seek(zeroOffset, STREAM_SEEK_SET, NULL);
	ipStream->Read(pszBuffer, length.LowPart, NULL);
	
	clipbuffer.unlock();

	// empty the clipboard
	ClipboardOpenerCloser clipboardOpener(this);	
	if (!EmptyClipboard())
	{
		UCLIDException ue("ELI05562", "Unable to empty the clipboard!");
		ue.addDebugInfo("GetLastError()", GetLastError());
		throw ue;
	}

	// copy the object to the clipboard
	if (!SetClipboardData(g_nFormat, clipbuffer))
	{
		UCLIDException ue("ELI05563", "Unable to copy data to the clipboard!");
		ue.addDebugInfo("GetLastError()", GetLastError());
		throw ue;
	}
}
//-------------------------------------------------------------------------------------------------
IUnknownPtr ClipboardManagerWnd::getObjectFromClipboard()
{
	try
	{
		IUnknownPtr ipObj = __nullptr;

		// open the clipboard
		ClipboardOpenerCloser clipboardOpener(this);

		// check to see if what is in the clipboard is an UCLID object
		if (IsClipboardFormatAvailable(g_nFormat))
		{
			// get the clipboard data
			GlobalMemoryHandler hMemory = GetClipboardData(g_nFormat);		
			char *pBuffer = (char *) hMemory.getData();
			DWORD dwBufferLength = GlobalSize(hMemory);

			// create a temporary IStream object
			IStreamPtr ipStream;
			if (FAILED(CreateStreamOnHGlobal(NULL, TRUE, &ipStream)))
			{
				throw UCLIDException("ELI05541", "Unable to create stream object!");
			}

			// write the buffer to the stream
			ipStream->Write(pBuffer, dwBufferLength, NULL);

			// reset the stream current position to the beginning of the stream
			LARGE_INTEGER zeroOffset;
			zeroOffset.QuadPart = 0;
			ipStream->Seek(zeroOffset, STREAM_SEEK_SET, NULL);

			// stream the object out of the IStream
			IPersistStreamPtr ipPersistObj;
			readObjectFromStream(ipPersistObj, ipStream, "ELI09978");
			ipObj = ipPersistObj;
		}

		m_ipObj = ipObj;
		return m_ipObj;
	}
	CATCH_ALL_AND_RETHROW_AS_UCLID_EXCEPTION("ELI27258");
}
//-------------------------------------------------------------------------------------------------
bool ClipboardManagerWnd::objectIsIUnknownVectorOfType(REFIID riid)
{
	bool bResult = false;

	getObjectFromClipboard();

	// Object must be defined
	if (m_ipObj != __nullptr)
	{
		// Check to see if object is IIUnknownVector
		UCLID_COMUTILSLib::IIUnknownVectorPtr ipVector = m_ipObj;
		if (ipVector != __nullptr)
		{
			// Retrieve number of items in vector
			long lSize = ipVector->Size();

			// Check each item in the vector
			bool	bItemsMatch = true;
			for (int i = 0; i < lSize; i++)
			{
				// Retrieve this item
				IUnknownPtr	ipItem = ipVector->At( i );
				if (ipItem == __nullptr)
				{
					// Throw exception
					UCLIDException	ue( "ELI05485", 
						"Unable to retrieve IUnknown pointer from vector" );
					ue.addDebugInfo( "Index", i );
					throw ue;
				}

				// Call QueryInterface on item
				IUnknownPtr	ipTest;
				ipItem.QueryInterface( riid, &ipTest );

				if (ipTest == __nullptr)
				{
					// Object does not support the desired interface
					bItemsMatch = false;

					// if one item failed then no need to check the rest, just break from loop
					break;
				}
			}		// end for each item in vector

			// Check search result
			if (bItemsMatch)
			{
				bResult = true;
			}
		}			// end if object is IIUnknownVector
	}				// end if object != __nullptr

	return bResult;
}
//-------------------------------------------------------------------------------------------------
bool ClipboardManagerWnd::vectorIsOWDOfType(REFIID riid)
{
	bool bResult = false;

	getObjectFromClipboard();

	if (m_ipObj != __nullptr)
	{
		// check to make sure the object is an IUnknownVector
		UCLID_COMUTILSLib::IIUnknownVectorPtr ipVector = m_ipObj;
		if (ipVector != __nullptr)
		{
			bool bItemsMatch = true;

			// get the size of the vector
			long lSize = ipVector->Size();

			// loop through each item
			for (long i = 0; i < lSize; i++)
			{
				// retrieve the item
				IUnknownPtr ipItem = ipVector->At(i);
				if (ipItem == __nullptr)
				{
					UCLIDException ue("ELI17578", 
						"Unable to retrieve IUnknown pointer from the vector!");
					ue.addDebugInfo("Index", i);
					throw ue;
				}

				// check to make sure the item is an object with description
				UCLID_COMUTILSLib::IObjectWithDescriptionPtr ipOWD(ipItem);
				if (ipOWD == __nullptr)
				{
					// if it is not an ObjectWithDescription then set
					// bItemsMatch to false and exit loop
					bItemsMatch = false;
					break;
				}

				// get the object from the ObjectWithDescription
				IUnknownPtr ipObject = ipOWD->Object;
				ASSERT_RESOURCE_ALLOCATION("ELI17579", ipObject != __nullptr);

				// call the query interface on the object
				IUnknownPtr ipTest;
				ipObject.QueryInterface(riid, &ipTest);

				if (ipTest == __nullptr)
				{
					// the object does not support the desired interface,
					// set bItemsMatch to false and exit loop
					bItemsMatch = false;
					break;
				}
			}

			// Check search result
			if (bItemsMatch)
			{
				bResult = true;
			}
		}
	}

	return bResult;
}
//-------------------------------------------------------------------------------------------------
bool ClipboardManagerWnd::objectIsOfType(REFIID riid)
{
	bool bResult = false;

	getObjectFromClipboard();

	// Object must be defined
	if (m_ipObj != __nullptr)
	{
		// Call QueryInterface on data member
		IUnknownPtr	ipTest;
		m_ipObj.QueryInterface( riid, &ipTest );

		if (ipTest != __nullptr)
		{
			// Object supports the desired interface
			bResult = true;
		}
	}

	return bResult;
}
//-------------------------------------------------------------------------------------------------
bool ClipboardManagerWnd::objectIsTypeWithDescription(REFIID riid)
{
	bool bResult = false;

	getObjectFromClipboard();

	// Object must be defined
	if (m_ipObj != __nullptr)
	{
		// Check to see if object is IObjectWithDescription
		UCLID_COMUTILSLib::IObjectWithDescriptionPtr ipObject = m_ipObj;
		if (ipObject != __nullptr)
		{
			// Retrieve the embedded object
			IUnknownPtr ipEmbedded = ipObject->GetObject();

			// Check to see if the embedded object is of specified type
			if (ipEmbedded != __nullptr)
			{
				IUnknownPtr ipTest;
				ipEmbedded.QueryInterface( riid, &ipTest );

				if (ipTest != __nullptr)
				{
					// Object supports the desired interface
					bResult = true;
				}
			}		// end if embedded object != __nullptr
		}			// end if data member is IObjectWithDescription
	}				// end if data member != __nullptr

	return bResult;
}

//-------------------------------------------------------------------------------------------------
// Private methods
//-------------------------------------------------------------------------------------------------
