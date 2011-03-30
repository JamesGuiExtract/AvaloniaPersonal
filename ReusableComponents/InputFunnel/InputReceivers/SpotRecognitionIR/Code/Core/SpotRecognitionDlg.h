#pragma once

//{{AFX_INCLUDES()
#include "uclidgenericdisplay.h"
//}}AFX_INCLUDES

#include "Resource.h"
#include "ConfigMgrSRIR.h"
#include "ETool.h"
#include "PageExtents.h"
#include "SpotRecognitionIR.h"
#include "ZoomViewsManager.h"
#include "SpotRecCfgFileReader.h"

#include <BCMenu.h>

#include <memory>
#include <map>
#include <string>
#include <vector>

using namespace std;

// forward declarations
class DragOperation;
class SpotRecDlgToolBar;
class MRUList;
class GDDFileManager;
class CursorToolTipCtrl;

class SpotRecognitionDlg : public CDialog
{
public:
	// Construction & destruction
	SpotRecognitionDlg(IInputEntityManager *pInputEntityManager, bool bOCRIsLicensed, 
		CWnd* pParent = NULL);
	~SpotRecognitionDlg();

	void createModeless(CWnd *pParent = NULL);
	// the container COM object sends its event handler to this
	// dialog object, as this dialog object is the one that will
	// be sending out the events.
	void setEventHandler(IIREventHandler* ipEventHandler);
	void setLineTextEvaluator(ILineTextEvaluator *pLineTextEvaluator);
	void setLineTextCorrector(ILineTextCorrector *pLineTextCorrector);
	void setParagraphTextCorrector(IParagraphTextCorrector *pParagraphTextCorrector);
	void setParagraphTextHandlers(IIUnknownVector *pParagraphTextHandlers);
	void clearParagraphTextHandlers();
	void setSRWEventHandler(ISRWEventHandler *pHandler);
	void setSubImageHandler(ISubImageHandler *pHandler, const CString& zTooltip, const CString& zTrainingFile);
	void getLineTextEvaluator(UCLID_SPOTRECOGNITIONIRLib::ILineTextEvaluator **pLineTextEvaluator);
	void getLineTextCorrector(UCLID_SPOTRECOGNITIONIRLib::ILineTextCorrector **pLineTextCorrector);
	void getParagraphTextCorrector(UCLID_SPOTRECOGNITIONIRLib::IParagraphTextCorrector **pParagraphTextCorrector);
	void getParagraphTextHandlers(IIUnknownVector **pParagraphTextHandlers);
	void getSRWEventHandler(UCLID_SPOTRECOGNITIONIRLib::ISRWEventHandler **pHandler);
	void getSubImageHandler(UCLID_SPOTRECOGNITIONIRLib::ISubImageHandler **pHandler, BSTR *pstrToolbarBtnTooltip, BSTR *pstrTrainingFile);
	// whether or not a portion of a image is currently opened in the dialog
	bool isImagePortionOpened() {return m_bIsCurrentImageAnImagePortion;}
	void getImagePortion(IRasterZone **pRasterZone);

	//---------------------------------------------------------------------------------------------
	// PURPOSE: If true, allows the highlight tool to move and resize highlights;
	//          If false, the highlight tool cannot move or resize highlights.
	void enableHighlightsAdjustable(bool bEnable);
	bool isHighlightsAdjustableEnabled();

	//---------------------------------------------------------------------------------------------
	// PURPOSE: Gets or sets the current auto fitting mode.
	long getFittingMode();
	void setFittingMode(long eFittingMode);

