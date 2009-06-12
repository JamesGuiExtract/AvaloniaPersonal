
#include "stdafx.h"
#include "UCLIDFileProcessing.h"
#include "FileSupplyingRecord.h"

//---------------------------------------------------------------------------------------------
FileSupplyingRecord::FileSupplyingRecord()
:m_ulFileID(0), m_eFSRecordType(kNoAction), m_strOriginalFileName(""), m_strNewFileName(""),
m_ulNumPages(0), m_strFSDescription(""), m_bAlreadyExisted(false), 
m_ePreviousActionStatus(UCLID_FILEPROCESSINGLib::kActionUnattempted),
m_eQueueEventStatus(kQueueEventReceived)
{
}
//---------------------------------------------------------------------------------------------
