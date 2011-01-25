//==================================================================================================
//
// COPYRIGHT (c) 2008 EXTRACT SYSTEMS, LLC., IN PUBLISHED AND UNPUBLISHED WORKS
// ALL RIGHTS RESERVED.
//
// FILE:	AddWatermarkTask.h
//
// PURPOSE:	Header file for Add Watermark file processing task
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
// CAddWatermarkTask
//--------------------------------------------------------------------------------------------------
class ATL_NO_VTABLE CAddWatermarkTask : 
	public CComObjectRootEx<CComMultiThreadModel>,
	public CComCoClass<CAddWatermarkTask, &CLSID_AddWatermarkTask>,
	public IDispatchImpl<IAddWatermarkTask, &IID_IAddWatermarkTask, 
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
	public ISpecifyPropertyPagesImpl<CAddWatermarkTask>
{
public:
	CAddWatermarkTask();
	~CAddWatermarkTask();

DECLARE_REGISTRY_RESOURCEID(IDR_ADD_WATERMARK_TASK)

DECLARE_PROTECT_FINAL_CONSTRUCT()

//--------------------------------------------------------------------------------------------------
// MAP section
//--------------------------------------------------------------------------------------------------
BEGIN_COM_MAP(CAddWatermarkTask)
	COM_INTERFACE_ENTRY(IAddWatermarkTask)
	COM_INTERFACE_ENTRY2(IDispatch, IAddWatermarkTask)
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

BEGIN_PROP_MAP(CAddWatermarkTask)
	PROP_PAGE(CLSID_AddWatermarkTaskPP)
END_PROP_MAP()

BEGIN_CATEGORY_MAP(CAddWatermarkTask)
	IMPLEMENTED_CATEGORY(CATID_FP_FILE_PROCESSORS)
END_CATEGORY_MAP()
//--------------------------------------------------------------------------------------------------

public:

	//----------------------------------------------------------------------------------------------
	// IAddWatermarkTask
	//----------------------------------------------------------------------------------------------
	STDMETHOD(put_InputImageFile)(BSTR bstrInputImageFile);
	STDMETHOD(get_InputImageFile)(BSTR* pbstrInputImageFile);
	STDMETHOD(put_StampImageFile)(BSTR bstrStampImageFile);
	STDMETHOD(get_StampImageFile)(BSTR* pbstrStampImageFile);
	STDMETHOD(put_HorizontalPercentage)(double dHorizPercentage);
	STDMETHOD(get_HorizontalPercentage)(double* pdHorizPercentage);
	STDMETHOD(put_VerticalPercentage)(double dVertPercentage);
	STDMETHOD(get_VerticalPercentage)(double* pdVertPercentage);
	STDMETHOD(put_PagesToStamp)(BSTR bstrPagesToStamp);
	STDMETHOD(get_PagesToStamp)(BSTR* pbstrPagesToStamp);

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

	// filename of input image file
	string m_strInputImage;

	// filename of the watermark image
	string m_strStampImage;

	// horizontal percentage offset for watermark
	double m_dHorizontalPercentage;

	// vertical percentage offset for watermark
	double m_dVerticalPercentage;

	// pages to place stamp on
	string m_strPagesToStamp;

	// dirty flag
	bool m_bDirty;

//--------------------------------------------------------------------------------------------------
// Private methods
//--------------------------------------------------------------------------------------------------

	//----------------------------------------------------------------------------------------------
	// PROMISE: Throws an exception if this component is not licensed. Runs successfully otherwise.
	void validateLicense();
};

OBJECT_ENTRY_AUTO(CLSID_AddWatermarkTask, CAddWatermarkTask)
