// DateTimeSplitter.h : Declaration of the CDateTimeSplitter

#pragma once

#include "resource.h"       // main symbols
#include "..\..\AFCore\Code\AFCategories.h"

#include <string>
using namespace std;

/////////////////////////////////////////////////////////////////////////////
// CDateTimeSplitter
class ATL_NO_VTABLE CDateTimeSplitter : 
	public CComObjectRootEx<CComMultiThreadModel>,
	public CComCoClass<CDateTimeSplitter, &CLSID_DateTimeSplitter>,
	public ISpecifyPropertyPagesImpl<CDateTimeSplitter>,
	public ISupportErrorInfo,
	public IDispatchImpl<IDateTimeSplitter, &IID_IDateTimeSplitter, &LIBID_UCLID_AFSPLITTERSLib>,
	public IPersistStream,
	public IDispatchImpl<IAttributeSplitter, &IID_IAttributeSplitter, &LIBID_UCLID_AFCORELib>,
	public IDispatchImpl<ICategorizedComponent, &IID_ICategorizedComponent, &LIBID_UCLID_COMUTILSLib>,
	public IDispatchImpl<ICopyableObject, &IID_ICopyableObject, &LIBID_UCLID_COMUTILSLib>,
	public IDispatchImpl<ILicensedComponent, &IID_ILicensedComponent, &LIBID_UCLID_COMLMLib>
{
public:
	CDateTimeSplitter();
	~CDateTimeSplitter();

DECLARE_REGISTRY_RESOURCEID(IDR_DATETIMESPLITTER)

DECLARE_PROTECT_FINAL_CONSTRUCT()

BEGIN_COM_MAP(CDateTimeSplitter)
	COM_INTERFACE_ENTRY(IDateTimeSplitter)
//DEL 	COM_INTERFACE_ENTRY(IDispatch)
	COM_INTERFACE_ENTRY(ISupportErrorInfo)
	COM_INTERFACE_ENTRY(IPersistStream)
	COM_INTERFACE_ENTRY2(IDispatch, IDateTimeSplitter)
	COM_INTERFACE_ENTRY(IAttributeSplitter)
	COM_INTERFACE_ENTRY(ICategorizedComponent)
	COM_INTERFACE_ENTRY_IMPL(ISpecifyPropertyPages)
	COM_INTERFACE_ENTRY(ICopyableObject)
	COM_INTERFACE_ENTRY(ILicensedComponent)
END_COM_MAP()

BEGIN_PROP_MAP(CDateTimeSplitter)
	PROP_PAGE(CLSID_DateTimeSplitterPP)
END_PROP_MAP()

BEGIN_CATEGORY_MAP(CDateTimeSplitter)
	IMPLEMENTED_CATEGORY(CATID_AFAPI_ATTRIBUTE_SPLITTERS)
END_CATEGORY_MAP()

// ISupportsErrorInfo
	STDMETHOD(InterfaceSupportsErrorInfo)(REFIID riid);

// IDateTimeSplitter
	STDMETHOD(get_SplitMonthAsName)(VARIANT_BOOL *pVal);
	STDMETHOD(put_SplitMonthAsName)(VARIANT_BOOL newVal);
	STDMETHOD(get_SplitFourDigitYear)(VARIANT_BOOL *pVal);
	STDMETHOD(put_SplitFourDigitYear)(VARIANT_BOOL newVal);
	STDMETHOD(get_SplitDayOfWeek)(VARIANT_BOOL *pVal);
	STDMETHOD(put_SplitDayOfWeek)(VARIANT_BOOL newVal);
	STDMETHOD(get_SplitMilitaryTime)(VARIANT_BOOL *pVal);
	STDMETHOD(put_SplitMilitaryTime)(VARIANT_BOOL newVal);
	STDMETHOD(get_ShowFormattedOutput)(VARIANT_BOOL *pVal);
	STDMETHOD(put_ShowFormattedOutput)(VARIANT_BOOL newVal);
	STDMETHOD(get_OutputFormat)(BSTR *pVal);
	STDMETHOD(put_OutputFormat)(BSTR newVal);
	STDMETHOD(get_SplitDefaults)(VARIANT_BOOL *pVal);
	STDMETHOD(put_SplitDefaults)(VARIANT_BOOL newVal);
	STDMETHOD(get_MinimumTwoDigitYear)(long *plVal);
	STDMETHOD(put_MinimumTwoDigitYear)(long lVal);
	STDMETHOD(get_TwoDigitYearBeforeCurrent)(VARIANT_BOOL *pvbVal);
	STDMETHOD(put_TwoDigitYearBeforeCurrent)(VARIANT_BOOL vbVal);

// IAttributeSplitter
	STDMETHOD(raw_SplitAttribute)(IAttribute *pAttribute, IAFDocument *pAFDoc, 
		IProgressStatus *pProgressStatus);

// ICategorizedComponent
	STDMETHOD(raw_GetComponentDescription)(BSTR * pstrComponentDescription);

// ICopyableObject
	STDMETHOD(raw_Clone)(IUnknown * * pObject);
	STDMETHOD(raw_CopyFrom)(IUnknown * pObject);

// ILicensedComponent
	STDMETHOD(raw_IsLicensed)(VARIANT_BOOL * pbValue);

// IPersistStream
	STDMETHOD(GetClassID)(CLSID *pClassID);
	STDMETHOD(IsDirty)(void);
	STDMETHOD(Load)(IStream *pStm);
	STDMETHOD(Save)(IStream *pStm, BOOL fClearDirty);
	STDMETHOD(GetSizeMax)(ULARGE_INTEGER *pcbSize);

private:

	// Flag to keep track of whether object is dirty
	bool	m_bDirty;

	// If true, January, else 1
	bool	m_bMonthAsName;

	// If true, 2004, else 04
	bool	m_bFourDigitYear;

	// If true, Monday, else this sub-attribute is not shown
	bool	m_bShowDayOfWeek;

	// If true, Hour = 20 AND AMPM sub-attribute is not shown, 
	// else Hour = 08 AND AMPM = PM
	bool	m_bMilitaryTime;

	// If true, "Formatted" sub-attribute is shown, else not shown
	bool	m_bShowFormattedOutput;

	// Format string suitable for strftime()
	string	m_strOutputFormat;

	// If true, create Month, Day, Year (and Time if available) sub-attributes, 
	// else not shown
	bool	m_bSplitDefaults;

	// The earliest year that a two digit year will be interpreted as. For example, if it is 1970
	// all two digit years range from 1970-2069.
	long m_lMinTwoDigitYear;

	// If true, interpret all two digit years as occurring on or before the current year. 
	// If false, use m_lMinTwoDigitYear as the threshold.
	bool m_bTwoDigitYearBeforeCurrentYear;

	// MFC Date-Time object to provide split results and "Formatted" text
	COleDateTime	m_dt;

	// Methods

	// Tests strFormat in strftime().  Returns true if valid format string
	// otherwise false.
	bool	isFormatValid(const string& strFormat);

	// Gets the earliest year that a two digit year will be interpreted as. For example, if 1970 is
	// returned all two digit years should be interpreted to range from 1970-2069.
	long getMinimumTwoDigitYear();

	// Ensure that this component is licensed
	void	validateLicense();

	//=======================================================================
	// PURPOSE: Removes spaces before and after colons in a time (if they exist)
	//				ie: 12: 25 => 12:25, or 5 :30 => 5:30. 6 : 17 => 6:17 
	// REQUIRE: Requires a string, suggested to use replaceMultipleCharsWithOne first. 
	// PROMISE: Returns nothing, string is passed by reference.
	// ARGS:	std::string to be trimmed.
	void CDateTimeSplitter::trimColonWS(std::string & strText);

	//=======================================================================
	// PURPOSE: The following Get helper functions set their respective item
	//			in the ipMainAttrSub object and most return (by ref) the value in
	//			the long parameter.
	// REQUIRE: Original Spatial String, the ipMainAttrSub object that has 
	//			been previously declared 
	// PROMISE: will return useful long and bool values
	// ARGS:	ipMainAttrSub passed by reference
	//			Miscellaneous bools and longs to be used to find or set
	//			appropriately.
	void CDateTimeSplitter::getDayOfWeek(ISpatialStringPtr& ipValue, 
		IIUnknownVectorPtr& ipMainAttrSub);
	void CDateTimeSplitter::getMonth(ISpatialStringPtr& ipValue, 
		IIUnknownVectorPtr& ipMainAttrSub, long &lMonth, bool &bStillValid);
	void CDateTimeSplitter::getDay(ISpatialStringPtr& ipValue, 
		IIUnknownVectorPtr& ipMainAttrSub, long &lDay, bool &bStillValid);
	void CDateTimeSplitter::getYear(ISpatialStringPtr& ipValue, 
		IIUnknownVectorPtr& ipMainAttrSub, long &lYear, bool &bStillValid);
	void CDateTimeSplitter::getHour(ISpatialStringPtr& ipValue, 
		IIUnknownVectorPtr &ipMainAttrSub, long& lYear, bool &bIsAM);
	void CDateTimeSplitter::getMinute(ISpatialStringPtr& ipValue, 
		IIUnknownVectorPtr &ipMainAttrSub);
	void CDateTimeSplitter::getSecond(ISpatialStringPtr& ipValue, 
		IIUnknownVectorPtr &ipMainAttrSub);

	//=======================================================================
	// PURPOSE: Formats the output of the date/time
	// REQUIRE: Check if the date/time is still valid
	// PROMISE: will set ipMainAttrSub with the formatting info
	// ARGS:	ipString - original Spatial String
	//			ipMainAttrSub passed by reference	
	//			bStillValid - verifies that the date/time is valid
	void CDateTimeSplitter::formatOutput(ISpatialStringPtr& ipString, 
		IIUnknownVectorPtr &ipMainAttrSub, const bool bStillValid);

	//=======================================================================
	// PURPOSE: parseDate and parseTime will parse their respective data from
	//			the string they are passed.
	// REQUIRE: string to be parsed and various bools and longs (by ref) to 
	//			reflect changes
	// PROMISE: Checks for valid input, and parses date and time which is sent
	//			back via the parameters
	// ARGS:	various bools and longs that will hold relevant values.
	bool CDateTimeSplitter::parseDate(std::string& strWord, bool& bFoundMonth, 
								bool& bFoundDay, bool& bFoundYear, bool& bFoundCentury,
								long& lMonth, long& lDay, long& lYear, long& lCentury);
	void CDateTimeSplitter::parseTime(std::string strWord, bool& bFoundTime, long& lHour,
								long& lMinute, long& lSecond, bool& bFoundAMPM,
								bool& bIsAM);

	//=======================================================================
	// PURPOSE: getDate and getTime will get their respective data from
	//			the string they are passed, and set 
	// REQUIRE: Original Spatial String, IIUnknownVector for sub-attributes, 
	//			and various bools and longs to control data flow. 
	// PROMISE: Checks for valid input, then gets/sets date and time which is sent
	//			back via the parameters
	// ARGS:	various bools and longs that will hold relevant values.
	void CDateTimeSplitter::getDate(ISpatialStringPtr& ipValue, 
								IIUnknownVectorPtr& ipMainAttrSub, bool& m_bShowDayOfWeek, 
								bool& bFoundDay, bool& m_bSplitDefaults, long& lMonth, 
								bool& bStillValid, long& lDay, long& lYear);
	void CDateTimeSplitter::getTime(ISpatialStringPtr& ipValue, 
								IIUnknownVectorPtr& ipMainAttrSub, bool& bFoundTime,
								bool& m_bSplitDefaults, long& lHour, bool& bIsAM);

	//=======================================================================
	// PURPOSE: Finds the appropriate substring from the original Spatial String 
	//			if present, otherwise returns a hybrid spatial string with the 
	//			desired text.  
	// REQUIRE: Original Spatial String, and text to be found as a substring. 
	// PROMISE: Returns substring of original if found or new hybrid spatial 
	//			string with strSearch
	// ARGS:	ipOriginal - original spatial text
	//			strSearch - character string to be found as a substring in ipOriginal
	//			bForceHybrid - directly create and return a hybrid string 
	//			bPossibleDuplicate - return a hybrid string if there is any 
	//				a duplicate of the search text is found in ipOriginal.
	ISpatialStringPtr findSubstring(ISpatialStringPtr& ipOriginal, string strSearch, 
		bool bForceHybrid, bool bPossibleDuplicate);
};
