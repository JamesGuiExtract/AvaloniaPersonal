#pragma once

// PURPOSE: To tell Dialog that user left-clicked in the Grid
// WPARAM:	Control ID
// LPARAM:	low-order word = Grid Row, high-order word = Grid Column
// RETURNS: 0
const int WM_NOTIFY_GRID_LCLICK = WM_USER + 100;

// PURPOSE: To tell Dialog that user right-clicked in the Grid
// WPARAM:	Control ID
// LPARAM:	low-order word = Grid Row, high-order word = Grid Column
// RETURNS: 0
const int WM_NOTIFY_GRID_RCLICK = WM_USER + 101;

// PURPOSE: To tell Dialog that grid selection has changed
// WPARAM:	pointer to CGXRange
// LPARAM:	LOWORD(lParam) corresponds to bIsDragging (see notes below)
//			HIWORD(lParam) corresponds to bKey (see notes below)
// RETURNS: 0
// NOTES:	bIsDragging: Specifies if the mouse is pressed yet
//			bKey: Specifies if the user has selected the cells 
//				  by pressing SHIFT and an arrow key
const int WM_NOTIFY_SELECTION_CHANGED = WM_USER + 102;

// PURPOSE: A cell is double clicked
// WPARAM:	Control ID
// LPARAM:	low-order word = Grid Row, high-order word = Grid Column
// RETURNS: 0
const int WM_NOTIFY_CELL_DBLCLK = WM_USER + 103;

// PURPOSE: Parent should move to the next cell in the left direction
//			If at the top-left cell of the active grid, the effect will 
//			be to allow the active cell to move from first cell 
//			of Grid B to the last cell of Grid A.
// WPARAM:	Control ID
// LPARAM:	0
// RETURNS: 0
const int WM_NOTIFY_MOVE_CELL_LEFT = WM_USER + 104;

// PURPOSE: Parent should move to the next cell in the right direction
//			If at the bottom-right cell of the active grid, the effect 
//			will be to allow the active cell to move from last cell 
//			of Grid A to the first cell of Grid B.
// WPARAM:	Control ID
// LPARAM:	0
// RETURNS: 0
const int WM_NOTIFY_MOVE_CELL_RIGHT = WM_USER + 105;

// PURPOSE: Parent should move to the next cell in the up direction
//			If on the top-left cell of the active grid, the effect will 
//			be to allow the active cell to move from first cell 
//			of Grid B to the last cell of Grid A.
// WPARAM:	Control ID
// LPARAM:	0
// RETURNS: 0
const int WM_NOTIFY_MOVE_CELL_UP = WM_USER + 106;

// PURPOSE: Parent should move to the next cell in the down direction
//			If at the bottom-right cell of the active grid, the effect 
//			will be to allow the active cell to move from last cell 
//			of Grid A to the first cell of Grid B.
// WPARAM:	Control ID
// LPARAM:	0
// RETURNS: 0
const int WM_NOTIFY_MOVE_CELL_DOWN = WM_USER + 107;

// PURPOSE: A cell has been modified
// WPARAM:	Control ID
// LPARAM:	low-order word = Grid Row, high-order word = Grid Column
// RETURNS: 0
// NOTE:	[p16 #2722]
const int WM_NOTIFY_CELL_MODIFIED = WM_USER + 108;