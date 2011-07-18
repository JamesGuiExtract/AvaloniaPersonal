// VerifyToolbar.cpp : implementation file
//
#include "stdafx.h"
#include "LaserficheCustomComponents.h"
#include "VerifyToolbar.h"
#include "IDShieldLF.h"

#include <LFMiscUtils.h>
#include <COMUtils.h>
#include <UCLIDException.h>

//--------------------------------------------------------------------------------------------------
// Statics
//--------------------------------------------------------------------------------------------------
CVerifyToolbar *CVerifyToolbar::m_pActiveToolbar = NULL;
CMutex CVerifyToolbar::m_mutexDoVerification;
CLFItemCollection *CVerifyToolbar::m_pDocumentCollection(NULL);
list<CLFItem> *CVerifyToolbar::m_plistDocumentCollection;
long CVerifyToolbar::m_nDocsRemaining = 0;

//--------------------------------------------------------------------------------------------------
// Constants
//--------------------------------------------------------------------------------------------------
const int WM_CLOSE_DOCUMENT			= WM_USER + 100;
const string strLF_HIDE_PROMPT		= "6";

//--------------------------------------------------------------------------------------------------
// CVerifyToolbar
//--------------------------------------------------------------------------------------------------
IMPLEMENT_DYNAMIC(CVerifyToolbar, CDialog)
//--------------------------------------------------------------------------------------------------
CVerifyToolbar::CVerifyToolbar(CIDShieldLF *pIDShieldLF, ILFDocumentPtr ipDocument, 
							   int nVerifiedCount, int nRemainingCount)
	: CIDShieldLFHelper(pIDShieldLF)
	, m_zDocumentNum(asString(nVerifiedCount).c_str())
	, m_zRemainingDocs(asString(nRemainingCount).c_str())
	, m_hWndDocWindow(NULL)
	, m_ipDocument(ipDocument)
	, m_nDocumentID(0)
	, m_hDocWindowCloseEventHook(NULL)
	, m_bStopped(false)
	, m_bVerified(false)
	, m_bInitialFocus(true)
{
	// Worker thread //
	try
	{
		ASSERT_RESOURCE_ALLOCATION("ELI21440", m_ipDocument != NULL);
		
		m_nDocumentID = m_ipDocument->ID;
		m_strDocumentName = asString(m_ipDocument->Name);

		// Set the active toolbar pointer.
		m_pActiveToolbar = this;
	}
	CATCH_ALL_AND_RETHROW_AS_UCLID_EXCEPTION("ELI20895")
}
//--------------------------------------------------------------------------------------------------
CVerifyToolbar::~CVerifyToolbar()
{
	// Worker thread //
	try
	{
		// Ensure we are unregistered from m_hDocWindowCloseEventHook
		if (m_hDocWindowCloseEventHook != NULL)
		{
			UnhookWinEvent(m_hDocWindowCloseEventHook);
		}

		if (m_ipDocument != NULL)
		{
			m_ipDocument->Dispose();
		}

		// Set the active toolbar pointer.
		m_pActiveToolbar = NULL;
	}
	CATCH_AND_LOG_ALL_EXCEPTIONS("ELI20884");
}
//--------------------------------------------------------------------------------------------------
BEGIN_MESSAGE_MAP(CVerifyToolbar, CDialog)
	ON_BN_CLICKED(IDC_VERIFY, &CVerifyToolbar::OnBnClickedVerify)
	ON_BN_CLICKED(IDC_STOP, &CVerifyToolbar::OnBnClickedStop)
	ON_MESSAGE(WM_CLOSE_DOCUMENT, OnCloseDocument)
	ON_WM_SETFOCUS()
