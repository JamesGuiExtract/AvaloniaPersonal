// AFUtility.h : Declaration of the CAFUtility

#pragma once

#include "resource.h"       // main symbols

#include <IConfigurationSettingsPersistenceMgr.h>

#include <string>
#include <vector>
#include <map>
#include <memory>
#include <afxmt.h>

using namespace std;

/////////////////////////////////////////////////////////////////////////////
// CAFUtility
class ATL_NO_VTABLE CAFUtility : 
	public CComObjectRootEx<CComMultiThreadModel>,
	public CComCoClass<CAFUtility, &CLSID_AFUtility>,
	public ISupportErrorInfo,
	public IDispatchImpl<ILicensedComponent, &IID_ILicensedComponent, &LIBID_UCLID_COMLMLib>,
	public IDispatchImpl<IAFUtility, &IID_IAFUtility, &LIBID_UCLID_AFUTILSLib>
{
public:
	CAFUtility();
	~CAFUtility();

DECLARE_REGISTRY_RESOURCEID(IDR_AFUTILITY)

DECLARE_CLASSFACTORY_SINGLETON(CAFUtility)

DECLARE_PROTECT_FINAL_CONSTRUCT()

BEGIN_COM_MAP(CAFUtility)
	COM_INTERFACE_ENTRY(IAFUtility)
	COM_INTERFACE_ENTRY2(IDispatch,IAFUtility)
	COM_INTERFACE_ENTRY(ILicensedComponent)
	COM_INTERFACE_ENTRY(ISupportErrorInfo)
END_COM_MAP()

public:
// ISupportsErrorInfo
	STDMETHOD(InterfaceSupportsErrorInfo)(REFIID riid);

// ILicensedComponent
	STDMETHOD(raw_IsLicensed)(VARIANT_BOOL * pbValue);

// IAFUtility
	STDMETHOD(GetNameToAttributesMap)(
		/*[in]*/ IIUnknownVector* pVecAttributes, 
		/*[out, retval]*/ IStrToObjectMap** ppMapNameToAttributes);
	STDMETHOD(GetComponentDataFolder)(
		/*[out, retval]*/ BSTR* pstrComponentDataFolder);
	STDMETHOD(GetPrefixedFileName)(
		/*[in]*/ BSTR strNonPrefixFullPath, 
		/*[out, retval]*/ BSTR* pstrFileToRead);
	STDMETHOD(GetLoadFilePerSession)(
		/*[out, retval]*/ VARIANT_BOOL *pbSetting);
	STDMETHOD(SetAutoEncrypt)(
		/*[in]*/ VARIANT_BOOL bAutoEncryptOn);
	STDMETHOD(GetAttributesAsString)(
		/*[in]*/ IIUnknownVector *pAttributes, 
		/*[out, retval]*/ BSTR *pAttributesInString);
	STDMETHOD(GenerateAttributesFromEAVFile)(
		/*[in]*/ BSTR strEAVFileName, 
		/*[out, retval]*/ IIUnknownVector** ppAttributes);
	STDMETHOD(ExpandTags)(
		/*[in]*/ BSTR strInput, 
		/*[in*/ IAFDocument *pDoc, 
		/*[out, retval]*/ BSTR *pstrOutput);
	STDMETHOD(StringContainsInvalidTags)(
		/*[in]*/ BSTR strInput, 
		/*[out, retval]*/ VARIANT_BOOL *pbValue);
	STDMETHOD(StringContainsTags)(
		/*[in]*/ BSTR strInput, 
		/*[out, retval]*/ VARIANT_BOOL *pbValue);
	STDMETHOD(GetAttributesFromFile)(
		/*[in]*/ BSTR strFileName, 
		/*[out, retval]*/ IIUnknownVector** ppAttributes);
	STDMETHOD(GetAttributesFromDelimitedString)(
		/*[in]*/ BSTR bstrAttributes, 
		/*[in]*/ BSTR bstrDelimiter, 
		/*[out, retval]*/ IIUnknownVector **ppAttributes);
	STDMETHOD(QueryAttributes)(
		/*[in]*/ IIUnknownVector *pvecAttributes, 
		/*[in]*/ BSTR strQuery, 
		/*[in]*/ VARIANT_BOOL bRemoveMatches,
		/*[out, retval]*/ IIUnknownVector** ppAttributes);
	STDMETHOD(ApplyAttributeModifier)(
	/*[in]*/ IIUnknownVector *pvecAttributes, 
		/*[in]*/ IAFDocument *pDoc, 
		/*[in]*/ IAttributeModifyingRule *pAM, 
		/*[in]*/ VARIANT_BOOL bRecursive);
	STDMETHOD(GetAttributeParent)(
		/*[in]*/ IIUnknownVector *pvecAttributes, 
		/*[in]*/ IAttribute *pAttribute, 
		/*[out, retval]*/ IAttribute** pRetVal);
	STDMETHOD(GetAttributeRoot)(
		/*[in]*/ IIUnknownVector *pvecAttributes, 
		/*[in]*/ IAttribute *pAttribute, 
		/*[out, retval]*/ IAttribute** pRetVal);
	STDMETHOD(RemoveAttribute)(
		/*[in]*/ IIUnknownVector *pvecAttributes, 
		/*[in]*/ IAttribute *pAttribute);
	STDMETHOD(RemoveAttributes)(
		/*[in]*/ IIUnknownVector *pvecAttributes, 
		/*[in]*/ IIUnknownVector *pvecRemove);
	STDMETHOD(GetMinQueryDepth)(
		/*[in]*/ BSTR bstrQuery, 
		/*[out, retval]*/ long* pRetVal);
	STDMETHOD(GetMaxQueryDepth)(
		/*[in]*/ BSTR bstrQuery, 
		/*[out, retval]*/ long* pRetVal);
	STDMETHOD(ExpandFormatString)(
		/*[in]*/ IAttribute *pAttribute, 
		/*[in]*/ BSTR bstrFormat,
		/*[out, retval]*/ ISpatialString** pRetVal);
	STDMETHOD(IsValidQuery)(
		/*[in]*/ BSTR bstrQuery,
		/*[out, retval]*/ VARIANT_BOOL* pRetVal);
	STDMETHOD(SortAttributesSpatially)(
		/*[in, out]*/ IIUnknownVector* pAttributes);
	STDMETHOD(GetBuiltInTags)( 
		/*[out, retval]*/ IVariantVector** ppTags);
	STDMETHOD(GetINIFileTags)( 
		/*[out, retval]*/ IVariantVector** ppTags);
	STDMETHOD(GetAllTags)( 
		/*[out, retval]*/ IVariantVector** ppTags);
	STDMETHOD(get_ShouldCacheRSD)(
		/*[out, retval]*/ VARIANT_BOOL *pvbCacheRSD);
	STDMETHOD(ExpandTagsAndFunctions)(BSTR bstrInput, IAFDocument *pDoc, BSTR *pbstrOutput);

private:
	//////////////////
	// Internal classes
	//////////////////
	class QueryPattern
	{
	public:
		QueryPattern(string strName = "");
		QueryPattern(string strName, string strType);

		// name of the attribute that needs to be matched
		string m_strName;

		// type of the attribute that needs to be matched
		string m_strType;

		// whether or not type is a match criteria
		bool m_bTypeSpecified;
	};

	//////////
	// Methods
	//////////
	//----------------------------------------------------------------------------------------------
	UCLID_AFUTILSLib::IAFUtilityPtr getThisAsCOMPtr();
	//----------------------------------------------------------------------------------------------
	// at position nCurrentMatchPos stored in vecPatterns.  If so, 
	// add the attribute to ripMatches, and remove attribute from 
	// ripParentOfAttribute if bRemoveMatchFromParent==true.  An attribute
	// is considered as matching to a query only if all query patterns
	// that make up the query match the attribute and its ancestors.
	void processAttributeForMatches(IAttributePtr& ripAttribute, 
		const vector<QueryPattern>& vecPatterns,
		const vector<QueryPattern>& vecNonSelectPatterns,
		long nCurrentMatchPos, IIUnknownVectorPtr& ripMatches, 
		bool bRemoveMatchFromParent, 
		bool& rbAttributeWasMatched);

	void processAttributesForMatches( const vector<QueryPattern>& vecPatterns, 
		const vector<QueryPattern>& vecNonSelectPatterns, 
		long nCurrentMatchPos, IIUnknownVectorPtr& ripMatches, 
		bool bRemoveMatchFromParent, IIUnknownVectorPtr& ripAttributes);

	// process a query of the form "AttributeA/SubAttribute@TypeValue"
	// and return a vector of query pattern objects
	void getQueryPatterns(string strQuery, 
		vector<QueryPattern>& rvecPatterns);

	// Recursive method to return the parent of ipAttribute
	IAttributePtr getParent(IAttributePtr ipTestParent, IAttributePtr ipAttribute);

	// recursive method to remove ipAttribute
	IAttributePtr remove(IAttributePtr ipTestParent, IAttributePtr ipAttribute);

	// If there's a prefix for each rules file.
	// ex. if prefix string is "WS_" then the file shall be prefixed with the string
	string getRulesFilePrefix();

	// cache of tag name/value read from the INI file
	static map<string, string> ms_mapINIFileTagNameToValue;

	IIUnknownVectorPtr getCandidateAttributes(IIUnknownVectorPtr ipAttributes, string strQuery, bool bRemoveMatches);

	void splitQuery(string strQuery, vector<QueryPattern>& rvecPatterns, vector<QueryPattern>& rvecNonSelectPatterns);

	// Whether or not to automatically use EncryptTextFile.exe to
	// create .etf file
	bool isAutoEncryptOn();

	// Check license state
	void validateLicense();

	// Removes the specified attribute from the collection of attributes (no matter where
	// in the collection it lives)
	void removeAttribute(IIUnknownVectorPtr ipAttributes, IAttributePtr ipAttribute);

	// Searches the collection of attributes and finds the parent attribute for the
	// specified attribute.  If no parent attribute is found just returns NULL.
	IAttributePtr getAttributeParent(IIUnknownVectorPtr ipAttributes, IAttributePtr ipAttribute);

	/////////////
	// Variables
	/////////////

	// Handles registry settings
	auto_ptr<IConfigurationSettingsPersistenceMgr> ma_pUserCfgMgr;

	// pointer to the singleton instance of the Rule Execution
	// Environment (REE) object
	IRuleExecutionEnvPtr m_ipREE;

	// pointer to the utility object that deals with encryption
	IMiscUtilsPtr m_ipMiscUtils;

	// a vector of all valid tags that can be expanded
	static vector<string> ms_vecValidTags;

	// Since multiple threads may be using AFUtils at the same time
	// we will use this Mutex to prevent simultaneous
	// access to the member data.  This is necessary.
	static CMutex ms_mutex;

	// methods to expand the various tags
	// each of these methods can be called anytime.  They will expand
	// the string after checking to see if the appropriate tag exists
	void expandRSDFileDirTag(string& rstrInput, IAFDocumentPtr& ripDoc);
	void expandRuleExecIDTag(string& rstrInput, IAFDocumentPtr& ripDoc);
	void expandSourceDocNameTag(string& rstrInput, IAFDocumentPtr& ripDoc);
	void expandDocTypeTag(string& rstrInput, IAFDocumentPtr& ripDoc);
	void expandComponentDataDirTag(string& rstrInput, IAFDocumentPtr& ripDoc);
	void expandINIFileTags(string& rstrInput, IAFDocumentPtr& ripDoc);
	void expandAFDocTags(string& rstrInput, IAFDocumentPtr& ripDoc);
	//---------------------------------------------------------------------------------------------
	// REQUIRE: strTagName has the '<' and '>' as the first and last chars
	// PROMISE:	To return the value of strTagName, as specified in the INI file.
	//			If a value is found for strTagName in the INI file, then the value is returned
	//			through the out parameter, and true is returned (to indicate that the value was
	//			successfully found)
	//			If strTagName is not found in the INI file, then false will be returned.
	bool getTagValueFromINIFile(string strTagName, string& rstrTagValue);
	//---------------------------------------------------------------------------------------------
	// PROMISE: To return all tag names in strInput.
	//			The returned strings will include the < and > chars
	void getTagNames(const string& strInput, 
		vector<string>& rvecTagNames) const;
	//---------------------------------------------------------------------------------------------
	void expandTags(string& rstrInput, IAFDocumentPtr ipDoc);
	//---------------------------------------------------------------------------------------------

	// method that returns the component data folder.
	// This functionality has been moved to a c++ method instead of the
	// COM exposed method because the expandComponentDataDirTag() method
	// needs access to this information as well.
	void getComponentDataFolder(string& rFolder);

	void loadAttributesFromEavFile(IIUnknownVectorPtr ipAttributes, unsigned long ulCurrLevel, 
		unsigned int& uiCurrLine, vector<string> vecLines);
	unsigned int getAttributeLevel(string strName);
	void removeDots(string& strName);

	class FormatVariable
	{
	public:
		ISpatialStringPtr m_ipValue;
		ISpatialStringPtr m_ipType;
		bool m_bExists;
	};

	typedef map<string, vector<ISpatialStringPtr> > VariableMap;
	ISpatialStringPtr getReformattedName(const string& strFormat, IAttributePtr ipAttribute);
	ISpatialStringPtr getVariableValue(const string& strVariable, IAttributePtr ipAttribute);

};