	// the following are methods for the owner to call
	void clear();
	void enableInput(BSTR bstrInputType, BSTR bstrPrompt);
	void setOCRFilter(IOCRFilter *pFilter);
	void setOCREngine(IOCREngine *pEngine);
	bool inputIsEnabled() const;
	void disableInput();
	bool isModified();
	long getCurrentPageNumber();
	void setCurrentPageNumber(long lPageNumber);
	long getTotalPages();
	void openFile(const std::string& strFileName);
	std::string getImageFileName();
	std::string getGDDFileName();
	// if there's a zone created, create the zone image file, and give back the file name
	std::string getCurrentZoneImageFileName();
	void save();
	void saveAs(const std::string& strFileName);
	std::string getZoneEntityText(long lEntityID);
	void setZoneEntityText(long lEntityID, const std::string& strNewText);
	bool isMarkedAsUsed(long lEntityID);
	void markAsUsed(long lEntityID, bool bMarkAsUsed);
	IRasterZonePtr getOCRZone(long lID);
	bool isAlwaysAllowHighlighting() {return m_bAlwaysAllowHighlighting;}
	void setAlwaysAllowHighlighting(bool bAllow) {m_bAlwaysAllowHighlighting = bAllow;}
	// whether or not to show the open dialog box once the spot rec dialog is opened.
	void showOpenDialog();
	// open sub image file in the spot rec dlg. 
	// Require: the save button shall be disabled, the title bar shall display the 
	//			original image file name along with the indication that this is
	//			is a portion of the original image
	void openImagePortion(const std::string& strOriginalImageFileName, 
		IRasterZone *pImagePortionInfo, double dRotationAngle);
	//---------------------------------------------------------------------------------------------
	// PURPOSE: Gets the currently activated tool.
	ETool getCurrentTool();
	// use the following method to initialize one of tools in the toolbar
	void setCurrentTool(ETool eTool);
	// Returns a spatial string for the current page
	ISpatialStringPtr getCurrentPageText();
	
	// Use settings relevant to paragraph text recognition, then
	// OCR the given image, and then correct the recognized text with the
	// paragraph text corrector (if any).  The recognized spatial text is 
	// returned.
	ISpatialStringPtr recognizeParagraphTextInImage(
		const std::string& strImageFileName, long lStartPage, long lEndPage, LPRECT pRect = NULL);
    //---------------------------------------------------------------------------------------------
	// PURPOSE: Send the recognized spatial text to the current paragraph text handler nOffsetX 
	// and nOffsetY represent the offset that should be applied to the spatial string coming out 
	// of the OCR engine
	// PARAMS:  (1) strImageFileToOCR - the name of the image to OCR.
	//			(2) lStartPage - the first page to OCR.
	//          (3) lEndPage - the last page to OCR, or -1 to use the last page of the document.
	//              Pages from lStartPage to lEndPage inclusive will be OCRed.
	//          (4) bSetCurrentPageNumber - whether the page number attribute of the OCR'ed 
	//              spatial string must be altered. If bSetCurrentPageNumber == true, then the 
	//				page number attributes of the OCRed letter objects are set to the current page 
	//              number. It is appropriate to specify bSetCurrentPageNumber = true for instance 
	//				when a zone has been extracted as a separate image and must be processed 
	//				(without losing information about the original page number from where the zone 
	//				was extracted).
	//			(5) nOffsetX - the horizontal offset that should be applied to the spatial 
	//				positions retrieved from the OCR engine.
	//          (6) nOffsetY - the vertical offset that should be applied to the spatial positions 
	//              retrieved from the OCR engine.
	//			(7) pRect - The zone to OCR
	void processImageForParagraphText(const std::string& strImageFileToOCR,	long lStartPage, 
		long lEndPage, bool bSetCurrentPageNumber = false, long nOffsetX = 0, long nOffsetY = 0, 
		LPRECT pRect = NULL);

	// create new zone entity as specified and return its ID
	long createZoneEntity(IRasterZone *pZone, long nColor);

	// delete the specified zone entity
	void deleteZoneEntity(long nID);

	// zoom around the specified zone entity
	void zoomAroundZoneEntity(long nID);

	// First make sure the same image can't be opened in two different window,
	// then open the image or gdd file.
	void openFile2(const std::string& strFileToOpen);

	// Temporarily highlight the spatial string reprented by pText.
	// If there exists a temporary highlight in this window
	// at the time of this method call, that temporary highlight
	// will automatically be deleted (i.e. this window ensures that
	// at any given time, there is no more than 1 temporary highlight)
	// PROMISE: The following two methods will NOT modify the "modified" 
	//			state of the opened document
	void createTemporaryHighlight(ISpatialString *pText);
	void addTemporaryHighlight(long nStartX, long nStartY, long nEndX, long nEndY, long nHeight, long nPage);
	void deleteTemporaryHighlight();

