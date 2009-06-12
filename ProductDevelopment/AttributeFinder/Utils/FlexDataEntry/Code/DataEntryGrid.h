#include <BasicGridWnd.h>
#include <string>

/////////////////////////////////////////////////
// Constants for [GridX] Settings within INI file
/////////////////////////////////////////////////

// String constant for Grid Height measured in rows
const std::string	gstrGRID_HEIGHT = "Height";

// String constant for Label displayed at top left of grid
const std::string	gstrLABEL = "Label";

// String constant for Query as applied to IIUnknownVector of IAttributes
const std::string	gstrQUERY = "Query";

// String constant for Grid Type { "A", "B", "C", "D" }
const std::string	gstrGRID_TYPE = "Type";

// String constant for Column definitions
//   i.e. "First:40,Middle:20,Last:40"
//  where text indicates an order-specific column heading
//    and numeric value indicates column width as a percentage of total grid width
//  NOTE: Total width of (columns + row header )> 100 
//        will result in a horizontal scroll bar
const std::string	gstrGRID_COLUMNS = "Columns";

// String constant for Row definitions
//   i.e. "TaxID:12345,Book:2345,PropertyUse:R"
//  where text to the left of the colon indicates an order-specific row heading
//    and text to the right of the colon indicates default cell text
//  NOTE: Allowed only for Type C (with multiple rows) and Type D (for one row)
const std::string	gstrGRID_ROWS = "Rows";

// String constant for width of Row Header as a percentage of total grid width
//  This item is not supported for Type = D
//  NOTE: Total width of (columns + row header )> 100 
//        will result in a horizontal scroll bar
const std::string	gstrGRID_ROW_HEADER_WIDTH = "RowHeaderWidth";

// String constant for label of default column
//  Default column setting applies to Type A grid
const std::string	gstrGRID_DEFAULT_COLUMN = "DefaultColumn";

// String constant for Swipe support
//   0 : not supported
//   1 :     supported
// NOTE: Swipe support may be implemented differently for each grid type
const std::string	gstrSWIPE_SUPPORT = "Swipe";

// String constant for Rubberband support
//   0 : not supported
//   1 :     supported
// NOTE: Rubberband support may be implemented differently for each grid type
const std::string	gstrRUBBERBAND_SUPPORT = "Rubberband";

// String constant for relative path to the RSD file that processes dynamic input
const std::string	gstrDYNAMIC_RSD = "DynamicInputRSD";

// String constant for warning the user before Save if less than minimum # rows present
// Set value in INI file to 0 to turn off the warning
const std::string	gstrWARN_ON_SAVE_IF_ROWS_LESS_THAN = "WarnOnSaveIfRowsLessThan";

// String constant for warning the user before Save if more than maximum # rows present
// Set value in INI file to 0 to turn off the warning
const std::string	gstrWARN_ON_SAVE_IF_ROWS_MORE_THAN = "WarnOnSaveIfRowsMoreThan";

// String constant for disabling the Add button
const std::string	gstrDISABLE_ADD_BUTTON = "DisableAddButton";

// String constant for disabling Up/Down/Left/Right from navigating out of Type D grid
const std::string	gstrDISABLE_ARROW_NAVIGATION = "DisableArrowNavigation";

// String constant for grid-based Vertical Scroll Bar
const std::string	gstrGRID_SCROLL_PIXELS = "PixelsForGridScrollBar";

//////////////////////////////////////////
// Constants for control spacing on dialog
//////////////////////////////////////////

// Offset in pixels between labels, grids, buttons and edge of dialog
const int giOFFSET_EDGE = 7;

// Vertical offset in pixels between controls
const int giOFFSET_CONTROL = 5;

// Additional height in pixels of grid object plus associated controls
// beyond height of grid
const int giGROUP_SIZE = 25;

// Height in pixels of Label
const int giLABEL_SIZE = 14;

/////////////////////////////////////////////////////////////////////////////

class CDataEntryGrid : public CBasicGridWnd
{
public:
	CDataEntryGrid();
	~CDataEntryGrid();

	// Adds an empty row to grid.  Returns 1-relative index of new row or 
	// -1 if unsucessful.
	int		AddNewRecord();

	// True iff adding and deleting records makes sense with this grid
	bool	AllowAddDeleteRecords();

	// True iff Previous and Next records makes sense with this grid
	bool	AllowRecordNavigation();

	// Clear all attributes inside the Collection of IAttribute objects
	void	ClearAllAttributes();

	// Deletes the selected record from the grid
	void	DeleteSelectedRecord();

