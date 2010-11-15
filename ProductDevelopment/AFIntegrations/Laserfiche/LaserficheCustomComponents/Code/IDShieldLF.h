//==================================================================================================
//
// COPYRIGHT (c) 2008 EXTRACT SYSTEMS LLC., IN PUBLISHED AND UNPUBLISHED WORKS
// ALL RIGHTS RESERVED.
//
// FILE:	IDShieldLF.h
//
// PURPOSE:	Defines a coclass for the IIDShieldLF interface.  This interface provides the 
//			implementation of ID Shield integration with Laserfiche. Although the executables
//			called for and ID Shield operation are external to this project, virtually all of the
//			functionality for them is implemented in IIDShieldLF.
//
// NOTES:	
//
// AUTHORS:	Steve Kurth
//
//==================================================================================================

#pragma once
#include "resource.h"       // main symbols
#include "LaserficheCustomComponents.h"
#include "WaitDlg.h"
#include "LFItemCollection.h"

#include <ByteStream.h>
#include <ByteStreamManipulator.h>
#include <Win32Event.h>
#include <afxmt.h>

#include <string>
#include <vector>
#include <map>
#include <set>
#include <memory>
#include <algorithm>
using namespace std;

//--------------------------------------------------------------------------------------------------
// Forward Declarations
//--------------------------------------------------------------------------------------------------
class IConfigurationSettingsPersistenceMgr;
class CRepositorySettingsPP;
class CRedactionSettingsPP;
class CAboutPP;
class CServiceSettingsDlg;
class TemporaryFileName;
class INIFilePersistenceMgr;

//--------------------------------------------------------------------------------------------------
// Externs (of constants defined in IDShieldLF.cpp)
//--------------------------------------------------------------------------------------------------
extern const string gstrPRODUCT_NAME;
extern const string gstrSETTINGS_FILE;
extern const string gstrTAG_SETTINGS;
extern const string	gstrTAG_PENDING_PROCESSING;
extern const string	gstrTAG_PROCESSED;
extern const string	gstrTAG_PENDING_VERIFICATION;
extern const string	gstrTAG_VERIFYING;
extern const string	gstrTAG_VERIFIED;
extern const string	gstrTAG_FAILED_PROCESSING;
extern const string gstrREG_LASERFICHE_KEY;
extern const string gstrREG_SERVICE_KEY;
extern const string gstrREG_VERIFY_WINDOW_KEY;
extern const string gstrREG_SERVER;
extern const string gstrREG_REPOSITORY;
extern const string gstrREG_USER;
extern const string gstrREG_PASSWORD;
extern const string gstrREG_THREAD_COUNT;
extern const string gstrREG_LF7_HIDDEN_PROMPTS;
extern const string gstrREG_LF8_HIDDEN_PROMPTS;
extern const string gstrREG_LF_SAVEDOC_PROMPT;
extern const OLE_COLOR gcolorCLUE_HIGHLIGHT;
extern const string gstrREG_VERIFY_WINDOW_LEFT;
extern const string gstrREG_VERIFY_WINDOW_TOP;

