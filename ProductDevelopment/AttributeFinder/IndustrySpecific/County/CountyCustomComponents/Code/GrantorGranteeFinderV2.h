// GrantorGranteeFinderV2.h : Declaration of the CGrantorGranteeFinderV2

#pragma once

#include "resource.h"       // main symbols

#include "..\..\..\..\AFCore\Code\AFCategories.h"

#include <IConfigurationSettingsPersistenceMgr.h>
#include "MERSFinder.h"


#include <memory>
#include <map>
#include <vector>
#include <stringCSIS.h>

/////////////////////////////////////////////////////////////////////////////
// CGrantorGranteeFinderV2
class ATL_NO_VTABLE CGrantorGranteeFinderV2 : 
	public CComObjectRootEx<CComMultiThreadModel>,
	public CComCoClass<CGrantorGranteeFinderV2, &CLSID_GrantorGranteeFinderV2>,
	public ISupportErrorInfo,
	public IDispatchImpl<IGrantorGranteeFinderV2, &IID_IGrantorGranteeFinderV2, &LIBID_UCLID_COUNTYCUSTOMCOMPONENTSLib>,
	public IDispatchImpl<IAttributeFindingRule, &IID_IAttributeFindingRule, &LIBID_UCLID_AFCORELib>,
	public IDispatchImpl<ICategorizedComponent, &IID_ICategorizedComponent, &LIBID_UCLID_COMUTILSLib>,
	public IDispatchImpl<ILicensedComponent, &IID_ILicensedComponent, &LIBID_UCLID_COMLMLib>,
	public IDispatchImpl<ICopyableObject, &IID_ICopyableObject, &LIBID_UCLID_COMUTILSLib>,
	public IPersistStream,
	public ISpecifyPropertyPagesImpl<CGrantorGranteeFinderV2>
{
public:
	CGrantorGranteeFinderV2();
	~CGrantorGranteeFinderV2();

DECLARE_REGISTRY_RESOURCEID(IDR_GRANTORGRANTEEFINDERV2)

DECLARE_PROTECT_FINAL_CONSTRUCT()

BEGIN_COM_MAP(CGrantorGranteeFinderV2)
	COM_INTERFACE_ENTRY(IGrantorGranteeFinderV2)
	COM_INTERFACE_ENTRY2(IDispatch,IGrantorGranteeFinderV2)
	COM_INTERFACE_ENTRY(ISupportErrorInfo)
	COM_INTERFACE_ENTRY(IAttributeFindingRule)
	COM_INTERFACE_ENTRY(ICategorizedComponent)
	COM_INTERFACE_ENTRY(ILicensedComponent)
	COM_INTERFACE_ENTRY(ICopyableObject)
	COM_INTERFACE_ENTRY(IPersistStream)
	COM_INTERFACE_ENTRY_IMPL(ISpecifyPropertyPages)
END_COM_MAP()

BEGIN_PROP_MAP(CGrantorGranteeFinderV2)
	PROP_PAGE(CLSID_GrantorGranteeFinderV2PP)
END_PROP_MAP()

BEGIN_CATEGORY_MAP(CGrantorGranteeFinderV2)
	IMPLEMENTED_CATEGORY(CATID_AFAPI_VALUE_FINDERS)
END_CATEGORY_MAP()

public:
// IGrantorGranteeFinderV2
	STDMETHOD(get_UseSelectedDatFiles)(/*[out, retval]*/ BOOL *pVal);
	STDMETHOD(put_UseSelectedDatFiles)(/*[in]*/ BOOL newVal);
	STDMETHOD(get_DocTypeToFileMap)(/*[out, retval]*/ IStrToObjectMap* *pVal);
	STDMETHOD(put_DocTypeToFileMap)(/*[in]*/ IStrToObjectMap *newVal);
	STDMETHOD(GetAFUtility)(/*[out, retval]*/ IAFUtility **ppAFUtil);

// ISupportsErrorInfo
	STDMETHOD(InterfaceSupportsErrorInfo)(REFIID riid);

// IAttributeFindingRule
	STDMETHOD(raw_ParseText)(IAFDocument* pAFDoc, IProgressStatus *pProgressStatus, 
		IIUnknownVector **pAttributes);

// ICategorizedComponent
	STDMETHOD(raw_GetComponentDescription)(BSTR * pstrComponentDescription);

// ILicensedComponent
	STDMETHOD(raw_IsLicensed)(VARIANT_BOOL * pbValue);

// ICopyableObject
	STDMETHOD(raw_Clone)(/*[out, retval]*/ IUnknown* *pObject);
	STDMETHOD(raw_CopyFrom)(/*[in]*/ IUnknown *pObject);

// IPersistStream
	STDMETHOD(GetClassID)(CLSID *pClassID);
	STDMETHOD(IsDirty)(void);
	STDMETHOD(Load)(IStream *pStm);
	STDMETHOD(Save)(IStream *pStm, BOOL fClearDirty);
	STDMETHOD(GetSizeMax)(ULARGE_INTEGER *pcbSize);

private:
	///////////
	// Methods
	///////////
	IAFUtilityPtr getAFUtility();

	IEntityFinderPtr getEntityFinder();

	// Returns the full path name that contains all the rules files.
	// This function will return different paths based on current build configuration
	// i.e. if current build configuration is set to Debug, then it returns the path
	// to the Req-Design folder under the IndustrySpecific\County. If current build
	// is Release, then it will return the path wherever the current module resides.
	std::string getRulesFolder();

	ISPMFinderPtr getSPMFinder();

	// Create tags for the AFDocument if necessary
	void processAFDcoument(IAFDocumentPtr ipAFDoc);

	// Retrieves StoreRulesWorked registry setting
	bool storeRulesWorked();

	void validateLicense();

	// Returns the vector of SPMFinders for the document type and 
	// optional sub-type
	std::vector<ISPMFinderPtr> getDocTypeSPMFinders(const std::string& strDocType, 
		const std::string& strDocSubType);

	// Returns the common data scorer object m_ipDataScorer if NULL a new object will be created
	IDataScorerPtr getDataScorer();

	// Returns whether there's one and only one doc type available.
	// If there's only one doc type, strDocType will contain the doc type.
	// Requires document classifier to have already been applied
	bool getDocType(IAFDocumentPtr ipAFDoc, std::string& strDocType);

	// Returns whether there's one and only one document sub-type available.
	// If there's only one sub-type, strDocSubType will contain the doc type.
	// Requires document classifier to have already been applied
	bool getDocSubType(IAFDocumentPtr ipAFDoc, std::string& strDocSubType);

	// Returns vector of documents types from .idx file in DocumentClassifiers\County Document directory
	std::vector< stringCSIS > &getValidDocTypes();

	//	Purpose:	To Return true if strDocType is within the m_vecValidDocTypes vector
	//	Note:		The string is compared without case sensitivity
	bool isValidDocType ( stringCSIS strDocType );

	// Creates DocTypeToFileMap and initializes with all of the dat file names if DocType directories
	void initDocTypeToFileMap();

	//	Purpose:	To remove any invalid doc types or mappings from the m_ipDocTypeToFileMap
	//				by comparing doc types against those in document Classification DocTypes.idx file
	//	Requires:	m_ipDocTypeToFileMap to be loaded
	//	Promise:	Will move the mappings in the loaded m_ipDocTypeToFileMap to another map object
	//				that will be insensitive to case.  If a doctype is found in the mapping with another
	//				case the first one will be moved to the new DocTypeToFileMap object and all others will
	//				be discard with an exception logged. Each DocType will be validated against the types 
	//				listed in the DocTypes.idx file in the document classification directory. If the
	//				docType is not found the mapping will be discarded and an exception will be logged
	//				The new DocTypeToFileMap object will be assigned to m_ipDocTypeToFileMap;
	void removeInvalidDocMappings();
	
	////////////
	// Constants
	////////////
	static const std::string GGFINDERS_SECTIONNAME;
	static const std::string DOCTYPE_STORERULES;
	static const std::string DEFAULT_DOCTYPE_STORERULES;

	////////////
	// Variables
	////////////
	bool m_bDirty;

	IEntityFinderPtr m_ipEntityFinder;
	IDocumentPreprocessorPtr m_ipDocPreprocessor;
	MERSFinder m_MERSFinder;
	IAFUtilityPtr m_ipAFUtility;

	// Value for UseSelectedDatFiles
	bool m_bUseSelectedDatFiles;

	std::unique_ptr<IConfigurationSettingsPersistenceMgr> ma_pUserCfgMgr;

	// Maps Document type string to the vector of string pattern finders to be used
	// for that document type
	std::map< stringCSIS, std::vector<ISPMFinderPtr> > m_mapStringToVecSPMFinders;

	// data scorer to be used by all instances of SPMFinder
	IDataScorerPtr m_ipDataScorer;

	// map for Doctype string to variant vector of filenames without path or extension
	IStrToObjectMapPtr m_ipDocTypeToFileMap;

	// Flag indicating the DocTypeToFileMap has been setup
	bool m_bDocTypeToFileMapLoaded;

	// Vector to contain the loaded doc types for the document classifier idx file
	vector< stringCSIS > m_vecValidDocTypes;

	// flag to indicate that the ValdiDocTypes has been loaded
	bool m_bValidDocTypesLoaded;
};