	// Returns the visible record from navigation in this grid
	int		GetActiveRecord();

	// Returns Attribute from nRow if Type A grid or Type C grid
	// Returns Subattribute from nRow if Type B grid
	// Returns visible Attribute if Type D grid
	// REQUIRES: 0 < nRow <= GetRowCount()
	IAttributePtr	GetAttributeFromRow(int nRow);

	// Returns column number of first non-empty cell.
	// Returns 0 if column header is empty or if all non-header cells are empty
	// REQUIRES:	Type != "D"
	//				0 < nRow <= GetRowCount()
	int		GetFirstNonEmptyCell(int nRow);

	// Returns label displayed above grid
	std::string	GetGridLabel();

	// Returns number of pixels desired for vertical scroll bar within this grid
	long		GetGridScrollPixels();

	// Returns grid type = {"Grid1", "Grid2", ...}
	std::string	GetID();

	// Returns number of rows in this grid with named row header and 
	// at least one non-empty cell.
	// REQUIRES: m_strType != "D"
	int		GetNonEmptyRowCount();

	// Returns number of records available for navigation in this grid
	int		GetRecordCount();

	// Returns selected Attribute
	IAttributePtr	GetSelectedAttribute();

	// Returns grid type = {"A", "B", "C", "D"}
	std::string		GetType();

	// Processes strInput via DynamicInputRSD and updates the Grid appropriately
	// Returns: 0-relative index into m_ipAttributesForShow of new Attribute for Type D
	int		HandleRubberband(ISpatialStringPtr ipInput);

	// Processes strInput via DynamicInputRSD and updates the Grid appropriately
	// Returns: 0-relative index into m_ipAttributesForShow of new Attribute
	int		HandleSwipe(std::string strInput);

	// True if an item within this grid is selected
	// True if this grid has a current cell
	// False otherwise
	bool	IsActive();

	// True if these items defined in INI file
	bool	IsAddButtonDisabled();
	bool	IsArrowNavigationDisabled();

	// True iff this grid supports Rubberband input
	bool	IsRubberbandEnabled();

	// True iff the selected row in this grid has only empty cells
	bool	IsSelectedRowEmpty();

	// True iff this grid supports Swiping input
	bool	IsSwipingEnabled();

	// Notification when grid loses focus.  Used to store edit control
	// selection information for paste / append / replace of swiped text
	BOOL	OnActivateGrid(BOOL bActivate);

	// Adds rows to grid based on application of Query to ipAttributes
	void	Populate(IIUnknownVectorPtr ipAttributes);

	// Refreshes display based on m_ipAttributesForShow that may 
	// have been edited
	void	Refresh();

	// Changes displayed Attribute to nItem.
	// REQUIRES: AllowRecordNavigation() == true
	//			 0 <= nItem < GetRecordCount()
	void	SetActiveRecord(int nItem);

	// Sets text of specified row header.
	//    nRow >= 1
	//    if nRow == nNumRows + 1, adds 1 new row and sets the header label
	//    if nRow >  nNumRows + 1, throws exception
	void	SetRowHeaderLabel(int nRow, std::string strLabel);

	// Sets ID string associated with section within specified INI file
	//   and associates the specified control ID with the Grid.  IDs of 
	//   associated controls will be relative to lControlID
	// Returns true if strID section is found within strINIFile, 
	//   false otherwise
	bool	SetID(std::string strID, long lControlID, std::string strINIFile, 
		CWnd* pParent);

	// Removes any selection from this grid allowing another grid to show focus
	void	UnselectRecords();

private:

	//////////
	// Methods
	//////////
	// Inserts data into the Type A grid
	void	addTypeARow(int iRow, IAttributePtr ipAttribute);

	// Inserts data into the Type B grid
	void	addTypeBRows(IAttributePtr ipAttribute);

	// Inserts data into the Type C grid
	void	addTypeCRow(int iRow, IAttributePtr ipAttribute);

	// Inserts data into the single-cell Type D grid
	void	addTypeDText(IAttributePtr ipAttribute);

	// Creates appropriate grid using relevant settings from m_vecKeys
	void	applySettings();

	// Returns true if at least one sub-attribute name is found in the column headings
	// REQUIRES: m_strType == "A"
	bool	subAttributeNameIsColumnName(IAttributePtr ipAttribute);

	// Searches m_ipAttributesForShow for Attribute with Name = strName
	// and returns 0-relative index of last one.  Returns -1 if not found.
	int		findLastAttributeIndex(std::string strName);

	// Searches m_vecColumnHeaders for name of DefaultColumn
	// and returns column number if found.  Returns -1 if not found.
	int		getDefaultColumnIndex();

