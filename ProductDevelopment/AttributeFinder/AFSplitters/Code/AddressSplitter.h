// AddressSplitter.h : Declaration of the CAddressSplitter

#ifndef __ADDRESSSPLITTER_H_
#define __ADDRESSSPLITTER_H_

#include "resource.h"       // main symbols
#include "..\..\AFCore\Code\AFCategories.h"

#include <string>
using namespace std;

/////////////////////////////////////////////////////////////////////////////
// CAddressSplitter
class ATL_NO_VTABLE CAddressSplitter : 
	public CComObjectRootEx<CComMultiThreadModel>,
	public CComCoClass<CAddressSplitter, &CLSID_AddressSplitter>,
	public IPersistStream,
	public ISupportErrorInfo,
	public IDispatchImpl<IAddressSplitter, &IID_IAddressSplitter, &LIBID_UCLID_AFSPLITTERSLib>,
	public IDispatchImpl<IAttributeSplitter, &IID_IAttributeSplitter, &LIBID_UCLID_AFCORELib>,
	public IDispatchImpl<ILicensedComponent, &IID_ILicensedComponent, &LIBID_UCLID_COMLMLib>,
	public IDispatchImpl<ICategorizedComponent, &IID_ICategorizedComponent, &LIBID_UCLID_COMUTILSLib>,
	public IDispatchImpl<ICopyableObject, &IID_ICopyableObject, &LIBID_UCLID_COMUTILSLib>,
	public ISpecifyPropertyPagesImpl<CAddressSplitter>
{
public:
	CAddressSplitter();

DECLARE_REGISTRY_RESOURCEID(IDR_ADDRESSSPLITTER)

DECLARE_PROTECT_FINAL_CONSTRUCT()

BEGIN_COM_MAP(CAddressSplitter)
	COM_INTERFACE_ENTRY(IAttributeSplitter)
	COM_INTERFACE_ENTRY(IAddressSplitter)
	COM_INTERFACE_ENTRY2(IDispatch, IAttributeSplitter)
	COM_INTERFACE_ENTRY(ISupportErrorInfo)
	COM_INTERFACE_ENTRY(IPersistStream)
	COM_INTERFACE_ENTRY(ILicensedComponent)
	COM_INTERFACE_ENTRY(ICategorizedComponent)
	COM_INTERFACE_ENTRY(ICopyableObject)
	COM_INTERFACE_ENTRY_IMPL(ISpecifyPropertyPages)
END_COM_MAP()

BEGIN_PROP_MAP(CAddressSplitter)
	PROP_PAGE(CLSID_AddressSplitterPP)
END_PROP_MAP()

BEGIN_CATEGORY_MAP(CAddressSplitter)
	IMPLEMENTED_CATEGORY(CATID_AFAPI_ATTRIBUTE_SPLITTERS)
END_CATEGORY_MAP()

// ISupportsErrorInfo
	STDMETHOD(InterfaceSupportsErrorInfo)(REFIID riid);

// IAddressSplitter
	STDMETHOD(get_CombinedNameAddress)(/* [out, retval] */ VARIANT_BOOL *pVal);
	STDMETHOD(put_CombinedNameAddress)(/* [in] */ VARIANT_BOOL newVal);

// IAttributeSplitter
	STDMETHOD(raw_SplitAttribute)(IAttribute * pAttribute, IAFDocument *pAFDoc, 
		IProgressStatus *pProgressStatus);

// ILicensedComponent
	STDMETHOD(raw_IsLicensed)(VARIANT_BOOL * pbValue);

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
	///////
	// Data
	///////
	// Parses using regular expressions
	IRegularExprParserPtr	m_ipParser;

	// Provides collections of street and building keywords
	IEntityKeywordsPtr	m_ipKeys;

	// Flag to keep track of whether object is dirty
	bool m_bDirty;

	// Flags to track progress for single-element items
	bool m_bFoundCity;
	bool m_bFoundState;
	bool m_bFoundZip;

	// Counters for Recipient and Address lines
	long	m_lNumRecipientLines;
	long	m_lNumAddressLines;

	// Collection of trailing lines to be handled last
	IIUnknownVectorPtr	m_ipTrailingLines;

	// Both Recipient and Address lines will have AddressN Attribute names
	bool	m_bCombinedNameAddress;

	//////////
	// Methods
	//////////

	// Adds the specified text as a named subattribute in the 
	// specified position
	void	addSubAttribute(ISpatialStringPtr ipText, std::string strName, 
		long lPosition, IIUnknownVectorPtr ipSubAttr);

	// Divides the specified single-line text into one Recipient sub-attribute
	// and one Address sub-attribute.  If no division is found, the text will 
	// be considered an Address line.
	//   This method is called by doNameAddress().
	void	divideRecipientAddress(ISpatialStringPtr ipText, 
		IIUnknownVectorPtr ipSubAttr);

	// Finds city, state and zip code and adds them to collected subattributes.
	//   Call this method before calling doNameAddress().
	long	doCityStateZip(ISpatialStringPtr ipText, IIUnknownVectorPtr ipSubAttr);

	// Finds name and address lines and adds them to collected subattributes.
	//   Call this method after calling doCityStateZip().
	void	doNameAddress(ISpatialStringPtr ipText, IIUnknownVectorPtr ipSubAttr);

	// Evaluates a line of text for Address
	//   Used in combination with evaluateStringForRecipient() by doNameAddress() with 
	//   the highest score indicating line type.  A modification here probably also 
	//   requires a balancing modification to Recipient().
	long	evaluateStringForAddress(std::string strTest, long *plStartPos, long *plEndPos );

	// Evaluates a line of text for City
	//   Used in combination with evaluateStringForState() and evaluateStringForZipCode() 
	//     by doCityStateZip().  This method should be called after calling 
	//     evaluateStringForZipCode() and evaluateStringForState().
	//   Works backwards from the end of the unprocessed text.
	bool	evaluateStringForCity(std::string strText, long *plStartPos, long *plEndPos);

	// Evaluates a line of text for Recipient
	//   Used in combination with evaluateStringForAddress() by doNameAddress() with 
	//   the highest score indicating line type.  A modification here probably also 
	//   requires a balancing modification to Address().
	long	evaluateStringForRecipient(std::string strTest, long *plStartPos, long *plEndPos );

	// Evaluates a line of text for State
	//   Used in combination with evaluateStringForCity() and evaluateStringForZipCode() 
	//     by doCityStateZip().  This method should be called after calling 
	//     evaluateStringForZipCode() and before calling evaluateStringForCity().
	//   Works backwards from the end of the unprocessed text.
	bool	evaluateStringForState(std::string strText, long *plStartPos, long *plEndPos);

	// Evaluates a line of text for Zip Code
	//   Used in combination with evaluateStringForCity() and evaluateStringForState() 
	//     by doCityStateZip().  This method should be called before calling 
	//     evaluateStringForState() and evaluateStringForCity().
	//   Works backwards from the end of the unprocessed text.
	bool	evaluateStringForZipCode(std::string strText, long *plStartPos, long *plEndPos);

	// Processes the collection of trailing lines.  This method should be called after 
	// doCityStateZip() and doNameAddress().
	void	handleUnprocessedLines(IIUnknownVectorPtr ipSubAttr);

	// Returns true if the specified word is an ordinal number (e.g. 1st, 2nd, 3rd, etc.)
	static bool isOrdinalNumber(const string& strWord);

	// Checks if the specified word is an address indicator and thus most likely belongs
	// on an Address line
	//   Used by evaluateStringForCity() when no section delimiters are found and 
	//     individual words are checked to determine the break between an address line
	//     and the City string.  strPreviousWord can be "".
	bool	isWordAddressIndicator(std::string strWord, std::string strPreviousWord);

	// Adds the specified string to the collection of trailing lines.  The 
	// collection will be processed after doCityStateZip() and doNameAddress().
	void	storeUnprocessedLine(ISpatialStringPtr ipExtra);

	// Checks that this component is licensed
	void	validateLicense();
};

#endif //__ADDRESSSPLITTER_H_