	void zoomToTemporaryHighlight();
	void centerOnTemporaryHighlight();

	// When a key is pressed this method can be called to take the
	// appropriate action based on the defined shorcut keys
	bool handleShortCutKeys(long nKeyCode);

	// handles reloading the image if the user presses F5 to refresh
	void OnRefreshImage();

	void showToolbarCtrl(ESRIRToolbarCtrl eCtrl, bool bShow);
	void showTitleBar(bool bShow);

	void zoomPointWidth(long nX, long nY, long nWidth);

	void enableAutoOCR(bool bEnable);

	void loadOptionsFromFile(const std::string& strFile);

	void zoomIn();
	void zoomOut();
	void zoomExtents();
	void zoomFitToWidth();

// Dialog Data
	//{{AFX_DATA(SpotRecognitionDlg)
	enum { IDD = IDD_SPOT_RECOGNITION_DLG };
	CUCLIDGenericDisplay	m_UCLIDGenericDisplayCtrl;
	//}}AFX_DATA
	BOOL OnToolTipNotify(UINT id, NMHDR *pNMHDR, LRESULT *pResult);

// Overrides
	// ClassWizard generated virtual function overrides
	//{{AFX_VIRTUAL(SpotRecognitionDlg)
	public:
	protected:
	virtual void DoDataExchange(CDataExchange* pDX);    // DDX/DDV support
	//}}AFX_VIRTUAL

// Implementation
protected:
	// Generated message map functions
	//{{AFX_MSG(SpotRecognitionDlg)
	virtual BOOL OnInitDialog();
	afx_msg void OnSize(UINT nType, int cx, int cy);
	afx_msg void OnBTNOpenImage();
	afx_msg void OnBTNSave();
	afx_msg void OnBTNPan();
	afx_msg void OnBTNFitPage();
	afx_msg void OnBTNFitWidth();
	afx_msg void OnBTNZoomIn();
	afx_msg void OnBTNZoomOut();
	afx_msg void OnBTNZoomWindow();
	afx_msg void OnMouseMoveGenericDisplayCtrl(short Button, short Shift, long x, long y);
	afx_msg void OnMouseDownGenericDisplayCtrl(short Button, short Shift, long x, long y);
	afx_msg void OnMouseUpGenericDisplayCtrl(short Button, short Shift, long x, long y);
	afx_msg void OnBTNFirstPage();
	afx_msg void OnBTNLastPage();
	afx_msg void OnBTNNextPage();
	afx_msg void OnBTNPreviousPage();
	afx_msg void OnChangeGoToPageText();
	afx_msg void OnKillFocusGoToPageText();
	afx_msg void OnBTNEditZoneText();
	afx_msg void OnBTNDeleteEntities();
	afx_msg void OnBtnSelectHighlight();
	afx_msg void OnBTNSelectText();
	afx_msg void OnBTNSetHighlightHeight();
	afx_msg void OnBTNRecognizeTextAndProcess();
	afx_msg void OnEntitySelectedGenericDisplayCtrl(long ulEntityID);
	afx_msg void OnZoneEntityMovedGenericDisplayCtrl(long ulEntityID);
	afx_msg void OnZoneEntitiesCreatedGenericDisplayCtrl(IUnknown* pZoneIDs);
	afx_msg void OnToolbarDropDown(NMHDR* pNMHDR, LRESULT *plr);
	afx_msg void OnClose();
	afx_msg void OnBTNZoomPrev();
	afx_msg void OnBTNZoomNext();
	afx_msg void OnBTNRotateLeft();
	afx_msg void OnBTNRotateRight();
	afx_msg void OnContextMenu(CWnd* pWnd, CPoint point);
	afx_msg void OnMnuHighlighter();
	afx_msg void OnMnuPan();
	afx_msg void OnMnuZoomwindow();
	afx_msg void OnMnuCancel();
	afx_msg void OnBtnOpenSubImage();
	afx_msg void OnDblClickGenericdisplayctrl();
	afx_msg void OnDestroy();
	afx_msg void OnKeyDownGenericdisplayctrl(short FAR* KeyCode, short Shift);
	afx_msg void OnMenuRectSelectionTool();
	afx_msg void OnMenuSwipeSelectionTool();
	afx_msg void OnBTNPrint();
	afx_msg void OnDropFiles( HDROP hDropInfo );
	DECLARE_EVENTSINK_MAP()
	//}}AFX_MSG
	afx_msg void OnPTHMenuItemSelected(UINT nID);
	afx_msg void OnSelectMRUPopupMenu(UINT nID);
	DECLARE_MESSAGE_MAP()

protected:
	enum ERotationAngle {kNoRotation = 0, kRotateLeft, kRotateRight};
	enum EOCRRegionType {kOCREntireImage=0, kOCRCurrentPage, kOCRRectRegion, kOCRPolyRegion};
	// the following are references to the input entity manager (which is the COM
	// object which owns this dialog), and the event handler, to which all zone-selected
	// and zone-highlighted events are to be sent.
	IInputEntityManager *m_pInputEntityManager;
	IIREventHandler* m_ipEventHandler;
	UCLID_SPOTRECOGNITIONIRLib::ILineTextEvaluatorPtr m_ipLineTextEvaluator;
	UCLID_SPOTRECOGNITIONIRLib::ILineTextCorrectorPtr m_ipLineTextCorrector;
	UCLID_SPOTRECOGNITIONIRLib::IParagraphTextCorrectorPtr m_ipParagraphTextCorrector;
	IIUnknownVectorPtr m_ipParagraphTextHandlers;
	UCLID_SPOTRECOGNITIONIRLib::ISRWEventHandlerPtr m_ipSRWEventHandler;
	UCLID_SPOTRECOGNITIONIRLib::ISubImageHandlerPtr m_ipSubImageHandler;