	// Gets folder path within INI file
	std::string	getFolder();

	// Creates the MiscUtils object if necessary and returns it
	IMiscUtilsPtr getMiscUtils();

	// Gets specified setting from INI file.
	// Returns empty string if not found.
	// Throws exception if not found AND bRequired = true
	std::string	getSetting(std::string strKey, bool bRequired);

	// Returns specified sub-attribute of ipAttribute or NULL
	// if strSubAttrName is not present in the collected sub-attributes
	IAttributePtr getSubAttribute(std::string strSubAttrName, IAttributePtr ipAttribute);

	// Returns specified sub-attribute Value of ipAttribute or empty string
	// if strSubAttrName is not present in the collected sub-attributes
	std::string getSubAttributeValue(std::string strSubAttrName, IAttributePtr ipAttribute);

	// Parses strColumnData into collection of Column Header labels plus
	//    collection of Column Widths.  Width input is as a percentage of 
	//    total grid width.  Column information is delimited by comma.
	//    Label and width for a specific column are delimited by colon.
	//    i.e. "First:40,Middle:20,Last:40"
	void	parseColumnHeadings(std::string strColumnData);

	// Sets specified text into specified cell of a Type A grid
	void	setTypeACell(int iRow, int iCol, IAttributePtr ipAttribute, std::string strText);

	// Setup grid control and associated label and buttons
	void	setupControls();

	// Checks m_strType
	//   Throws exception if Type != { "A", "B", "C", "D" }
	void	validateType();

	///////
	// Data
	///////

	// Handles auto-encryption
	IMiscUtilsPtr		m_ipMiscUtils;

	// Parent CWnd
	CWnd*	m_pParentWnd;

	// Associated controls
	CStatic*	m_pLabel;
	CButton*	m_pAdd;
	CButton*	m_pDelete;
	CButton*	m_pPrevious;
	CButton*	m_pNext;
	CEdit*		m_pActual;

	// Selection information for paste
	int			m_nSelStart;
	int			m_nSelEnd;

	// Collection of IAttribute objects used to populate the Grid
	// This collection is after application of Query
	IIUnknownVectorPtr	m_ipAttributesForShow;

	// Collection of keys provided in appropriate section of INI file
	std::vector<std::string>	m_vecKeys;

	// Grid type
	std::string	m_strType;

	// ID of grid in INI file
	std::string	m_strID;

	// Control ID of Grid
	long		m_lGridID;

	// Path to INI file containing settings for grid
	std::string	m_strINIFile;

	// Query to be applied to IIUnknownVector of IAttribute objects
	std::string	m_strQuery;

	// Label to be displayed above top left corner of grid
	std::string	m_strLabel;

	// Full path to RSD file used to process dynamic input
	std::string	m_strDynamicInputRSDFile;

	// Number of rows to be displayed by default
	int		m_iNumDefaultRows;

	// Width of row header as percentage of available grid width
	//   0 <= m_iRowHeaderWidth <= 100
	int		m_iRowHeaderWidth;

	// Add button disabled?
	bool	m_bDisableAddButton;

	// Space within grid for a vertical scroll bar - default = 0
	long	m_lGridScrollPixels;

	// Up/Down/Left/Right navigation disabled?
	// This is valid only for Type D grids
	bool	m_bDisableArrowNavigation;

	// Dynamic input accepted?
	bool	m_bAcceptSwipe;
	bool	m_bAcceptRubberband;

	///////////////////////////
	// Type A specific settings
	// NOTE: These settings are ignored for grids of Type "B", "C", "D"
	///////////////////////////

	// Collection of Column Header labels for Type A grid
	std::vector<std::string>	m_vecColumnHeaders;

	// Collection of Column widths for Type A grid.
	//   Widths are defined as percentage of total grid width.
	//   0 <= m_vecColumnWidths[i] <= 100
	std::vector<int>	m_vecColumnWidths;

	// Header label for column in which Attribute Value text will be displayed
	//    in Type A grid iff Attribute has no sub-attribute with a Name that 
	//    matches the Header label of an existing column.
	std::string	m_strDefaultColumn;

	///////////////////////////
	// Type D specific settings
	// NOTE: These settings are ignored for grids of Type "A", "B", "C"
	///////////////////////////

	// Attribute name associated with text displayed in single-cell grid
	std::string	m_strTypeDName;

	// 0-relative Index within m_ipAttributesForShow of active item
	// This applies to grids where AllowRecordNavigation() = TRUE
	long	m_lActiveAttribute;
};
