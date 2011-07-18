
#include "stdafx.h"
#include "DataEntryGrid.h"
#include "resource.h"
#include "..\\..\\..\\AFCore\\Code\\Common.h"

#include <UCLIDException.h>
#include <cpputil.h>
#include <INIFilePersistenceMgr.h>
#include <StringTokenizer.h>
#include <COMUtils.h>

#ifdef _DEBUG
#define new DEBUG_NEW
#undef THIS_FILE
static char THIS_FILE[] = __FILE__;
#endif

//-------------------------------------------------------------------------------------------------
// CDataEntryGrid
//-------------------------------------------------------------------------------------------------
CDataEntryGrid::CDataEntryGrid()
: CBasicGridWnd(),
  m_bAcceptSwipe(false),
  m_bAcceptRubberband(false),
  m_iNumDefaultRows(1),
  m_iRowHeaderWidth(20),
  m_nSelStart(-1),
  m_nSelEnd(-1),
  m_pLabel(NULL),
  m_pAdd(NULL),
  m_bDisableAddButton(false),
  m_bDisableArrowNavigation(false),
  m_pDelete(NULL),
  m_pPrevious(NULL),
  m_pActual(NULL),
  m_pNext(NULL),
  m_pParentWnd(NULL),
  m_ipAttributesForShow(NULL),
  m_lGridScrollPixels(0),
  m_lActiveAttribute(0)
{
}
//-------------------------------------------------------------------------------------------------
CDataEntryGrid::~CDataEntryGrid()
{
	try
	{
		// Delete added controls
		if (m_pLabel != NULL)
		{
			delete m_pLabel;
		}

		if (m_pAdd != NULL)
		{
			delete m_pAdd;
		}

		if (m_pDelete != NULL)
		{
			delete m_pDelete;
		}

		if (m_pPrevious != NULL)
		{
			delete m_pPrevious;
		}

		if (m_pActual != NULL)
		{
			delete m_pActual;
		}

		if (m_pNext != NULL)
		{
			delete m_pNext;
		}
	}
	CATCH_AND_LOG_ALL_EXCEPTIONS("ELI16440");
}

