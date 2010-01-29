// ValueAfterClue.h : Declaration of the CValueAfterClue

#pragma once

#include "resource.h"       // main symbols
#include "..\..\AFCore\Code\AFCategories.h"

#include <string>
/////////////////////////////////////////////////////////////////////////////
// CValueAfterClue
class ATL_NO_VTABLE CValueAfterClue : 
	public CComObjectRootEx<CComMultiThreadModel>,
	public CComCoClass<CValueAfterClue, &CLSID_ValueAfterClue>,
	public ISupportErrorInfo,
	public IDispatchImpl<IAttributeFindingRule, &IID_IAttributeFindingRule, &LIBID_UCLID_AFCORELib>,
	public IDispatchImpl<IValueAfterClue, &IID_IValueAfterClue, &LIBID_UCLID_AFVALUEFINDERSLib>,
	public IDispatchImpl<ICategorizedComponent, &IID_ICategorizedComponent, &LIBID_UCLID_COMUTILSLib>,
	public IDispatchImpl<ILicensedComponent, &IID_ILicensedComponent, &LIBID_UCLID_COMLMLib>,
	public IDispatchImpl<ICopyableObject, &IID_ICopyableObject, &LIBID_UCLID_COMUTILSLib>,
	public IDispatchImpl<IMustBeConfiguredObject, &IID_IMustBeConfiguredObject, &LIBID_UCLID_COMUTILSLib>,
	public IPersistStream,
	public ISpecifyPropertyPagesImpl<CValueAfterClue>
{
public:
	CValueAfterClue();
	~CValueAfterClue();

DECLARE_REGISTRY_RESOURCEID(IDR_VALUEAFTERCLUE)

DECLARE_PROTECT_FINAL_CONSTRUCT()

BEGIN_COM_MAP(CValueAfterClue)
	COM_INTERFACE_ENTRY(ISupportErrorInfo)
	COM_INTERFACE_ENTRY(IAttributeFindingRule)
	COM_INTERFACE_ENTRY2(IDispatch, IAttributeFindingRule)
	COM_INTERFACE_ENTRY(ICategorizedComponent)
	COM_INTERFACE_ENTRY(IValueAfterClue)
	COM_INTERFACE_ENTRY(ILicensedComponent)
	COM_INTERFACE_ENTRY(ICopyableObject)
	COM_INTERFACE_ENTRY(IMustBeConfiguredObject)
	COM_INTERFACE_ENTRY(IPersistStream)
	COM_INTERFACE_ENTRY_IMPL(ISpecifyPropertyPages)
END_COM_MAP()

BEGIN_PROP_MAP(CValueAfterClue)
	PROP_PAGE(CLSID_ValueAfterCluePP)
END_PROP_MAP()

BEGIN_CATEGORY_MAP(CValueAfterClue)
	IMPLEMENTED_CATEGORY(CATID_AFAPI_VALUE_FINDERS)
END_CATEGORY_MAP()

public:
// ISupportsErrorInfo
	STDMETHOD(InterfaceSupportsErrorInfo)(REFIID riid);

// IAttributeFindingRule
	STDMETHOD(raw_ParseText)(IAFDocument* pAFDoc, IProgressStatus *pProgressStatus,
		IIUnknownVector **pAttributes);

// ICategorizedComponent
	STDMETHOD(raw_GetComponentDescription)(BSTR * pstrComponentDescription);

// IValueAfterClue
	STDMETHOD(get_ClueAsRegExpr)(/*[out, retval]*/ VARIANT_BOOL *pVal);
	STDMETHOD(put_ClueAsRegExpr)(/*[in]*/ VARIANT_BOOL newVal);
	STDMETHOD(get_Clues)(/*[out, retval]*/ IVariantVector* *pVal);
	STDMETHOD(put_Clues)(/*[in]*/ IVariantVector* newVal);
	STDMETHOD(get_IsCaseSensitive)(/*[out, retval]*/ VARIANT_BOOL *pVal);
	STDMETHOD(put_IsCaseSensitive)(/*[in]*/ VARIANT_BOOL newVal);
	STDMETHOD(get_RefiningType)(/*[out, retval]*/ ERuleRefiningType *pVal);
	STDMETHOD(SetClueToString)(/*[in]*/ BSTR strString);
	STDMETHOD(GetClueToString)(/*[out, retval]*/ BSTR* strString);
	STDMETHOD(SetUptoXLines)(/*[in]*/ long nNumOfLines, /*[in]*/ VARIANT_BOOL bIncludeClueLine);
	STDMETHOD(GetUptoXLines)(/*[in, out]*/ long* nNumOfLines, /*[in, out]*/ VARIANT_BOOL* bIncludeClueLine);
	STDMETHOD(SetUptoXWords)(/*[in]*/ long nNumOfWords, BSTR pstrPunctuations, /*[in]*/ VARIANT_BOOL bStopAtNewLine, BSTR strStopChars);
	STDMETHOD(GetUptoXWords)(/*[in, out]*/ long* nNumOfWords, BSTR *pstrPunctuations, /*[in, out]*/ VARIANT_BOOL* bStopAtNewLine, BSTR *pstrStopChars);
	STDMETHOD(SetClueLineType)();
	STDMETHOD(SetNoRefiningType)();
	STDMETHOD(get_ClueToStringAsRegExpr)(/*[out, retval]*/ VARIANT_BOOL *pVal);
	STDMETHOD(put_ClueToStringAsRegExpr)(/*[in]*/ VARIANT_BOOL newVal);

// ILicensedComponent
	STDMETHOD(raw_IsLicensed)(VARIANT_BOOL * pbValue);

// ICopyableObject
	STDMETHOD(raw_Clone)(/*[out, retval]*/ IUnknown* *pObject);
	STDMETHOD(raw_CopyFrom)(/*[in]*/ IUnknown *pObject);

// IMustBeConfiguredObject
	STDMETHOD(raw_IsConfigured)(VARIANT_BOOL * pbValue);

// IPersistStream
	STDMETHOD(GetClassID)(CLSID *pClassID);
	STDMETHOD(IsDirty)(void);
	STDMETHOD(Load)(IStream *pStm);
	STDMETHOD(Save)(IStream *pStm, BOOL fClearDirty);
	STDMETHOD(GetSizeMax)(ULARGE_INTEGER *pcbSize);

private:
	////////////
	// Variables
	////////////
	IVariantVectorPtr m_ipClues;
	
	// whether or not all provided clue texts are treated as regular expressions
	bool m_bClueAsRegExpr;

	UCLID_AFVALUEFINDERSLib::ERuleRefiningType m_eRefiningType;
	bool m_bCaseSensitive;
	// If m_eRefiningType is up to x words
	long m_nNumOfWords;
	// If m_eRefiningType is up to x words, what are the punctuations that separate
	// each word? By default, it is the space, tab and new line chars.
	std::string m_strPunctuations;
	// If m_eRefiningType is up to x words, if new line character is encountered,
	// should we stop any further search?
	bool m_bStopAtNewLine;
	// If m_eRefiningType is up to x words, what are the characters that stop further searching?
	// By default, none.
	bool m_bStopForOther;
	std::string m_strStops;

	// If m_eRefiningType is up to x lines
	long m_nNumOfLines;
	// If m_eRefiningType is up to x lines, shall we include the line that 
	// contains the clue text?
	bool m_bIncludeClueLine;

	// If m_eRefiningType is from clue text till specified string...
	// the string that will limit text from clue string to this string
	std::string m_strLimitingString;

	// If m_eRefiningType is from clue text till specified string...
	// whether or not specified string is treated as a regular expression
	bool m_bClueToStringAsRegExpr;

	// whether the current object is modified
	bool m_bDirty;

	// Misc utils used to get regex parser
	IMiscUtilsPtr m_ipMiscUtils;

	///////////
	// Methods
	///////////
	void validateLicense();

};