	// the prompt to be shown to the user when they are selecting text
	std::string m_strSelectPrompt;
	std::string m_strInputType;
	IOCRFilterPtr m_ipOCRFilter;


	// perform OCR on the entire image and send to current PTH
	void performOCROnEntireImage();

	// perform OCR on the current page and send to current PTH
	void performOCROnCurrentPage();

	// the last cursor position at which the PTH menu was brought up
	POINT m_lastPTHMenuPos;

	const ETool DEFAULT_TOOL;

	// m_eCurrentTool is used to keep track of the currently active tool
	ETool m_eCurrentTool;
	ETool m_ePreviousTool;

	HICON m_hIcon;						// icon for the dialog

	std::unique_ptr<SpotRecDlgToolBar> m_apToolBar; // toolbar has buttons and a edit box in it

	bool m_bEnableTextSelection;		// whether or not text may selected from the image
	bool m_bInitialized;				// whether or not the dialog has been initialized
	std::string m_strLastGoToPageText;

	long m_nLastSelectedPTHIndex;

	// whether or not always allow highlighting even if the
	// input is disabled.
	bool m_bAlwaysAllowHighlighting;

	// all drag operations (such as zoom-window, pan, etc) are implemented via drag operation
	// objects.  m_pCurrentDragOperation points to the current drag operation, if any.  If no
	// drag operation is currently in progress, then this variable should be set to NULL. Only
	// one drag operation can be in progress at any given time.  To initialize a new drag 
	// operation, use the initDragOperation() method
	std::unique_ptr<DragOperation> m_apCurrentDragOperation;

	// to recognize a given image file and retrieve the output string
	IOCREnginePtr m_ipOCREngine;
	// temp zone for multiple uses.
	IRasterZonePtr m_ipTempRasterZone;

	// the sub image file to be opened in the spot rec dlg
	std::string m_strSubImageFileName;
	// the original source that created the sub image file
	std::string m_strOriginalImageFileName;
	// file to be deleted before destruction
	std::string m_strFileToBeDeleted;
	// the offset x position from the origin inside original source in image pixels
	// This is always the upper left corner (even though image might rotate to a
	// different angle, this position might not be the upper left corner, however, 
	// its image pixel coordinates will not change. i.e. it's always the smallest 
	// coordinates among four corners of the rectangular.
	IRasterZonePtr m_ipSubImageZone;