END_MESSAGE_MAP()
//--------------------------------------------------------------------------------------------------
void CVerifyToolbar::doVerification(CIDShieldLF *pIDShieldLF, CLFItemCollection &rdocumentsToVerify)
{
	// Worker thread //
	// (static)
	ASSERT_ARGUMENT("ELI21441", pIDShieldLF != NULL);

	CSingleLock lock(&m_mutexDoVerification, TRUE);

	m_plistDocumentCollection = NULL;
	m_pDocumentCollection = &rdocumentsToVerify;
	m_nDocsRemaining = rdocumentsToVerify.getCount();

	doVerification(pIDShieldLF);
}
//--------------------------------------------------------------------------------------------------
void CVerifyToolbar::doVerification(CIDShieldLF *pIDShieldLF, list<CLFItem> &rlistDocumentsToVerify)
{
	// Worker thread //
	// (static)
	ASSERT_ARGUMENT("ELI21442", pIDShieldLF != NULL);

	CSingleLock lock(&m_mutexDoVerification, TRUE);

	m_pDocumentCollection = NULL;
	m_plistDocumentCollection = &rlistDocumentsToVerify;
	m_nDocsRemaining = (long) rlistDocumentsToVerify.size();
	// Insert placeholder item at the front of the list since getNextDocument will start by removing
	// the first item from the list.
	m_plistDocumentCollection->insert(m_plistDocumentCollection->begin(), CLFItem());  

	doVerification(pIDShieldLF);
}

//--------------------------------------------------------------------------------------------------
// Overrides
//--------------------------------------------------------------------------------------------------
void CVerifyToolbar::DoDataExchange(CDataExchange* pDX)
{
	// UI thread //
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		CDialog::DoDataExchange(pDX);
		DDX_Control(pDX, IDC_VERIFY, m_btnVerify);
		DDX_Control(pDX, IDC_STOP, m_btnStop);
		DDX_Text(pDX, IDC_VERIFYING_COUNT, m_zDocumentNum);
		DDX_Text(pDX, IDC_REMAINING_COUNT, m_zRemainingDocs);
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI20904");
}
//--------------------------------------------------------------------------------------------------
BOOL CVerifyToolbar::OnInitDialog()
{
	// UI thread //
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		CDialog::OnInitDialog();

		m_btnVerify.SetIcon(LoadIcon(AfxGetInstanceHandle(), MAKEINTRESOURCE(IDI_VERIFIED1)));
		m_btnStop.SetIcon(LoadIcon(AfxGetInstanceHandle(), MAKEINTRESOURCE(IDI_STOP)));

		// If we have cached to the registry the previous toolbar window position, initialize
		// the window in this position rather than centered on the document window (default)
		if (m_pIDShieldLF->m_apCurrentUserRegSettings->keyExists(
				gstrREG_VERIFY_WINDOW_KEY, gstrREG_VERIFY_WINDOW_LEFT) &&
			m_pIDShieldLF->m_apCurrentUserRegSettings->keyExists(
				gstrREG_VERIFY_WINDOW_KEY, gstrREG_VERIFY_WINDOW_TOP))
		{
			// Load the top & left window coordinates
			string strLeft = m_pIDShieldLF->m_apCurrentUserRegSettings->getKeyValue(
				gstrREG_VERIFY_WINDOW_KEY, gstrREG_VERIFY_WINDOW_LEFT, "0");
			string strTop = m_pIDShieldLF->m_apCurrentUserRegSettings->getKeyValue(
				gstrREG_VERIFY_WINDOW_KEY, gstrREG_VERIFY_WINDOW_TOP, "0");

			// Retrieve the current window rect (so that we can preserve the window size)
			CRect rectWindow;
			GetWindowRect(&rectWindow);

			// Shift the XY position to match the cached coordinates.
			rectWindow.MoveToXY(asLong(strLeft), asLong(strTop));
			MoveWindow(&rectWindow); 
		}

		UpdateData(FALSE);
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI20903");
	
	// [FlexIDSIntegrations:71] Return FALSE so Windows doesn't force focus on the toolbar
	return FALSE;
}

