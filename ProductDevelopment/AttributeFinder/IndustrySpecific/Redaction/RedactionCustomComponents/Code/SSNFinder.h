// SSNFinder.h : Declaration of the CSSNFinder

#pragma once

#include "resource.h"       // main symbols

#include <AFCategories.h>

#include <string>
#include <list>

using namespace std;

struct StringSegmentType
{
	int iStartIndex;
	int iEndIndex;
};

/////////////////////////////////////////////////////////////////////////////
// CSSNFinder
class ATL_NO_VTABLE CSSNFinder : 
	public CComObjectRootEx<CComMultiThreadModel>,
	public CComCoClass<CSSNFinder, &CLSID_SSNFinder>,
	public IDispatchImpl<ISSNFinder, &IID_ISSNFinder, &LIBID_UCLID_REDACTIONCUSTOMCOMPONENTSLib>,
	public IDispatchImpl<IAttributeModifyingRule, &IID_IAttributeModifyingRule, &LIBID_UCLID_AFCORELib>,
	public IDispatchImpl<ICategorizedComponent, &IID_ICategorizedComponent, &LIBID_UCLID_COMUTILSLib>,
	public IDispatchImpl<ICopyableObject, &IID_ICopyableObject, &LIBID_UCLID_COMUTILSLib>,
	public IDispatchImpl<ILicensedComponent, &IID_ILicensedComponent, &LIBID_UCLID_COMLMLib>,
	public IDispatchImpl<IMustBeConfiguredObject, &IID_IMustBeConfiguredObject, &LIBID_UCLID_COMUTILSLib>,
	public IPersistStream,
	public ISupportErrorInfo,
	public ISpecifyPropertyPagesImpl<CSSNFinder>
{
public:
	CSSNFinder();
	~CSSNFinder();

DECLARE_REGISTRY_RESOURCEID(IDR_SSNFINDER)

DECLARE_PROTECT_FINAL_CONSTRUCT()

BEGIN_COM_MAP(CSSNFinder)
	COM_INTERFACE_ENTRY(ISSNFinder)
	COM_INTERFACE_ENTRY2(IDispatch, ISSNFinder)
	COM_INTERFACE_ENTRY(IAttributeModifyingRule)
	COM_INTERFACE_ENTRY(ICategorizedComponent)
	COM_INTERFACE_ENTRY(ICopyableObject)
	COM_INTERFACE_ENTRY(ILicensedComponent)
	COM_INTERFACE_ENTRY(IMustBeConfiguredObject)
	COM_INTERFACE_ENTRY(IPersistStream)
	COM_INTERFACE_ENTRY(ISupportErrorInfo)
	COM_INTERFACE_ENTRY_IMPL(ISpecifyPropertyPages)
END_COM_MAP()

BEGIN_PROP_MAP(CSSNFinder)
	PROP_PAGE(CLSID_SSNFinderPP)
END_PROP_MAP()

BEGIN_CATEGORY_MAP(CSSNFinder)
	IMPLEMENTED_CATEGORY(CATID_AFAPI_VALUE_MODIFIERS)
END_CATEGORY_MAP()

public:
// ISSNFinder
	STDMETHOD(SetOptions)(/*[in]*/ BSTR bstrSubattributeName, /*[in]*/ VARIANT_BOOL vbSpatialSubattribute, 
		/*[in]*/ VARIANT_BOOL vbClearIfNoneFound);
	STDMETHOD(GetOptions)(/*[out]*/ BSTR* pbstrSubattributeName, /*[out]*/ VARIANT_BOOL* pvbSpatialSubattribute, 
		/*[out]*/ VARIANT_BOOL* pvbClearIfNoneFound);

// IAttributeModifyingRule
	STDMETHOD(raw_ModifyValue)(/*[in]*/ IAttribute* pAttribute, /*[in]*/ IAFDocument* pOriginInput,
		/*[in]*/ IProgressStatus* pProgressStatus);

// ICategorizedComponent
	STDMETHOD(raw_GetComponentDescription)(/*[out, retval]*/ BSTR* pstrComponentDescription);

// ICopyableObject
	STDMETHOD(raw_Clone)(/*[out, retval]*/ IUnknown** pObject);
	STDMETHOD(raw_CopyFrom)(/*[in]*/ IUnknown* pObject);

// ILicensedComponent
	STDMETHOD(raw_IsLicensed)(/*[out, retval]*/ VARIANT_BOOL* pbValue);

// IMustBeConfiguredObject
	STDMETHOD(raw_IsConfigured)(/*[out]*/ VARIANT_BOOL* pbValue);

// IPersistStream
	STDMETHOD(GetClassID)(/*[out]*/ CLSID *pClassID);
	STDMETHOD(IsDirty)();
	STDMETHOD(Load)(/*[unique][in]*/ IStream *pStm);
	STDMETHOD(Save)(/*[unique][in]*/ IStream *pStm, /*[in]*/ BOOL fClearDirty);
	STDMETHOD(GetSizeMax)(/*[out]*/ ULARGE_INTEGER *pcbSize);

// ISupportsErrorInfo
	STDMETHOD(InterfaceSupportsErrorInfo)(/*[in]*/ REFIID riid);

private:

// Private variables

	// dirty flag
	bool m_bDirty;

	// name of subattributes
	string m_strSubattributeName;

	// whether the attribute and subattributes should be stored as spatial (true) or hybrid (false)
	bool m_bSpatialSubattribute;

	// if no matches are found, whether the attribute should be cleared (true) or unmodified (false)
	bool m_bClearIfNoneFound;

// Private methods

	//---------------------------------------------------------------------------------------------
	// PURPOSE: To return a valid SSOCR engine.
	// PROMISE: Instantiates and licenses m_ipOCREngine if it was NULL. Returns m_ipOCREngine.
	IOCREnginePtr getOCREngine();
	//---------------------------------------------------------------------------------------------
	// PROMISE: Throws an exception if this component is not licensed. Runs successfully otherwise.
	void validateLicense();
	//-------------------------------------------------------------------------------------------------
	// PURPOSE: Searches strText in the range of [iStartIndex, iEndIndex) for likely social security
	//          numbers. For each SSN found, a string segment, containing the index of the first and 
	//          last digit of the social security number, is added to listSegments.
	// REQUIRE: 0 <= iStartIndex < iEndIndex <= strText.length()
	static void findSSNs(const string& strText, int iStartIndex, int iEndIndex, 
		list<StringSegmentType>& listSegments);
	//--------------------------------------------------------------------------------------------------
	// PURPOSE: Increments riNumDigits, riNumUnrec, riNumSpaces counters based on the value of cChar.
	//          Returns false if cChar is a disqualifying character, true otherwise. If 
	//          bSpaceDisqualifies is true, spaces will be considered disqualifying characters.
	// PROMISE: (1) If cChar is a digit, riNumDigits will be incremented.
	//          (2) If cChar is the unrecognized symbol, riNumUnrec will be incremented.
	//          (3) If cChar is a space, riNumSpaces will be incremented. Underscores are treated like 
	//          spaces. 
	//          (4) If cChar is not one of the above characters, it is a disqualifying characer.
	//
	//          Returns false if cChar is a disqualifying character, true otherwise. If 
	//          bSpaceDisqualifies is true, the space is a disqualifying character.
	static bool incrementCounters(char cChar, int& riNumDigits, int& riNumUnrec, int& riNumSpaces, 
						   bool bSpaceDisqualifies=false);
	//-------------------------------------------------------------------------------------------------
	// PROMISE: If the sum of the digit and unrecognized character counters are within the minimum
	//          (iMinDigits) and maximum (iMaxDigits), sets riMatchingIndex to iIndex. Returns true if 
	//          a terminal matching index has been found (ie. riMatchingIndex != -1 and the sum of 
	//          digits and unrecognized characters equals iMaxDigits).
	static bool findMatchingIndex(int iMinDigits, int iMaxDigits, int iIndex, int& riNumDigits, 
		int& riNumUnrec, int& riMatchingIndex);
};