	// whether or not current opened image is a portion of a image file, i.e. it's
	// a temporary image.
	bool m_bIsCurrentImageAnImagePortion;

	// If this flag is false the saving of gdd files is not allowed
	// that means there will be no prompting for saves when the program is closed
	// This flag is controlled by the showing\hiding of the Save button
	// This flag currently defaults to true because the button defaults to 
	// visible but a better job could be done of tying these defaults
	// together
	// Note that saving initiated by calls to save() and saveAs() are still valid
	// so programatic saves are allowed but not user saves
	bool m_bUserSavingAllowed;

	// Current fit-to status
	ESRIRFitToStatus m_eFitToStatus;

	//**************************************************************
	// Helper functions

	void removeCommasFromTextAttributes();

	void OnOK();		// overridden from base class
	void OnCancel();	// overridden from base class

	// the following method shows the popup menu that appears when the
	// "Recognize text and process" button is clicked.
	void showPTHMenu(POINT menuPos);

	// The following method uses various boolean attributes and configures the buttons
	// and the UGD to be in the correct state depending upon what the currently selected
	// tool is.
	void configureToolBarButtonsAndUGD();
	void configureZoomPrevNextButtons();
	void positionUGDControl();
	void createToolBar();
	void resetGoToPageText();
	void gotoPage(long lPageNum);
	void updateWindowTitle(const std::string& strFileName);

	long getTextScore(const std::string& strText);
	std::string getZoneText(IRasterZonePtr& ripZone, IProgressStatus* pProgressStatus);
	std::string getBestTextAroundZone(IRasterZonePtr& ripZone, IProgressStatus* pProgressStatus);

	// the initDragOperation() method is to be called when initializing new drag operations.
	// this method will automatically delete the current drag operation object, if any.
	void initDragOperation(DragOperation *pNewDragOperation);
	void releaseCurrentDragOperationMemory();

	void fireOnInputReceived(const vector<unsigned long> &vecZoneIDs);

	// updates current cursor handle
	void updateCursorHandle(ETool eTool);

	static COLORREF ms_USED_ENTITY_COLOR;
	static COLORREF ms_UNUSED_ENTITY_COLOR;

	// the following members associated with the saving and loading of 
	// window positions from the persistent store
	void loadWindowPosition();
	void saveWindowPosition();

	// following members attributes and methods are related to 
	// managing multiple instances of this dialog so that window
	// positions can be correctly restored, etc.
	static std::vector<SpotRecognitionDlg *> ms_vecInstances;
	SpotRecognitionDlg* getDlgInstanceWithImage(const std::string& strPathName);

	// rotate current page to either 90° left or right or no rotation
	void rotateCurrentPage(ERotationAngle eRotationAngle);

private:
	struct FlagedViewExtents
	{
		PageExtents theView;
		// indicates whether or not this view is the view
		// right before rotation happened.
		bool bIsRightBeforeRotation;
	};

	// make the GDDFileManager as a friend class to be able to access
	// all member methods and variables
	friend class GDDFileManager;
	std::unique_ptr<GDDFileManager> ma_pGDDFileManager;

	std::unique_ptr<ConfigMgrSRIR> ma_pSRIRCfgMgr;

	std::string m_strLastZoneImageFile;

	// config mgr to hold root key at 
	//"Software\\UCLID Software\\InputReceivers"
	std::unique_ptr<IConfigurationSettingsPersistenceMgr> ma_pUserCfgMgr;

	std::unique_ptr<MRUList> ma_pRecentFiles;

	// the training file (if any) to use for the RecognizeTextInWindow operation
	CString m_zTrainingFile;

	// Each page shall have its own stack of zoom window view extents
	// to be used in Zoom Prev and Zoom Next
	// Note that it shall be 1-based since there's no page #0.
	std::map<unsigned long, ZoomViewsManager> m_mapPageToViews;

