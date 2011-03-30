// LocateImageRegion.h : Declaration of the CLocateImageRegion

#pragma once

#include "resource.h"       // main symbols
#include "..\..\AFCore\Code\AFCategories.h"
#include "..\..\AFCore\Code\StringLoader.h"

#include <CachedListLoader.h>

#include <map>

/////////////////////////////////////////////////////////////////////////////
// CLocateImageRegion
class ATL_NO_VTABLE CLocateImageRegion : 
	public CComObjectRootEx<CComMultiThreadModel>,
	public CComCoClass<CLocateImageRegion, &CLSID_LocateImageRegion>,
	public ISupportErrorInfo,
	public IDispatchImpl<ILocateImageRegion, &IID_ILocateImageRegion, &LIBID_UCLID_AFVALUEFINDERSLib>,
	public IPersistStream,
	public ISpecifyPropertyPagesImpl<CLocateImageRegion>,
	public IDispatchImpl<IAttributeFindingRule, &IID_IAttributeFindingRule, &LIBID_UCLID_AFCORELib>,
	public IDispatchImpl<IAttributeModifyingRule, &IID_IAttributeModifyingRule, &LIBID_UCLID_AFCORELib>,
	public IDispatchImpl<IDocumentPreprocessor, &IID_IDocumentPreprocessor, &LIBID_UCLID_AFCORELib>,
	public IDispatchImpl<ICategorizedComponent, &IID_ICategorizedComponent, &LIBID_UCLID_COMUTILSLib>,
	public IDispatchImpl<ICopyableObject, &IID_ICopyableObject, &LIBID_UCLID_COMUTILSLib>,
	public IDispatchImpl<IMustBeConfiguredObject, &IID_IMustBeConfiguredObject, &LIBID_UCLID_COMUTILSLib>,
	public IDispatchImpl<ILicensedComponent, &IID_ILicensedComponent, &LIBID_UCLID_COMLMLib>
{
public:
	CLocateImageRegion();
	~CLocateImageRegion();

DECLARE_REGISTRY_RESOURCEID(IDR_LOCATEIMAGEREGION)

DECLARE_PROTECT_FINAL_CONSTRUCT()

BEGIN_COM_MAP(CLocateImageRegion)
	COM_INTERFACE_ENTRY(ILocateImageRegion)
	COM_INTERFACE_ENTRY2(IDispatch, ILocateImageRegion)
	COM_INTERFACE_ENTRY(ISupportErrorInfo)
	COM_INTERFACE_ENTRY(IAttributeFindingRule)
	COM_INTERFACE_ENTRY(IAttributeModifyingRule)
	COM_INTERFACE_ENTRY(IDocumentPreprocessor)
	COM_INTERFACE_ENTRY(IPersistStream)
	COM_INTERFACE_ENTRY(ICategorizedComponent)
	COM_INTERFACE_ENTRY(ILicensedComponent)
	COM_INTERFACE_ENTRY(ICopyableObject)
	COM_INTERFACE_ENTRY(IMustBeConfiguredObject)
	COM_INTERFACE_ENTRY_IMPL(ISpecifyPropertyPages)
END_COM_MAP()

BEGIN_PROP_MAP(CLocateImageRegion)
	PROP_PAGE(CLSID_LocateImageRegionPP)
END_PROP_MAP()

BEGIN_CATEGORY_MAP(CLocateImageRegion)
	IMPLEMENTED_CATEGORY(CATID_AFAPI_VALUE_FINDERS)
	IMPLEMENTED_CATEGORY(CATID_AFAPI_VALUE_MODIFIERS)
	IMPLEMENTED_CATEGORY(CATID_AFAPI_DOCUMENT_PREPROCESSORS)
END_CATEGORY_MAP()

public:
// ISupportsErrorInfo
	STDMETHOD(InterfaceSupportsErrorInfo)(REFIID riid);

// IDocumentPreprocessor
	STDMETHOD(raw_Process)(/*[in]*/ IAFDocument* pDocument, /*[in]*/ IProgressStatus *pProgressStatus);

// ILocateImageRegion
	STDMETHOD(GetClueList)(/*[in]*/ EClueListIndex eIndex, 
						   /*[in, out]*/ IVariantVector** ppvecClues, 
						   /*[in, out]*/ VARIANT_BOOL *pbCaseSensitive, 
						   /*[in, out]*/ VARIANT_BOOL *pbAsRegExpr, 
						   /*[in, out]*/ VARIANT_BOOL *pbRestrictByBoundary);
	STDMETHOD(SetClueList)(/*[in]*/ EClueListIndex eIndex, 
						   /*[in]*/ IVariantVector* pvecClues, 
						   /*[in]*/ VARIANT_BOOL bCaseSensitive, 
						   /*[in]*/ VARIANT_BOOL bAsRegExpr, 
						   /*[in]*/ VARIANT_BOOL bRestrictByBoundary);
	STDMETHOD(ClearAllClueLists)();
	STDMETHOD(get_FindType)(EFindType* pVal);
	STDMETHOD(put_FindType)(EFindType newVal);
	STDMETHOD(get_ImageRegionText)(BSTR* pVal);
	STDMETHOD(put_ImageRegionText)(BSTR newVal);
	STDMETHOD(GetRegionBoundary)(/*[in]*/ EBoundary eRegionBoundary, 
		/*[in, out]*/ EBoundary *peSide, /*[in, out]*/ EBoundaryCondition *peCondition, 
		/*[in, out]*/ EExpandDirection *peExpandDirection, /*[in, out]*/ double *pdExpandNumber);
	STDMETHOD(SetRegionBoundary)(/*[in]*/ EBoundary eRegionBoundary, 
		/*[in]*/ EBoundary eSide, /*[in]*/ EBoundaryCondition eCondition, 
		/*[in]*/ EExpandDirection eExpandDirection, /*[in]*/ double ExpandNumber);
	STDMETHOD(get_IntersectingEntityType)(/*[out, retval]*/ ESpatialEntity *pVal);
	STDMETHOD(put_IntersectingEntityType)(/*[in]*/ ESpatialEntity newVal);
	STDMETHOD(get_IncludeIntersectingEntities)(/*[out, retval]*/ VARIANT_BOOL *pVal);
	STDMETHOD(put_IncludeIntersectingEntities)(/*[in]*/ VARIANT_BOOL newVal);
	STDMETHOD(get_DataInsideBoundaries)(/*[out, retval]*/ VARIANT_BOOL *pVal);
	STDMETHOD(put_DataInsideBoundaries)(/*[in]*/ VARIANT_BOOL newVal);
	STDMETHOD(get_MatchMultiplePagesPerDocument)(/*[out, retval]*/ VARIANT_BOOL *pVal);
	STDMETHOD(put_MatchMultiplePagesPerDocument)(/*[in]*/ VARIANT_BOOL newVal);

// IPersistStream
	STDMETHOD(GetClassID)(CLSID *pClassID);
	STDMETHOD(IsDirty)(void);
	STDMETHOD(Load)(IStream *pStm);
	STDMETHOD(Save)(IStream *pStm, BOOL fClearDirty);
	STDMETHOD(GetSizeMax)(ULARGE_INTEGER *pcbSize);

// IAttributeFindingRule
	STDMETHOD(raw_ParseText)(IAFDocument* pAFDoc, IProgressStatus *pProgressStatus,
		IIUnknownVector **pAttributes);

// IAttributeModifyingRule
	STDMETHOD(raw_ModifyValue)(IAttribute* pAttribute, IAFDocument* pOriginInput, 
		IProgressStatus *pProgressStatus);

// ICategorizedComponent
	STDMETHOD(raw_GetComponentDescription)(BSTR * pstrComponentDescription);

// ILicensedComponent
	STDMETHOD(raw_IsLicensed)(VARIANT_BOOL * pbValue);

// IMustBeConfiguredObject
	STDMETHOD(raw_IsConfigured)(VARIANT_BOOL * pbValue);

// ICopyableObject
	STDMETHOD(raw_Clone)(/*[out, retval]*/ IUnknown* *pObject);
	STDMETHOD(raw_CopyFrom)(/*[in]*/ IUnknown *pObject);

private:
	////////////
	// Variables
	////////////

	// When constructing ClueListGuard, pass it this object's pointer and 
	// a back-up copy of this object's clue list. When ClueListGuard's 
	// destructor is called it will recover this object's clue list
	// to its backed-up values.
	class ClueListGuard 
	{
	public:
		ClueListGuard(CLocateImageRegion* pLocateImageRegion,
						 IVariantVectorPtr* pipVecClues);
		~ClueListGuard();
	private:
		CLocateImageRegion* m_pLocateImageRegion;
		IVariantVectorPtr m_ipVecClues[4];
	};

	// For each clue list contents as well as 
	// several flags associated with current clue list
	struct ClueListInfo
	{
		ClueListInfo()
			: m_ipClues(NULL),
			  m_bCaseSensitive(false),
			  m_bAsRegExpr(false),
			  m_bRestrictByBoundary(false),
			  m_bSearchThisList(false),
			  m_ipFoundString(NULL)
		{
		}

		IVariantVectorPtr m_ipClues;
		bool m_bCaseSensitive;
		bool m_bAsRegExpr;
		bool m_bRestrictByBoundary;
		// whether or not to search this list
		bool m_bSearchThisList;
		// record tempory found spatial string if m_eCondition is
		// defined as one of the clue lists
		ISpatialStringPtr m_ipFoundString;
	};

	// For each boundary
	struct BoundaryInfo
	{
		BoundaryInfo()
			: m_eSide(kNoBoundary),
			  m_eCondition(kNoCondition),	// always default to document edge
			  m_eExpandDirection(kNoDirection),
			  m_dExpandNumber(0)
		{
		}

		// Is it going to be the top, bottom, left or right
		// of certain clue list or page or document edge?
		EBoundary m_eSide;
		// Is it going to be one of the clue lists or the page
		// containing the clues or the entire document that will
		// define the region's boundary?
		EBoundaryCondition m_eCondition;
		// Does the boundary need to be expanded a little?
		EExpandDirection m_eExpandDirection;
		// How many SpatialLines|Characters|Words shall we expand?
		double m_dExpandNumber;
	};

	// determines whether to find text or an image region
	EFindType m_eFindType;

	// text to use as filler for the image region
	std::string m_strImageRegionText;

	// clue lists
	typedef std::map<EClueListIndex, ClueListInfo> IndexToClueListInfo;
	IndexToClueListInfo m_mapIndexToClueListInfo;

	// map of boundaries
	typedef std::map<EBoundary, BoundaryInfo> BoundaryToInfo;
	BoundaryToInfo m_mapBoundaryToInfo;

	// whether or not the data is within the boundaries
	bool m_bDataInsideBoundaries;

	// whether or not to include intersecting entities
	bool m_bIncludeIntersecting;

	// whether or not to match multiple pages per document
	bool m_bMatchMultiplePagesPerDocument;

	// if m_bIncludeIntersecting == true, this must be specified.
	// By default it is kNoEntity.
	ESpatialEntity m_eIntersectingEntity;

	// map for recording found borders
	// Position is at pixel's level
	// if position = -1, it's the page edge
	std::map<EBoundary, long> m_mapBorderToPosition;

	// spatial string searcher
	ISpatialStringSearcherPtr m_ipSpatialStringSearcher;

	// flag to keep track of whether this object has been modified
	// since the last save-to-stream operation
	bool m_bDirty;

	// Cached list loader object to read clues from files
	CCachedListLoader m_cachedListLoader;

	IMiscUtilsPtr m_ipMisc;

	//////////
	// Methods
	//////////

	//---------------------------------------------------------------------------------------------
	// PURPOSE: Sets m_mapBorderToPosition to the boundaries specified by mapBorderToInfo. Sets 
	//          m_mapIndexToClueListInfo based on the specified region of ipPageText. If boundaries, 
	//          are not able to be calculated, returns false.
	// REQUIRE: mapBorderToInfo != __nullptr
	//          ipPageText != __nullptr
	// PROMISE: Result doesn't include any expansion. Returns true if it's possible to calculate 
	//          the boundaries, false otherwise
	bool calculateRoughBorderPosition(BoundaryToInfo mapBorderToInfo, ISpatialStringPtr ipPageText,
		IRegularExprParserPtr ipParser);

	//---------------------------------------------------------------------------------------------
	// PURPOSE: Returns the vertical or horizontal position in pixels of the specified side of the
	//          specified page.
	long getPageBoundaryPosition(EBoundary eSide, ISpatialStringPtr ipPageText);

	//---------------------------------------------------------------------------------------------
	// PURPOSE: Finds the contents of each requested region of ipInputText based on 
	//          LocateImageRegion's settings and returns an IIUnknownVector containing a spatial 
	//          string for each region found.
	// REQUIRE: ipInputText != __nullptr
	//          pDocument != __nullptr
	// PROMISE: Returns an IIUnknownVector containing a spatial string for each region found. If no 
	//          regions are found the IIUnknownVector will be empty. The regions found are determined
	//          by the region boundaries, tag expansion, find type, inside/outside region, 
	//          find each/first matching page per document, and include/exclude intersecting entities.
	// NOTE:    If m_bMatchMultiplePagesPerDocument is true or m_bDataInsideBoundaries is false
	//          the IIUnknownVector will contain at most one region.
	IIUnknownVectorPtr findRegionContent(ISpatialStringPtr ipInputText, IAFDocument* pDocument);

	//---------------------------------------------------------------------------------------------
	// PURPOSE: Get the content of the specified page, bounded by the region.
	// REQUIRE: ipPage must be single-paged. ipPage cannot be NULL.
	// PROMISE: Returns NULL if it is not possible to calculate a rough border position (see
	//          calculateRoughBorderPosition), otherwise returns a spatial string containing the 
	//          desired region of ipPage. 
	//
	//          The result is influenced by the value of these member variables prior to calling:
	//          m_eFindType - the type of spatial string returned
	//          m_mapBoundaryToInfo - defines the region
	//          m_bDataInsideBoundaries - whether to include or exclude the region
	//          m_bIncludeIntersecting - whether to include intersecting entities
	// NOTE:    At most, only one region per page will be found.
	ISpatialStringPtr getPageRegionContent(ISpatialStringPtr ipPage, IRegularExprParserPtr ipParser);

	//---------------------------------------------------------------------------------------------
	// PURPOSE: Returns the first page of ipPages after lStartPage that contains all the clues of 
	//          m_mapIndexToClueListInfo. Stores the bounding rectangle of the clues found.
	// REQUIRE: ipPages' elements implement single-paged spatial strings (ipPages != __nullptr)
	//          0 <= lStartPage < ipPages->Size()
	// PROMISE: Returns the index of the first page of ipPages such that 
	//          lStartPage <= returnValue < ipPages->Size(). If no such page is found, will return -1.
	//          Stores the bounding rectangle for the found clues into clueListInfo.m_ipFoundString.
	long findCluesOnSamePage(IIUnknownVectorPtr ipPages, long lStartIndex,
		IRegularExprParserPtr ipParser);

	// Require : m_mapBorderToPosition is not empty
	// Find pass-in clue list in the ipPageText within boundaries stored in m_mapBorderToPosition
	// Return true if found, false otherwise.
	// The found clue string will be stored in listInfo
	bool findCluesWithinBoundary(ISpatialStringPtr ipPageText, ClueListInfo& rlistInfo,
		IRegularExprParserPtr ipParser);

	// get the expand pixels according to the border
	long getExpandPixels(EBoundary eRegionBound, ISpatialStringPtr ipPageText);

	long getExpandPixels(EBoundary eRegionBound, BoundaryInfo boundInfo, 
					 ISpatialStringPtr ipSpatialString);

	// Based on the m_mapBorderToPosition, expansion, inside/outside region,
	// include/exclude intersecting entities, we finally can get the region 
	// of text that we're looking for
	ISpatialStringPtr getFinalResultString(ISpatialStringPtr ipPageText);

	// Require : m_mapBorderToPosition is not empty
	// Base on m_mapBorderToPosition to get partially/fully defined boundaries, and
	// apply the boundaries on the ipInput to get the string within boundaries
	ISpatialStringPtr getStringWithinBoundary(ISpatialStringPtr ipInput);

	// Based on the m_mapBorderToPosition, expansion, inside/outside region,
	// include/exclude intersecting entities, this function will identify the desired
	// image region and populate it with artificially generated spatial string that
	// encompasses its entire bounds
	ISpatialStringPtr getImageRegion(ISpatialStringPtr ipPageText);

	// This function returns the found region's rectangular bounds
	// it will return a NULL ptr when the bounds are an invalid rectangle
	ILongRectanglePtr getRegionBounds(ISpatialStringPtr ipPageText, 
		ISpatialPageInfoPtr ipPageInfo);

	// Recover the clue list
	// When there is a file name inside the list box as a clue, it will be replaced by the real clues inside this file. 
	// This file name will be covered after processing is finished.
	void recoverClueList(IVariantVectorPtr *ppvecClues);

	//---------------------------------------------------------------------------------------------
	// PURPOSE: Combine the specified vector of image regions into a single spatial string.
	// REQUIRE: vecImageRegions != __nullptr
	// PROMISE: Each spatial string in vecImageRegions will be appended together to form
	//          a single spatial string, which will be returned. If vecImageRegions is empty, an
	//          empty spatial string will be returned. If bstrSourceDocName is not empty, its value
	//          will be assigned to source document name of the resultant string (important for 
	//          document preprocessors [P16 #2729]).
	ISpatialStringPtr combineRegions(IIUnknownVectorPtr ipVecImageRegions, 
		_bstr_t bstrSourceDocName = "");

	void validateLicense();
};