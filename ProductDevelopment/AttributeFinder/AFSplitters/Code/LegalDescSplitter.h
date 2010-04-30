// LegalDescSplitter.h : Declaration of the CLegalDescSplitter

#pragma once

#include "resource.h"       // main symbols
#include "..\..\AFCore\Code\AFCategories.h"

#include <CachedObjectFromFile.h>
#include <RegExLoader.h>

#include <map>

/////////////////////////////////////////////////////////////////////////////
// CLegalDescSplitter
class ATL_NO_VTABLE CLegalDescSplitter : 
	public CComObjectRootEx<CComMultiThreadModel>,
	public CComCoClass<CLegalDescSplitter, &CLSID_LegalDescSplitter>,
	public IPersistStream,
	public ISupportErrorInfo,
	public IDispatchImpl<IAttributeSplitter, &IID_IAttributeSplitter, &LIBID_UCLID_AFCORELib>,
	public IDispatchImpl<ICategorizedComponent, &IID_ICategorizedComponent, &LIBID_UCLID_COMUTILSLib>,
	public IDispatchImpl<ICopyableObject, &IID_ICopyableObject, &LIBID_UCLID_COMUTILSLib>,
	public IDispatchImpl<ILicensedComponent, &IID_ILicensedComponent, &LIBID_UCLID_COMLMLib>
{
public:
	CLegalDescSplitter();
	~CLegalDescSplitter();

DECLARE_REGISTRY_RESOURCEID(IDR_LEGALDESCSPLITTER)

DECLARE_PROTECT_FINAL_CONSTRUCT()

BEGIN_COM_MAP(CLegalDescSplitter)
	COM_INTERFACE_ENTRY(ISupportErrorInfo)
	COM_INTERFACE_ENTRY(IPersistStream)
	COM_INTERFACE_ENTRY2(IDispatch, IAttributeSplitter)
	COM_INTERFACE_ENTRY(IAttributeSplitter)
	COM_INTERFACE_ENTRY(ICategorizedComponent)
	COM_INTERFACE_ENTRY(ICopyableObject)
	COM_INTERFACE_ENTRY(ILicensedComponent)
END_COM_MAP()

BEGIN_CATEGORY_MAP(CAddressSplitter)
	IMPLEMENTED_CATEGORY(CATID_AFAPI_ATTRIBUTE_SPLITTERS)
END_CATEGORY_MAP()


// ISupportsErrorInfo
	STDMETHOD(InterfaceSupportsErrorInfo)(REFIID riid);

// ILegalDescSplitter
public:
// IPersistStream
	STDMETHOD(GetClassID)(CLSID *pClassID);
	STDMETHOD(IsDirty)(void);
	STDMETHOD(Load)(IStream *pStream);
	STDMETHOD(Save)(IStream *pStream, BOOL fClearDirty);
	STDMETHOD(GetSizeMax)(ULARGE_INTEGER *pcbSize);

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

private:
	
	// Map containing the Regular Expression strings indexed on file name
	std::map<std::string, CachedObjectFromFile<string, RegExLoader> > m_mapFileNameToCachedRegExLoader;

	// Map containing rulesets for each Legal Description Type.
	// The Ruleset is used to apply modifiers to individual subattriubtes
	IStrToObjectMapPtr m_ipLegalTypeToRuleSetMap;
	
	IAFUtilityPtr	m_ipAFUtility;
	IMiscUtilsPtr	m_ipMiscUtils;

	// the regular expresion parser engine (must be reset with each call to SplitAttributes)
	IRegularExprParserPtr m_ipRegExParser;

	// Flag to keep track of whether object is dirty
	bool m_bDirty;

	IDocumentPreprocessorPtr m_ipDocPreprocessor;

	// ruleset to find and split the municipality part
	IRuleSetPtr m_ipMuniRuleSet;

	// Create tags for the AFDocument if necessary
	void processAFDcoument(IAFDocumentPtr& ipAFDoc);

	// return the regular expression to find the location portion of the 
	std::string getRegExpForType( std::string strDocType, std::string strFileNameWOPath );

	// Processes a legal description Municipality part
	//	Calls ExecuteRulesOnText for m_ipMuniRuleSet and checks for multiple Types
	//	if only one type sets the type ( Village, City, Town ) renames the found 
	//  attribute ( Village, City, Town ) to Name
	//  if more than one found creates a new municipality attribute with the same value for each 
	//	of the (Village, City, Town ) values and assigns the appropriate type
	void processMuni( ISpatialStringPtr ipInputText, IIUnknownVectorPtr ipSubAttributes);

	// Processes a legal description
	//		strLegalType is the Type from the classification
	//		ipInputText is the Legal description being split
	//		ipSubAttributes is the Vector to add the found subattributes to
	void processLegal( std::string strLegalType, ISpatialStringPtr ipInputText, IIUnknownVectorPtr ipSubAttributes);
	// Processes a legal description of type SUB-BLO
	//		ipInputText is the Location of type SUB-BLO to split
	//		ipSubAttributes is the vector to add the found subattributes to
	void processSubBLOLocation( ISpatialStringPtr ipInputText, IIUnknownVectorPtr ipSubAttributes);
	
	// Processes a legal description of type CONDO-U
	//		ipInputText is the Location of type CONDO-U to split
	//		ipSubAttributes is the vector to add the found subattributes to
	void processCondoULocation( ISpatialStringPtr ipInputText, IIUnknownVectorPtr ipSubAttributes);

	// Processes a legal description of type PLS-L
	//		ipInputText is the Location of type PLS-L to split
	//		ipSubAttributes is the vector to add the found subattributes to
	void processPLSLocation( ISpatialStringPtr ipInputText, IIUnknownVectorPtr ipSubAttributes);

	// Processes a legal description of type CSM
	//		ipInputText is the Location of type CSM to split
	//		ipSubAttributes is the vector to add the found subattributes to
	void processCSMLocation( ISpatialStringPtr ipInputText, IIUnknownVectorPtr ipSubAttributes);

	// Adds each spatial string in the ipAttributeStrings Vector to the ipAttributeList object \
	// using strAttributeName as the name
	// if bMultiple is true each value is another attribute with the same name
	// if bMultiple is false the ipAttributeStrings are appended to each other with a , separator
	void addAsAttributes( std::string strAttributeName, IIUnknownVectorPtr ipAttributeStrings, 
		IIUnknownVectorPtr ipAttributeList, bool bAsMultiple, std::string strLegalType = "",
		bool bApplyModifiers = false, std::string strAttrType = "");

	// Returns all spatial strings in the input that are found with strFindRegExp and do not have any 
	// strings found with the strExBetweenRegExp in the text between found strings
	// if only one spatial string is found the strExBetweenRegExp is ignored
	// after the strExBetweenRegExp has been found between strings the strIncBetweenRegExp is applied
	// if found then the strings after are included unless strExBetweenRegEx is again found and strIncBetweenRegExp
	// is not found
	// strIncludeRegExp is a regular expression that must be found withing each of the return values
	// if the value is an empty string all will be returned 
	// any found values that are between the start and ending positions of all items found
	// with strExcludeRegionExp will not be put in the results
	IIUnknownVectorPtr getFoundStrings( ISpatialStringPtr ipInputText, 
										std::string strFindRegExp, 
										std::string strExBetweenRegExp = "", 
										std::string strIncBetweenRegExp = "",
										std::string strIncludeRegExp = "",
										std::string strAlwaysKeepRegExp = "",
										std::string strExcludeRegionExp = "",
										bool bExcludeAfter1stInitially = false);

	// Searches the strSearchText for the string in strRegExp if found returns true\
	// if not found or strRegExp is empty returns false
	bool isRegExpInText ( std::string strSearchText, std::string strRegExp );

	// Allocates and returns and attribute withe the given values
	IAttributePtr createAttribute( std::string strAttrName, ISpatialStringPtr ipAttrValue );
	IAttributePtr createAttribute( std::string strAttrName, std::string strAttrValue );

	// Returns the values found in all ISpatialStrings in ipPartStrings with regular expression
	// strExtractRegExp if the values found represent a numeric range the range is expanded.
	// a numeric range is indicated if there are 2 numbers with thru, through, to, or - between them
	// The spatial string ipMainValue is modified by removing the values in ipPartStrings and
	// all charaters before the partString unless strBeforeRegExp != "" and is found in ipMainValue
	// before the part string being processed in which case the part string and all text after
	// it in ipMainValue will be removed
	IIUnknownVectorPtr processValueParts(	ISpatialStringPtr ipMainValue,
											IIUnknownVectorPtr ipPartStrings, 
											std::string strExtractRegExp,
											std::string strBeforeRegExp );


	// Returns true if the nStart and nEnd positions are within any match withing ipRegions
	bool valueWithinRegions(long nStart, long nEnd, IIUnknownVectorPtr &ipRegions);

	// Returns a vector of all the values in ipRangedValues with the ranges indicated by 2 numbers
	// separated by through, thru, to, or - 
	IIUnknownVectorPtr expandNumericRanges ( IIUnknownVectorPtr ipRangedValues );

	// Modifies the attribute values contained within the ipRangedValues vector to have
	//	range values separated with a dash ( replacing thru, through )
	void makeRangeSeparatorDash( IIUnknownVectorPtr ipRangedValues );

	// Returns m_ipAFUtility, after initializing it if necessary
	IAFUtilityPtr getAFUtility();
	
	// Returns m_ipA, after initializing it if necessary
	IMiscUtilsPtr getMiscUtils();

	// Sets up the Muni Ruleset object by loading the file
	//	<componentDataDir>\LegalDescSplitter\MuniFind.rsd
	//	The file contains one attribute named Municipality that uses a RSD file splitter
	//  <ComponentDataDir>\LegalDescSplitter\MuniSplit.rsd that contains the attributes
	//  Village, City, Town, County, and State
	void setupMuniRuleSet();

	// Returns a pointer to the modifier ruleset if AFUtility::GetLoadFilePerSession is true
	// returs value stored in m_ipLegalTypeToRuleSetMap if it has been previously loaded
	// if it has not been previously loaded or GetLoadFilePerSession is false the ruleset
	// is loaded with the appropriate file ( "ModifierRulesSet.rsd.etf" )
	IRuleSetPtr getModifierRuleSet( std::string strLegalType );

	// Returns a spatial string that is the result of applying the rules in the ModifierRuleSet
	// for the attribute strAttrName to ipValue 
	ISpatialStringPtr applyModifiers( std::string strLegalType, std::string strAttrName, ISpatialStringPtr ipValue );

	// Splits each of the Town # Dir values into TownNum and TownDir attributes and adds them to the 
	// given subAttribute Vector
	void splitAndAddTown( IIUnknownVectorPtr ipTownValues, IIUnknownVectorPtr ipSubAttrs );

	// Splits each of the Range # Dir values into RangeNum and RangeDir attributes and adds them to the 
	// given subAttribute Vector
	void splitAndAddRange( IIUnknownVectorPtr ipRangeValues, IIUnknownVectorPtr ipSubAttrs );

	// Modifies the type and adds attribute to ipParentSubAttrs
	//		if SUB-BLO and subattributes include Block but not Lot or Outlot - SUB-B
	//		if SUB-BLO and subattributes include Lot but not Outlot - SUB-L
	//		if SUB-BLO and subattributes include Outlot but not Lot - SUB-O
	//		if SUB-BLO and subattributes include Lot and Outlot 
	//				Creates another attribute with all sub attributes but Lot and types it SUB-O
	//				and removes the Outlot from original Attribute and types it SUB-L
	//		if CONDO-U and subattributes include Building but not Unit - CONDO-B
	//		other wise stays the same ( may need to do the car port and other cases )
	//
	void reTypeAttribute ( IAttributePtr ipLocationAttr, IIUnknownVectorPtr ipParentSubAttrs );


	// Returns an vector of the values in the ipValueStrings that are not duplicated and will put
	// the values in to ranges if they are consecutive if the expandRanges is false
	// if expandRanges is true any ranges found will be expanded
	IIUnknownVectorPtr consolidateValues ( IIUnknownVectorPtr ipValueStrings, bool expandRanges = false );
	
	// Looks for the keywords of Feet, part as values and all items after them are considered partials
	void separateFullAndPartial ( IIUnknownVectorPtr ipValueStrings, IIUnknownVectorPtr &ipFullValues, IIUnknownVectorPtr &ipPartValues );

	// Gets a new regular expression parser
	IRegularExprParserPtr getParser();

	// Checks that this component is licensed
	void	validateLicense();

};