//--------------------------------------------------------------------------------------------------
// Message handlers
//--------------------------------------------------------------------------------------------------
void CVerifyToolbar::OnBnClickedVerify()
{
	// UI thread //
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		// Indicate the user has deemed the document to be verified.
		m_bVerified = true;

		// Close the document window so that the verification can move onto the next document
		CloseDocument();
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI20899");
}
//--------------------------------------------------------------------------------------------------
void CVerifyToolbar::OnBnClickedStop()
{
	// UI thread //
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		// Indicate that the user has requested that the verification process be stopped.
		m_bStopped = true;

		// Close the document window so that the verification can move onto the next document
		CloseDocument();
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI20898");
}
//--------------------------------------------------------------------------------------------------
LRESULT CVerifyToolbar::OnCloseDocument(WPARAM wParam, LPARAM lParam)
{
	// UI thread //
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		// If the document window is closed by the user, treat this as a request to stop the
		// verification process.
		m_bStopped = true;

		// CloseDocument is responsible for closing the toolbar so it must be called even
		// though the user has already closed the document.
		CloseDocument();
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI20906");

	return 0;
}
//--------------------------------------------------------------------------------------------------
void CVerifyToolbar::OnSetFocus(CWnd* pOldWnd)
{
	// UI thread //
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		// [FlexIDSIntegrations:71]
		// The first time the toolbar receives focus, set focus to the parent document window instead.
		 
		if (m_bInitialFocus)
		{
			CWnd *pParent = GetParent();
			if (pParent != NULL)
			{
				// [FlexIDSIntegrations:17] If the document is mimimized, restore it.
				if (pParent->IsIconic())
				{
					pParent->ShowWindow(SW_RESTORE);
				}

				pParent->SetFocus();
			}
		}
		else
		{
			CDialog::OnSetFocus(pOldWnd);
		}
		
		m_bInitialFocus = false;
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI21685");
}

