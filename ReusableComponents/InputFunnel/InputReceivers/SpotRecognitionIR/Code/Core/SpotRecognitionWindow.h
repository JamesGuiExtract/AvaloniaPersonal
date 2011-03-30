// SpotRecognitionWindow.h : Declaration of the CSpotRecognitionWindow
//
// NOTE:	Use CoInitializeEx(NULL, COINIT_APARTMENTTHREADED);
//			not CoInitializeEx(NULL, COINIT_MULTITHREADED);
//			This app uses the Spot Recognition Window that uses an OCX
//			that will not work with the multithreaded option
//			It causes the following error: 
//			Debug Assertion Failed!
//			Program: YourProgram.exe
//			File: f:\sp\vctools\vc7libs\ship\atlmfc\src\mfc\occcont.cpp
//			Line: 926

#pragma once

#include "resource.h"       // main symbols
#include <memory>
#include <vector>

using namespace std;

// forward declarations
class SpotRecognitionDlg;

/////////////////////////////////////////////////////////////////////////////
// CSpotRecognitionWindow
class ATL_NO_VTABLE CSpotRecognitionWindow : 
	public CComObjectRootEx<CComMultiThreadModel>,
	public CComCoClass<CSpotRecognitionWindow, &CLSID_SpotRecognitionWindow>,
	public ISupportErrorInfo,
	public IDispatchImpl<ISpotRecognitionWindow, &IID_ISpotRecognitionWindow, &LIBID_UCLID_SPOTRECOGNITIONIRLib>,
	public IDispatchImpl<IInputEntityManager, &IID_IInputEntityManager, &LIBID_UCLID_INPUTFUNNELLib>,
	public IDispatchImpl<IInputReceiver, &IID_IInputReceiver, &LIBID_UCLID_INPUTFUNNELLib>,
	public IDispatchImpl<ICategorizedComponent, &IID_ICategorizedComponent, &LIBID_UCLID_COMUTILSLib>,
	public IDispatchImpl<ILicensedComponent, &IID_ILicensedComponent, &LIBID_UCLID_COMLMLib>
{
public:
	CSpotRecognitionWindow();
	~CSpotRecognitionWindow();

DECLARE_REGISTRY_RESOURCEID(IDR_SPOTRECOGNITIONWINDOW)

DECLARE_PROTECT_FINAL_CONSTRUCT()

BEGIN_COM_MAP(CSpotRecognitionWindow)
	COM_INTERFACE_ENTRY(ISpotRecognitionWindow)
	COM_INTERFACE_ENTRY(ISupportErrorInfo)
	COM_INTERFACE_ENTRY2(IDispatch, ISpotRecognitionWindow)
	COM_INTERFACE_ENTRY(IInputEntityManager)
	COM_INTERFACE_ENTRY(IInputReceiver)
	COM_INTERFACE_ENTRY(ICategorizedComponent)
	COM_INTERFACE_ENTRY(ILicensedComponent)
END_COM_MAP()

public:
	// ISupportsErrorInfo
	STDMETHOD(InterfaceSupportsErrorInfo)(REFIID riid);

	// ISpotRecognitionWindow
	STDMETHOD(OpenImageFile)(/*[in]*/ BSTR strImageFileFullPath);
	STDMETHOD(OpenGDDFile)(/*[in]*/ BSTR strGDDFileFullPath);
	STDMETHOD(Save)();
	STDMETHOD(SaveAs)(/*[in]*/ BSTR strFileFullPath);
	STDMETHOD(Clear)();
	STDMETHOD(IsModified)(/*[out, retval]*/ VARIANT_BOOL *pbIsModified);
	STDMETHOD(GetCurrentPageNumber)(/*[out, retval]*/ long *plPageNum);
	STDMETHOD(SetCurrentPageNumber)(/*[in]*/ long lPageNumber);
	STDMETHOD(GetTotalPages)(/*[out, retval]*/ long *plTotalPages);
	STDMETHOD(GetImageFileName)(/*[out, retval]*/ BSTR *pstrImageFileName);
	STDMETHOD(GetGDDFileName)(/*[out, retval]*/ BSTR *pstrGDDFileName);
	STDMETHOD(SetParagraphTextCorrector)(/*[in]*/ IParagraphTextCorrector *pParagraphTextCorrector);
	STDMETHOD(SetLineTextEvaluator)(/*[in]*/ ILineTextEvaluator *pLineTextEvaluator);
	STDMETHOD(SetLineTextCorrector)(/*[in]*/ ILineTextCorrector *pLineTextCorrector);
	STDMETHOD(SetParagraphTextHandlers)(/*[in]*/ IIUnknownVector *pHandlers);
	STDMETHOD(SetSRWEventHandler)(/*([in]*/ ISRWEventHandler *pHandler);
	STDMETHOD(SetSubImageHandler)(/*[in]*/ ISubImageHandler *pSubImageHandler, /*[in]*/BSTR strToolbarBtnTooltip, /*[in]*/BSTR strTrainingFile);
	STDMETHOD(get_AlwaysAllowHighlighting)(/*[out, retval]*/ VARIANT_BOOL *pVal);
	STDMETHOD(put_AlwaysAllowHighlighting)(/*[in]*/ VARIANT_BOOL newVal);
	STDMETHOD(ShowOpenDialogBox)();
	STDMETHOD(OpenImagePortion)(/*[in]*/ BSTR strOriginalImageFileName, 
		/*[in]*/ IRasterZone* pImagePortionInfo, /*[in]*/ double dRotationAngle);
	STDMETHOD(GetSubImageHandler)(/*[in, out]*/ ISubImageHandler **ppSubImageHandler, /*[in, out]*/BSTR *pstrToolbarBtnTooltip, /*[in, out]*/BSTR *pstrTrainingFile);
	STDMETHOD(GetSRWEventHandler)(/*[out, retval]*/ ISRWEventHandler **ppHandler);
	STDMETHOD(GetParagraphTextHandlers)(/*[in, out]*/ IIUnknownVector **ppHandlers);
	STDMETHOD(GetParagraphTextCorrector)(/*[out, retval]*/ IParagraphTextCorrector **ppParagraphTextCorrector);
	STDMETHOD(GetLineTextEvaluator)(/*[out, retval]*/ ILineTextEvaluator **ppLineTextEvaluator);
	STDMETHOD(GetLineTextCorrector)(/*[out, retval]*/ ILineTextCorrector **ppLineTextCorrector);
	STDMETHOD(GetImagePortion)(/*[out, retval]*/ IRasterZone **pImagePortion);
	STDMETHOD(IsImagePortionOpened)(/*[out, retval]*/ VARIANT_BOOL *pbValue);
	STDMETHOD(ClearParagraphTextHandlers)();
	STDMETHOD(OCRCurrentPage)(/*[out, retval]*/ ISpatialString** ppSpatialString);
	STDMETHOD(CreateZoneEntity)(/*[in]*/ IRasterZone *pZone, /*[in]*/ long nColor, /*[out, retval]*/ long *pID);
	STDMETHOD(DeleteZoneEntity)(/*[in]*/ long nID);
	STDMETHOD(ZoomAroundZoneEntity)(/*[in]*/ long nID);
	STDMETHOD(CreateTemporaryHighlight)(/*[in]*/ ISpatialString *pText);
	STDMETHOD(DeleteTemporaryHighlight)();
	STDMETHOD(GetGenericDisplayOCX)(/*[out, retval]*/ IDispatch **pOCX);
	STDMETHOD(NotifyKeyPressed)(/*[in]*/ long nKeyCode);
	STDMETHOD(ShowToolbarCtrl)(/*[in]*/ ESRIRToolbarCtrl eCtrl, /*[in]*/ VARIANT_BOOL bShow);
	STDMETHOD(ShowTitleBar)(/*[in]*/ VARIANT_BOOL bShow);
	STDMETHOD(ZoomPointWidth)(/*[in]*/ long nX, /*[in]*/ long nY, /*[in]*/ long nWidth);
	STDMETHOD(EnableAutoOCR)(/*[in]*/ VARIANT_BOOL bEnable);
	STDMETHOD(get_WindowPos)(/*[out, retval]*/ ILongRectangle **ppVal);
	STDMETHOD(put_WindowPos)(/*[in]*/ ILongRectangle* pNewVal);
	STDMETHOD(LoadOptionsFromFile)(/*[in]*/ BSTR bstrFileName);
	STDMETHOD(SetCurrentTool)(/*[in]*/ ESRIRToolbarCtrl eCtrl);
	STDMETHOD(IsOCRLicensed)(/*[out, retval]*/ VARIANT_BOOL *pbLicensed);
	STDMETHOD(GetCurrentTool)(/*[out, retval]*/ ESRIRToolbarCtrl *peCtrl);
	STDMETHOD(get_HighlightsAdjustableEnabled)(/*[out, retval]*/ VARIANT_BOOL* pvbEnable);
	STDMETHOD(put_HighlightsAdjustableEnabled)(/*[in]*/ VARIANT_BOOL vbEnable);
	STDMETHOD(get_FittingMode)(/*[out, retval]*/ long* peFittingMode);
	STDMETHOD(put_FittingMode)(/*[in]*/ long eFittingMode);

	// IInputEntityManager
	STDMETHOD(raw_CanBeDeleted)(BSTR strID, VARIANT_BOOL *bCanBeDeleted);
	STDMETHOD(raw_Delete)(BSTR strID);
	STDMETHOD(raw_SetText)(BSTR strID, BSTR strText);
	STDMETHOD(raw_GetText)(BSTR strID, BSTR * pstrText);
	STDMETHOD(raw_CanBeMarkedAsUsed)(BSTR strID, VARIANT_BOOL * pbCanBeMarkedAsUsed);
	STDMETHOD(raw_MarkAsUsed)(BSTR strID, VARIANT_BOOL bValue);
	STDMETHOD(raw_IsMarkedAsUsed)(BSTR strID, VARIANT_BOOL * pbIsMarkedAsUsed);
	STDMETHOD(raw_IsFromPersistentSource)(BSTR strID, VARIANT_BOOL * pbIsFromPersistentSource);
	STDMETHOD(raw_GetPersistentSourceName)(BSTR strID, BSTR * pstrSourceName);
	STDMETHOD(raw_HasBeenOCRed)(BSTR strID, VARIANT_BOOL * pbHasBeenOCRed);
	STDMETHOD(raw_GetOCRImage)(BSTR strID, BSTR* pbstrImageFileName);
	STDMETHOD(raw_HasIndirectSource)(BSTR strID, VARIANT_BOOL *pbHasIndirectSource);
	STDMETHOD(raw_GetIndirectSource)(BSTR strID, BSTR *pstrIndirectSourceName);
	STDMETHOD(raw_GetOCRZones)(BSTR strID, IIUnknownVector **pRasterZones);

	// IInputReceiver
	STDMETHOD(raw_get_ParentWndHandle)(/*[out, retval]*/ long *pVal);
	STDMETHOD(raw_put_ParentWndHandle)(/*[in]*/ long newVal);
	STDMETHOD(raw_get_WindowShown)(VARIANT_BOOL * pVal);
	STDMETHOD(raw_get_InputIsEnabled)(VARIANT_BOOL * pVal);
	STDMETHOD(raw_get_HasWindow)(VARIANT_BOOL * pVal);
	STDMETHOD(raw_get_WindowHandle)(LONG * pVal);
	STDMETHOD(raw_EnableInput)(BSTR strInputType, BSTR strPrompt);
	STDMETHOD(raw_DisableInput)();
	STDMETHOD(raw_SetEventHandler)(IIREventHandler * pEventHandler);
	STDMETHOD(raw_ShowWindow)(VARIANT_BOOL bShow);
	STDMETHOD(raw_get_UsesOCR)(VARIANT_BOOL *pVal);
	STDMETHOD(raw_SetOCRFilter)(IOCRFilter *pFilter);
	STDMETHOD(raw_SetOCREngine)(IOCREngine *pEngine);

	// ICategorizedComponent
	STDMETHOD(raw_GetComponentDescription)(BSTR * pbstrComponentDescription);

	// ILicensedComponent
	STDMETHOD(raw_IsLicensed)(VARIANT_BOOL * pbValue);

private:

	// Gets a vector of zone entity ids from the specified input entity id. Clears the vector first.
	void getIDs(BSTR bstrID, vector<long> &rvecIDs);
	SpotRecognitionDlg* getSpotRecognitionDlg();
	std::unique_ptr<SpotRecognitionDlg> m_apDlg;

	// Check general license state and OCR_ON_CLIENT license state
	void validateLicense();
	void validateOCRLicense();

	// Check for OCR_ON_CLIENT license - required to OCR text within SRW
	bool isOCRLicensed();

	long m_lParentWndHandle;
};
