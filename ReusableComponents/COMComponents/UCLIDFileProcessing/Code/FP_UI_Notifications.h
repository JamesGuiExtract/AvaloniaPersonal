#pragma once

//---------------------------------------------------------------------------------------------
// PURPOSE: To reset the UI to a clean state (as though no processing has occurred)
// WPARAM:	0
// LPARAM:	0
const UINT FP_CLEAR_UI = WM_USER + 2003;
//---------------------------------------------------------------------------------------------
// PURPOSE: To be sent when all processing has completed
// WPARAM:	0
// LPARAM:	0
const UINT FP_PROCESSING_COMPLETE = WM_USER + 2005;
//---------------------------------------------------------------------------------------------
// PURPOSE: When the processing status of a file has changed from "P" to "R", or "R" to "C", 
//			or "R" to "F", this message is sent to the UI.  
//			The UI will update the Processing Log page by:
//			- adding a record to the being processed list (for "P" to "R")
//			- removing a record from the being processed list AND adding a record to the 
//			  completed processing list (for "R" to "C")
//			- removing a record from the being processed list AND adding a record to the 
//			  failed processing list (for "R" to "F")
// WPARAM:	Task ID
// LPARAM:	MAKELPARAM((WORD)eOldStatus, (WORD)eNewStatus)
const UINT FP_STATUS_CHANGE = WM_USER + 2006;
//---------------------------------------------------------------------------------------------
// PURPOSE: To be sent when the processing status of a file supplier has changed.  Status 
//			values include { Inactive, Active, Paused, Stopped, Done }.  The Status property 
//			of the associated FileSupplierData object owned by the File Processing Manager 
//			is also modified before the notification is sent.  The UI will update the status 
//			of the specified File Supplier on the Queue Setup page.
// WPARAM:	EFileSupplierStatus
// LPARAM:	FileSupplier *
const UINT FP_SUPPLIER_STATUS_CHANGE = WM_USER + 2007;
//---------------------------------------------------------------------------------------------
// PURPOSE: When the list of files in a particular state is "full" and another file is to be 
//			added, the first file in the list should be removed.  This message is sent to the 
//			UI to indicate which file should be removed from the appropriate grid on the 
//			Processing Log page
// WPARAM:	File ID
// LPARAM:	eLastStatus
const UINT FP_REMOVE_FILE = WM_USER + 2008;
//---------------------------------------------------------------------------------------------
// PURPOSE: To be sent whenever a queue event is generated.  The message recipient is 
//			responsible for freeing the memory associated with the FileSupplyingRecord 
//			provided in WPARAM.  Upon receipt of this message, the UI will update the Queue 
//			Log page.
// WPARAM:	FileSupplyingRecord *
// LPARAM:	0
const UINT FP_QUEUE_EVENT = WM_USER + 2009;
//---------------------------------------------------------------------------------------------
// PURPOSE: To be sent when all file suppliers (if any) are finished supplying files AND
//			all file processing (if any) has finished.  Upon receipt of this message, the UI 
//			is expected to re-enable the Start Processing toolbar button.  The Queue Setup 
//			page will also re-enable the Add / Remove / Configure buttons as appropriate and 
//			reset File Supplier status from Done to Inactive.
// WPARAM:	0
// LPARAM:	0
const UINT FP_DONE = WM_USER + 2010;
//---------------------------------------------------------------------------------------------
// PURPOSE: To be sent by the statistics thread when the new statistics have been obtained from 
//			the database. Upon receiving this message, the UI is expected to use the ActionStatistics *
//			passed in the WPARAM to update the status bar and statistics page. The ActionStatistics *
//			that is passed in the WPARAM must be deleted bye the receiver of this message.
// WPARAM:  ActionStatistics * with the new stats
// LPARAM:	0
const UINT FP_STATISTICS_UPDATE = WM_USER + 2011;
//---------------------------------------------------------------------------------------------
// PURPOSE: To be sent when the file processing has begun to be cancelled
// WPARAM:	0
// LPARAM:	0
const UINT FP_PROCESSING_CANCELLING = WM_USER + 2012;
//---------------------------------------------------------------------------------------------
// PURPOSE: To notify a window of the new status of the database wrapper object
// WPARAM:  An enum representing the state of the database.  Enum is of type EDatabaseWrapperObjectStatus
enum EDatabaseWrapperObjectStatus
{
	kConnectionNotEstablished,
	kConnectionEstablished,
	kWaitingForLock,
	kConnectionBusy
};

const UINT FP_DB_STATUS_UPDATE = WM_USER + 2012;
//---------------------------------------------------------------------------------------------
