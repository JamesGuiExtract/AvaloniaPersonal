#pragma once

#include "..\..\..\..\ReusableComponents\APIs\RogueWave\Inc\Grid\gxall.h"
#include "InputProcessor.h"

#include <string>
#include <vector>
#include <memory>

#include <EInputType.h>
#include <DistanceCore.h>
#include <DirectionHelper.h>

class ICurveCalculationEngine;
class IIcoMapUI;

/////////////////////////////////////////////////////////////////////////////
// DynamicInputGridWnd window

class DynamicInputGridWnd : public CGXGridWnd
{
public:
	DynamicInputGridWnd();
	~DynamicInputGridWnd();

// Overridables
public:

	virtual BOOL GetStyleRowCol(ROWCOL nRow, ROWCOL nCol, CGXStyle& style, 
								GXModifyType mt, int nType);
	virtual void DrawInvertCell(CDC* /*pDC*/, ROWCOL nRow, ROWCOL nCol, CRect rectItem);
	virtual BOOL OnStartEditing(ROWCOL nRow, ROWCOL nCol);
	virtual BOOL OnEndEditing(ROWCOL nRow, ROWCOL nCol);
	// start dragging the row
	virtual BOOL OnSelDragRowsStart(ROWCOL nFirstRow, ROWCOL nLastRow);
	virtual BOOL OnTrackColWidthMove(ROWCOL nCol, int nWidth);
	virtual BOOL StoreMoveRows(ROWCOL nFromRow, ROWCOL nToRow, ROWCOL nDestRow, BOOL bProcessed = FALSE);
	virtual BOOL OnLButtonClickedRowCol(ROWCOL nRow, ROWCOL nCol, UINT flags, CPoint point);
	virtual BOOL OnRButtonClickedRowCol(ROWCOL nRow, ROWCOL nCol, UINT flags, CPoint point);
	virtual BOOL CanChangeSelection(CGXRange* pRange, BOOL bIsDragging, BOOL bKey);
	virtual BOOL SetCurrentCell(ROWCOL nRow, ROWCOL nCol, UINT flags = GX_UPDATENOW | GX_SCROLLINVIEW);
	virtual BOOL CanSelectCurrentCell(BOOL bSelect, ROWCOL dwSelectRow, ROWCOL dwSelectCol, ROWCOL dwOldRow, ROWCOL dwOldCol);
	virtual BOOL OnGridKeyDown(UINT nChar, UINT nRepCnt, UINT nFlags);

protected:
	virtual BOOL PreTranslateMessage(MSG* pMsg);

// Operations
public:

	void addCurrentSegment();

	// DIG will not accepting any input and the current
	// selection will be set to the last row
	void deactivateDIG();

	// delete sketch
	void deleteSketch();

	// discard the segment in progress due to some reasons such
	// as selected another type of segment to draw, etc.
	void discardSegmentInProgress();

	// resize the grid based on its parent window size
	void doResize();

	// This part is finished, add it to the feature stored internally
	void finishCurrentPart();

	// Is tracking segments enabled
	bool needTracking();

	// This sketch is finished. Store the feature to the database
	// Return feature id in string
	std::string finishCurrentSketch();

	EInputType getCurrentInputType() const;

	ESegmentType getCurrentSegmentType() const;

	// return the error segment error bearing and distance
	std::string getErrorSegmentReport();

	// initialize the grid, for instance, set column 
	// and row header, set number of columns, etc.
	void initDIG();

	// if number of segments of the current sketch is changed
	// Return true if the notification is already processed
	bool notifySketchModified(long nNumOfSegmentsOfSketch);

	BOOL preTranslateMsg(MSG* pMSG) {return PreTranslateMessage(pMSG);}

	// process the input.
	// Return true if the input has been processed by this
	// grid already, no more process shall be done after this.
	// Return false otherwise.
	bool processInput(const std::string& strInputText);

	// redraw the selected segment if exists.
	void redrawCurrentSegment();

	// reset grid to have nothing in display
	void reset();

	void setAttributeManager(IAttributeManager* pAttributeManager);
	void setDisplayAdapter(IDisplayAdapter* pDisplayAdapter);
	void setIcoMapUI(IIcoMapUI* pIcoMapUI);
	void setInputProcessor(InputProcessor* pInputProcessor);