//-------------------------------------------------------------------------------------------------
// CDataEntryGrid public methods
//-------------------------------------------------------------------------------------------------
int CDataEntryGrid::AddNewRecord()
{
	int iNewIndex = -1;

	if (!m_ipAttributesForShow)
	{
		m_ipAttributesForShow.CreateInstance( CLSID_IUnknownVector );
	}
	ASSERT_RESOURCE_ALLOCATION( "ELI13239", m_ipAttributesForShow != NULL );

	// Not supported for Type D
	if (m_strType != "D")
	{
		// Determine row number of new row
		iNewIndex = GetRowCount() + 1;

		// Add one new row at the bottom
		InsertRows( iNewIndex, 1 );

		// Clear row header
		SetRowHeaderLabel( iNewIndex, "" );

		// Create an empty IAttribute
		IAttributePtr	ipNew( CLSID_Attribute );
		ASSERT_RESOURCE_ALLOCATION( "ELI11070", ipNew != NULL );

		// Add new Attribute to collected sub-attributes
		if (m_strType == "B")
		{
			// Retrieve first Attribute
			if ((m_ipAttributesForShow != NULL) && (m_ipAttributesForShow->Size() > 0))
			{
				IAttributePtr	ipMain = m_ipAttributesForShow->At( 0 );
				ASSERT_RESOURCE_ALLOCATION( "ELI11068", ipMain != NULL );

				// Retrieve collected sub-attributes
				IIUnknownVectorPtr	ipSubs = ipMain->SubAttributes;
				ASSERT_RESOURCE_ALLOCATION( "ELI11069", ipSubs != NULL );

				// Add new item to collection
				ipSubs->PushBack( ipNew );
			}
			else
			{
				// Create an empty main IAttribute
				IAttributePtr	ipMain( CLSID_Attribute );
				ASSERT_RESOURCE_ALLOCATION( "ELI13240", ipNew != NULL );

				// Retrieve collected sub-attributes
				IIUnknownVectorPtr	ipSubs = ipMain->SubAttributes;
				ASSERT_RESOURCE_ALLOCATION( "ELI13241", ipSubs != NULL );

				// Add new item to sub attributes collection
				ipSubs->PushBack( ipNew );

				// Push ipMain into m_ipAttributesForShow
				m_ipAttributesForShow->PushBack( ipMain);
			}
		}
		// Add new Attribute to collected attributes
		else
		{
			// Retrieve collected attributes
			if (m_ipAttributesForShow)
			{
				// Add new item to collection
				m_ipAttributesForShow->PushBack( ipNew );
			}
		}

		// Set modified flag
		SetModified();
	}

	return iNewIndex;
}
//-------------------------------------------------------------------------------------------------
bool CDataEntryGrid::AllowAddDeleteRecords()
{
	// Add and Delete not supported for Type D grid
	return !(m_strType == "D");
}
//-------------------------------------------------------------------------------------------------
bool CDataEntryGrid::AllowRecordNavigation()
{
	bool bAllow = false;

	// Previous, Page, Next controls supported for Type D grid
//	if ((m_strType == "B") || (m_strType == "D"))
	if (m_strType == "D")
	{
		bAllow = true;
	}

	return bAllow;
}
//-------------------------------------------------------------------------------------------------
void CDataEntryGrid::ClearAllAttributes()
{
	// Clear all attributes inside IAttribute objects
	if (m_ipAttributesForShow != NULL)
	{
		m_ipAttributesForShow->Clear();
	}

	// Reset modified flag
	SetModified( false );
}
//-------------------------------------------------------------------------------------------------
void CDataEntryGrid::DeleteSelectedRecord()
{
	// Determine selected row if Swipe is accepted
	CGXRangeList selList;
	if (CopyRangeList( selList, FALSE ))
	{
		CGXRange range = selList.GetTail();
		int nRow = range.top;

		// Remove the selected row(s)
		RemoveRows( nRow, nRow );

		// Remove the associated Attribute
		if (m_strType != "B")
		{
			if ((nRow > 0) && (nRow <= m_ipAttributesForShow->Size()))
			{
				m_ipAttributesForShow->Remove( nRow - 1 );
			}
		}
		else
		{
			if (nRow > 0)
			{
				IAttributePtr	ipMain = m_ipAttributesForShow->At( 0 );
				ASSERT_RESOURCE_ALLOCATION( "ELI13000", ipMain != NULL );

				// Retrieve collected sub-attributes
				IIUnknownVectorPtr	ipSubs = ipMain->SubAttributes;
				ASSERT_RESOURCE_ALLOCATION( "ELI13001", ipSubs != NULL );
				
				// Remove the associated sub-attributes
				ipSubs->Remove(nRow-1);
				
				// If there are no sub-attributes left, delete this entry completely.
				// [PVCS P16 - 1817]
				if( ipSubs->Size() <= 0 )
				{
					m_ipAttributesForShow->Remove( 0 );
				}
			}
		}

		// Reselect the row now at top of selection area
		long lCount = GetRowCount();
		if (lCount >= nRow)
		{
			SelectRange( CGXRange( nRow, 1, nRow, GetColCount() ), TRUE );
			m_lActiveAttribute = nRow - 1;
		}
		else if (lCount > 0)
		{
			// Otherwise select the last row
			SelectRange( CGXRange( lCount, 1, lCount, GetColCount() ), TRUE );
			m_lActiveAttribute = lCount - 1;
		}

		SetModified();
	}
}
//-------------------------------------------------------------------------------------------------
int CDataEntryGrid::GetActiveRecord()
{
	return m_lActiveAttribute;
}
//-------------------------------------------------------------------------------------------------
IAttributePtr CDataEntryGrid::GetAttributeFromRow(int nRow)
{
	ASSERT_RESOURCE_ALLOCATION( "ELI13120", m_ipAttributesForShow != NULL );

	// Validate nRow
	long lCount = m_ipAttributesForShow->Size();
	if ((nRow < 1) || ((nRow > lCount) && (m_strType != "B")))
	{
		UCLIDException ue("ELI13121", "Invalid row index for GetAttributeFromRow().");
		ue.addDebugInfo( "Grid Number", m_strID );
		ue.addDebugInfo( "Row Index", nRow );
		ue.addDebugInfo( "Attribute Count", lCount );
		throw ue;
	}

	// Rows in Type B grid are sub-attributes
	IAttributePtr	ipAttr = NULL;
	if (m_strType == "B")
	{
		// Retrieve main Attribute
		IAttributePtr	ipMainAttr = m_ipAttributesForShow->At( 0 );
		ASSERT_RESOURCE_ALLOCATION( "ELI13235", ipMainAttr != NULL );

		// Default to entire Attribute in case 
		// - header has no label
		// - header label was changed
		ipAttr = ipMainAttr;

		// Get header label of selected item
		CString zLabel = GetCellValue( nRow, 0 );

		// Make sure that this row has a header label
		if (zLabel.GetLength() > 0)
		{
			// Get sub-attributes
			IIUnknownVectorPtr ipSubs = ipMainAttr->SubAttributes;
			ASSERT_RESOURCE_ALLOCATION("ELI15608", ipSubs != NULL);

			// Check each sub-attribute
			long lSubAttrCount = ipSubs->Size();
			for (int i = 0; i < lSubAttrCount; i++)
			{
				// Retrieve this sub-attribute
				IAttributePtr	ipSubAttr = ipSubs->At( i );
				ASSERT_RESOURCE_ALLOCATION( "ELI13236", ipSubAttr != NULL );

				// Compare name with header label
				string strName = asString( ipSubAttr->Name );
				if (zLabel.Compare( strName.c_str() ) == 0)
				{
					// Found the desired sub-attribute
					ipAttr = ipSubAttr;
					break;
				}
			}
		}
	}
	// Only one row in Type D grid, get the active record
	else if (m_strType == "D")
	{
		// Retrieve active record
		ipAttr = m_ipAttributesForShow->At( GetActiveRecord() );
	}
	else
	{
		// Retrieve Attribute from 0-relative collection
		ipAttr = m_ipAttributesForShow->At( nRow - 1 );
	}

	return ipAttr;
}
//-------------------------------------------------------------------------------------------------
int CDataEntryGrid::GetFirstNonEmptyCell(int nRow)
{
	// Cannot be Type D grid
	if (m_strType == "D")
	{
		throw UCLIDException( "ELI13150", 
			"Invalid call to IsNonEmptyRow() for Type D grid!" );
	}

	// Validate parameter
	if ((nRow < 1) || (nRow > (int)GetRowCount()))
	{
		UCLIDException ue("ELI13151", "Invalid row parameter.");
		ue.addDebugInfo("Grid ID", m_strID);
		ue.addDebugInfo("nRow", nRow);
		ue.addDebugInfo("RowCount", GetRowCount());
		throw ue;
	}

	// Check for named row header
	CString zCell = GetCellValue( nRow, 0 );
	if (zCell.IsEmpty())
	{
		// Return 0 since a header must be present
		return 0;
	}

	// Check remaining cells in this row, at least one to contain text
	int iNumCols = GetColCount();
	for (int j = 1; j <= iNumCols; j++)
	{
		zCell = GetCellValue( nRow, j );
		if (!zCell.IsEmpty())
		{
			// Found a non-empty cell
			return j;
		}
	}		// end for each non-header column

	if (m_strType == "C")
	{
		// Type C grid allowed to have row header and empty value cell
		return 1;
	}
	else
	{
		// Did not find a non-empty cell
		return 0;
	}
}
//-------------------------------------------------------------------------------------------------
std::string CDataEntryGrid::GetGridLabel()
{
	return m_strLabel;
}
//-------------------------------------------------------------------------------------------------
long CDataEntryGrid::GetGridScrollPixels()
{
	return m_lGridScrollPixels;
}
//-------------------------------------------------------------------------------------------------
int CDataEntryGrid::GetNonEmptyRowCount()
{
	// Cannot be Type D grid
	if (m_strType == "D")
	{
		throw UCLIDException( "ELI13146", 
			"Invalid call to GetNonEmptyRowCount() for Type D grid!" );
	}

	// Check each row
	int iNumNonEmptyRows = 0;
	int iNumRows = GetRowCount();
	for (int i = 1; i <= iNumRows; i++)
	{
		if (GetFirstNonEmptyCell( i ) > 0)
		{
			iNumNonEmptyRows++;
		}
	}

	return iNumNonEmptyRows;
}
//-------------------------------------------------------------------------------------------------
int CDataEntryGrid::GetRecordCount()
{
	if (m_ipAttributesForShow != NULL)
	{
		return m_ipAttributesForShow->Size();
	}
	else
	{
		return 0;
	}
}
//-------------------------------------------------------------------------------------------------
IAttributePtr CDataEntryGrid::GetSelectedAttribute()
{
	IAttributePtr	ipSel( NULL );

	// Protect against empty grid
	if (m_ipAttributesForShow != NULL)
	{
		// Determine selected row if Swipe is accepted
		CGXRangeList selList;
		if (CopyRangeList( selList, FALSE ))
		{
			CGXRange range = selList.GetTail();
			int nRow = range.top;

			// Retrieve Attribute from the selected row(s)
			if (m_strType == "B")
			{
				// Selection is a sub-attribute
				if (m_ipAttributesForShow->Size() > 0)
				{
					// Retrieve first Attribute
					IAttributePtr	ipMain = m_ipAttributesForShow->At( 0 );
					ASSERT_RESOURCE_ALLOCATION( "ELI13002", ipMain != NULL );

					// Retrieve collected sub-attributes
					IIUnknownVectorPtr	ipSubs = ipMain->SubAttributes;
					ASSERT_RESOURCE_ALLOCATION( "ELI13003", ipSubs != NULL );

					// Check selected sub-attribute
					if ((nRow > 0) && (nRow <= ipSubs->Size()))
					{
						ipSel = ipSubs->At( nRow - 1 );
					}

					// Active Attribute is always the first
					m_lActiveAttribute = 0;
				}
			}
			// Active Attribute will be selected
			else if (m_strType == "D")
			{
				// Selection is an attribute
				if (m_ipAttributesForShow->Size() > 0)
				{
					ipSel = m_ipAttributesForShow->At( m_lActiveAttribute );
				}
			}
			else
			{
				// Selection is an attribute
				if ((nRow > 0) && (nRow <= m_ipAttributesForShow->Size()))
				{
					ipSel = m_ipAttributesForShow->At( nRow - 1 );

					// Active Attribute is zero-relative
					m_lActiveAttribute = nRow - 1;
				}
			}
		}
	}

	return ipSel;
}
//-------------------------------------------------------------------------------------------------
std::string CDataEntryGrid::GetID()
{
	return m_strID;
}
//-------------------------------------------------------------------------------------------------
std::string CDataEntryGrid::GetType()
{
	return m_strType;
}
//-------------------------------------------------------------------------------------------------
int CDataEntryGrid::HandleRubberband(ISpatialStringPtr ipInput)
{
	ASSERT_ARGUMENT("ELI10955", ipInput != NULL);

	//////////////////////////////////
	// Treatment is based on Grid Type
	//////////////////////////////////
	std::vector<std::string>	vecText;

	// Type B
	if (m_strType == "B")
	{
		// Create placeholder AFDocument
		IAFDocumentPtr	ipDoc( CLSID_AFDocument );
		ASSERT_RESOURCE_ALLOCATION( "ELI10953", ipDoc != NULL );

		// Create vector object for found Attributes
		IIUnknownVectorPtr	ipAttributes( CLSID_IUnknownVector );
		ASSERT_RESOURCE_ALLOCATION( "ELI10954", ipAttributes != NULL );

		// Check Dynamic RSD file
		string strPath;
		if (m_strDynamicInputRSDFile.length() > 0)
		{
			strPath = getCurrentProcessEXEDirectory();
			strPath += "\\";
			strPath += m_strDynamicInputRSDFile;
		}

		// Perform any appropriate auto-encrypt actions on the RSD file
		getMiscUtils()->AutoEncryptFile( get_bstr_t( strPath.c_str() ), 
			get_bstr_t(gstrAF_AUTO_ENCRYPT_KEY_PATH.c_str()) );

		// Send text through DynamicInputRSD
		if (isFileOrFolderValid( strPath ))
		{
			// Provide text to AFDocument
			ipDoc->PutText( ipInput );

			// Find Attributes from the text
			IAttributeFinderEnginePtr ipEngine( CLSID_AttributeFinderEngine );
			ASSERT_RESOURCE_ALLOCATION("ELI10956", ipEngine != NULL);

			ipAttributes = ipEngine->FindAttributes( ipDoc, "", -1, 
				strPath.c_str(), NULL, VARIANT_TRUE, NULL );
		}
		else
		{
			UCLIDException ue("ELI13117", "Dynamic Input RSD file not found.");
			ue.addDebugInfo("RSD File", strPath);
			throw ue;
		}

		// Make sure something was found
		if (ipAttributes != NULL && ipAttributes->Size() > 0)
		{
			// Clear current entries
			Clear();

			// Apply the Query and update the grid
			long lCount = ipAttributes->Size();
			Populate( ipAttributes );

			SetModified();
			// TODO: Fix this when Type B supports record navigation
			return 0;
		}
	}
	// Type D
	else if (m_strType == "D")
	{
		// Create a new Attribute object
		IAttributePtr	ipNew( CLSID_Attribute );
		ASSERT_RESOURCE_ALLOCATION("ELI13111", ipNew != NULL);

		// Set Name to LegalDescription
		ipNew->Name = get_bstr_t( "LegalDescription" );

		// Set Value to input
		ipNew->Value = ipInput;

		if (m_ipAttributesForShow == NULL)
		{
			IIUnknownVectorPtr	ipTemp( CLSID_IUnknownVector );
			ASSERT_RESOURCE_ALLOCATION("ELI13112", ipTemp != NULL);
			m_ipAttributesForShow = ipTemp;
		}

		// Add this Attribute to collection, display it and select it
		long lCount = m_ipAttributesForShow->Size();
		m_ipAttributesForShow->PushBack( ipNew );
		SetActiveRecord( lCount );
		SetModified();

		return lCount;
	}

	// No new IAttribute added to m_ipAttributesForShow
	return -1;
}
//-------------------------------------------------------------------------------------------------
int CDataEntryGrid::HandleSwipe(std::string strInput)
{
	// Determine selected row if Swipe is accepted
	CGXRangeList selList;
	CGXRange range;
	ROWCOL nRow = 0;
	ROWCOL nCol = 0;

	if (CopyRangeList( selList, FALSE ) && m_bAcceptSwipe)
	{
		CGXRange range = selList.GetTail();
		nRow = range.top;
	}
	// Find row of current cell
	else if (m_bAcceptSwipe)
	{
		if (GetCurrentCell( nRow, nCol ) == FALSE)
		{
			// Cannot get a current row, just ignore the swipe and return
			return -1;
		}
	}
	else
	{
		// Swipe is not accepted in this grid, just return
		return -1;
	}

	//////////////////////////////////////////
	// Retrieve whole current text and replace 
	// selected text with swipe text
	//////////////////////////////////////////
	// Get whole text
	CString zNewText = GetValueRowCol( nRow, nCol );

	// Check selection
	int iInsertPos = m_nSelStart;
	if (m_nSelStart == -1)
	{
		// No selection, remove everything
		zNewText.Empty();

		// Reset insert position to beginning of string
		iInsertPos = 0;
	}
	else
	{
		// Selection is defined, determine number of characters to remove
		int nLength = zNewText.GetLength();
		int nCount = (m_nSelEnd == -1) ? nLength - m_nSelStart : m_nSelEnd - m_nSelStart;

		// Remove the selected characters
		zNewText.Delete( m_nSelStart, nCount );
	}

	// Insert swiped text at insert position
	zNewText.Insert( iInsertPos, strInput.c_str() );

	//////////////////////////////////
	// Treatment is based on Grid Type
	//////////////////////////////////
	std::vector<std::string>	vecText;

	// Type A
	if (m_strType == "A")
	{
		// Create placeholder AFDocument
		IAFDocumentPtr	ipDoc( CLSID_AFDocument );
		ASSERT_RESOURCE_ALLOCATION( "ELI10933", ipDoc != NULL );

		// Create vector object for found Attributes
		IIUnknownVectorPtr	ipAttributes( CLSID_IUnknownVector );
		ASSERT_RESOURCE_ALLOCATION( "ELI10934", ipAttributes != NULL );

		// Check support for swiping directly to the current cell
		if (nCol > 0)
		{
			// Check for Attribute in this row
			IAttributePtr ipAttr = GetAttributeFromRow( nRow );
			if (ipAttr)
			{
				// Provide text to cell
				setTypeACell( nRow, nCol, ipAttr, zNewText.operator LPCTSTR() );
			}
		}
		else
		{
			// Check Dynamic RSD file
			string strPath;
			if (m_strDynamicInputRSDFile.length() > 0)
			{
				strPath = getCurrentProcessEXEDirectory();
				strPath += "\\";
				strPath += m_strDynamicInputRSDFile;
			}

			// Send text through DynamicInputRSD
			if (isFileOrFolderValid( strPath ))
			{
				// Provide text to AFDocument
				ISpatialStringPtr	ipSpatial( CLSID_SpatialString );
				ASSERT_RESOURCE_ALLOCATION( "ELI10935", ipSpatial != NULL );
				ipSpatial->CreateNonSpatialString(_bstr_t(zNewText), "");
				ipDoc->PutText( ipSpatial );

				// Find Attributes from the text
				IAttributeFinderEnginePtr ipEngine( CLSID_AttributeFinderEngine );
				ASSERT_RESOURCE_ALLOCATION("ELI10937", ipEngine != NULL);

				ipAttributes = ipEngine->FindAttributes( ipDoc, "", -1, 
					strPath.c_str(), NULL, VARIANT_TRUE, NULL );
			}
			else
			{
				UCLIDException ue("ELI13116", "Dynamic Input RSD file not found.");
				ue.addDebugInfo("RSD File", strPath);
				throw ue;
			}

			// Make sure something was found
			if (ipAttributes != NULL && ipAttributes->Size() > 0)
			{
				// Retrieve the first Attribute - only one expected from a swipe
				IAttributePtr	ipAttribute = ipAttributes->At( 0 );

				// Add records or cells for each sub-attribute
				IIUnknownVectorPtr ipSubs = ipAttribute->SubAttributes;
				ASSERT_RESOURCE_ALLOCATION("ELI15609", ipSubs != NULL);
				long lCount = ipSubs->Size();

				// Do sub-attribute names match column names?
				bool bSingleItem = false;
				if (subAttributeNameIsColumnName( ipAttribute ))
				{
					bSingleItem = true;
					lCount = 1;
				}

				int iAttrIndex = -1;
				for (int i = 0; i < lCount; i++)
				{
					// Retrieve this sub-attribute
					IAttributePtr ipNewAttr = NULL;
					if (bSingleItem)
					{
						ipNewAttr = ipAttribute;
					}
					else
					{
						ipNewAttr = ipSubs->At( i );
					}
					ASSERT_RESOURCE_ALLOCATION( "ELI10936", ipNewAttr != NULL );

					// Add (sub-)attribute to collection
					m_ipAttributesForShow->PushBack( ipNewAttr );

					// Add sub-attribute to the display
					if (IsSelectedRowEmpty())
					{
						// Add this entry to the empty row and select it
						addTypeARow( nRow, ipNewAttr );
						SelectRow( nRow );

						// Provide 0-relative index into m_ipAttributesForShow
						iAttrIndex = nRow - 1;
					}
					else
					{
						// Add this entry to a new row and select it
						int iNew = AddNewRecord();
						addTypeARow( iNew, ipNewAttr );
						SelectRow( iNew );

						// Provide 0-relative index into m_ipAttributesForShow
						iAttrIndex = iNew - 1;
					}
				}

				SetModified();

				// Adjust height
				ResizeRowHeightsToFit( CGXRange( ).SetTable() );
				return iAttrIndex;
			}
		}
	}
	// Type B
	else if (m_strType == "B")
	{
		// Create (non-spatial) Spatial String for input
		ISpatialStringPtr	ipText( CLSID_SpatialString );
		ASSERT_RESOURCE_ALLOCATION( "ELI11100", ipText != NULL );
		ipText->CreateNonSpatialString(_bstr_t(zNewText), "");

		// Retrieve first Attribute
		IAttributePtr	ipMain = m_ipAttributesForShow->At( 0 );
		ASSERT_RESOURCE_ALLOCATION( "ELI11101", ipMain != NULL );

		// Get collected sub-attributes
		IIUnknownVectorPtr ipSubs = ipMain->SubAttributes;
		ASSERT_RESOURCE_ALLOCATION("ELI15610", ipSubs != NULL);

		// Replace spatial string in collection of sub-attributes
		IAttributePtr	ipReplacement = ipSubs->At( nRow - 1 );
		ASSERT_RESOURCE_ALLOCATION( "ELI11102", ipReplacement != NULL );
		ipReplacement->PutValue( ipText );

		// Replace text in selected row, retaining row header
		vecText.push_back( zNewText.operator LPCTSTR() );
		SetRowInfo( nRow, vecText );
		SetModified();

		// Adjust height
		ResizeRowHeightsToFit( CGXRange( ).SetTable() );
		return nRow - 1;
	}
	// Type C
	else if (m_strType == "C")
	{
		// Create (non-spatial) Spatial String for input
		ISpatialStringPtr	ipText( CLSID_SpatialString );
		ASSERT_RESOURCE_ALLOCATION( "ELI11099", ipText != NULL );
		ipText->CreateNonSpatialString(_bstr_t(zNewText), "");

		// Replace spatial string in attribute collection
		IAttributePtr	ipReplacement = m_ipAttributesForShow->At( nRow - 1 );
		ASSERT_RESOURCE_ALLOCATION( "ELI11074", ipReplacement != NULL );
		ipReplacement->PutValue( ipText );

		// Replace text in selected row, retaining row header
		vecText.push_back( zNewText.operator LPCTSTR() );
		SetRowInfo( nRow, vecText );
		SetModified();

		// Adjust height
		ResizeRowHeightsToFit( CGXRange( ).SetTable() );
		return nRow - 1;
	}
	// Type D
	else if (m_strType == "D")
	{
		// Update text
		vecText.push_back( zNewText.operator LPCTSTR() );
		SetRowInfo( 1, vecText );
		SetModified();

		// Update Attribute text
		int iIndex = GetActiveRecord();
		if (m_ipAttributesForShow != NULL)
		{
			// Retrieve Attribute to be modified
			IAttributePtr	ipModified = m_ipAttributesForShow->At( iIndex );
			ASSERT_RESOURCE_ALLOCATION("ELI13110", ipModified != NULL);

			// Retrieve Value
			ISpatialStringPtr ipValue = ipModified->Value;
			ASSERT_RESOURCE_ALLOCATION("ELI15611", ipValue != NULL);

			if (ipValue->HasSpatialInfo() == VARIANT_TRUE)
			{
				ipValue->ReplaceAndDowngradeToHybrid(_bstr_t(zNewText));
			}
			else
			{
				ipValue->ReplaceAndDowngradeToNonSpatial(_bstr_t(zNewText));
			}
		}

		// Adjust height
		ResizeRowHeightsToFit( CGXRange( ).SetTable() );
		return iIndex;
	}

	// No new record available, retain existing highlight information
	return -1;
}
//-------------------------------------------------------------------------------------------------
bool CDataEntryGrid::IsActive()
{
	// Check for selection
	CGXRangeList selList;
	if (CopyRangeList(selList, FALSE))
	{
		return true;
	}
	else
	{
		// No selection, check for current cell
		ROWCOL	nRow = 0;
		ROWCOL	nCol = 0;
		BOOL bResult = GetCurrentCell( nRow, nCol );
		if (bResult == TRUE)
		{
			return true;
		}
	}

	// No selection or no current cell
	return false;
}
//-------------------------------------------------------------------------------------------------
bool CDataEntryGrid::IsAddButtonDisabled()
{
	return m_bDisableAddButton;
}
//-------------------------------------------------------------------------------------------------
bool CDataEntryGrid::IsArrowNavigationDisabled()
{
	//// Always false except for Type D grids
	// Allow disabling of arrow navigation for any grid - 08/31/06 WEL
	return m_bDisableArrowNavigation;
}
//-------------------------------------------------------------------------------------------------
bool CDataEntryGrid::IsRubberbandEnabled()
{
	return m_bAcceptRubberband;
}
//-------------------------------------------------------------------------------------------------
bool CDataEntryGrid::IsSelectedRowEmpty()
{
	bool bEmpty = true;

	// Determine selected row if Swipe is accepted
	CGXRangeList selList;
	if (CopyRangeList( selList, FALSE ))
	{
		CGXRange range = selList.GetTail();
		int nRow = range.top;

		// Check each column in this row - including row header
		for (unsigned int ui = 0; ui <= GetColCount(); ui++)
		{
			// Check text in this cell
			CString zText = GetValueRowCol( nRow, ui );
			if (!zText.IsEmpty())
			{
				bEmpty = false;
				break;
			}
		}
	}
	else
	{
		// There is no row selected but there must be a current cell.
		// Do not assume that it is empty.
		bEmpty = false;
	}

	return bEmpty;
}
//-------------------------------------------------------------------------------------------------
bool CDataEntryGrid::IsSwipingEnabled()
{
	return m_bAcceptSwipe;
}
//-------------------------------------------------------------------------------------------------
BOOL CDataEntryGrid::OnActivateGrid(BOOL bActivate)
{
	// Check for loss of focus while editing
	if (bActivate == FALSE)
	{
		if (GetRowCount() > 0)
		{
			// Get the current cell
			ROWCOL	nRow = 0;
			ROWCOL	nCol = 0;
			BOOL bResult = GetCurrentCell( nRow, nCol );

			// Get CEdit pointer for this non-header cell
			if (bResult && (nRow > 0) && (nCol > 0))
			{
				CGXEditControl* pControl = (CGXEditControl*)GetControl( nRow, nCol ); 
				if (pControl->CGXControl::IsKindOf(CONTROL_CLASS(CGXEditControl)))
				{
					// Get selection positions
					pControl->GetSel( m_nSelStart, m_nSelEnd );
				}
			}
		}
	}

	// Call base class method
	return CBasicGridWnd::OnActivateGrid( bActivate );
}
//-------------------------------------------------------------------------------------------------
void CDataEntryGrid::Populate(IIUnknownVectorPtr ipAttributes)
{
	// Create AFUtility object that can run the query
	IAFUtilityPtr ipAFUtility( CLSID_AFUtility );
	ASSERT_RESOURCE_ALLOCATION("ELI10694", ipAFUtility != NULL);

	// Instantiate the Attributes data member
	if (m_ipAttributesForShow == NULL)
	{
		m_ipAttributesForShow.CreateInstance( CLSID_IUnknownVector );
		ASSERT_RESOURCE_ALLOCATION( "ELI10784", m_ipAttributesForShow != NULL );
	}

	// Store initial collection of Attributes
	m_ipAttributesForShow = ipAttributes;

	// Run the query, if defined
	if (m_strQuery.length() > 0)
	{
		m_ipAttributesForShow = ipAFUtility->QueryAttributes( ipAttributes, 
			m_strQuery.c_str(), VARIANT_FALSE );
	}

	// Check for Default item for Type D grid
	if ((m_ipAttributesForShow->Size() == 0) && (m_strType == "D"))
	{
		// Check for Default Row - not required
		string strTemp = getSetting( gstrGRID_ROWS, false );
		if (!strTemp.empty())
		{
			// Create new Spatial String with default text
			ISpatialStringPtr	ipNewString( CLSID_SpatialString );
			ASSERT_RESOURCE_ALLOCATION( "ELI13260", ipNewString != NULL );
			ipNewString->CreateNonSpatialString(strTemp.c_str(), "");

			// Create new Attribute with default text
			IAttributePtr		ipNewAttr( CLSID_Attribute );
			ASSERT_RESOURCE_ALLOCATION( "ELI13261", ipNewAttr != NULL );
			ipNewAttr->Name = get_bstr_t( "Name" );
			ipNewAttr->Value = ipNewString;

			// Add Attribute to collection
			m_ipAttributesForShow->PushBack( ipNewAttr );
		}
	}

	// Check for Default items for Type C grid
	if (m_strType == "C")
	{
		// Check for Default Rows - not required
		string strTemp = getSetting( gstrGRID_ROWS, false );
		if (!strTemp.empty())
		{
			// Tokenize the text with comma delimiters
			std::vector<std::string>	vecRows;
			StringTokenizer	st( ',' );
			st.parse( strTemp.c_str(), vecRows );

			// Check Query results for each default row
			string strPrevious;
			unsigned long ulCount = vecRows.size();
			for (unsigned int ui = 0; ui < ulCount; ui++)
			{
				// Retrieve this row header
				string strRowInfo = vecRows[ui];
				std::vector<std::string>	vecTemp;
				StringTokenizer	st2( ':' );
				st2.parse( strRowInfo.c_str(), vecTemp );
				if (vecTemp.size() != 2)
				{
					break;
				}
				string strRowHeader = vecTemp[0];

				// Check each Attribute in the Query results
				bool bFoundAttr = false;
				for (int j = 0; j < m_ipAttributesForShow->Size(); j++)
				{
					// Retrieve this Attribute
					IAttributePtr	ipThisAttr = m_ipAttributesForShow->At( j );
					ASSERT_RESOURCE_ALLOCATION( "ELI13262", ipThisAttr != NULL );

					// Get the Attribute Name
					string strAttributeName = asString( ipThisAttr->Name );

					// Compare the Names
					if (strAttributeName == strRowHeader)
					{
						bFoundAttr = true;
						break;
					}
				}

				// Add default row to grid if not found in Query results
				if (!bFoundAttr)
				{
					// Create new Spatial String with default text
					ISpatialStringPtr	ipNewString( CLSID_SpatialString );
					ASSERT_RESOURCE_ALLOCATION( "ELI13263", ipNewString != NULL );
					ipNewString->CreateNonSpatialString(vecTemp[1].c_str(), "");

					// Create new Attribute with default text
					IAttributePtr		ipNewAttr( CLSID_Attribute );
					ASSERT_RESOURCE_ALLOCATION( "ELI13264", ipNewAttr != NULL );
					ipNewAttr->Name = get_bstr_t( strRowHeader );
					ipNewAttr->Value = ipNewString;

					// Get position within collection of last example of previous Attribute
					int iInsertIndex = 0;
					if (!strPrevious.empty())
					{
						// Get position within collection of last example of previous Attribute
						iInsertIndex = findLastAttributeIndex( strPrevious );

						// Increment since this default Attribute should appear 
						// immediately after the previous
						iInsertIndex++;
					}

					// Add Attribute to collection
					if (iInsertIndex < m_ipAttributesForShow->Size())
					{
						// Insert the new Attribute
						m_ipAttributesForShow->Insert( iInsertIndex, ipNewAttr );
					}
					else
					{
						// New index beyond end, just append the Attribute
						m_ipAttributesForShow->PushBack( ipNewAttr );
					}
				}	// end if this item not found in Query results

				// Save this row header as previous
				strPrevious = strRowHeader;
			}		// end for this Default Row item
		}			// end if Default Rows text is defined for this grid
	}				// end if Type C grid

	// Refresh the display
	Refresh();
}
//-------------------------------------------------------------------------------------------------
void CDataEntryGrid::Refresh()
{
	Clear();

	// Operate on the collected Attributes after application of the query
	// NOTE: Action depends on Grid Type
	long lSize = m_ipAttributesForShow->Size();
	for (int i = 0; i < lSize; i++)
	{
		// Retrieve this Attribute
		IAttributePtr	ipAttribute = m_ipAttributesForShow->At( i );
		ASSERT_RESOURCE_ALLOCATION( "ELI10695", ipAttribute != NULL );

		// Active Attribute is zero-relative
		m_lActiveAttribute = i;

		// Use main Attributes for Type A
		if (m_strType == "A")
		{
			// Add the data
			addTypeARow( i + 1, ipAttribute );
		}
		// Use sub-attributes for Type B
		else if (m_strType == "B")
		{
			// Check number of sub-attributes
			IIUnknownVectorPtr ipSubs = ipAttribute->SubAttributes;
			ASSERT_RESOURCE_ALLOCATION("ELI15612", ipSubs != NULL);
			long lNumSub = ipSubs->Size();
			long lRowsNeeded = lNumSub - GetRowCount();
			if (lRowsNeeded > 0)
			{
				// Extend grid size
				InsertRows( 1, lRowsNeeded );
			}

			// Add the data
			addTypeBRows( ipAttribute );
		}
		// Use main Attributes for Type C
		else if (m_strType == "C")
		{
			// Add the data
			addTypeCRow( i + 1, ipAttribute );
		}
		// Use first Attribute for Type D
		else if ((m_strType == "D") && (i == 0))
		{
			// Add the text
			addTypeDText( ipAttribute );
		}
	}

	// Adjust row heights as needed
	ResizeRowHeightsToFit( CGXRange( ).SetTable() );
}
//-------------------------------------------------------------------------------------------------
void CDataEntryGrid::SetActiveRecord(int nItem)
{
	// Continue only if navigation controls are supported
	if (AllowRecordNavigation())
	{
		// Validate nItem
		long lSize = GetRecordCount();
		if ((nItem < 0) || (nItem >= lSize))
		{
			UCLIDException ue("ELI13097", "Invalid record number.");
			ue.addDebugInfo("Requested Record", nItem);
			ue.addDebugInfo("Num Records", lSize);
			throw ue;
		}

		// Get the requested Attribute
		IAttributePtr ipNew = m_ipAttributesForShow->At( nItem );
		ASSERT_RESOURCE_ALLOCATION( "ELI13101", ipNew != NULL);

		//////////////////////
		// Refresh the display
		//////////////////////
		Clear();

		// Use sub-attributes for Type B
		if (m_strType == "B")
		{
			// Check number of sub-attributes
			IIUnknownVectorPtr ipSubs = ipNew->SubAttributes;
			ASSERT_RESOURCE_ALLOCATION("ELI15613", ipSubs != NULL);
			long lNumSub = ipSubs->Size();
			long lRowsNeeded = lNumSub - GetRowCount();
			if (lRowsNeeded > 0)
			{
				// Extend grid size
				InsertRows( 1, lRowsNeeded );
			}

			// Add the data
			addTypeBRows( ipNew );
			m_lActiveAttribute = nItem;
		}
		// Use first Attribute for Type D
		else if (m_strType == "D")
		{
			// Add the text
			addTypeDText( ipNew );
			m_lActiveAttribute = nItem;
		}

		// Resize row height
		ResizeRowHeightsToFit( CGXRange( ).SetTable() );
	}
}
//-------------------------------------------------------------------------------------------------
void CDataEntryGrid::SetRowHeaderLabel(int nRow, std::string strLabel)
{
	// Validate row number
	if (nRow == GetRowCount() + 1)
	{
		// Extend grid size
		InsertRows( nRow, 1 );
	}
	else if ((nRow < 1) || (nRow > (int)GetRowCount() + 1))
	{
		UCLIDException ue( "ELI10657", "Invalid row number for SetRowHeader()." );
		ue.addDebugInfo( "Row Number", nRow );
		ue.addDebugInfo( "Row Count", GetRowCount() );
		throw ue;
	}

	// Unlock header cells
	GetParam()->SetLockReadOnly( FALSE );

	// Set label text
	SetStyleRange(CGXRange( nRow, 0 ),
					CGXStyle()
						.SetValue( strLabel.c_str() )
					);

	// Lock header cells
	GetParam()->SetLockReadOnly( TRUE );
}
//-------------------------------------------------------------------------------------------------
bool CDataEntryGrid::SetID(std::string strID, long lControlID, std::string strINIFile, 
						   CWnd* pParent)
{
	bool	bReturn = false;

	if (isFileOrFolderValid( strINIFile ))
	{
		// Store settings
		m_strINIFile = strINIFile;
		m_strID = strID;
		m_lGridID = lControlID;
		m_pParentWnd = pParent;

		// Create temporary PersistenceMgr
		INIFilePersistenceMgr	mgrSettings( m_strINIFile );

		// Retrieve and store the keys
		m_vecKeys = mgrSettings.getKeysInFolder( getFolder(), true );
		if (m_vecKeys.size() > 0)
		{
			// Setup grid using details from m_vecKeys
			applySettings();

			// Set flag
			bReturn = true;
		}
	}

	return bReturn;
}
//-------------------------------------------------------------------------------------------------
void CDataEntryGrid::UnselectRecords()
{
	// Selection is OFF for the entire table
	SelectRange( CGXRange().SetTable(), FALSE );
}

