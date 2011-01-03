//==================================================================================================
//
// COPYRIGHT (c) 2009 EXTRACT SYSTEMS, LLC., IN PUBLISHED AND UNPUBLISHED WORKS
// ALL RIGHTS RESERVED.
//
// FILE:	ArchiveRestoreTask.h
//
// PURPOSE:	Header file for File Archive file processing task
//
// AUTHORS:	Jeff Shergalis
//
//==================================================================================================

#pragma once

#include "resource.h"

#include <FPCategories.h>

#include <string>

using namespace std;

//--------------------------------------------------------------------------------------------------
// CArchiveRestoreTask
//--------------------------------------------------------------------------------------------------
class ATL_NO_VTABLE CArchiveRestoreTask : 
	public CComObjectRootEx<CComMultiThreadModel>,
	public CComCoClass<CArchiveRestoreTask, &CLSID_ArchiveRestoreTask>,
	public IDispatchImpl<IArchiveRestoreTask, &IID_IArchiveRestoreTask, 
		&LIBID_UCLID_FILEPROCESSINGLib>,
	public IDispatchImpl<ICategorizedComponent, &IID_ICategorizedComponent, 
		&LIBID_UCLID_COMUTILSLib>,
	public IDispatchImpl<ICopyableObject, &IID_ICopyableObject, 
		&LIBID_UCLID_COMUTILSLib>,
	public IDispatchImpl<IFileProcessingTask, &IID_IFileProcessingTask, 
		&LIBID_UCLID_FILEPROCESSINGLib>,
	public IDispatchImpl<ILicensedComponent, &IID_ILicensedComponent, &LIBID_UCLID_COMLMLib>,
	public IDispatchImpl<IMustBeConfiguredObject, &IID_IMustBeConfiguredObject, 
		&LIBID_UCLID_COMUTILSLib>,
	public IPersistStream,
	public ISupportErrorInfo,
	public ISpecifyPropertyPagesImpl<CArchiveRestoreTask>
{
public:
	CArchiveRestoreTask();
	~CArchiveRestoreTask();

DECLARE_REGISTRY_RESOURCEID(IDR_ARCHIVE_RESTORE_TASK)

DECLARE_PROTECT_FINAL_CONSTRUCT()

//--------------------------------------------------------------------------------------------------
// MAP section
//--------------------------------------------------------------------------------------------------
BEGIN_COM_MAP(CArchiveRestoreTask)
	COM_INTERFACE_ENTRY(IArchiveRestoreTask)
	COM_INTERFACE_ENTRY2(IDispatch, IArchiveRestoreTask)
	COM_INTERFACE_ENTRY(ICategorizedComponent)
	COM_INTERFACE_ENTRY(ICopyableObject)
	COM_INTERFACE_ENTRY(IFileProcessingTask)
	COM_INTERFACE_ENTRY(IAccessRequired)
	COM_INTERFACE_ENTRY(ILicensedComponent)
	COM_INTERFACE_ENTRY(IMustBeConfiguredObject)
	COM_INTERFACE_ENTRY(IPersistStream)
	COM_INTERFACE_ENTRY(ISupportErrorInfo)
	COM_INTERFACE_ENTRY_IMPL(ISpecifyPropertyPages)
END_COM_MAP()

BEGIN_PROP_MAP(CArchiveRestoreTask)
	PROP_PAGE(CLSID_ArchiveRestoreTaskPP)
END_PROP_MAP()

BEGIN_CATEGORY_MAP(CArchiveRestoreTask)
	IMPLEMENTED_CATEGORY(CATID_FP_FILE_PROCESSORS)
END_CATEGORY_MAP()
//--------------------------------------------------------------------------------------------------

public:

//----------------------------------------------------------------------------------------------
// IArchiveRestoreTask
//----------------------------------------------------------------------------------------------
	STDMETHOD(put_Operation)(EArchiveRestoreOperationType newVal);
	STDMETHOD(get_Operation)(EArchiveRestoreOperationType* pVal);
	STDMETHOD(put_ArchiveFolder)(BSTR bstrArchiveFolder);
	STDMETHOD(get_ArchiveFolder)(BSTR* pbstrArchiveFolder);
	STDMETHOD(put_FileTag)(BSTR bstrFileTag);
	STDMETHOD(get_FileTag)(BSTR* pbstrFileTag);
	STDMETHOD(put_AllowOverwrite)(VARIANT_BOOL newVal);
	STDMETHOD(get_AllowOverwrite)(VARIANT_BOOL* pVal);
	STDMETHOD(put_FileToArchive)(BSTR bstrFileToArchive);
	STDMETHOD(get_FileToArchive)(BSTR* pbstrFileToArchive);
	STDMETHOD(put_DeleteFileAfterArchive)(VARIANT_BOOL newVal);
	STDMETHOD(get_DeleteFileAfterArchive)(VARIANT_BOOL* pVal);

//----------------------------------------------------------------------------------------------
// ICategorizedComponent
//----------------------------------------------------------------------------------------------
	STDMETHOD(raw_GetComponentDescription)(BSTR* pstrComponentDescription);

//----------------------------------------------------------------------------------------------
// ICopyableObject
//----------------------------------------------------------------------------------------------
	STDMETHOD(raw_Clone)(IUnknown** pObject);
	STDMETHOD(raw_CopyFrom)(IUnknown* pObject);

//----------------------------------------------------------------------------------------------
// IFileProcessingTask
//----------------------------------------------------------------------------------------------
	STDMETHOD(raw_Init)(long nActionID, IFAMTagManager* pFAMTM, IFileProcessingDB *pDB);
	STDMETHOD(raw_ProcessFile)(BSTR bstrFileFullName, long nFileID, long nActionID, 
		IFAMTagManager *pTagManager, IFileProcessingDB *pDB, IProgressStatus *pProgressStatus,
		VARIANT_BOOL bCancelRequested, EFileProcessingResult *pResult);
	STDMETHOD(raw_Cancel)();
	STDMETHOD(raw_Close)();

// IAccessRequired
	STDMETHOD(raw_RequiresAdminAccess)(VARIANT_BOOL* pbResult);

//----------------------------------------------------------------------------------------------
// ILicensedComponent
//----------------------------------------------------------------------------------------------
	STDMETHOD(raw_IsLicensed)(VARIANT_BOOL* pbValue);

//----------------------------------------------------------------------------------------------
// IMustBeConfiguredObject
//----------------------------------------------------------------------------------------------
	STDMETHOD(raw_IsConfigured)(VARIANT_BOOL* pbValue);

//----------------------------------------------------------------------------------------------
// IPersistStream
//----------------------------------------------------------------------------------------------
	STDMETHOD(GetClassID)(CLSID *pClassID);
	STDMETHOD(IsDirty)();
	STDMETHOD(Load)(IStream *pStm);
	STDMETHOD(Save)(IStream *pStm, BOOL fClearDirty);
	STDMETHOD(GetSizeMax)(ULARGE_INTEGER *pcbSize);

//----------------------------------------------------------------------------------------------
// ISupportsErrorInfo
//----------------------------------------------------------------------------------------------
	STDMETHOD(InterfaceSupportsErrorInfo)(REFIID riid);

private:

//--------------------------------------------------------------------------------------------------
// Private variables
//--------------------------------------------------------------------------------------------------

	// The operation to perform archive/restore
	EArchiveRestoreOperationType m_operationType;

	// The root folder for the archive/restore operation
	string m_strArchiveFolder;

	// The file tag for the archive/restore operation
	string m_strFileTag;

	// Whether to allow the archive operation to overwrite an existing file
	bool m_bAllowOverwrite;

	// The file to archive/restore
	string m_strFileToArchive;

	// Whether to delete the file after archiving
	bool m_bDeleteFileAfterArchiving;

	// dirty flag
	bool m_bDirty;

//--------------------------------------------------------------------------------------------------
// Private methods
//--------------------------------------------------------------------------------------------------

	//----------------------------------------------------------------------------------------------
	// PROMISE: Throws an exception if this component is not licensed. Runs successfully otherwise.
	void validateLicense();
};

OBJECT_ENTRY_AUTO(CLSID_ArchiveRestoreTask, CArchiveRestoreTask)