	// set one of the currently editing record's parameters
	// Note: This method will refresh the grid display. Do not call
	// this method internally if the user edited one of the cells
	// from the grid. You can call internalStoreParameter() instead
	// to store the data
	// This method can be used in adding new segment, toggling existing segment
	// and modifying existing segment parameter
	// bAddNewRecord: true - start a new record for this segment
	//				  false - keep adding to the existing segment record
	void setSegmentParameter(ESegmentType eSegmentType,
					  ECurveParameterType eParamType, 
					  const std::string& strValue,
					  bool bAddNewRecord = false);

	// set starting point for current part
	void setStartPointForPart(ICartographicPointPtr ipStartPoint);


private:

	///////////
	// structs
	///////////
	// each cell has it's own coordinates
	struct CellCoords
	{
	public:
		CellCoords()
			: m_nRow(0), m_nCol(0) {}

		// whether or not this cell has meaningful coords
		bool isNull()
		{
			// in order for the cell to have meaningful coords,
			// row and col must be greater than 0 
			return m_nRow == 0 || m_nCol == 0;
		}

		void resetCell()
		{
			m_nRow = 0;
			m_nCol = 0;
		}

		unsigned long m_nRow;
		unsigned long m_nCol;
	};

	////////////
	// Variables
	////////////
	IDisplayAdapterPtr m_ipDisplayAdapter;
	IAttributeManagerPtr m_ipAttributeManager;
	IIcoMapUI* m_pIcoMapUI;
	InputProcessor* m_pInputProcessor;

	DistanceCore m_distanceCore;
	DirectionHelper m_dirHelper;

	// whether or not the grid has been initialized
	bool m_bInitGrid;

	// whether or not current selection can be changed
	bool m_bCanChangeSelection;

	// the selected row index. 1-based
	long m_nCurrentSelectedRowIndex;

	// start point for the current part
	ICartographicPointPtr m_ipCurrentPartStartPoint;

	// internally keep a vector of segments for current part
	// Note: this vector is actually the internal data source for 
	// the records displayed in the grid
	IIUnknownVectorPtr m_ipCurrentPartSegments;

	// Feature formed by current sketch
	// This feature will be gradually populated as each part has 
	// been added to the sketch
	UCLID_FEATUREMGMTLib::IUCLDFeaturePtr m_ipFeature;

	// the segment that is being edited or currently being selected.
	// It could be a new segment that hasn't been fully populated or drawn
	// in the map. Or it could be an existing segment from the sketch
	std::vector<IParameterTypeValuePairPtr> m_vecCurrentSegmentParams;

	// this is 0-based index number for current part.
	int m_nCurrentPartIndex;

	// this is 0-based index number for currently selected segment
	// record in the grid. 
	// This number will be set to -1 if the segment hasn't been 
	// drawn yet, or there's no segment in the drawing yet.
	// Note: this index is for the current part
	int m_nCurrentSegmentIndexInPart;

	// whether or not current segment has been modified
	bool m_bCurrentSegmentModified;

	// what's current segment type, arc or line?
	ESegmentType m_eCurrentSegmentType;

	// what's the currently editing parameter type if any?
	ECurveParameterType m_eCurrentEditingParamType;

	// curve calculation engine for validating input data
	std::auto_ptr<ICurveCalculationEngine> ma_pCCE;

	EInputType m_eCurrentInputType;

	// this is the segment that hasn't been drawn in the map yet.
	// i.e. this is the latest segment in progress in the sketch
	IESSegmentPtr m_ipLatestSegmentInProgress;

	// whether or not the latest segment is completely drawn
	// i.e. whether or not the segment has complete set of parameter
	// to form this segment.
	bool m_bLatestSegmentInfoComplete;

	// if there's a current cell
	CellCoords m_CurrentCell;

	// what's the total number of segments drawn using IcoMap
	// in current sketch?
	int m_nTotalSegmentsDrawnByIcoMap;

	// whether or not to track each segment and display in the grid.
	// true - if there's no outside interference with icomap tool
	// false - some other tool is used outside of icomap
	bool m_bTrackingSegments;

	// Whether or not to ignore sketch modification
	bool m_bBlockSketchNotification;

