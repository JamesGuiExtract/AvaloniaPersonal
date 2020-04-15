//============================================================================
//
// COPYRIGHT (c) 2002 UCLID SOFTWARE, LLC., IN PUBLISHED AND UNPUBLISHED WORKS
// ALL RIGHTS RESERVED.
//
// FILE:	UCLIDComponentLM.h
//
// PURPOSE:	Declaration of CUCLIDComponentLM
//
// NOTES:	
//
// AUTHORS:	Wayne Lenius
//
//============================================================================

#pragma once

#include "resource.h"       // main symbols

/////////////////////////////////////////////////////////////////////////////
// CUCLIDComponentLM
class ATL_NO_VTABLE CUCLIDComponentLM : 
	public CComObjectRootEx<CComMultiThreadModel>,
	public CComCoClass<CUCLIDComponentLM, &CLSID_UCLIDComponentLM>,
	public ISupportErrorInfo,
	public IDispatchImpl<ILicenseInfo, &__uuidof(ILicenseInfo), &LIBID_Extract_Interfaces>,
	public IDispatchImpl<IUCLIDComponentLM, &IID_IUCLIDComponentLM, &LIBID_UCLID_COMLMLib>
{
public:
	CUCLIDComponentLM()
	{
	}

DECLARE_REGISTRY_RESOURCEID(IDR_UCLIDCOMPONENTLM)

DECLARE_PROTECT_FINAL_CONSTRUCT()

BEGIN_COM_MAP(CUCLIDComponentLM)
	COM_INTERFACE_ENTRY(IUCLIDComponentLM)
	COM_INTERFACE_ENTRY2(IDispatch, IUCLIDComponentLM)
	COM_INTERFACE_ENTRY(ISupportErrorInfo)
	COM_INTERFACE_ENTRY(ILicenseInfo)
	
END_COM_MAP()

public:
// ISupportsErrorInfo
	STDMETHOD(InterfaceSupportsErrorInfo)(REFIID riid);

// IUCLIDComponentLM
	STDMETHOD(InitializeFromFile)(/*[in]*/BSTR bstrLicenseFile, /*[in]*/long ulKey1, /*[in]*/long ulKey2, /*[in]*/long ulKey3, /*[in]*/long ulKey4);
	STDMETHOD(IgnoreLockConstraints)(/*[in]*/long lKey);
	STDMETHOD(IsLicensed)(/*[in]*/ long ulComponentID, /*[out,retval]*/ VARIANT_BOOL *pbValue);

// ILicenseInfo
	STDMETHOD(raw_GetLicensedComponents)(SAFEARRAY** psaExpirationDates, SAFEARRAY** psaComponents);	
	STDMETHOD(raw_GetLicensedPackageNames)(SAFEARRAY** psaExpirationDates, SAFEARRAY** psaPackageNames);
	STDMETHOD(raw_GetLicensedPackageNamesWithExpiration)(SAFEARRAY** psaPackageNames);
};
