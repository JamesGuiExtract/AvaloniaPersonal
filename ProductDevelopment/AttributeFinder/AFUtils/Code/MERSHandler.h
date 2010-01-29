// MERSHandler.h : Declaration of the CMERSHandler

#pragma once

#include "resource.h"       // main symbols

#include "..\..\AFCore\Code\AFCategories.h"

#include <string>

/////////////////////////////////////////////////////////////////////////////
// CMERSHandler
class ATL_NO_VTABLE CMERSHandler : 
	public CComObjectRootEx<CComMultiThreadModel>,
	public CComCoClass<CMERSHandler, &CLSID_MERSHandler>,
	public ISupportErrorInfo,
	public IDispatchImpl<IAttributeModifyingRule, &IID_IAttributeModifyingRule, &LIBID_UCLID_AFCORELib>,
	public IDispatchImpl<ILicensedComponent, &IID_ILicensedComponent, &LIBID_UCLID_COMLMLib>,
	public IDispatchImpl<IMERSHandler, &IID_IMERSHandler, &LIBID_UCLID_AFUTILSLib>,
	public IDispatchImpl<ICategorizedComponent, &IID_ICategorizedComponent, &LIBID_UCLID_COMUTILSLib>,
	public IDispatchImpl<ICopyableObject, &IID_ICopyableObject, &LIBID_UCLID_COMUTILSLib>,
	public IPersistStream
{
public:
	CMERSHandler();
	~CMERSHandler();

DECLARE_REGISTRY_RESOURCEID(IDR_MERSHANDLER)

DECLARE_PROTECT_FINAL_CONSTRUCT()

BEGIN_COM_MAP(CMERSHandler)
	COM_INTERFACE_ENTRY(IMERSHandler)
	COM_INTERFACE_ENTRY2(IDispatch,IMERSHandler)
	COM_INTERFACE_ENTRY(ILicensedComponent)
	COM_INTERFACE_ENTRY(ISupportErrorInfo)
	COM_INTERFACE_ENTRY(IAttributeModifyingRule)
	COM_INTERFACE_ENTRY(ICategorizedComponent)
	COM_INTERFACE_ENTRY(ICopyableObject)
	COM_INTERFACE_ENTRY(IPersistStream)
END_COM_MAP()

BEGIN_CATEGORY_MAP(CEntityFinder)
	IMPLEMENTED_CATEGORY(CATID_AFAPI_VALUE_MODIFIERS)
END_CATEGORY_MAP()

public:
// ISupportsErrorInfo
	STDMETHOD(InterfaceSupportsErrorInfo)(REFIID riid);

// ILicensedComponent
	STDMETHOD(raw_IsLicensed)(VARIANT_BOOL * pbValue);

// IMERSHandler
	STDMETHOD(ModifyEntities)(/*[in]*/ IIUnknownVector* pVecEntities, /*[in]*/ IAFDocument* pOriginalText, /*[out, retval]*/ VARIANT_BOOL *pbFound);

// IAttributeModifyingRule
	STDMETHOD(raw_ModifyValue)(IAttribute * pAttribute, IAFDocument* pOriginInput, IProgressStatus *pProgressStatus);

// ICategorizedComponent
	STDMETHOD(raw_GetComponentDescription)(BSTR * pstrComponentDescription);

// ICopyableObject
	STDMETHOD(raw_Clone)(IUnknown * * pObject);
	STDMETHOD(raw_CopyFrom)(IUnknown * pObject);

// IPersistStream
	STDMETHOD(GetClassID)(CLSID *pClassID);
	STDMETHOD(IsDirty)(void);
	STDMETHOD(Load)(IStream *pStm);
	STDMETHOD(Save)(IStream *pStm, BOOL fClearDirty);
	STDMETHOD(GetSizeMax)(ULARGE_INTEGER *pcbSize);

private:
	//////////
	// Methods
	//////////
	//----------------------------------------------------------------------------------------------
	UCLID_AFUTILSLib::IMERSHandlerPtr getThisAsCOMPtr();
	//----------------------------------------------------------------------------------------------

	//=============================================================================
	// Extracts entity out from input text, return NULL if no entity is found
	ISpatialStringPtr extractEntity(ISpatialStringPtr ipInputText);
	
	//=============================================================================
	// Takes the input text and return the found match entity,
	// NULL if nothing is found
	ISpatialStringPtr findMatchEntity(ISpatialString* pInputText, 
		const std::string& strPattern,	// pattern for searching
		bool bCaseSensitive,			// case sensitivity
		bool bExtractEntity,			// whether or not to extract entity
		bool bGreedySearch);			// whether SPM search should be greedy

	//=============================================================================
	// If certain MERS keywords exist in the input
	// If found, return the start and end positions for the keyword
	bool findMERSKeyword(const std::string& strInput, int& nStartPos, int& nEndPos);

	//=============================================================================
	// Look for the first word (i.e. ignore any leading non-alpha chars if any). And
	// check to see if the first letter of that word is in upper case.
	bool isFirstWordStartsUpperCase(const std::string& strInput);

	//=============================================================================
	// Use StringPatternMatcher to find pattern in strInput and return 
	// found variable matches
	IStrToObjectMapPtr match(ISpatialStringPtr& ripInput, 
		const std::string& strPattern, bool bCaseSensitive, bool bGreedy);

	//=============================================================================
	// search based on pattern set 1
	// Return : found spatial string, NULL if nothing is found
	ISpatialStringPtr patternSearch1(ISpatialString* pOriginalText);

	//=============================================================================
	// search based on pattern set 2
	// Return : found spatial string, NULL if nothing is found
	ISpatialStringPtr patternSearch2(ISpatialString* pOriginalText);

	//=============================================================================
	// search based on pattern set 3
	// Return : found spatial string, NULL if nothing is found
	ISpatialStringPtr patternSearch3(ISpatialString* pOriginalText);

	//=============================================================================
	// Sets predefined expressions for String Pattern Matcher
	void setPredefinedExpressions();

	//=============================================================================
	IRegularExprParserPtr getParser();

	//=============================================================================
	void validateLicense();

	/////////////
	// Variables
	/////////////
	UCLID_AFUTILSLib::IEntityFinderPtr m_ipEntityFinder;
	IStringPatternMatcherPtr m_ipSPM;
	IStrToStrMapPtr m_ipExprDefined;

	IMiscUtilsPtr m_ipMiscUtils;

	bool m_bDirty;
};
