// AttributeRule.h : Declaration of the CAttributeRule

#pragma once

#include "resource.h"       // main symbols

#include <string>

/////////////////////////////////////////////////////////////////////////////
// CAttributeRule
class ATL_NO_VTABLE CAttributeRule : 
	public CComObjectRootEx<CComMultiThreadModel>,
	public CComCoClass<CAttributeRule, &CLSID_AttributeRule>,
	public IPersistStream,
	public ISupportErrorInfo,
	public IDispatchImpl<IAttributeRule, &IID_IAttributeRule, &LIBID_UCLID_AFCORELib>,
	public IDispatchImpl<ILicensedComponent, &IID_ILicensedComponent, &LIBID_UCLID_COMLMLib>,
	public IDispatchImpl<ICopyableObject, &IID_ICopyableObject, &LIBID_UCLID_COMUTILSLib>
{
public:
	CAttributeRule();
	~CAttributeRule();

DECLARE_REGISTRY_RESOURCEID(IDR_ATTRIBUTERULE)

DECLARE_PROTECT_FINAL_CONSTRUCT()

BEGIN_COM_MAP(CAttributeRule)
	COM_INTERFACE_ENTRY(IAttributeRule)
	COM_INTERFACE_ENTRY(IPersistStream)
	COM_INTERFACE_ENTRY(ISupportErrorInfo)
	COM_INTERFACE_ENTRY2(IDispatch, IAttributeRule)
	COM_INTERFACE_ENTRY(ICopyableObject)
	COM_INTERFACE_ENTRY(ILicensedComponent)
END_COM_MAP()

public:
// ISupportsErrorInfo
	STDMETHOD(InterfaceSupportsErrorInfo)(REFIID riid);

// ICopyableObject
	STDMETHOD(raw_Clone)(IUnknown * * pObject);
	STDMETHOD(raw_CopyFrom)(IUnknown * pObject);

// IPersistStream
	STDMETHOD(GetClassID)(CLSID *pClassID);
	STDMETHOD(IsDirty)(void);
	STDMETHOD(Load)(IStream *pStm);
	STDMETHOD(Save)(IStream *pStm, BOOL fClearDirty);
	STDMETHOD(GetSizeMax)(ULARGE_INTEGER *pcbSize);

// IAttributeRule
	STDMETHOD(get_Description)(/*[out, retval]*/ BSTR *pVal);
	STDMETHOD(put_Description)(/*[in]*/ BSTR newVal);
	STDMETHOD(get_IsEnabled)(/*[out, retval]*/ VARIANT_BOOL *pVal);
	STDMETHOD(put_IsEnabled)(/*[in]*/ VARIANT_BOOL newVal);
	STDMETHOD(get_AttributeFindingRule)(/*[out, retval]*/ IAttributeFindingRule* *pVal);
	STDMETHOD(put_AttributeFindingRule)(/*[in]*/ IAttributeFindingRule* newVal);
	STDMETHOD(get_ApplyModifyingRules)(/*[out, retval]*/ VARIANT_BOOL *pVal);
	STDMETHOD(put_ApplyModifyingRules)(/*[in]*/ VARIANT_BOOL newVal);
	STDMETHOD(get_AttributeModifyingRuleInfos)(/*[out, retval]*/ IIUnknownVector* *pVal);
	STDMETHOD(put_AttributeModifyingRuleInfos)(/*[in]*/ IIUnknownVector* newVal);
	STDMETHOD(ExecuteRuleOnText)(/*[in]*/ IAFDocument* pAFDoc, /*[in]*/ IProgressStatus *pProgressStatus,
		/*[out, retval]*/ IIUnknownVector** pAttributes);
	STDMETHOD(get_RuleSpecificDocPreprocessor)(/*[out, retval]*/ IObjectWithDescription* *pVal);
	STDMETHOD(put_RuleSpecificDocPreprocessor)(/*[in]*/ IObjectWithDescription *newVal);
	STDMETHOD(get_IgnoreErrors)(/*[out, retval]*/ VARIANT_BOOL *pVal);
	STDMETHOD(put_IgnoreErrors)(/*[in]*/ VARIANT_BOOL newVal);
	STDMETHOD(get_IgnorePreprocessorErrors)(/*[out, retval]*/ VARIANT_BOOL *pVal);
	STDMETHOD(put_IgnorePreprocessorErrors)(/*[in]*/ VARIANT_BOOL newVal);
	STDMETHOD(get_IgnoreModifierErrors)(/*[out, retval]*/ VARIANT_BOOL *pVal);
	STDMETHOD(put_IgnoreModifierErrors)(/*[in]*/ VARIANT_BOOL newVal);

// ILicensedComponent
	STDMETHOD(raw_IsLicensed)(VARIANT_BOOL * pbValue);

private:
	////////////////////
	// Member variables
	////////////////////
	// current value finding rule
	UCLID_AFCORELib::IAttributeFindingRulePtr m_ipAttributeFindingRule;
	
	// vector for modifying rules
	IIUnknownVectorPtr m_ipAttributeModifyingRuleInfos;

	// Document Preprocessor
	IObjectWithDescriptionPtr m_ipDocPreprocessor;

	// description for current attribute rule
	std::string m_strAttributeRuleDescription;
	
	// whether or not the current attribute rule is enabled
	bool m_bIsEnabled;

	// whether or not all modifying rules defined shall be applied
	bool m_bApplyModifyingRules;

	bool m_bIgnoreErrors;
	bool m_bIgnorePreprocessorErrors;
	bool m_bIgnoreModifierErrors;

	// flag to keep track of whether this object has changed
	// since the last save-to-stream operation
	bool m_bDirty;

	/////////////////
	// Helper functions
	/////////////////
	//----------------------------------------------------------------------------------------------
	//	PURPOSE: to return m_ipAttributeModifyingRuleInfos
	//
	//	PROMISE: to create an instance of AttributeModifyingRuleInfos and assign it to 
	//			 m_ipAttributeModifyingRuleInfos if m_ipAttributeModifyingRuleInfos is __nullptr
	IIUnknownVectorPtr getAttribModifyRuleInfos();
	//----------------------------------------------------------------------------------------------
	// PURPOSE: to return m_ipDocPreprocessor
	//
	// PROMISE: to create an instance of DocumentPreprocessor and assign it to m_ipDocPreprocessor 
	//			if m_ipDocPreprocessor is __nullptr
	IObjectWithDescriptionPtr getDocPreprocessor();
	//----------------------------------------------------------------------------------------------
	// apply modifying rules on the attribute
	// bRecursive - whether or not to apply rules on all its sub attributes recursively
	void applyModifyingRulesOnAttribute(UCLID_AFCORELib::IAttributePtr& ripAttribute, 
										UCLID_AFCORELib::IAFDocument* pOriginInput,
										IProgressStatusPtr ipProgressStatus,
										bool bRecursive = false);
	//----------------------------------------------------------------------------------------------
	// execute the set of modifying rules on the input Attribute and
	// get the resulting text
	void executeModifyingRulesOnAttribute(UCLID_AFCORELib::IAttributePtr& ripAttribute, 
		UCLID_AFCORELib::IAFDocument* pOriginInput, IProgressStatusPtr ipProgressStatus);
	//----------------------------------------------------------------------------------------------
	void validateLicense();
	//----------------------------------------------------------------------------------------------
	// A method to determine the count of enabled value modifying rules associated
	// with this object
	long getEnabledValueModifyingRulesCount();
	//----------------------------------------------------------------------------------------------
	// A method to determine if a preprocessor object is associated with this attribute
	// rule, and also whether it is enabled.
	bool enabledAttributePreProcessorExists();
};
