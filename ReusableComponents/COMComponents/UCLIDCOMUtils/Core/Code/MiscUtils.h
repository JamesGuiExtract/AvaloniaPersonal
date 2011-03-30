// MiscUtils.h : Declaration of the CMiscUtils

#pragma once

#include "resource.h"       // main symbols
#include <memory>
#include <IConfigurationSettingsPersistenceMgr.h>
#include <afxmt.h>

/////////////////////////////////////////////////////////////////////////////
// CMiscUtils
class ATL_NO_VTABLE CMiscUtils : 
	public CComObjectRootEx<CComMultiThreadModel>,
	public CComCoClass<CMiscUtils, &CLSID_MiscUtils>,
	public ISupportErrorInfo,
	public IDispatchImpl<ILicensedComponent, &IID_ILicensedComponent, &LIBID_UCLID_COMLMLib>,
	public IDispatchImpl<IMiscUtils, &IID_IMiscUtils, &LIBID_UCLID_COMUTILSLib>
{
public:
	CMiscUtils();

DECLARE_REGISTRY_RESOURCEID(IDR_MISCUTILS)

DECLARE_PROTECT_FINAL_CONSTRUCT()

BEGIN_COM_MAP(CMiscUtils)
	COM_INTERFACE_ENTRY(IMiscUtils)
	COM_INTERFACE_ENTRY2(IDispatch, IMiscUtils)
	COM_INTERFACE_ENTRY(ISupportErrorInfo)
END_COM_MAP()

public:
// ISupportsErrorInfo
	STDMETHOD(InterfaceSupportsErrorInfo)(REFIID riid);

// ILicensedComponent
	STDMETHOD(raw_IsLicensed)(VARIANT_BOOL * pbValue);

// IMiscUtils
	STDMETHOD(AutoEncryptFile)(/*[in]*/ BSTR strFile, /*[in]*/ BSTR strRegistryKey);

	STDMETHOD(GetRegExpData)(/*[in]*/ IIUnknownVector* pFoundData, 
		/*[in]*/ long nIndex, /*[in]*/ long nSubIndex, 
		/*[out]*/ long* pnStartPos, /*[out]*/ long* pnEndPos);

	STDMETHOD(GetNewRegExpParserInstance)(/*[in]*/BSTR strComponentName, 
		/*[out, retval]*/ IRegularExprParser **pRegExpParser);

	STDMETHOD(GetStringOptionallyFromFile)(/*[in]*/ BSTR bstrFileName, /*[out, retval]*/ BSTR *pbstrFromFile);

	STDMETHOD(GetFileHeader)(/*[out, retval]*/ BSTR *pbstrFileHeader);

	STDMETHOD(GetColumnStringsOptionallyFromFile)(/*[in]*/ BSTR bstrFileName, /*[out, retval]*/ IVariantVector* *pVal);

	STDMETHOD(GetFileNameWithoutHeader)(/*[in]*/ BSTR bstrText, /*[out, retval]*/ BSTR *pbstrFileName);

	STDMETHOD(AllowUserToConfigureObjectProperties)(IObjectWithDescription* pObject, VARIANT_BOOL* pvbDirty);

	STDMETHOD(AllowUserToConfigureObjectDescription)(IObjectWithDescription* pObject, VARIANT_BOOL* pvbDirty);

	STDMETHOD(HandlePlugInObjectDoubleClick)(IObjectWithDescription* pObject, BSTR bstrCategory, 
		BSTR bstrAFAPICategory, VARIANT_BOOL bAllowNone, LONG lNumRequiredIIDs, GUID* pRequiredIIDs, 
		VARIANT_BOOL* pvbDirty);

	STDMETHOD(HandlePlugInObjectCommandButtonClick)(IObjectWithDescription* pObject, BSTR bstrCategory, 
		BSTR bstrAFAPICategory, VARIANT_BOOL bAllowNone, LONG lNumRequiredIIDs, GUID* pRequiredIIDs, 
		int iLeft, int iTop, VARIANT_BOOL* pvbDirty);

	STDMETHOD(AllowUserToSelectAndConfigureObject)(IObjectWithDescription* pObject, BSTR bstrCategory, 
		BSTR bstrAFAPICategory, VARIANT_BOOL bAllowNone, LONG lNumRequiredIIDs, GUID* pRequiredIIDs, 
		VARIANT_BOOL* pvbDirty);

	STDMETHOD(AllowUserToSelectAndConfigureObject2)(BSTR bstrTitleAfterSelect, BSTR bstrCategory, 
		BSTR bstrAFAPICategory, LONG lNumRequiredIIDs, GUID* pRequiredIIDs, IUnknown **ppObject);

	STDMETHOD(CountEnabledObjectsIn)(IIUnknownVector* pVector, long* lNumEnabledObjects);

	STDMETHOD(GetEnabledState)(IIUnknownVector* pVector, LONG nItemIndex, VARIANT_BOOL* pvbEnabled);

	STDMETHOD(SetEnabledState)(IIUnknownVector* pVector, LONG nItemIndex, VARIANT_BOOL bEnabled);

	STDMETHOD(IsAnyObjectDirty1)(IUnknown* pObject, VARIANT_BOOL* pvbDirty);
	STDMETHOD(IsAnyObjectDirty2)(IUnknown* pObject1, IUnknown* pObject2, VARIANT_BOOL* pvbDirty);
	STDMETHOD(IsAnyObjectDirty3)(IUnknown* pObject1, IUnknown* pObject2, IUnknown* pObject3, 
		VARIANT_BOOL* pvbDirty);

	STDMETHOD(ShellOpenDocument)(BSTR bstrFilename);
	STDMETHOD(GetObjectAsStringizedByteStream)(IUnknown* pObject, BSTR* pbstrByteStream);
	STDMETHOD(GetObjectFromStringizedByteStream)(BSTR bstrByteStream, IUnknown** ppObject);

	STDMETHOD(GetExpandedTags)(BSTR bstrString, BSTR bstrSourceDocName, BSTR* pbstrExpanded);
	STDMETHOD(GetExpansionFunctionNames)(IVariantVector** ppFunctionNames);

	// PROMISE:	Determines whether the specified object supports configuration either via
	//			ISpecifyPropertyPages or IConfigurableObject
	// RETURNS: true if configuration is supported; false if it is not.
	static bool SupportsConfiguration(IUnknown *pObject);

private:
	//----------------------------------------------------------------------------------------------
	UCLID_COMUTILSLib::IMiscUtilsPtr getThisAsCOMPtr();
	//----------------------------------------------------------------------------------------------
	void validateLicense(); // validate license of this component 
	void validateETFEngineLicense(); // validate license for encryption component

	// persistence mgr
	std::unique_ptr<IConfigurationSettingsPersistenceMgr> m_apSettings;
	CMutex m_mutex;
};