	// whether or not grid has the focus
	bool m_bFocusOnGrid;

	// The cell that is being modified
	CellCoords m_ModifyingCell;

	// whether or not the current cell is modified
	bool m_bIsCurrentCellModified;


	////////////
	// Methods
	////////////

	// displays type and value in two consecutive cells in the same row
	// nRecordIndex - 0-based index for each segment
	// nParameterIndex - 0-based index for each type value pair
	void displayParameterTypeValuePair(int nRecordIndex,
								   int nParameterIndex, 
								   IParameterTypeValuePairPtr ipTypeValuePair);

	// current grid width excluding row header and some extra space
	long getCurrentTrueGridWidth();

	// based on the curve parameter type, returns its name string
	// bAbbr: true - return name in abbreviated format, false - return name in full
	std::string getCurveTypeName(ECurveParameterType eCurveParamType, bool bAbbrievated = true);

	// create a new segment that is the copy of the pass-in segment
	IESSegmentPtr getDuplicateSegment(IESSegmentPtr ipToBeCopiedSegment);

	// convert the distance to string format with proper decimal place
	// and the current unit
	std::string getProperDistanceFormat(double dDistanceInCurrentUnit);

	// get info from m_ipCurrentSegmentParams, and create
	// a new unknown vector for returning
	IIUnknownVectorPtr getSegmentInfo();

	// internally store the parameter type value into 
	// m_ipCurrentSegmentParams
	void internalStoreParameter(ECurveParameterType eParamType, const std::string& strValue);

	// with current info all together, can current segment 
	// be calculated without error?
	bool isCurrentSegmentCalculatable();

	// when one of the context menu item is selected
	// return TRUE if the nID is in between ID_DIG_INSERT_DUPLICATE and ID_DIG_DOWN
	BOOL onSelectContextMenu(UINT nID);

	// refresh the display of current selected record based
	// on the internally stored segment data
	void refreshCurrentRecordDisplay();

	// refresh the screen display of a specific row
	void refreshRowDraw(unsigned long nRow);

	void setCCEParameter(ECurveParameterType eParamType, const std::string& strValue);

	// select rows. It will clear any previous selection first.
	// bUpdateSegmentInfo - whether or not to update the newly selected
	// segment information.
	// bFlashSelection - whether or not flash the current selection row
	void setRowsSelection(long nStartingRow, long nNumberOfRows, 
		bool bUpdateSegmentInfo = true, bool bFlashSelection = true);

	// make sure all necessary objects are initiated
	// get info from the vector of parameters and store them
	// in m_ipCurrentSegmentParams
	void setSegmentInfo(IIUnknownVectorPtr ipParams);

	// swap two segments in the grid as well as in the drawing
	// bSelectRow1 - default to select nRow1
	void swapRows(unsigned int nRow1, unsigned int nRow2, bool bSelectRow1 = true);

	// take the input and translate that into a text in current format
	// For instance, if the input text is in feet, it'll be translated
	// into meter if that's the current unit. And if input text is 
	// in bearing format, it'll be translated into polar angle if that's
	// the current direction format
	// If input parameter type is not one of the direction or distance type,
	// the original value will be returned.
	// Note: This function only translates directions in quadrant bearing
	// format to current direction format.
	std::string translateBDToCurrentFormat(ECurveParameterType eParamType, 
											const std::string& strInText);

	// interpret the input parameter type, and convert it to input type
	EInputType translateParameterTypeToInputType(ECurveParameterType eParamType);

	// update set of variables for current selected segment, such as 
	// m_vecCurrentSegmentParams, m_nCurrentSegmentIndexInPart, 
	// m_eCurrentSegmentType, etc. 
	// nNewSegmentIndex - 0-based index number for currently selected segment
	//					  record in the grid. Reset all of them if 
	//					  nNewSegmentIndex < 0
	// bUpdateIfSelectionSame: 
	// true - update the variable set even if the selection hasn't been changed
	void updateCurrentSegmentVariableSet(int nNewSegmentIndex, bool bUpdateIfSelectionSame = false);

	// Based on the specified row record in the grid, update
	// its related info stored internally
	void updateRowRecordInfo(unsigned int nRowIndex);

	// all necessary object must be set
	void validObjects();
};