//--------------------------------------------------------------------------------------------------
// CIDShieldLF
//--------------------------------------------------------------------------------------------------
class ATL_NO_VTABLE CIDShieldLF :
	public CComObjectRootEx<CComMultiThreadModel>,
	public CComCoClass<CIDShieldLF, &CLSID_IDShieldLF>,
	public ISupportErrorInfo,
	public IDispatchImpl<IIDShieldLF, &IID_IIDShieldLF, &LIBID_UCLID_LASERFICHECCLib>
{
	// Helper classes to implement ID Shield for Laserfiche UI elements
	friend class CRepositorySettingsPP;
	friend class CRedactionSettingsPP;
	friend class CVerifyToolbar;
	friend class CServiceSettingsDlg;
	// Base class of all UI helper classes
	friend class CIDShieldLFHelper;

public:
	CIDShieldLF();
	virtual ~CIDShieldLF();

	DECLARE_PROTECT_FINAL_CONSTRUCT()

	HRESULT FinalConstruct();
	void FinalRelease();

	DECLARE_REGISTRY_RESOURCEID(IDR_IDSHIELDLF)

	BEGIN_COM_MAP(CIDShieldLF)
		COM_INTERFACE_ENTRY(IIDShieldLF)
		COM_INTERFACE_ENTRY2(IDispatch, IIDShieldLF)
		COM_INTERFACE_ENTRY(ISupportErrorInfo)
	END_COM_MAP()

// IIDShieldLF
	STDMETHOD(ConnectPrompt)(EConnectionMode eConnectionMode, VARIANT_BOOL *pbSuccess);
	STDMETHOD(ConnectToRepository)(BSTR bstrServer, BSTR bstrRepository, BSTR bstrUser, 
			BSTR bstrPassword, EConnectionMode eConnectionMode);
	STDMETHOD(ConnectToActiveClient)(EConnectionMode eConnectionMode, VARIANT_BOOL *pbSuccess);
	STDMETHOD(Disconnect)(void);
	STDMETHOD(ShowAdminConsole)(void);
	STDMETHOD(RedactSelected)(void);
	STDMETHOD(SubmitSelectedForRedaction)(void);
	STDMETHOD(VerifySelected)(void);
	STDMETHOD(ShowServiceConsole)(void);
	STDMETHOD(StartBackgroundProcessing)(void);
	STDMETHOD(StopBackgroundProcessing)(void);

// ISupportsErrorInfo
	STDMETHOD(InterfaceSupportsErrorInfo)(REFIID riid);

private:

	/////////////////
	// Variables
	/////////////////

	// m_RepositorySettings reflects the repository settings (from IDShieldLF.ini) for any
	// repository this class is currently logged into.
	struct
	{
		// Path to the rules file to use when performing redaction
		string strMasterRSD;
		
		// Which types of data to redact.
		bool bRedactHCData;
		bool bRedactMCData;
		bool bRedactLCData;

		// Whether to automatically tag redacted documents for verification
		bool bAutoTagForVerify;
		
		// If bAutoTagForVerify, whether to tag all documents (true), or just those with sensitive
		// data (false).
		bool bTagAllForVerify;
		
		// If bAutoTagForVerify, whether to automatically launch verification after the last
		// document in an On-Demand redaction request has been processed.
		bool bOnDemandVerify;

		// Whether to programatically ensure corresponding text/image redactions upon document 
		// verification.
		bool bEnsureTextRedactions;

		// [FlexIDSIntegrations:137] For searches in a Laserfiche repository, a type of "B" needs
		// to be used to find documents without a template.  But because at least one customer
		// is experiencing slow search performance using the "D" type, the search types used can now be
		// specified in the ini file to allow customers flexibility in how searches are performed.
		// (The search types are not exposed in the admin console UI, however)
		string strDocumentSearchType;
		string strFolderSearchType;
		string strAllSearchType;
	} m_RepositorySettings;

	// Thread saftey to ensure a class instance cannot be destroyed before its helpers are.
	Win32Event m_eventHelpersDone;
	volatile LONG m_nHelperReferenceCount;

	// Keeps track of the current connection state
	bool				m_bAttachedToClient;
	string				m_strClientVersion;
	EConnectionMode		m_eConnectionMode;

	auto_ptr<CWaitDlg>	m_apDlgWait;

	// Important Laserfiche objects in the currently connected repository
	ILFClientPtr		m_ipClient;
	ILFConnectionPtr	m_ipConnection;
	ILFDatabasePtr		m_ipDatabase;
	ILFFolderPtr		m_ipSettingsFolder;
	ILFDocumentPtr		m_ipSettingsFile;

	// Maximum number of docs that can be processed at once via the client.
	long m_nMaxDocsToProcess;

	// Used to store/retrieve registry settings
	auto_ptr<TemporaryFileName>						m_apLocalSettingsFile;
	auto_ptr<INIFilePersistenceMgr>					m_apSettingsMgr;
	auto_ptr<IConfigurationSettingsPersistenceMgr>	m_apCurrentUserRegSettings;
	auto_ptr<IConfigurationSettingsPersistenceMgr>	m_apLocalMachineRegSettings;

	// Window handles to Laserfiche windows
	HWND m_hwndClient;

	// Administration Console windows
	auto_ptr<CPropertySheet>		m_apPropertySheet;
	auto_ptr<CRepositorySettingsPP> m_appageRepository;
	auto_ptr<CRedactionSettingsPP>	m_appageRedaction;
	auto_ptr<CAboutPP>				m_appageAbout;

	// Variables used to coordinate background redaction threads
	static volatile LONG	m_nRunningThreads;
	static volatile bool	m_bStopService;
	static long				m_nBackgroundSearchInterval;
	static Win32Event		m_eventWorkerThreadFailed;
	static Win32Event		m_eventWorkerThreadsDone;
	static Win32Event		m_eventServiceStarted;
	static Win32Event		m_eventServiceStopped;
	static auto_ptr<CLFItemCollection> m_apDocumentsToProcess;
	static CMutex			m_mutexLFOperations;

	/////////////////
	// Methods
	/////////////////

	// Ensure the current connection and database objects exist. (Throws exception on failed validation)
	void validateConnection();
	
	// Ensures the current repository is properly configured and that the logged in user has all
	// required permissions. Throws exception on failed validation.  If a connect had been established
	// prior to validation failure, the connection will be closed prior to throwing the exception.
	void validateConnection(EConnectionMode eConnectionMode);
	
	// Throws an exception if the current user does not have all necessary permissions.
	void validateRepositoryPrivileges(EConnectionMode eConnectionMode);
	
	// Throws an exception if the current repository is not initialized/configured correctly.
	void validateRepositorySettings(EConnectionMode eConnectionMode);
	
	// Locates the ID Shield settings folder (bRefresh = true to force it to search again for the
	// folder even if it had previously found it)
	ILFFolderPtr getSettingsFolder(bool bRefresh = false);
	
	// Locate the ID Shield settings file (bRefresh = true to force it to search again for the
	// folder even if it had previously found it)
	ILFDocumentPtr getSettingsFile(bool bRefresh = false);
	
	// Returns a list of tags ID Shield needs but that do not exist in the current repository
	vector<string> findMissingTags();
	
	// Creates all tags ID Shield needs in the current repository
	void createTags(const vector<string> &vecTagsToCreate);
	
	// Load the current repository's settings into m_RepositorySettings
	void loadSettings(bool bReload = false);
	
	// Saves m_RepositorySettings into the current repository's settings file.
	void saveSettings();
	
	// true to show the redaction settings tab, false to hide it.
	void showRedactionTab(bool bShow = true);

	// Submits or redacts the currently selected documents in the Laserfice Client.
	// If bOnDemand == true, the documents are redacted immediately.  
	// If bOnDemand == false, the documents are only submitted for background redaction
	void processSelected(bool bOnDemand);
	
	// Checks the supplied set of documents (rlistDocuments) for documents that don't contain
	// images to redact, that are locked, that the user doesn't have permission to process or 
	// that have already been processed or have failed processing.  Prompts user as necessary
	// and returns a set of documents that reflects the user response (possibly empty).
	// Returns false if the user pressed cancel on any of the potential prompts, true otherwise.
	bool prepareListForRedaction(list<CLFItem> &rlistDocuments, bool bOnDemand);
	
	// Redacts the specified file and modifies its tags as necessary.
	bool processDocument(ILFDocumentPtr &ripDocument, bool bOnDemand, bool *pbVerifyNow = NULL,
		IProgressStatusPtr ipProgressStatus = NULL);
	
	// Submits the specified document for background redaction.
	bool submitDocument(ILFDocumentPtr &ripDocument);
	
	// Calculates and applies redactions for the current document and/or adds clue highlights 
	// as necessary.
	bool redactDocument(ILFDocumentPtr ipDocument, IProgressStatusPtr ipProgressStatus = NULL);
	
	// Opens all documents in rdocumentsToVerify or rlistDocumentsToVerify set in sequence for 
	// verification.
	void verify(CLFItemCollection &rdocumentsToVerify);
	void verify(list<CLFItem> &rlistDocumentsToVerify);
	
	// Unlocks any remaining documents in rlistDocuments parameter. Displays cancelling status
	// in ipProgressStatus (if provided)
	void cancel(list<CLFItem> &rlistDocuments, IProgressStatusPtr ipProgressStatus = NULL);
	
	// Same as cancel except intended for a situation in which an exception has been caught. 
	// Unlocks the documents with safeDispose to unlock as many as possible without generating
	// further exceptions.
	void safeDisposeDocuments(list<CLFItem> &rlistDocuments);

	// The master thread for running background redaction. It is in charge of generating the
	// search for documents that need redaction (m_ipActiveSearch) and launching the worker 
	// threads to process the documents.
	static UINT runBackgroundMasterThread(LPVOID pData);
	
	// Each worker thread will continue to pull documents out of m_ipActiveSearch and process 
	// them until there are no more documents awaiting redaction. Once all documents in the
	// current set have been processed, the thread will end.
	static UINT runBackgroundWorkerThread(LPVOID pData);
	
	// Checks the given document's permissions, locks and processing flags to determine if 
	// background processing should be attempted on the document.
	static bool checkCanRedact(ILFDocumentPtr ipDocument);

	// Retrieves the image redactions from the specified page and caches the results to
	// rmapDocumentCache. Subsequent calls using this cache will retrieve the data
	// from the cache rather than re-accessing the page itself.
	vector<ILFImageBlockAnnotationPtr> getRedactionsOnPage(ILFPagePtr ipPage,
							map<long long, vector<ILFImageBlockAnnotationPtr> > &rmapDocumentCache);

	// Supplies the key for encrypting/decrypting passwords
	ByteStream &getPasswordKey();
	
	// Encrypts a password for storage in the registry;
	void encryptPassword(string &rstrPassword);
	
	// Decrypts an encrypted password from registry storage.
	void decryptPassword(string &rstrPassword);

	// Closes the opened connection and releases all associated resources.
	void disconnect();

	// Validate the license file at a level that supports the indicated connection mode.
	void validateLicense(EConnectionMode eConnectionMode);

	// Retrieve the active LF repository and user from the 8.0 API
	bool GetLoginInfoFrom80(string &rstrServer, string &rstrRepository, string &rstrUser);

	// Retrieve the active LF repository and user from the 8.1 API
	bool GetLoginInfoFrom81(string &rstrServer, string &rstrRepository, string &rstrUser);
};

OBJECT_ENTRY_AUTO(__uuidof(IDShieldLF), CIDShieldLF)
