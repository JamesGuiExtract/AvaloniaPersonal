// FolderFS.h : Declaration of the CFolderFS

#pragma once
#include "resource.h"       // main symbols

#include "..\..\Code\FPCategories.h"
#include "ESFileSuppliers.h"
#include <Win32Event.h>
#include <FileDirectorySearcher.h>
#include <FolderEventsListener.h>

#include <vector>

#if defined(_WIN32_WCE) && !defined(_CE_DCOM) && !defined(_CE_ALLOW_SINGLE_THREADED_OBJECTS_IN_MTA)
#error "Single-threaded COM objects are not properly supported on Windows CE platform, such as the Windows Mobile platforms that do not include full DCOM support. Define _CE_ALLOW_SINGLE_THREADED_OBJECTS_IN_MTA to force ATL to support creating single-thread COM object's and allow use of it's single-threaded COM object implementations. The threading model in your rgs file was set to 'Free' as that is the only threading model supported in non DCOM Windows CE platforms."
#endif

// CFolderFS

class ATL_NO_VTABLE CFolderFS :
	public FileDirectorySearcherBase,
	public FolderEventsListener,
	public CComObjectRootEx<CComMultiThreadModel>,
	public CComCoClass<CFolderFS, &CLSID_FolderFS>,
	public ISupportErrorInfo,
	public IDispatchImpl<IFolderFS, &IID_IFolderFS, &LIBID_EXTRACT_FILESUPPLIERSLib, /*wMajor =*/ 1, /*wMinor =*/ 0>,
	public IDispatchImpl<ICategorizedComponent, &__uuidof(ICategorizedComponent), &LIBID_UCLID_COMUTILSLib, /* wMajor = */ 1>,
	public IDispatchImpl<ICopyableObject, &__uuidof(ICopyableObject), &LIBID_UCLID_COMUTILSLib, /* wMajor = */ 1>,
	public IPersistStream,
	public IDispatchImpl<IFileSupplier, &__uuidof(IFileSupplier), &LIBID_UCLID_FILEPROCESSINGLib, /* wMajor = */ 1>,
	public IDispatchImpl<ILicensedComponent, &__uuidof(ILicensedComponent), &LIBID_UCLID_COMLMLib, /* wMajor = */ 1>,
	public IDispatchImpl<IMustBeConfiguredObject, &__uuidof(IMustBeConfiguredObject), &LIBID_UCLID_COMUTILSLib, /* wMajor = */ 1>,
	public ISpecifyPropertyPagesImpl<CFolderFS>
{
public:
	CFolderFS();

	DECLARE_REGISTRY_RESOURCEID(IDR_FOLDERFS)

	DECLARE_PROTECT_FINAL_CONSTRUCT()

	HRESULT FinalConstruct()
	{
		return S_OK;
	}

	void FinalRelease()
	{
	}

	BEGIN_COM_MAP(CFolderFS)
		COM_INTERFACE_ENTRY(IFolderFS)
		COM_INTERFACE_ENTRY2(IDispatch, ICategorizedComponent)
		COM_INTERFACE_ENTRY(ISupportErrorInfo)
		COM_INTERFACE_ENTRY(ICategorizedComponent)
		COM_INTERFACE_ENTRY(ICopyableObject)
		COM_INTERFACE_ENTRY(IFileSupplier)
		COM_INTERFACE_ENTRY(ILicensedComponent)
		COM_INTERFACE_ENTRY(IPersistStream)
		COM_INTERFACE_ENTRY(IMustBeConfiguredObject)
		COM_INTERFACE_ENTRY_IMPL(ISpecifyPropertyPages)
	END_COM_MAP()

	BEGIN_PROP_MAP(CFolderFS)
		PROP_PAGE(CLSID_FolderFSPP)
	END_PROP_MAP()

	BEGIN_CATEGORY_MAP(CFolderFS)
		IMPLEMENTED_CATEGORY(CATID_FP_FILE_SUPPLIERS)
	END_CATEGORY_MAP()

public:

	// ISupportsErrorInfo
	STDMETHOD(InterfaceSupportsErrorInfo)(REFIID riid);

	// ICategorizedComponent Methods
	STDMETHOD(raw_GetComponentDescription)(BSTR * pstrComponentDescription);

	// ICopyableObject Methods
	STDMETHOD(raw_Clone)(IUnknown * * pObject);
	STDMETHOD(raw_CopyFrom)(IUnknown * pObject);

	// IFileSupplier Methods
	STDMETHOD(raw_Start)(IFileSupplierTarget * pTarget, IFAMTagManager *pFAMTM,
		IFileProcessingDB* pDB, long nActionID);
	STDMETHOD(raw_Stop)();
	STDMETHOD(raw_Pause)();
	STDMETHOD(raw_Resume)();

	// ILicensedComponent Methods
	STDMETHOD(raw_IsLicensed)(VARIANT_BOOL * pbValue);

	// IMustBeConfiguredObject Methods
	STDMETHOD(raw_IsConfigured)(VARIANT_BOOL * bConfigured);

	// IPersistStream
	STDMETHOD(GetClassID)(CLSID *pClassID);
	STDMETHOD(IsDirty)(void);
	STDMETHOD(Load)(IStream *pStream);
	STDMETHOD(Save)(IStream *pStream, BOOL fClearDirty);
	STDMETHOD(GetSizeMax)(ULARGE_INTEGER *pcbSize);

	// IFolderFS
	STDMETHOD(put_FolderName)(BSTR newVal);
	STDMETHOD(get_FolderName)(BSTR *pVal );
	STDMETHOD(put_FileExtensions)(BSTR newVal);
	STDMETHOD(get_FileExtensions)(BSTR *pVal );
	STDMETHOD(put_RecurseFolders)( VARIANT_BOOL bVal);
	STDMETHOD(get_RecurseFolders)( VARIANT_BOOL *pbVal);
	STDMETHOD(put_AddedFiles)( VARIANT_BOOL bVal);
	STDMETHOD(get_AddedFiles)( VARIANT_BOOL *pbVal);
	STDMETHOD(put_ModifiedFiles)(VARIANT_BOOL bVal);
	STDMETHOD(get_ModifiedFiles)( VARIANT_BOOL *pbVal);
	STDMETHOD(put_TargetOfMoveOrRename)( VARIANT_BOOL bVal);
	STDMETHOD(get_TargetOfMoveOrRename)( VARIANT_BOOL *pbVal);
	STDMETHOD(put_NoExistingFiles)( VARIANT_BOOL bVal);
	STDMETHOD(get_NoExistingFiles)( VARIANT_BOOL *pbVal);

protected:
	// override FileDirectorySearcherBase addFile method
	virtual void addFile( const std::string &strFile );
	virtual bool shouldStop();

	// overrides for FolderEventsListener
	virtual bool fileMatchPattern(const std::string& strFileName);
	virtual void onFileAdded(const std::string& strFileName);
	virtual void onFileRemoved(const std::string& strFileName);
	virtual void onFileRenamed(const std::string& strOldName, const std::string strNewName);
	virtual void onFileModified(const std::string& strFileName);

	virtual void onFolderRemoved(const std::string& strFolderName);
	virtual void onFolderRenamed(const std::string& strOldName, const std::string& strNewName);

private:
	// Variables

	// Dirty Flag
	bool m_bDirty;

	// This event indicates the searching and/or listening should stop
	Win32Event m_StopEvent;

	// This event indicates that searching thead has exited
	Win32Event m_eventSearchingExited;

	// This event indicates that suppling has been started
	Win32Event m_eventSupplyingStarted;

	// Signaled indicates the supplying should be resumed
	// Not signaled indicates that the supplying is paused
	Win32Event m_eventResume;

	// Name of the folder to search and/or listen
	std::string m_strFolderName;
	
	// Extensions to search and/or listen
	std::vector<std::string> m_vecFileExtensions;

	// Used to pause execution in the thread, this should not be referenced unless the m_eventSearchingExited is not signaled
	CWinThread * m_pSearchThread;

	// The ID of the most recently started searching thread.
	volatile int m_nCurrentSearchThreadID;

	// Flag to indicate searching and/or listening should recurse folders
	bool m_bRecurseFolders;

	// Flag to indicate Added files should be processed if listening
	bool m_bAddedFiles;

	// Flag to indicate Modified files should be processed if listening
	bool m_bModifiedFiles;

	// Flag to indicate that the name destination name should be processed if listening
	bool m_bTargetOfMoveOrRename;

	// Flag to indicate that only no existing files should be processed ( only valid for listening )
	bool m_bNoExistingFiles;

	// Pointer to the Target passed in the Start method
	IFileSupplierTargetPtr m_ipTarget;

	// Name of the folder to search and/or listen after expanding all tags and functions
	std::string m_strExpandFolderName;

	// Methods
	
	void validateLicense();

	// Thread function to search for files
	static UINT searchFileThread(LPVOID pParam );

	// Thread function to stop searching for files
	static UINT stopSearchFileThread(LPVOID pParam);

	// This function is called from the searchFileThread function and finds all files
	// for each extension in m_vecFileExtensions
	void searchForFiles();

	// return IFAMTagManager pointer
	IFAMTagManagerPtr getFAMTagManager();

	// Save the folder settings
	bool saveFolderFS(EXTRACT_FILESUPPLIERSLib::IFolderFSPtr ipFolderFS);
};

OBJECT_ENTRY_AUTO(__uuidof(FolderFS), CFolderFS)
