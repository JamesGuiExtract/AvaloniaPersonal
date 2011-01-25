//==================================================================================================
//
// COPYRIGHT (c) 2009 EXTRACT SYSTEMS, LLC., IN PUBLISHED AND UNPUBLISHED WORKS
// ALL RIGHTS RESERVED.
//
// FILE:	SleepTask.h
//
// PURPOSE:	Header file for Manage Tags file processing task
//
// AUTHORS:	Jeff Shergalis
//
//==================================================================================================

#pragma once

#include "resource.h"

#include <FPCategories.h>
#include <Random.h>

using namespace std;

//--------------------------------------------------------------------------------------------------
// CSleepTask
//--------------------------------------------------------------------------------------------------
class ATL_NO_VTABLE CSleepTask : 
	public CComObjectRootEx<CComMultiThreadModel>,
	public CComCoClass<CSleepTask, &CLSID_SleepTask>,
	public IDispatchImpl<ISleepTask, &IID_ISleepTask, 
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
	public ISpecifyPropertyPagesImpl<CSleepTask>
{
public:
	CSleepTask();
	~CSleepTask() {};

DECLARE_REGISTRY_RESOURCEID(IDR_SLEEP_TASK)

DECLARE_PROTECT_FINAL_CONSTRUCT()
	void FinalRelease() {};

//--------------------------------------------------------------------------------------------------
// MAP section
//--------------------------------------------------------------------------------------------------
BEGIN_COM_MAP(CSleepTask)
	COM_INTERFACE_ENTRY(ISleepTask)
	COM_INTERFACE_ENTRY2(IDispatch, ISleepTask)
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

BEGIN_PROP_MAP(CSleepTask)
	PROP_PAGE(CLSID_SleepTaskPP)
END_PROP_MAP()

BEGIN_CATEGORY_MAP(CSleepTask)
	IMPLEMENTED_CATEGORY(CATID_FP_FILE_PROCESSORS)
END_CATEGORY_MAP()
//--------------------------------------------------------------------------------------------------

public:

//----------------------------------------------------------------------------------------------
// ISleepTask
//----------------------------------------------------------------------------------------------
	STDMETHOD(get_SleepTime)(long* plSleepTime);
	STDMETHOD(put_SleepTime)(long lSleepTime);
	STDMETHOD(get_TimeUnits)(ESleepTimeUnitType* peTimeUnits);
	STDMETHOD(put_TimeUnits)(ESleepTimeUnitType eTimeUnits);
	STDMETHOD(get_Random)(VARIANT_BOOL* pbRandom);
	STDMETHOD(put_Random)(VARIANT_BOOL bRandom);

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

	// The amount of time to sleep
	long m_lSleepTime;

	// The units for the time value
	ESleepTimeUnitType m_TimeUnits;

	// Whether the sleep time should be a random amount of time
	bool m_bRandom;

	// The random class for handling random number generation
	Random m_Random;

	// dirty flag
	bool m_bDirty;

	// Indicates the task is being cancelled
	volatile bool m_bCancel;

//--------------------------------------------------------------------------------------------------
// Private methods
//--------------------------------------------------------------------------------------------------

	// PROMISE: Throws an exception if this component is not licensed. Runs successfully otherwise.
	void validateLicense();

	DWORD computeSleepTime();

};

OBJECT_ENTRY_AUTO(CLSID_SleepTask, CSleepTask)
