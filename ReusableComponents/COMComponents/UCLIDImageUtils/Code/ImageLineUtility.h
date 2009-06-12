// ImageLineUtility.h : Declaration of the CImageLineUtility

#pragma once
#include "resource.h"       // main symbols

#include "UCLIDImageUtils.h"
#include <FindLines.h>
#include <GroupLines.h>

/////////////////////////////////////////////////////////////////////////////
// CImageLineUtility
/////////////////////////////////////////////////////////////////////////////
class ATL_NO_VTABLE CImageLineUtility :
	public CComObjectRootEx<CComMultiThreadModel>,
	public CComCoClass<CImageLineUtility, &CLSID_ImageLineUtility>,
	public IPersistStream,
	public ISupportErrorInfo,
	public IDispatchImpl<IImageLineUtility, &IID_IImageLineUtility, &LIBID_UCLID_IMAGEUTILSLib>,
	public IDispatchImpl<ILicensedComponent, &IID_ILicensedComponent, &LIBID_UCLID_COMLMLib>,
	public IDispatchImpl<ICopyableObject, &IID_ICopyableObject, &LIBID_UCLID_COMUTILSLib>
{
public:
	CImageLineUtility();
	~CImageLineUtility();

	DECLARE_PROTECT_FINAL_CONSTRUCT()

	HRESULT FinalConstruct();
	void FinalRelease();

DECLARE_REGISTRY_RESOURCEID(IDR_IMAGELINEUTILITY)

BEGIN_COM_MAP(CImageLineUtility)
	COM_INTERFACE_ENTRY(IImageLineUtility)
	COM_INTERFACE_ENTRY2(IDispatch, IImageLineUtility)
	COM_INTERFACE_ENTRY(IPersistStream)
	COM_INTERFACE_ENTRY(ISupportErrorInfo)
	COM_INTERFACE_ENTRY(ILicensedComponent)
	COM_INTERFACE_ENTRY(ICopyableObject)
END_COM_MAP()

// IImageLineUtility
	STDMETHOD (FindLines)(BSTR bstrImageFileName, long nPageNum, double dRotation, 
		IIUnknownVector **ppHorzLineRects, IIUnknownVector **ppVertLineRects);
	STDMETHOD (FindLineRegions)(BSTR bstrImageFileName, long nPageNum, double dRotation, 
		VARIANT_BOOL bHorizontal, IIUnknownVector **ppSubLineRects, IIUnknownVector **ppGroupRects);
	STDMETHOD (FindBoxContainingRect)(ILongRectangle *pRect, IIUnknownVector *pHorzLineRects, 
		IIUnknownVector *pVertLineRects, long nRequiredMatchingBoundaries,
		VARIANT_BOOL *pbIncompleteResult, ILongRectangle **ppBoxRect);
	STDMETHOD(get_LineLengthMin)(long *pVal);
	STDMETHOD(put_LineLengthMin)(long newVal);
	STDMETHOD(get_LineThicknessMax)(long *pVal);
	STDMETHOD(put_LineThicknessMax)(long newVal);
	STDMETHOD(get_LineGapMax)(long *pVal);
	STDMETHOD(put_LineGapMax)(long newVal);
	STDMETHOD(get_LineVarianceMax)(long *pVal);
	STDMETHOD(put_LineVarianceMax)(long newVal);
	STDMETHOD(get_LineWall)(long *pVal);
	STDMETHOD(put_LineWall)(long newVal);
	STDMETHOD(get_LineWallPercentMax)(long *pVal);
	STDMETHOD(put_LineWallPercentMax)(long newVal);
	STDMETHOD(get_LineBridgeGap)(long *pVal);
	STDMETHOD(put_LineBridgeGap)(long newVal);
	STDMETHOD(get_ExtendLineFragments)(VARIANT_BOOL *pVal);
	STDMETHOD(put_ExtendLineFragments)(VARIANT_BOOL newVal);
	STDMETHOD(get_ExtensionScanWidth)(long *pVal);
	STDMETHOD(put_ExtensionScanWidth)(long newVal);
	STDMETHOD(get_ExtensionTelescoping)(long *pVal);
	STDMETHOD(put_ExtensionTelescoping)(long newVal);
	STDMETHOD(get_ExtensionGapAllowance)(long *pVal);
	STDMETHOD(put_ExtensionGapAllowance)(long newVal);
	STDMETHOD(get_ExtensionConsecutiveMin)(long *pVal);
	STDMETHOD(put_ExtensionConsecutiveMin)(long newVal);
	STDMETHOD(get_RowCountMin)(long *pVal);
	STDMETHOD(put_RowCountMin)(long newVal);
	STDMETHOD(get_RowCountMax)(long *pVal);
	STDMETHOD(put_RowCountMax)(long newVal);
	STDMETHOD(get_ColumnCountMin)(long *pVal);
	STDMETHOD(put_ColumnCountMin)(long newVal);
	STDMETHOD(get_ColumnCountMax)(long *pVal);
	STDMETHOD(put_ColumnCountMax)(long newVal);
	STDMETHOD(get_ColumnWidthMin)(long *pVal);
	STDMETHOD(put_ColumnWidthMin)(long newVal);
	STDMETHOD(get_ColumnWidthMax)(long *pVal);
	STDMETHOD(put_ColumnWidthMax)(long newVal);
	STDMETHOD(get_OverallWidthMin)(long *pVal);
	STDMETHOD(put_OverallWidthMin)(long newVal);
	STDMETHOD(get_OverallWidthMax)(long *pVal);
	STDMETHOD(put_OverallWidthMax)(long newVal);
	STDMETHOD(get_LineSpacingMin)(long *pVal);
	STDMETHOD(put_LineSpacingMin)(long newVal);
	STDMETHOD(get_LineSpacingMax)(long *pVal);
	STDMETHOD(put_LineSpacingMax)(long newVal);
	STDMETHOD(get_ColumnSpacingMax)(long *pVal);
	STDMETHOD(put_ColumnSpacingMax)(long newVal);
	STDMETHOD(get_AlignmentScoreMin)(long *pVal);
	STDMETHOD(put_AlignmentScoreMin)(long newVal);
	STDMETHOD(get_AlignmentScoreExact)(long *pVal);
	STDMETHOD(put_AlignmentScoreExact)(long newVal);
	STDMETHOD(get_SpacingScoreMin)(long *pVal);
	STDMETHOD(put_SpacingScoreMin)(long newVal);
	STDMETHOD(get_SpacingScoreExact)(long *pVal);
	STDMETHOD(put_SpacingScoreExact)(long newVal);
		
// ISupportsErrorInfo
	STDMETHOD(InterfaceSupportsErrorInfo)(REFIID riid);

// ICopyableObject
	STDMETHOD(raw_Clone)(IUnknown* *pObject);
	STDMETHOD(raw_CopyFrom)(IUnknown *pObject);

// IPersistStream
	STDMETHOD(GetClassID)(CLSID *pClassID);
	STDMETHOD(IsDirty)(void);
	STDMETHOD(Load)(IStream *pStm);
	STDMETHOD(Save)(IStream *pStm, BOOL fClearDirty);
	STDMETHOD(GetSizeMax)(ULARGE_INTEGER *pcbSize);

// ILicensedComponent
	STDMETHOD(raw_IsLicensed)(VARIANT_BOOL * pbValue);

private:
	/////////////////
	// Variables
	/////////////////

	// These variables store distances as a percentage of page width;
	long m_nColumnWidthMin;
	long m_nColumnWidthMax;
	long m_nOverallWidthMin;
	long m_nOverallWidthMax;
	long m_nLineSpacingMin;
	long m_nLineSpacingMax;
	long m_nColumnSpacingMax;

	// Bridge gap varible specified in thousandths of an inch
	long m_nBridgeGapSmallerThan;

	// LineFinder and LineGrouper accept distances in thousandths of an inch
	// or in pixels depending on the setting, so these need to be converted 
	// accoring on the DPI of the target image before processing

	LeadToolsLineFinder m_LineFinder;
	LeadToolsLineGroup m_LineGrouper;

	bool m_bDirty;

	/////////////////
	// Methods
	/////////////////

	// Finds horizontal and/or vertical lines on the specified page of the specified document.
	// pHorzLineRects and/or pVertLineRects will be populated if non-NULL. (Either can
	// be NULL if not needed).  Rotates image dRotation degrees clockwise prior to searching
	// for lines.  If prectPageBounds is provided, it will be populated with the bounds
	// of the page that was searched.
	void findLines(string strImageFileName, long nPageNum, double dRotation, 
		vector<LineRect> *pHorzLineRects, vector<LineRect> *pVertLineRects,
		CRect *prectPageBounds = NULL);

	// Converts a vector<LineRect> to an IIUnknownVector of ILongRectangles
	IIUnknownVectorPtr lineRectVecToILongRectangleVec(const vector<LineRect> &vecRects);

	// Converts an IIUnknownVector of ILongRectangles to a vector<LineRect>
	void longRectangleVecToLineRectVec(IIUnknownVectorPtr ipRects, bool bHorizontal, 
		vector<LineRect> &rvecRects);

	// This function will apply percentage of page settings to m_LineFinder and m_LineGrouper 
	// based on the DPI and size of the provided FILEINFO
	void adjustSettingsForImage(const FILEINFO &fileinfo, bool bHorizontal);

	void validateLicense();
};

OBJECT_ENTRY_AUTO(__uuidof(ImageLineUtility), CImageLineUtility)