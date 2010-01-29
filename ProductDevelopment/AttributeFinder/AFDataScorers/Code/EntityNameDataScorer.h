// EntityNameDataScorer.h : Declaration of the CEntityNameDataScorer

#pragma once

#include "resource.h"       // main symbols
#include "..\\..\\AFCore\\Code\\AFCategories.h"

#include <IConfigurationSettingsPersistenceMgr.h>
#include <vector>
using namespace std;

/////////////////////////////////////////////////////////////////////////////
// CEntityNameDataScorer
class ATL_NO_VTABLE CEntityNameDataScorer : 
	public CComObjectRootEx<CComMultiThreadModel>,
	public CComCoClass<CEntityNameDataScorer, &CLSID_EntityNameDataScorer>,
	public IPersistStream,
	public ISupportErrorInfo,
	public IDispatchImpl<IEntityNameDataScorer, &IID_IEntityNameDataScorer, &LIBID_UCLID_AFDATASCORERSLib>,
	public IDispatchImpl<ICategorizedComponent, &IID_ICategorizedComponent, &LIBID_UCLID_COMUTILSLib>,
	public IDispatchImpl<ICopyableObject, &IID_ICopyableObject, &LIBID_UCLID_COMUTILSLib>,
	public IDispatchImpl<IDataScorer, &IID_IDataScorer, &LIBID_UCLID_AFCORELib>,
	public IDispatchImpl<ILicensedComponent, &IID_ILicensedComponent, &LIBID_UCLID_COMLMLib>
{
public:
	CEntityNameDataScorer();
	~CEntityNameDataScorer();

DECLARE_REGISTRY_RESOURCEID(IDR_ENTITYNAMEDATASCORER)

DECLARE_PROTECT_FINAL_CONSTRUCT()

BEGIN_COM_MAP(CEntityNameDataScorer)
	COM_INTERFACE_ENTRY(IEntityNameDataScorer)
	COM_INTERFACE_ENTRY(ISupportErrorInfo)
	COM_INTERFACE_ENTRY(IPersistStream)
	COM_INTERFACE_ENTRY2(IDispatch, IEntityNameDataScorer)
	COM_INTERFACE_ENTRY(IDataScorer)
	COM_INTERFACE_ENTRY(ICategorizedComponent)
	COM_INTERFACE_ENTRY(ICopyableObject)
	COM_INTERFACE_ENTRY(ILicensedComponent)
END_COM_MAP()

BEGIN_CATEGORY_MAP(CEntityNameDataScorer)
	IMPLEMENTED_CATEGORY(CATID_AFAPI_DATA_SCORERS)
END_CATEGORY_MAP()

// ISupportsErrorInfo
	STDMETHOD(InterfaceSupportsErrorInfo)(REFIID riid);

// IDataScorer
	STDMETHOD(raw_GetDataScore1)(IAttribute * pAttribute, LONG * pScore);
	STDMETHOD(raw_GetDataScore2)(IIUnknownVector * pAttributes, LONG * pScore);

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

// ILicensedComponent
	STDMETHOD(raw_IsLicensed)(VARIANT_BOOL * pbValue);

private:
	//////////
	// Variables
	//////////

	// member variables
	bool m_bDirty; // dirty flag to indicate modified state of this object

	// Handles configuration persistence
	auto_ptr<IConfigurationSettingsPersistenceMgr> ma_pUserCfgMgr;

	// if m_bLoggingEnabled is true the scores should be logged
	bool m_bLoggingEnabled;

	// Common Words pattern
	string m_strCommonWords;
	// Indicates if the m_strCommonWords string is loaded
	bool m_bIsCommonWordsLoaded;

	// vector containing words that will invalidate a person
	vector<string> m_vecInvalidPersonWords;

	bool m_bIsInvalidPersonWordsLoaded;

	IAFUtilityPtr	m_ipAFUtility;

	IMiscUtilsPtr m_ipMiscUtils;
	
	//////////
	// Methods
	//////////

	// returns the score for a single attribute
	long getAttrScore( IAttributePtr ipAttribute );

	long getCompanyScore( string strCompanyString, string strOriginal,
		IRegularExprParserPtr ipParser );
	long getPersonScore( IAttributePtr ipAttribute, string strOriginal,
		IRegularExprParserPtr ipParser );

	// returns true if 1/2 of the words in the vector have first letter capitalized
	bool isTitleCase( vector<string> &vecWords );


	// Takes a company or person value and removes common words and gives a
	// score of 2 if anything is left in the string and 0 if not
	bool isAllCommonWords( const string& strInput,
		IRegularExprParserPtr ipParser);
	
	// Returns the pattern for testing for common words
	string &getCommonWordsPattern();

	// Returns m_ipAFUtility, after initializing it if necessary
	IAFUtilityPtr getAFUtility();

	// Adds line to end of ENDSLog.dat file in the directory the AFDataScorers.dll is in
	// the format of the lines is: <nScore>|<strItemScored>
	void logResults(long nScore, string strItemScored, bool bLineAfter = false);
	
	// Obtains the Logging Enabled setting from the registry
	long getLoggingEnabled();

	// returns true if string contains both vowels and consonants
	bool hasVowelsAndConsonants( const string& strItem );

	// returns true if there are only characters in strValidChars are in strItem
	bool noInvalidChars( const string& strItem, const string& strValidChars );

	void loadInvalidPersonVector();

	// Promise:	To return true if the vector vecWords contains any word in the
	//			vector m_vecInvalidPersonWords and false other wise
	bool containsInvalidPersonWords( const vector<string> &vecWords );

	// Promise: To return the number of times the regular expressing strRegExpToFind appears in strInput
	// Note:	Performs a case insensitive search
	long countOfRegExpInInput( const string &strRegExpToFind, const string &strInput,
		IRegularExprParserPtr ipParser);

	// Gets a new instance of the regex parser
	IRegularExprParserPtr getParser();

	void validateLicense();
};