	// The first one is the current view, the second one 
	// is the flag to indicate whether this
	// view is the view right before rotation occurs.
	// Note that this variable always stores the current view. It's
	// page independent, i.e. it's the current view of the current page.
	FlagedViewExtents m_CurrentView;

	// the CursorToolTipCtrl is used to display the most recently recognized text
	// as a visual feedback at the current mouse cursor position
	std::unique_ptr<CursorToolTipCtrl> m_apCursorToolTipCtrl;
	unsigned long m_ulLastCreatedEntity;

	// will be used for all instances
	CString m_zOpenImagePortionToolTip;

	// This is a number that is the result of ID_OCR_ENTIRE_IMAGE_02 - ID_OCR_ENTIRE_IMAGE_01
	int m_nOCROptionsSpanLen;

	BCMenu m_menuSelectionTools;
	ETool m_eCurrSelectionTool;

	// when this is false a swipe will not OCR the region within a swipe 
	// (or rubberband) nor fire an event for the input manager (as there 
	// is no text)
	// It will, however, still send the zone created notification to the 
	// SRWEventHandler
	// The flag basically controls auto OCR in the spot recognition window
	bool m_bAutoOCR;

	// whether the user can open new files (true) or file opening is disabled (false)
	bool m_bCanOpenFiles;

	SpotRecCfgFileReader m_cfgFileReader;

	//*******************
	// Helper functions
	//*******************

	// bRightBeforeRotation indicates that current view is the view
	// right before rotation happens.
	void addCurrentViewToStack(bool bRightBeforeRotation = false);

	// add file to the MRU list
	void addFileToMRUList(const std::string& strFileToBeAdded);

	void createSelectionToolMenu();

	// following hWmd is needed because m_hWnd is sometimes NULL by the time
	// we get to the destructor (where we are calling UnhookWindowsHookEx
	HWND m_hWndCopy;

	// following static variables and callback functions are required to ensure that
	// the UGD is not refreshed during a window resize operation
	static std::map<HWND, HHOOK> ms_mapSRIRToHook;
	static bool ms_bSizingInProgress;
	friend LRESULT CALLBACK CallWndProc(int nCode, WPARAM wParam, LPARAM lParam);

	// create sub image file from original image using the raster zone info
	// and the current rotation angle
	std::string createSubImage(const std::string& strOriginalImageFileName, 
		IRasterZone* pImagePortionInfo, double dRotationAngle);

	// delete temporary file
	void deleteFile(const std::string& strToBeDeleted);

	// remove file from the MRU list
	void removeFileFromMRUList(const std::string& strFileToBeRemoved);

	void setEnvironmentPath();

	// vector of IDs of the last zone entities created for the sake of temporary highlighting
	// of the currently selected found attribute in the grid control
	std::vector<long> m_vecTempHighlightEntityIDs;

	// PROMISE: this method will calculate whether an OCR progress dialog
	//			should be display given the area of the region to be OCR'd
	IProgressStatusPtr getShowOCRProgress(long nRecArea);

	void getFirstTempHighlightPageBounds(long& nStartPage, long& nLeft, long& nTop, long& nRight, long& nBottom);

	// If Fit to Page or Fit to Width is selected, zoom the figure accordingly.
	void zoomToFit();

	// Set the status of the Fit-to-Page and Fit-to-Width buttons
	void setFitToButtonsStatus();

	// Set the status of the Fit-to-Page and Fit-to-Width buttons
	void setFitToStatus();

	// PROMISE: Create temporary highlights for each of the lines in the IIUnknownVector. Also update
	// the m_vecTempHighlightEntityIDs with the highlights that are made
	void createTemporaryHighlightsForLines( IIUnknownVectorPtr ipLines );

	// true if highlights should be movable and resizable using the highlight tool;
	// false if highlights should NOT be movable or resizable using the highlight tool.
	bool m_bHighlightsAdjustable;
};

//{{AFX_INSERT_LOCATION}}
// Microsoft Visual C++ will insert additional declarations immediately before the previous line.