//-------------------------------------------------------------------------------------------------
// CDataEntryGrid private methods
//-------------------------------------------------------------------------------------------------
void CDataEntryGrid::addTypeARow(int iRow, IAttributePtr ipAttribute)
{
	// Add Attribute Name as Row Header
	string strText = ipAttribute->GetName();
	SetRowHeaderLabel( iRow, strText );

	// Determine number of sub-attributes
	IIUnknownVectorPtr ipSubs = ipAttribute->SubAttributes;
	ASSERT_RESOURCE_ALLOCATION("ELI15614", ipSubs != NULL);
	long lCount = ipSubs->Size();

	long lNumColumns = m_vecColumnHeaders.size();
	std::vector<std::string>	vecRowText;
	bool bDisplayText = true;
	bool bFoundColumn = false;

	// Step through collected sub-attributes
	if (lCount > 0)
	{
		// Build vector of strings - one string for each displayed column
		for (int i = 0; i < lNumColumns; i++)
		{
			// Get sub-attribute text for this column OR
			// empty string if specific sub-attribute is not found
			string strValue = getSubAttributeValue( m_vecColumnHeaders[i], ipAttribute );

			if (strValue != "")
			{
				bFoundColumn = true;
			}

			// Add this text to vector
			vecRowText.push_back( strValue );
		}
	}

	// Check for vector of empty strings or no sub-attributes
	if (!bFoundColumn)
	{
		// Clear a vector of empty strings
		if (lCount > 0)
		{
			vecRowText.clear();
		}

		int iLastColumnIndex = getDefaultColumnIndex();
		if (iLastColumnIndex == -1)
		{
			// Do not display text
			bDisplayText = false;
		}
		else
		{
			// Build vector of empty strings before default column
			for (int j = 1; j < iLastColumnIndex; j++)
			{
				vecRowText.push_back( "" );
			}

			// Get text for display in the Default Column
			ISpatialStringPtr ipValue = ipAttribute->Value;
			ASSERT_RESOURCE_ALLOCATION("ELI15615", ipValue != NULL);
			string strValue = ipValue->String;

			// Add it to the vector
			vecRowText.push_back( strValue );
		}
	}

	// Provide vector to Grid
	if (bDisplayText)
	{
		SetRowInfo( iRow, vecRowText );
	}
}
//-------------------------------------------------------------------------------------------------
void CDataEntryGrid::addTypeBRows(IAttributePtr ipAttribute)
{
	// Determine number of sub-attributes
	IIUnknownVectorPtr ipSubs = ipAttribute->SubAttributes;
	ASSERT_RESOURCE_ALLOCATION("ELI15616", ipSubs != NULL);
	long lCount = ipSubs->Size();

	// Step through collected sub-attributes
	if (lCount > 0)
	{
		for (int i = 0; i < lCount; i++)
		{
			// Get this sub-attribute
			IAttributePtr	ipSub = ipSubs->At( i );
			ASSERT_RESOURCE_ALLOCATION( "ELI10783", ipSub != NULL);

			// Add Sub-attribute Name as Row Header
			string strText = ipSub->GetName();
			SetRowHeaderLabel( i + 1, strText );

			// Get sub-attribute text
			ISpatialStringPtr ipValue = ipSub->Value;
			ASSERT_RESOURCE_ALLOCATION("ELI19436", ipValue != NULL);
			string strValue = ipValue->String;

			// Add this text to vector
			std::vector<std::string>	vecRowText;
			vecRowText.push_back( strValue );

			// Provide single-item vector to Grid
			SetRowInfo( i + 1, vecRowText );
		}
	}
}
//-------------------------------------------------------------------------------------------------
void CDataEntryGrid::addTypeCRow(int iRow, IAttributePtr ipAttribute)
{
	// Add Attribute Name as Row Header
	string strText = ipAttribute->GetName();
	SetRowHeaderLabel( iRow, strText );

	// Get text for display in the single Column
	ISpatialStringPtr ipValue = ipAttribute->Value;
	ASSERT_RESOURCE_ALLOCATION("ELI15617", ipValue != NULL);
	string strValue = ipValue->String;

	// Add this text to vector
	std::vector<std::string>	vecRowText;
	vecRowText.push_back( strValue );

	// Provide vector to Grid
	SetRowInfo( iRow, vecRowText );
}
//-------------------------------------------------------------------------------------------------
void CDataEntryGrid::addTypeDText(IAttributePtr ipAttribute)
{
	// Save Attribute Name
	m_strTypeDName = ipAttribute->GetName();

	// Get Attribute text
	ISpatialStringPtr ipValue = ipAttribute->Value;
	ASSERT_RESOURCE_ALLOCATION("ELI15618", ipValue != NULL);
	string strValue = ipValue->String;

	// Add this text to vector
	std::vector<std::string>	vecRowText;
	vecRowText.push_back( strValue );

	// Provide single-item vector to Grid
	SetRowInfo( 1, vecRowText );
}
//-------------------------------------------------------------------------------------------------
void CDataEntryGrid::applySettings()
{
	// Handle Type
	m_strType = getSetting( gstrGRID_TYPE, true );
	validateType();

	// Handle Height
	string strTemp = getSetting( gstrGRID_HEIGHT, true );
	m_iNumDefaultRows = asLong( strTemp );

	// Handle Query
	m_strQuery = getSetting( gstrQUERY, false );

	// Handle Label
	m_strLabel = getSetting( gstrLABEL, false );

	// Handle Default Column
	m_strDefaultColumn = getSetting( gstrGRID_DEFAULT_COLUMN, false );

	// Handle Swipe and Rubberband
	strTemp = getSetting( gstrSWIPE_SUPPORT, false );
	m_bAcceptSwipe = (strTemp == "1");

	strTemp = getSetting( gstrRUBBERBAND_SUPPORT, false );
	m_bAcceptRubberband = (strTemp == "1");

	// Handle Disabling of Add button
	strTemp = getSetting( gstrDISABLE_ADD_BUTTON, false );
	m_bDisableAddButton = (strTemp == "1");

	// Retrieve number of pixels desired for scrollbar within the cell
	strTemp = getSetting( gstrGRID_SCROLL_PIXELS, false );
	if (!strTemp.empty())
	{
		m_lGridScrollPixels = asLong( strTemp );
	}

	// Handle Disabling of Up & Down & Left & Right navigation - only valid for Type D
	strTemp = getSetting( gstrDISABLE_ARROW_NAVIGATION, false );
	m_bDisableArrowNavigation = (strTemp == "1");

	// Handle Dynamic Input RSD file
	m_strDynamicInputRSDFile = getSetting( gstrDYNAMIC_RSD, false );

	// Handle Column Headers
	if (m_strType == "A")
	{
		// Retrieve and parse text
		strTemp = getSetting( gstrGRID_COLUMNS, true );
		parseColumnHeadings( strTemp );
	}

	// Handle Row Headers - default width is 20%
	if (m_strType != "D")
	{
		// Retrieve and parse text
		strTemp = getSetting( gstrGRID_ROW_HEADER_WIDTH, false );
		m_iRowHeaderWidth = asLong( strTemp );
	}

	// Setup grid control, label, buttons
	setupControls();
}
//-------------------------------------------------------------------------------------------------
bool CDataEntryGrid::subAttributeNameIsColumnName(IAttributePtr ipAttribute)
{
	if (m_strType != "A")
	{
		UCLIDException ue("ELI13109", "Must test sub-attributes names on Type A grid!");
		ue.addDebugInfo( "Actual Type", m_strType );
		throw ue;
	}

	// Check each sub-attribute
	IIUnknownVectorPtr ipSubs = ipAttribute->SubAttributes;
	ASSERT_RESOURCE_ALLOCATION("ELI15619", ipSubs != NULL);
	long lSubAttrCount = ipSubs->Size();
	for (int i = 0; i < lSubAttrCount; i++)
	{
		// Get this sub-attribute
		IAttributePtr ipSub = ipSubs->At( i );
		ASSERT_RESOURCE_ALLOCATION( "ELI10770", ipSub != NULL);

		// Get name of this sub-attribute
		string strName = ipSub->GetName();

		// Compare names against column names
		unsigned long ulColumnCount = m_vecColumnHeaders.size();
		for (unsigned int uj = 0; uj < ulColumnCount; uj++)
		{
			string strColumn = m_vecColumnHeaders[uj];
			if (strColumn.compare( strName.c_str() ) == 0)
			{
				// A match was found, just return true
				return true;
			}
		}
	}

	// No sub-attribute names matched any column names
	return false;
}
//-------------------------------------------------------------------------------------------------
int CDataEntryGrid::findLastAttributeIndex(std::string strName)
{
	int nIndex = -1;

	// Check each Attribute
	int iSize = m_ipAttributesForShow->Size();
	for (int j = 0; j < iSize; j++)
	{
		// Retrieve this Attribute
		IAttributePtr	ipThisAttr = m_ipAttributesForShow->At( j );
		ASSERT_RESOURCE_ALLOCATION( "ELI13934", ipThisAttr != NULL );

		// Get the Attribute Name
		string strAttributeName = asString( ipThisAttr->Name );

		// Compare the Names
		if (strAttributeName == strName)
		{
			// Names match, save this index
			nIndex = j;
		}
	}

	return nIndex;
}
//-------------------------------------------------------------------------------------------------
int CDataEntryGrid::getDefaultColumnIndex()
{
	int iColumn = -1;

	if (m_strDefaultColumn.length() > 0)
	{
		int iCount = m_vecColumnHeaders.size();
		for (int i = 0; i < iCount; i++)
		{
			// Get this column header
			string strHeader = m_vecColumnHeaders[i];

			// Compare strings
			if (m_strDefaultColumn.compare( strHeader.c_str() ) == 0)
			{
				// Column numbers are 1-relative
				iColumn = i + 1;
				break;
			}
		}
	}

	return iColumn;
}
//-------------------------------------------------------------------------------------------------
std::string CDataEntryGrid::getFolder()
{
	// Build folder from INI file and section name
	string strFolder = m_strINIFile;
	strFolder += "\\";
	strFolder += m_strID;

	return strFolder;
}
//-------------------------------------------------------------------------------------------------
IMiscUtilsPtr CDataEntryGrid::getMiscUtils()
{
	if (m_ipMiscUtils == NULL)
	{
		m_ipMiscUtils.CreateInstance( CLSID_MiscUtils );
		ASSERT_RESOURCE_ALLOCATION( "ELI11104", m_ipMiscUtils != NULL );
	}
	
	return m_ipMiscUtils;
}
//-------------------------------------------------------------------------------------------------
std::string	CDataEntryGrid::getSetting(std::string strKey, bool bRequired)
{
	// Create temporary PersistenceMgr
	INIFilePersistenceMgr	mgrSettings( m_strINIFile );

	// Get value from key
	string strTemp = mgrSettings.getKeyValue( getFolder(), strKey, "" );

	// Remove any leading and trailing whitespace
	strTemp = trim( strTemp, " \t", " \t" );

	// Check result
	if (strTemp.size() == 0 && bRequired)
	{
		UCLIDException ue( "ELI10631", "Required setting not defined." );
		ue.addDebugInfo( "Required Setting", strKey );
		throw ue;
	}

	return strTemp;
}
//-------------------------------------------------------------------------------------------------
IAttributePtr CDataEntryGrid::getSubAttribute(std::string strSubAttrName, IAttributePtr ipAttribute)
{
	// Examine sub-attribute names
	IIUnknownVectorPtr ipSubs = ipAttribute->SubAttributes;
	ASSERT_RESOURCE_ALLOCATION("ELI15620", ipSubs != NULL);
	long lSubAttrCount = ipSubs->Size();
	for (int i = 0; i < lSubAttrCount; i++)
	{
		// Get this sub-attribute
		IAttributePtr ipSub = ipSubs->At( i );
		ASSERT_RESOURCE_ALLOCATION( "ELI13680", ipSub != NULL);

		// Get name of this sub-attribute
		string strName = ipSub->GetName();

		// Compare names
		if (strName.compare( strSubAttrName.c_str() ) == 0)
		{
			// Names match, return this sub-attribute
			return ipSub;
		}
	}

	return NULL;
}
//-------------------------------------------------------------------------------------------------
std::string CDataEntryGrid::getSubAttributeValue(std::string strSubAttrName, 
												 IAttributePtr ipAttribute)
{
	string strReturn;

	// Examine sub-attribute names
	IIUnknownVectorPtr ipSubs = ipAttribute->SubAttributes;
	ASSERT_RESOURCE_ALLOCATION("ELI15621", ipSubs != NULL);
	long lSubAttrCount = ipSubs->Size();
	for (int i = 0; i < lSubAttrCount; i++)
	{
		// Get this sub-attribute
		IAttributePtr ipSub = ipSubs->At( i );
		ASSERT_RESOURCE_ALLOCATION( "ELI19389", ipSub != NULL);

		// Get name of this sub-attribute
		string strName = ipSub->GetName();

		// Compare names
		if (strName.compare( strSubAttrName.c_str() ) == 0)
		{
			// Names match, get this Value
			ISpatialStringPtr ipValue = ipSub->Value;
			ASSERT_RESOURCE_ALLOCATION("ELI15622", ipValue != NULL);
			strReturn = ipValue->String;
			break;
		}
	}

	return strReturn;
}
//-------------------------------------------------------------------------------------------------
void CDataEntryGrid::parseColumnHeadings(std::string strColumnData)
{
	// Tokenize data into columns
	std::vector<std::string>	vecColumns;
	StringTokenizer	st( ',' );
	st.parse( strColumnData.c_str(), vecColumns );

	// Tokenize each column into Label and Width
	long lSize = vecColumns.size();
	std::vector<std::string>	vecSingleColumn;
	StringTokenizer	st2( ':' );
	for (int i = 0; i < lSize; i++)
	{
		// Parse info for this column
		vecSingleColumn.clear();
		st2.parse( vecColumns[i], vecSingleColumn );

		// Expecting two items: Header label and Column width
		if (vecSingleColumn.size() == 2)
		{
			m_vecColumnHeaders.push_back( vecSingleColumn[0] );

			long lWidth = asLong( vecSingleColumn[1] );
			m_vecColumnWidths.push_back( lWidth );
		}
	}
}
//-------------------------------------------------------------------------------------------------
void CDataEntryGrid::setTypeACell(int iRow, int iCol, IAttributePtr ipAttr, std::string strText)
{
	// Get column name
	string strName = m_vecColumnHeaders[iCol-1];

	// Retrieve this sub-attribute, if present
	IAttributePtr	ipSub = getSubAttribute( strName, ipAttr );

	// Update the text for this spatial string
	if (ipSub)
	{
		// Get the Value
		ISpatialStringPtr ipValue = ipSub->Value;
		ASSERT_RESOURCE_ALLOCATION("ELI15623", ipValue != NULL);

		if (ipValue->HasSpatialInfo() == VARIANT_TRUE)
		{
			ipValue->ReplaceAndDowngradeToHybrid(strText.c_str());
		}
		else
		{
			ipValue->ReplaceAndDowngradeToNonSpatial(strText.c_str());
		}
	}

	// Update the text in this cell
	SetStyleRange(CGXRange( iRow, iCol ),
					CGXStyle()
						.SetValue( strText.c_str() )
					);
}
//-------------------------------------------------------------------------------------------------
void CDataEntryGrid::setupControls()
{
	// Determine available width for grid
	CRect	rectDlg;
	GetParent()->GetWindowRect( &rectDlg );
	long lAvailablePixels = rectDlg.Width() - 2 * giOFFSET_EDGE;

	// Column headers and widths are defined iff Type = A
	std::vector<int>	vecColWidths;
	if (m_strType != "A")
	{
		// One column is needed for data
		m_vecColumnHeaders.clear();
		m_vecColumnHeaders.push_back( "" );

		// Allow DoResize to use entire width
		vecColWidths.push_back( 0 );
	}
	else
	{
		long lCount = m_vecColumnWidths.size();
		for (int j = 0; j < lCount; j++)
		{
			long lWidth = m_vecColumnWidths[j];
			vecColWidths.push_back( lWidth * lAvailablePixels / 100 );
		}
	}

	// Prepare empty collection of row headers - no blank rows at start
	std::vector<std::string>	vecRowHdrs;

	// Prepare row header width - not used for Type D
	long lWidth = m_iRowHeaderWidth * lAvailablePixels / 100;

	// Describe grid type
	bool bShowColHdrs = (m_strType == "A");
	bool bShowRowHdrs = (m_strType != "D");

	// Final grid setup
	SetControlID( m_lGridID );
	PrepareGrid( m_vecColumnHeaders, vecColWidths, vecRowHdrs, lWidth, bShowColHdrs, 
		bShowRowHdrs );

	// Add scrollbar to single cell for Type D grid
	if (m_strType == "D")
	{
		SetStyleRange(CGXRange().SetCols( 1 ),
						CGXStyle()
							.SetControl( GX_IDS_CTRL_SCROLLEDIT )
						);
	}

	// Entire grid has top-centered items (P16 #1517)
	SetStyleRange(CGXRange().SetTable(),
					CGXStyle()
						.SetVerticalAlignment( DT_TOP )
					);

	// Set font of column headers to non-bold (P16 #1520)
	ChangeColHeaderStyle( CGXStyle()  
		.SetFont( CGXFont().SetBold(FALSE) ), gxOverride ); 

	// Find position of associated grid
	RECT rectGrid;
	GetWindowRect( &rectGrid );

	// Get approximate size of toolbar plus 2 pixels on each side for space
	int iToolHeight = GetSystemMetrics( SM_CYCAPTION );
	iToolHeight *= 2;

	// Add label
	if (m_strLabel.length() > 0)
	{
		// Get default font for static control
		CFont* pFont = NULL;
		CWnd* pStatic = GetParent()->GetDlgItem( IDC_STATIC_TEMP );
		if (pStatic != NULL)
		{
			pFont = pStatic->GetFont();
		}

		// Label is above the top left corner
		RECT rectLabel;
		// Locate bottom of label relative to top of grid
		rectLabel.bottom = rectGrid.top - 1 * giOFFSET_CONTROL - iToolHeight;
		rectLabel.top = rectLabel.bottom - giLABEL_SIZE;
		rectLabel.left = rectGrid.left - 2;
		rectLabel.right = rectLabel.left + 200;

		// Compute ID and set style
		int iLabelID = m_lGridID + IDC_GROUP_MAX * (IDC_OFFSET_LABEL - IDC_OFFSET_GRID);
		DWORD	dwStyle = WS_CHILD | WS_VISIBLE;

		// Create the control and set the font
		m_pLabel = new CStatic;
		m_pLabel->Create( m_strLabel.c_str(), dwStyle, rectLabel, m_pParentWnd, iLabelID );
		if (pFont != NULL)
		{
			m_pLabel->SetFont( pFont );
		}
	}

	// Add and Delete buttons
	if (AllowAddDeleteRecords())
	{
		////////////////
		// Delete button is above the top right corner
		////////////////
		RECT rectButton;
		// Locate bottom of buttons relative to top of grid
		rectButton.bottom = rectGrid.top - 1 * giOFFSET_CONTROL - iToolHeight;
		rectButton.top = rectButton.bottom - giLABEL_SIZE;
		rectButton.right = rectGrid.right - 5;
		rectButton.left = rectButton.right - 30;

		// Compute ID and set style
		int iButtonID = m_lGridID + IDC_GROUP_MAX * (IDC_OFFSET_DELETE - IDC_OFFSET_GRID);
		DWORD	dwStyle = WS_CHILD | WS_VISIBLE;

		m_pDelete = new CButton;
		m_pDelete->Create( "-", dwStyle, rectButton, m_pParentWnd, iButtonID );

		/////////////
		// Add button is next to Delete
		/////////////
		rectButton.right = rectButton.left - giOFFSET_CONTROL;
		rectButton.left = rectButton.right - 30;

		// Compute ID and set style
		iButtonID = m_lGridID + IDC_GROUP_MAX * (IDC_OFFSET_ADD - IDC_OFFSET_GRID);

		m_pAdd = new CButton;
		m_pAdd->Create( "+", dwStyle, rectButton, m_pParentWnd, iButtonID );
	}

	// Previous and Next buttons plus Actual (read-only) edit box
	// TODO: Manage control placement if Add & Remove buttons are also present
	if (AllowRecordNavigation())
	{
		////////////////
		// Next button is above the top right corner
		////////////////
		RECT rectButton;
		// Locate bottom of buttons relative to top of grid
		rectButton.bottom = rectGrid.top - 1 * giOFFSET_CONTROL - iToolHeight;
		rectButton.top = rectButton.bottom - giLABEL_SIZE;
		rectButton.right = rectGrid.right - 5;
		rectButton.left = rectButton.right - 30;

		// Compute ID and set style
		int iButtonID = m_lGridID + IDC_GROUP_MAX * (IDC_OFFSET_NEXT - IDC_OFFSET_GRID);
		DWORD	dwStyle = WS_CHILD | WS_VISIBLE;

		m_pNext = new CButton;
		m_pNext->Create( ">", dwStyle, rectButton, m_pParentWnd, iButtonID );
		m_pNext->EnableWindow( FALSE );

		/////////////
		// Actual edit box is next to Next
		/////////////
		rectButton.right = rectButton.left - giOFFSET_CONTROL;
		rectButton.left = rectButton.right - 60;
		// Make control a little bigger to allow a border
		rectButton.bottom += 2;
		rectButton.top -= 4;

		// Compute ID and set style
		iButtonID = m_lGridID + IDC_GROUP_MAX * (IDC_OFFSET_ACTUAL - IDC_OFFSET_GRID);

		m_pActual = new CEdit;
		DWORD	dwEditStyle = WS_CHILD | WS_VISIBLE | WS_BORDER | ES_READONLY | ES_CENTER;
		m_pActual->Create( dwEditStyle, rectButton, m_pParentWnd, iButtonID );
		m_pActual->SetWindowTextA( "" );

		/////////////
		// Previous button is next to Actual
		/////////////
		rectButton.right = rectButton.left - giOFFSET_CONTROL;
		rectButton.left = rectButton.right - 30;
		// Undo changes to size
		rectButton.bottom -= 2;
		rectButton.top += 4;

		// Compute ID and set style
		iButtonID = m_lGridID + IDC_GROUP_MAX * (IDC_OFFSET_PREVIOUS - IDC_OFFSET_GRID);

		m_pPrevious = new CButton;
		m_pPrevious->Create( "<", dwStyle, rectButton, m_pParentWnd, iButtonID );
		m_pPrevious->EnableWindow( FALSE );
	}
}
//-------------------------------------------------------------------------------------------------
void CDataEntryGrid::validateType()
{
	// Remove leading & trailing whitespace, force to upper case
	m_strType = trim( m_strType, " \t", " \t" );
	makeUpperCase( m_strType );

	if ((m_strType != "A") && (m_strType != "B") && (m_strType != "C") && (m_strType != "D"))
	{
		UCLIDException ue( "ELI10633", "Invalid Grid Type." );
		ue.addDebugInfo( "Grid Type", m_strType );
		throw ue;
	}
}
//-------------------------------------------------------------------------------------------------
