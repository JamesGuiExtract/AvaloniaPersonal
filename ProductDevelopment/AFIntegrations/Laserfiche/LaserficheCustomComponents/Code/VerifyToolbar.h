//==================================================================================================
//
// COPYRIGHT (c) 2008 EXTRACT SYSTEMS LLC., IN PUBLISHED AND UNPUBLISHED WORKS
// ALL RIGHTS RESERVED.
//
// FILE:	VerifyToolbar.h
//
// PURPOSE:	A CIDShieldLF helper class. Displays the IDShieldLF's currently active documents
//			that need verification for verification in succession.
//
// NOTES:	This class uses 2 threads: A "worker" thread which is the same thread that launches
//			verification via the doVerification call, and a UI thread which displays the toolbar
//			and receives user input.
//
// AUTHORS:	Steve Kurth
//
//==================================================================================================
#pragma once

#include "stdafx.h"
#include "resource.h"
#include "IDShieldLFHelper.h"
#include "LFItemCollection.h"

#include <Win32Event.h>
#include <ImageButtonWithStyle.h>

#include <afxmt.h>

#include <string>
#include <map>
#include <list>
using namespace std;

class CIDShieldLF;

//--------------------------------------------------------------------------------------------------
// CVerifyToolbar
//--------------------------------------------------------------------------------------------------
class CVerifyToolbar : public CDialog, private CIDShieldLFHelper
{
	DECLARE_DYNAMIC(CVerifyToolbar)

private:
	CVerifyToolbar(CIDShieldLF *pIDShieldLF, ILFDocumentPtr ipDocument, 
				   int nVerifiedCount, int nRemainingCount);
	virtual ~CVerifyToolbar();

public:

	// Verify the specified documents
	static void doVerification(CIDShieldLF *pIDShieldLF, CLFItemCollection &rdocumentsToVerify);
	static void doVerification(CIDShieldLF *pIDShieldLF, list<CLFItem> &rlistDocumentsToVerify);

	enum { IDD = IDD_VERIFY_TOOLBAR };

protected:

	/////////////////////
	// Overrides
	/////////////////////
	virtual void DoDataExchange(CDataExchange* pDX);    // DDX/DDV support
	virtual BOOL OnInitDialog();

	/////////////////////
	// Message Handlers
	/////////////////////
	afx_msg void OnBnClickedVerify();
	afx_msg void OnBnClickedStop();
	LRESULT OnCloseDocument(WPARAM wParam, LPARAM lParam);
	afx_msg void OnSetFocus(CWnd* pOldWnd);

	/////////////////////
	// Control variables
	/////////////////////
	CImageButtonWithStyle m_btnVerify;
	CImageButtonWithStyle m_btnStop;
	CString m_zDocumentNum;
	CString m_zRemainingDocs;

private:

	////////////////
	// Variables
	////////////////

	// Statics to manage doVerification call
	static CMutex m_mutexDoVerification;
	static CVerifyToolbar *m_pActiveToolbar;
	static CLFItemCollection *m_pDocumentCollection;
	static list<CLFItem> *m_plistDocumentCollection;
	static long m_nDocsRemaining;

	// Events to manage synchronize threads & windows
	volatile HWINEVENTHOOK m_hDocWindowCloseEventHook;
	Win32Event m_eventClosed;

	// Verification result
	volatile bool m_bStopped;
	volatile bool m_bVerified;

	// Target document object and window handle
	ILFDocumentPtr m_ipDocument;
	long m_nDocumentID;
	string m_strDocumentName;
	HWND m_hWndDocWindow;

	// Keeps track of the first time the toolbar receives focus.
	bool m_bInitialFocus;

	/////////////////////
	// Static Methods
	/////////////////////

	// Performs verification.  Needs to be called by one of the public doVerification calls
	// to initialize the collection.
	static void doVerification(CIDShieldLF *pIDShieldLF);
	// Retrieves the next document to be verified whether m_pDocumentCollection or 
	// m_ipDocumentCollection represents the collection of documents to be verified.
	static ILFDocumentPtr getNextDocument(CIDShieldLF *pIDShieldLF, int &rnDocsRemaining);
	// Checks document access, locks and tags to make sure its able to be verified before
	// displaying the document for verification
	static bool checkCanVerify(ILFDocumentPtr ipDocument, bool &rbCancelled);
	// Displays the verification toolbar
	static UINT showToolbar(LPVOID pData);
	
	/////////////////////
	// Non-Static Methods
	/////////////////////

	// Displays the document for verification.  Returns true if the document was verified, false
	// if the document was not verified and the verification process should end.
	bool verifyDocument(map<long long, ILFTextAnnotationPtr> *pmapVerifiedTextAnnotations = NULL,
						map<long long, ILFImageBlockAnnotationPtr> *pmapVerifiedAnnotations = NULL);
	// Upon verifying a document, to lock the document to update the tags and remove highlights
	// we need to first reload the document to obtain the document with the users changes.
	void reloadDocument();
	// Performs verification.  Needs to be called by one of the public doVerification calls
	// to initialize the collection.
	void doVerification();
	// Opens m_ipDocument in Laserfiche.  Returns a CWnd object for the opened Laserfiche document
	// window.
	CWnd *openDocument();
	// Closes the active document and toolbar.
	void CloseDocument();
	// Handler for WinEvent hooks.  Used to handle the closure of Laserfiche document window.
	static VOID CALLBACK WinEventProc(HWINEVENTHOOK hWinEventHook,
		DWORD event,
		HWND hwnd,
		LONG idObject,
		LONG idChild,
		DWORD dwEventThread,
		DWORD dwmsEventTime);

	DECLARE_MESSAGE_MAP()
};
