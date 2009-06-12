//============================================================================
//
// COPYRIGHT (c) 2003 UCLID SOFTWARE, LLC., IN PUBLISHED AND UNPUBLISHED WORKS
// ALL RIGHTS RESERVED.
//
// FILE:	ObjectWithDescription.h
//
// PURPOSE:	Declaration of CObjectWithDescription class
//
// NOTES:	
//
// AUTHORS:	Wayne Lenius
//
//============================================================================

#pragma once

#include "resource.h"       // main symbols

#include <string>

/////////////////////////////////////////////////////////////////////////////
// CObjectWithDescription
class ATL_NO_VTABLE CObjectWithDescription : 
	public CComObjectRootEx<CComMultiThreadModel>,
	public CComCoClass<CObjectWithDescription, &CLSID_ObjectWithDescription>,
	public IPersistStream,
	public ISupportErrorInfo,
	public IDispatchImpl<IObjectWithDescription, &IID_IObjectWithDescription, &LIBID_UCLID_COMUTILSLib>,
	public IDispatchImpl<ILicensedComponent, &IID_ILicensedComponent, &LIBID_UCLID_COMLMLib>
{
public:
	CObjectWithDescription();
	~CObjectWithDescription();

DECLARE_REGISTRY_RESOURCEID(IDR_OBJECTWITHDESCRIPTION)

DECLARE_PROTECT_FINAL_CONSTRUCT()

BEGIN_COM_MAP(CObjectWithDescription)
	COM_INTERFACE_ENTRY(IObjectWithDescription)
//DEL 	COM_INTERFACE_ENTRY(IDispatch)
	COM_INTERFACE_ENTRY(ISupportErrorInfo)
	COM_INTERFACE_ENTRY(IPersistStream)
	COM_INTERFACE_ENTRY(ICopyableObject)
	COM_INTERFACE_ENTRY2(IDispatch, IObjectWithDescription)
	COM_INTERFACE_ENTRY(ILicensedComponent)
END_COM_MAP()


public:
// ISupportsErrorInfo
	STDMETHOD(InterfaceSupportsErrorInfo)(REFIID riid);

// ICopyableObject
	STDMETHOD(Clone)(/*[out, retval]*/ IUnknown* *pObject);
	STDMETHOD(CopyFrom)(/*[in]*/ IUnknown *pObject);

// ILicensedComponent
	STDMETHOD(raw_IsLicensed)(VARIANT_BOOL * pbValue);

// IObjectWithDescription
	STDMETHOD(get_Description)(/*[out, retval]*/ BSTR *pVal);
	STDMETHOD(put_Description)(/*[in]*/ BSTR newVal);
	STDMETHOD(get_Object)(/*[out, retval]*/ IUnknown* *pVal);
	STDMETHOD(put_Object)(/*[in]*/ IUnknown* newVal);
	STDMETHOD(get_Enabled)(/*[out, retval]*/ VARIANT_BOOL *pVal);
	STDMETHOD(put_Enabled)(/*[in]*/ VARIANT_BOOL newVal);

//IPersistStream
	STDMETHOD(GetClassID)(CLSID *pClassID);
	STDMETHOD(IsDirty)(void);
	STDMETHOD(Load)(IStream *pStm);
	STDMETHOD(Save)(IStream *pStm, BOOL fClearDirty);
	STDMETHOD(GetSizeMax)(ULARGE_INTEGER *pcbSize);

private:
	// Pointer to the object needing a user-supplied description
	IUnknownPtr m_ipObj;

	// User-supplied description
	std::string	m_strDescription;

	// Enabled / disabled flag (default = true)
	bool m_bEnabled;

	// flag to keep track of whether this object has been modified since
	// the last save-to-stream operation
	bool m_bDirty;

	// check license
	void validateLicense();
};
