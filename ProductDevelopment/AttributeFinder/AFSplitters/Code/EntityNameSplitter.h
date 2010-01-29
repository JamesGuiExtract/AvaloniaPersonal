// EntityNameSplitter.h : Declaration of the CEntityNameSplitter

#pragma once

#include "resource.h"       // main symbols
#include "..\..\AFCore\Code\AFCategories.h"
#include "ENSConfigMgr.h"

#include <string>
#include <vector>
using namespace std;

/////////////////////////////////////////////////////////////////////////////
// CEntityNameSplitter
// This Splitter is used to determine if a name is that of a company or of a person,
// and if the name belongs to a person, to split it into Title, First, Middle, Last, and Suffix
class ATL_NO_VTABLE CEntityNameSplitter : 
	public CComObjectRootEx<CComMultiThreadModel>,
	public CComCoClass<CEntityNameSplitter, &CLSID_EntityNameSplitter>,
	public IPersistStream,
//	public ISpecifyPropertyPagesImpl<CEntityNameSplitter>,
	public ISupportErrorInfo,
	public IDispatchImpl<IEntityNameSplitter, &IID_IEntityNameSplitter, &LIBID_UCLID_AFSPLITTERSLib>,
	public IDispatchImpl<ICategorizedComponent, &IID_ICategorizedComponent, &LIBID_UCLID_COMUTILSLib>,
	public IDispatchImpl<ICopyableObject, &IID_ICopyableObject, &LIBID_UCLID_COMUTILSLib>,
	public IDispatchImpl<ILicensedComponent, &IID_ILicensedComponent, &LIBID_UCLID_COMLMLib>,
	public IDispatchImpl<IAttributeSplitter, &IID_IAttributeSplitter, &LIBID_UCLID_AFSPLITTERSLib>
{
public:
	CEntityNameSplitter();
	~CEntityNameSplitter();
	
DECLARE_REGISTRY_RESOURCEID(IDR_ENTITYNAMESPLITTER)

DECLARE_PROTECT_FINAL_CONSTRUCT()

BEGIN_COM_MAP(CEntityNameSplitter)
	COM_INTERFACE_ENTRY(IAttributeSplitter)
	COM_INTERFACE_ENTRY(IEntityNameSplitter)
	COM_INTERFACE_ENTRY2(IDispatch, IAttributeSplitter)
	COM_INTERFACE_ENTRY(ISupportErrorInfo)
	COM_INTERFACE_ENTRY(IPersistStream)
//	COM_INTERFACE_ENTRY_IMPL(ISpecifyPropertyPages)
	COM_INTERFACE_ENTRY(ICopyableObject)
	COM_INTERFACE_ENTRY(ICategorizedComponent)
	COM_INTERFACE_ENTRY(ILicensedComponent)
END_COM_MAP()

//BEGIN_PROP_MAP(CEntityNameSplitter)
//	PROP_PAGE(CLSID_EntityNameSplitterPP)
//END_PROP_MAP()

BEGIN_CATEGORY_MAP(CEntityNameSplitter)
	IMPLEMENTED_CATEGORY(CATID_AFAPI_ATTRIBUTE_SPLITTERS)
END_CATEGORY_MAP()

public:
// ISupportsErrorInfo
	STDMETHOD(InterfaceSupportsErrorInfo)(REFIID riid);

// IAttributeSplitter
	STDMETHOD(raw_SplitAttribute)(IAttribute *pAttribute, IAFDocument *pAFDoc, 
		IProgressStatus *pProgressStatus);

// ICategorizedComponent
	STDMETHOD(raw_GetComponentDescription)(BSTR * pstrComponentDescription);

// ILicensedComponent
	STDMETHOD(raw_IsLicensed)(VARIANT_BOOL * pbValue);

// ICopyableObject
	STDMETHOD(raw_Clone)(IUnknown * * pObject);
	STDMETHOD(raw_CopyFrom)(IUnknown * pObject);

// IPersistStream
	STDMETHOD(GetClassID)(CLSID *pClassID);
	STDMETHOD(IsDirty)(void);
	STDMETHOD(Load)(IStream *pStm);
	STDMETHOD(Save)(IStream *pStm, BOOL fClearDirty);
	STDMETHOD(GetSizeMax)(ULARGE_INTEGER *pcbSize);

// IEntityNameSplitter
	STDMETHOD(get_EntityAliasChoice)(/*[out, retval]*/ EEntityAliasChoice *pChoice);
	STDMETHOD(put_EntityAliasChoice)(/*[in]*/ EEntityAliasChoice newChoice);

private:
	///////
	// Data
	///////

	// Misc utils object used to get the parser
	IMiscUtilsPtr m_ipMiscUtils;

	// Regular Expression parser to be used internally
	IRegularExprParserPtr m_ipRegExprParser;

	// flag to keep track of whether object is dirty
	bool m_bDirty;

	// Finds Person names and/or Company names
	IEntityFinderPtr	m_ipFinder;

	// Provides collections of person and company keywords
	IEntityKeywordsPtr	m_ipKeys;

	// Determines what should be done with alias entities
	// NOTE: This setting is now being ignored but is retained for 
	//       compatibility with older versions.
	EEntityAliasChoice	m_eAliasChoice;

	// Handles configuration persistence
	std::auto_ptr<IConfigurationSettingsPersistenceMgr> ma_pUserCfgMgr;
	std::auto_ptr<ENSConfigMgr> ma_pENSConfigMgr;

	// Determines what should be done with Trust names
	bool	m_bMoveTrustName;

	//////////
	// Methods
	//////////

	// Creates Trust sub-attribute with appropriate Person and Company 
	// sub-sub-attributes.  Returns false if no valid Entities found.
	bool doTrustSplitting(ISpatialStringPtr ipGroup, IAFDocumentPtr ipAFDoc, 
		IIUnknownVectorPtr ipMainAttrSub);

	// Replaces forward slash with " AKA " unless previous and next non-space
	// characters are digits
	void handleCompanySlash(ISpatialStringPtr &ripCompany);

	// Moves last name of person or persons to beginning of Trust text.
	// Also moves a four-digit date to after the names.
	void moveTrustNames(ISpatialStringPtr &ripTrust);

	// Returns true if Company Suffix or Company Designator is found within 
	// entity text, otherwise false
	bool entityIsCompany(ISpatialStringPtr ipEntity);

	// Finds appropriate delimiters within ipEntity.  Later processing of 
	// ipEntity and ripMatches isolates one or more entities
	void findNameDelimiters(ISpatialStringPtr ipEntity, 
		bool bSuffixFound, IIUnknownVectorPtr &ripMatches);

	// Retrieves specified portion of whole SpatialString based on provided 
	// collection of delimiters.
	// PROMISE
	// Will return copy of ipGroup if ipDelimiters is empty.
	// For iEntity < Size, returns substring before delimiter
	// For iEntity = Size, returns substring after last delimiter
	// REQUIRES
	// iEntity >= 0 AND iEntity <= ipDelimiters->Size()
	ISpatialStringPtr getEntityFromDelimiters(int iEntity, ISpatialStringPtr ipGroup, 
		IIUnknownVectorPtr ipDelimiters);

	// Evaluates substrings of ipEntity based on previously found tokens inside 
	// ripMatches.  Any strDelimiter strings within a substring of also added to 
	// ripMatches.
	void processDelimiter(ISpatialStringPtr ipEntity,  
		string strDelimiter, IIUnknownVectorPtr &ripMatches);

	// Adds lOffset to start and end positions for each Token in ripMatches
	void updateTokenPositions(IIUnknownVectorPtr &ripMatches, long lOffset);

	// Checks each token.  If token is strDelimiter and associated string information 
	// indicates that this should not be a delimiter, the token is removed from 
	// ripMatches
	void validateAmpersandTokens(ISpatialStringPtr ipEntity, string strDelimiter, 
		IIUnknownVectorPtr &ripMatches);

	// Checks each token.  If token is a blank line and associated string information 
	// indicates that this should not be a delimiter, the token is removed from 
	// ripMatches
	void validateBlankLineTokens(ISpatialStringPtr ipEntity, IIUnknownVectorPtr &ripMatches);

	// Checks each token.  If token is comma and associated string information 
	// indicates that this should not be a delimiter, the token is removed from 
	// ripMatches
	void validateCommaTokens(ISpatialStringPtr ipEntity, IIUnknownVectorPtr &ripMatches);

	// Checks each token.  If token is a semicolon and associated string information 
	// indicates that this should not be a delimiter, the token is removed from 
	// ripMatches
	void validateSemicolonTokens(ISpatialStringPtr ipEntity, IIUnknownVectorPtr &ripMatches);

	// Checks each token.  If token is a slash and associated string information 
	// indicates that this should not be a delimiter, the token is removed from 
	// ripMatches
	void validateSlashTokens(ISpatialStringPtr ipEntity, IIUnknownVectorPtr &ripMatches);

	// Divides text into collection of individual words.
	// Returns count of words
	long getWordsFromString(string strText, IIUnknownVectorPtr &ripWords);

	// Checks each word looking for a duplicate that is assumed to be the last name
	// Returns index of the earliest copy of the duplicate word
	int getDuplicateWordFirstIndex(IIUnknownVectorPtr ipMatches, int iFirstIndex, int iLastIndex);

	// Checks each word looking for a break point between two names
	// Returns index of the last word of name #1
	int getWordBreakIndex(IIUnknownVectorPtr ipMatches);

	// Deals with Person or Company Alias information
	bool handleAlias(ISpatialStringPtr ipEntity, ISpatialStringPtr &ipExtra, 
		UCLID_AFUTILSLib::EAliasType& reType, long& rlAliasItem);

	// Returns false if Length <= 2.  Returns false if text contains no upper-case characters
	bool isValidEntity(ISpatialStringPtr& ripEntity, bool bIsPerson);

	// Divides text into collection of individual words and then creates 
	// and returns a collection of names
	IIUnknownVectorPtr	getNamesFromWords(ISpatialStringPtr ipText);

	// Trims string removing Person Trim Identifiers
	void removePersonTrimIdentifiers(ISpatialStringPtr& ripEntity, bool bIsPerson);

	// Locates Person Alias and/or Company Alias for later removal
	// or special handling
	void findAlias(ISpatialStringPtr ipEntity, long* plStartPos, long* plEndPos, 
		UCLID_AFUTILSLib::EAliasType& reType, long& rlAliasItem);

	// Sets the Type field of the specified IAttribute based on specified Alias type
	// and specified Alias item
	void setTypeFromAlias(IAttributePtr ipAttr, UCLID_AFUTILSLib::EAliasType eType, 
		long lAliasItem);

	// Trims string removing trailing lower case words
	void trimTrailingLowerCaseWords(ISpatialStringPtr& ripEntity);

	// Trims string removing trailing specified word
	void trimTrailingWord(ISpatialStringPtr& ripEntity, string strTrim);

	// Trims string removing leading specified word
	// Returns: false - strTrim not found
	//           true - found and trimmed strTrim from beginning of ripEntity
	bool	doLeadingWordTrim(ISpatialStringPtr& ripEntity, string strTrim);

	// Gets a new regular expression parser
	IRegularExprParserPtr getParser();

	// ensure that this component is licensed
	void validateLicense();
};