//--------------------------------------------------------------------------------------------------
// Private 
//--------------------------------------------------------------------------------------------------
void CVerifyToolbar::doVerification(CIDShieldLF *pIDShieldLF)
{
	// Worker thread //
	// (static)
	ASSERT_RESOURCE_ALLOCATION("ELI21443", pIDShieldLF != NULL);

	// Declared outside try scope so that it can be disposed of in case of an exception.
	ILFDocumentPtr ipDocument;

	try
	{
		try
		{
			// Initialize document counts.
			int nDocsVerified = 0;
			int nDocsRemaining = 0;
			bool bVerificationCancelled = false;

			// Loop through each document in need of verification
			ipDocument = getNextDocument(pIDShieldLF, nDocsRemaining);
			while (ipDocument != NULL)
			{	
				// Ignore the document if it is not qualified for verification by the current user.
				if (checkCanVerify(ipDocument, bVerificationCancelled))
				{
					// Create an instance of the toolbar and have it display the document for 
					// verification
					CVerifyToolbar toolbar(pIDShieldLF, ipDocument, nDocsVerified+1, nDocsRemaining);
					if (toolbar.verifyDocument())
					{
						// Document was verified, increment the counter.
						nDocsVerified++;
					}
					else
					{
						// The user stopped the verification process
						bVerificationCancelled = true;
					}
				}

				ipDocument->Dispose();

				// If the user cancelled verification, exit the document loop
				if (bVerificationCancelled)
				{
					break;
				}

				ipDocument = getNextDocument(pIDShieldLF, nDocsRemaining);
			}

			if (bVerificationCancelled == false)
			{
				// [FlexIDSIntegrations:8] Display a message when the last document is verified.
				::MessageBox(pIDShieldLF->m_hwndClient, "Verification Complete!", 
							 gstrPRODUCT_NAME.c_str(), MB_OK | MB_ICONINFORMATION);
			}
		}
		CATCH_ALL_AND_RETHROW_AS_UCLID_EXCEPTION("ELI20905");
	}
	catch (...)
	{
		safeDispose(ipDocument);

		throw;
	}
}
//--------------------------------------------------------------------------------------------------
ILFDocumentPtr CVerifyToolbar::getNextDocument(CIDShieldLF *pIDShieldLF, int &rnDocsRemaining)
{
	// Worker thread //
	// (static)
	ASSERT_ARGUMENT("ELI21760", pIDShieldLF != NULL);

	ILFDocumentPtr ipDocument = NULL;

	m_nDocsRemaining--;
	rnDocsRemaining = m_nDocsRemaining;

	if (m_pDocumentCollection != NULL)
	{
		ipDocument = m_pDocumentCollection->getNextItem();
	}
	else if (m_plistDocumentCollection != NULL)
	{
		m_plistDocumentCollection->erase(m_plistDocumentCollection->begin());

		if (!m_plistDocumentCollection->empty())
		{
			ipDocument = m_plistDocumentCollection->begin()->getItem(pIDShieldLF->m_ipDatabase);
		}
	}
	else
	{
		THROW_LOGIC_ERROR_EXCEPTION("ELI21285");
	}

	ASSERT_RESOURCE_ALLOCATION("ELI21289", (m_nDocsRemaining == -1) == (ipDocument == NULL));

	return ipDocument;
}
//--------------------------------------------------------------------------------------------------
bool CVerifyToolbar::checkCanVerify(ILFDocumentPtr ipDocument, bool &rbCancelled)
{
	// Worker thread //
	// (static)
	try
	{
		rbCancelled = false;
		string strDocName = asString(ipDocument->Name);
	
		try
		{
			verifyHasRight(ipDocument, ACCESS_READ);
			verifyHasRight(ipDocument, ACCESS_ANNOTATE);
			verifyHasRight(ipDocument, ACCESS_WRITE_METADATA);

			if (hasTag(ipDocument, gstrTAG_PENDING_VERIFICATION) == false ||
				hasTag(ipDocument, gstrTAG_VERIFYING) == true)
			{
				return false;
			}
		}
		catch (...)
		{
			string strMessage = "You do not have sufficient access or permissions to verify "
						"document \"" + strDocName + "\"!";
			int nResponse = ::MessageBox(NULL, strMessage.c_str(), "ID Shield Redaction Verification",
				MB_OKCANCEL|MB_ICONWARNING);
			if (nResponse == IDCANCEL)
			{
				rbCancelled = true;
			}

			return false;
		}

		try
		{
			ipDocument->LockObject(LOCK_TYPE_WRITE);
			
			// [FlexIDSIntegrations:103] Set the "Verifying" tag to prevent other instances of 
			// ID Shield from verifying this document at the same time.
			addTag(ipDocument, gstrTAG_VERIFYING);

			ipDocument->UnlockObject();
		}
		catch (...)
		{
			string strMessage = "Document \"" + strDocName + "\" is locked and cannot be verified. " +
				"This document may be being verified on another machine.\r\n";
			int nResponse = ::MessageBox(NULL, strMessage.c_str(), "ID Shield Redaction Verification",
				MB_OKCANCEL|MB_ICONWARNING);
			if (nResponse == IDCANCEL)
			{
				rbCancelled = true;
			}

			return false;
		}
	}
	CATCH_ALL_AND_RETHROW_AS_UCLID_EXCEPTION("ELI20900");

	return true;
}
//--------------------------------------------------------------------------------------------------
UINT CVerifyToolbar::showToolbar(LPVOID pData)
{
	// UI thread //
	// (static)
	CVerifyToolbar *pToolbar = (CVerifyToolbar *) pData;

	try
	{
		try
		{
			ASSERT_RESOURCE_ALLOCATION("ELI21458", pToolbar != NULL);

			// Display the toolbar
			pToolbar->Create(IDD, pToolbar->openDocument());
			pToolbar->ShowWindow(SW_SHOW);

			// Wait until the m_eventClosed fires.  Use messageWait() otherwise the toolbar's message
			// loop will not run.
			pToolbar->m_eventClosed.messageWait();
		}
		CATCH_ALL_AND_RETHROW_AS_UCLID_EXCEPTION("ELI21907");
	}
	catch (UCLIDException &ue)
	{
		ue.display();

		// Signal the close event so the worker thread completes
		try
		{
			pToolbar->m_eventClosed.signal();
		}
		CATCH_AND_LOG_ALL_EXCEPTIONS("ELI21908");
	}

	return 0;
}
//--------------------------------------------------------------------------------------------------
bool CVerifyToolbar::verifyDocument(
					map<long long, ILFTextAnnotationPtr> *pmapVerifiedTextAnnotations/* = NULL*/,
					map<long long, ILFImageBlockAnnotationPtr> *pmapVerifiedImageAnnotations/* = NULL*/)
{
	// Worker thread //
	try
	{
		m_eventClosed.reset();

		// Dispose of the document object before displaying it for verification to ensure it doesn't
		// prevent the verifier from making changes via the client interface.  The document will be 
		// reloaded after the user has finished with the document.
		if (m_ipDocument != NULL)
		{
			m_ipDocument->Dispose();
			m_ipDocument = NULL;
		}

		// Display the toolbar in a new thread
		AfxBeginThread(showToolbar, (LPVOID) this);

		// Wait until the m_eventClosed fires.
		m_eventClosed.wait();

		// If the document window still exists, it needs to be closed programatically
		if (IsWindow(m_hWndDocWindow))
		{
			// [FlexIDSIntegrations:22]
			// If the user has selected verify, this should be interpreted as permission to save
			// the document.  If a prompt is configured to be displayed on close, disable it
			// temporarily while closing the document window.

			// The location of the key to disable the save prompt is dependent upon the Client
			// version.
			string strHiddenPromptKey;
			(m_pIDShieldLF->m_strClientVersion.substr(0, 1) == "8")
				? strHiddenPromptKey = gstrREG_LF8_HIDDEN_PROMPTS
				: strHiddenPromptKey = gstrREG_LF7_HIDDEN_PROMPTS;
			
			string strExistingPromptValue;
			if (m_bVerified)
			{	
				if (m_pIDShieldLF->m_apCurrentUserRegSettings->keyExists(
						strHiddenPromptKey, gstrREG_LF_SAVEDOC_PROMPT))
				{
					strExistingPromptValue = m_pIDShieldLF->m_apCurrentUserRegSettings->getKeyValue(
						strHiddenPromptKey, gstrREG_LF_SAVEDOC_PROMPT, "");
				}

				if (strExistingPromptValue != strLF_HIDE_PROMPT)
				{
					m_pIDShieldLF->m_apCurrentUserRegSettings->setKeyValue(
						strHiddenPromptKey, gstrREG_LF_SAVEDOC_PROMPT, strLF_HIDE_PROMPT);
				}
			}
			
			// If the document window still exists, close it.  Using SendMessage instead of 
			// PostMessage + waiting results in trying to update a document that is still locked.
			// It seems possible the post + wait loop code here is only working most of the 
			// time, but could fail due to timing in some circumstances, but I have yet to see
			// that happen.  Also, if the SetFocus message is not sent first, that seems to cause
			// the client to loose track of which documents are selected.
			::PostMessage(m_hWndDocWindow, WM_SETFOCUS, 0, 0);
			::PostMessage(m_hWndDocWindow, WM_SYSCOMMAND, SC_CLOSE, 0);

			while (IsWindow(m_hWndDocWindow))
			{
				Sleep(100);
			}
			m_hWndDocWindow = NULL;

			// Restore the previous prompt-on-close setting if necessary.
			if (m_bVerified)
			{
				if (strExistingPromptValue.empty())
				{
					m_pIDShieldLF->m_apCurrentUserRegSettings->deleteKey(
						strHiddenPromptKey, gstrREG_LF_SAVEDOC_PROMPT);
				}
				else if (strExistingPromptValue != strLF_HIDE_PROMPT)
				{
					m_pIDShieldLF->m_apCurrentUserRegSettings->setKeyValue(
						strHiddenPromptKey, gstrREG_LF_SAVEDOC_PROMPT, strExistingPromptValue);
				}
			}
		}

		// Reload the latest version of the document so the tags/annotations can be updated. 
		reloadDocument(); // asserts m_ipDocument != NULL
		
		if (m_bVerified)
		{
			// Remove the clue annotations and tag the document as appropriate.

			// [FlexIDSIntegrations:51]
			if (m_pIDShieldLF->m_RepositorySettings.bEnsureTextRedactions)
			{
				ASSERT_RESOURCE_ALLOCATION("ELI21922", m_pIDShieldLF->m_apDlgWait.get() != NULL)
				m_pIDShieldLF->m_apDlgWait->showMessage("Validating redactions...");

				// Create a default, empty set of annotatations to use as the set of verified 
				// annotations.
				map<long long, ILFImageBlockAnnotationPtr> mapEmptyImageAnnotations;
				map<long long, ILFTextAnnotationPtr> mapEmptyTextAnnotations;

				// If a set of verified annotations was not passed it, set the pointer to the
				// empty set.
				if (pmapVerifiedImageAnnotations == NULL)
				{
					pmapVerifiedImageAnnotations = &mapEmptyImageAnnotations;
				}
				if (pmapVerifiedTextAnnotations == NULL)
				{
					pmapVerifiedTextAnnotations = &mapEmptyTextAnnotations;
				}

				// Ensure there is a corresponding image redaction for every text redaction.
				// Checks all redactions except the ones in pmapVerifiedTextAnnotations.
				bool bModifiedRedactions = 
					ensureCorrespondingImageAnnotations(m_ipDocument, true, 
									*pmapVerifiedTextAnnotations, *pmapVerifiedImageAnnotations);

				// Ensure there is a corresponding text redaction for every image redaction.
				// Checks all redactions except the ones in pmapVerifiedImageAnnotations
				bModifiedRedactions = 
					(ensureCorrespondingTextAnnotations(m_ipDocument, true, 
									*pmapVerifiedTextAnnotations, *pmapVerifiedImageAnnotations) ||
					bModifiedRedactions);

				// This is one place where it is better to use close() instead of hide() for the 
				// wait dialog.  If we use close, it keeps ID Shield as the foreground process which
				// is nice if an exception is thrown, but is also allows Laserfiche to fall out
				// of the foreground which makes going from one document to the next during
				// verification a headache.
				m_pIDShieldLF->m_apDlgWait->close();

				// If ensureCorrespondingTextAnnotations created any new annotations, prompt the
				// verifier to review the changes.
				if (bModifiedRedactions)
				{
					int nReviewChanges = ::MessageBox(m_pIDShieldLF->m_hwndClient, "To ensure "
						"corresponding data is redacted in both the image and text,\r\n"
						"additional redactions have been applied to this document.\r\n\r\n" 
						"Do you wish to review the changes?", gstrPRODUCT_NAME.c_str(), MB_YESNO);
					
					if (nReviewChanges == IDYES)
					{
						// Unlock the document so the user will again have access to modify it.
						m_ipDocument->UnlockObject();

						// Redisplay the document for verification
						return verifyDocument(pmapVerifiedTextAnnotations, pmapVerifiedImageAnnotations);
					}
				}
			}
			removeAnnotations(m_ipDocument, gcolorCLUE_HIGHLIGHT);
			addTag(m_ipDocument, gstrTAG_VERIFIED);
			removeTag(m_ipDocument, gstrTAG_PENDING_VERIFICATION);
			removeTag(m_ipDocument, gstrTAG_VERIFYING);

			return true;
		}
		else
		{
			// No longer currently verifying this document
			removeTag(m_ipDocument, gstrTAG_VERIFYING);

			// If the document was not verified, simply return false to indicate the verification
			// process has ended.
			return false;
		}
	}
	catch (...)
	{
		if (m_ipDocument != NULL)
		{
			try
			{
				removeTag(m_ipDocument, gstrTAG_VERIFYING);
			}
			catch (...) {}

			safeDispose(m_ipDocument);
		}
		
		throw;
	}
}
//--------------------------------------------------------------------------------------------------
void CVerifyToolbar::reloadDocument()
{
	// Worker thread //
	try
	{
		if (m_ipDocument != NULL)
		{
			// Dispose of the existing copy of the document
			m_ipDocument->Dispose();
			m_ipDocument = NULL;
		}

		// Reload the document by ID.
		m_ipDocument = m_pIDShieldLF->m_ipDatabase->GetEntryByID(m_nDocumentID);
		ASSERT_RESOURCE_ALLOCATION("ELI20910", m_ipDocument != NULL);

		m_ipDocument->LockObject(LOCK_TYPE_WRITE);
	}
	CATCH_ALL_AND_RETHROW_AS_UCLID_EXCEPTION("ELI20908");
}
//--------------------------------------------------------------------------------------------------
CWnd *CVerifyToolbar::openDocument()
{
	// UI thread //
	m_bStopped = false;
	m_bVerified = false;

	// Open the document with Laserfiche and retrieve a handle to the window
	m_hWndDocWindow = showDocument(m_pIDShieldLF->m_ipClient, m_pIDShieldLF->m_hwndClient, 
		m_pIDShieldLF->m_strClientVersion, m_strDocumentName, m_nDocumentID);

	if (IsWindow(m_hWndDocWindow) == FALSE)
	{
		// [FlexIDSIntegrations:2] Make sure we found one and only one open copy of the doc.
		string strMessage = string("Error displaying Laserfiche document!\r\n\r\n") +
			string("Please ensure Laserfiche can open document \"") + 
			m_strDocumentName + 
			string("\" and that there are not multiple copies of the ") +
			string("document already open.");
		throw UCLIDException("ELI21437", strMessage);
	}

	// Create a CWnd object for the return value.
	CWnd *pDocWindow = CWnd::FromHandle(m_hWndDocWindow);
	ASSERT_RESOURCE_ALLOCATION("ELI21449", pDocWindow != NULL);

	// Register a windows event hook to detect the closure of the document window by the user.
	DWORD dwThreadId = ::GetWindowThreadProcessId(m_hWndDocWindow, NULL);
	m_hDocWindowCloseEventHook = SetWinEventHook(EVENT_OBJECT_DESTROY, EVENT_OBJECT_DESTROY, NULL,
		WinEventProc, 0, dwThreadId, WINEVENT_SKIPOWNPROCESS | WINEVENT_OUTOFCONTEXT);

	ASSERT_RESOURCE_ALLOCATION("ELI21450", m_hDocWindowCloseEventHook != NULL);

	return pDocWindow;
}
//--------------------------------------------------------------------------------------------------
void CVerifyToolbar::CloseDocument()
{
	// UI thread //

	// Unhook the close document event to ensure no more events fire.
	if (m_hDocWindowCloseEventHook != NULL)
	{
		UnhookWinEvent(m_hDocWindowCloseEventHook);
		m_hDocWindowCloseEventHook = NULL;
	}

	// Store the toolbar window position in the registry
	CRect rectWindow;
	GetWindowRect(&rectWindow);

	// Cache the toolbar position before closing so that we can open the next one in the same position
	m_pIDShieldLF->m_apCurrentUserRegSettings->setKeyValue(
		gstrREG_VERIFY_WINDOW_KEY, gstrREG_VERIFY_WINDOW_LEFT, asString(rectWindow.left));
	m_pIDShieldLF->m_apCurrentUserRegSettings->setKeyValue(
		gstrREG_VERIFY_WINDOW_KEY, gstrREG_VERIFY_WINDOW_TOP, asString(rectWindow.top));

	// Close the toolbar
	DestroyWindow();

	// Signal the worker thread to close the document (if necessary) and move on to the
	// next document.
	m_eventClosed.signal();
}
//--------------------------------------------------------------------------------------------------
VOID CALLBACK CVerifyToolbar::WinEventProc(HWINEVENTHOOK hWinEventHook,
		DWORD event,
		HWND hwnd,
		LONG idObject,
		LONG idChild,
		DWORD dwEventThread,
		DWORD dwmsEventTime)
{
	// UI thread //
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		// Ensure we completely process an event before starting on the next.  Any event
		// here should result in the destruction & closure of the toolbar which could cause
		// problems if the toolbar still exists in a closing state.
		static CMutex mutexCloseEvent;
		CSingleLock lock(&mutexCloseEvent, TRUE);

		// m_hDocWindowCloseEventHook should be set to NULL after the first event is processed.
		if (m_pActiveToolbar == NULL && hWinEventHook == m_pActiveToolbar->m_hDocWindowCloseEventHook)
		{
			throw UCLIDException("ELI21453", "Cannot find active verification toolbar!");
		}

		// If the document window was closed, send a message to UI thread requesting the toolbar
		// to be closed.
		if (hwnd == m_pActiveToolbar->m_hWndDocWindow)
		{
			m_pActiveToolbar->SendMessage(WM_CLOSE_DOCUMENT);
		}
	}
	CATCH_AND_LOG_ALL_EXCEPTIONS("ELI21452");
}
//--------------------------------------------------------------------------------------------------
