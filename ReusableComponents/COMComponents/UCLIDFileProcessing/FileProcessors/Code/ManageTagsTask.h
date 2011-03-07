//==================================================================================================
//
// COPYRIGHT (c) 2009 EXTRACT SYSTEMS, LLC., IN PUBLISHED AND UNPUBLISHED WORKS
// ALL RIGHTS RESERVED.
//
// FILE:	ManageTagsTask.h
//
// PURPOSE:	Header file for Manage Tags file processing task
//
// AUTHORS:	Jeff Shergalis
//
//==================================================================================================

#pragma once

#include "resource.h"

#include <FPCategories.h>

#include <string>
#include <vector>

using namespace std;

//--------------------------------------------------------------------------------------------------
// CManageTagsTask
//--------------------------------------------------------------------------------------------------
class ATL_NO_VTABLE CManageTagsTask : 
	public CComObjectRootEx<CComMultiThreadModel>,
	public CComCoClass<CManageTagsTask, &CLSID_ManageTagsTask>,
	public IDispatchImpl<IManageTagsTask, &IID_IManageTagsTask, 
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
	public ISpecifyPropertyPagesImpl<CManageTagsTask>
{
public:
	CManageTagsTask();
	~CManageTagsTask();

DECLARE_REGISTRY_RESOURCEID(IDR_MANAGE_TAGS_TASK)

DECLARE_PROTECT_FINAL_CONSTRUCT()
	void FinalRelease();

//--------------------------------------------------------------------------------------------------
// MAP section
//--------------------------------------------------------------------------------------------------
BEGIN_COM_MAP(CManageTagsTask)
	COM_INTERFACE_ENTRY(IManageTagsTask)
	COM_INTERFACE_ENTRY2(IDispatch, IManageTagsTask)
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

BEGIN_PROP_MAP(CManageTagsTask)
	PROP_PAGE(CLSID_ManageTagsTaskPP)
END_PROP_MAP()

BEGIN_CATEGORY_MAP(CManageTagsTask)
	IMPLEMENTED_CATEGORY(CATID_FP_FILE_PROCESSORS)
END_CATEGORY_MAP()
//--------------------------------------------------------------------------------------------------

public:

//----------------------------------------------------------------------------------------------
// IManageTagsTask
//----------------------------------------------------------------------------------------------
	STDMETHOD(put_Operation)(EManageTagsOperationType newVal);
	STDMETHOD(get_Operation)(EManageTagsOperationType* pVal);
	STDMETHOD(put_Tags)(BSTR bstrTags);
	STDMETHOD(get_Tags)(BSTR* pbstrTags);

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
	STDMETHOD(raw_ProcessFile)(IFileRecord* pFileRecord, long nActionID, 
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
	EManageTagsOperationType m_operationType;

	vector<string> m_vecTags;

	// dirty flag
	bool m_bDirty;

//--------------------------------------------------------------------------------------------------
// Private methods
//--------------------------------------------------------------------------------------------------

	// PROMISE: Throws an exception if this component is not licensed. Runs successfully otherwise.
	void validateLicense();

	// Checks the list of tags and ensures they still exist in the database before
	// continuing
	void validateTags(const IFileProcessingDBPtr& ipDB);

	// Adds all the tags to the specified file
	void addTagsToFile(const IFileProcessingDBPtr& ipDB, long nFileID);

	// Removes all the tags from the specified file
	void removeTagsFromFile(const IFileProcessingDBPtr& ipDB, long nFileID);

	// Toggles all the tags on the specified file
	void toggleTagsOnFile(const IFileProcessingDBPtr& ipDB, long nFileID);
	
	// Returns a tokenized string that represents all tags
	string tokenizeTags();
};

OBJECT_ENTRY_AUTO(CLSID_ManageTagsTask, CManageTagsTask)
