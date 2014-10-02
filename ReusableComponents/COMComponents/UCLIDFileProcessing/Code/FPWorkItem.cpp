#include "stdafx.h"
#include "FPWorkItem.h"

//-------------------------------------------------------------------------------------------------
// FPWorkItem Constructors
//-------------------------------------------------------------------------------------------------
FPWorkItem::FPWorkItem()
{
	reset();
}

//-------------------------------------------------------------------------------------------------
FPWorkItem::FPWorkItem(const UCLID_FILEPROCESSINGLib::IWorkItemRecordPtr& ipWorkItem)
	:m_ipWorkItemRecord(ipWorkItem), m_status(kWorkUnitPending),m_strException("")
{
}
//-------------------------------------------------------------------------------------------------
FPWorkItem::FPWorkItem(const FPWorkItem& workItem)
{
	copyFrom(workItem);
}
//-------------------------------------------------------------------------------------------------
FPWorkItem::~FPWorkItem(void)
{
}

//-------------------------------------------------------------------------------------------------
// Public methods
//-------------------------------------------------------------------------------------------------
FPWorkItem& FPWorkItem::operator=(const FPWorkItem& workItem)
{
	copyFrom(workItem);
	return *this;
}
//-------------------------------------------------------------------------------------------------
void FPWorkItem::reset()
{
	// reset all member variables
	m_ipWorkItemRecord = __nullptr;
	m_stopWatch.reset();
	m_ipProgressStatus = __nullptr;
	m_strException = "";
	m_status = kWorkUnitPending;
}
//-------------------------------------------------------------------------------------------------
void FPWorkItem::copyFrom(const FPWorkItem& workItem)
{
	m_ipWorkItemRecord = workItem.m_ipWorkItemRecord;
	m_stopWatch = workItem.m_stopWatch;
	m_ipProgressStatus = workItem.m_ipProgressStatus;
	m_strException = workItem.m_strException;
	m_status = workItem.m_status;
}
//-------------------------------------------------------------------------------------------------
void FPWorkItem::markAsStarted()
{
	m_stopWatch.start();
	m_status = kWorkUnitProcessing;

	// Create the progress status object
	m_ipProgressStatus.CreateInstance(CLSID_ProgressStatus);
	ASSERT_RESOURCE_ALLOCATION("ELI37277", m_ipProgressStatus != __nullptr);

	// Initialize the progress status object
	m_ipProgressStatus->InitProgressStatus("Initializing processing...", 0, 1, VARIANT_TRUE);
}
//-------------------------------------------------------------------------------------------------
void FPWorkItem::markAsCompleted()
{
	m_stopWatch.stop();
	m_ipProgressStatus = __nullptr;
	m_status = kWorkUnitComplete;
}
//-------------------------------------------------------------------------------------------------
void FPWorkItem::markAsFailed(const string& strException)
{
	m_stopWatch.stop();
	m_strException = strException;
	m_status = kWorkUnitFailed;
	m_ipProgressStatus = __nullptr;
}
//---------------------------------------------------------------------------------------------
double FPWorkItem::getWorkItemDuration()
{
	return m_stopWatch.getElapsedTime();
}
//-------------------------------------------------------------------------------------------------
SYSTEMTIME FPWorkItem::getStartTime() const
{
	return m_stopWatch.getBeginTime();
}
//-------------------------------------------------------------------------------------------------
string FPWorkItem::getFileName() const
{
	if (m_ipWorkItemRecord == __nullptr)
	{
		UCLIDException ue("ELI37259", "Work item record is not set");
		throw ue;
	}
	return asString(m_ipWorkItemRecord->FileName);
}
//-------------------------------------------------------------------------------------------------
long FPWorkItem::getFileID() const
{
	if (m_ipWorkItemRecord == __nullptr)
	{
		UCLIDException ue("ELI37275", "Work item record is not set");
		throw ue;
	}
	return m_ipWorkItemRecord->FileID;
}
//-------------------------------------------------------------------------------------------------
long FPWorkItem::getWorkItemID() const
{
	if (m_ipWorkItemRecord == __nullptr)
	{
		UCLIDException ue("ELI37260", "Work item record is not set");
		throw ue;
	}
	return m_ipWorkItemRecord->WorkItemID;
}
//-------------------------------------------------------------------------------------------------
long FPWorkItem::getWorkItemGroupID() const
{
	if (m_ipWorkItemRecord == __nullptr)
	{
		UCLIDException ue("ELI37261", "Work item record is not set");
		throw ue;
	}
	return m_ipWorkItemRecord->WorkItemGroupID;
}
//-------------------------------------------------------------------------------------------------
UCLID_FILEPROCESSINGLib::IWorkItemRecordPtr FPWorkItem::getWorkItemRecord()
{
	return m_ipWorkItemRecord;
}
//-------------------------------------------------------------------------------------------------
UCLID_FILEPROCESSINGLib::EFilePriority FPWorkItem::getPriority()
{
	if (m_ipWorkItemRecord == __nullptr)
	{
		UCLIDException ue("ELI37450", "Work item record is not set");
		throw ue;
	}
	return m_ipWorkItemRecord->Priority;
}
//-------------------------------------------------------------------------------------------------
