#include "stdafx.h"
#include "ObservableEvent.h"
#include "UCLIDException.h"

#include <string>
using namespace std;

#ifdef _DEBUG
#undef THIS_FILE
static char THIS_FILE[]=__FILE__;
#define new DEBUG_NEW
#endif

ObservableEvent::ObservableEvent(const EventID& _eventID) :
	m_eventID(_eventID)
{
}

ObservableEvent::ObservableEvent(const ObservableEvent& oeToCopy)
{
	m_eventID = oeToCopy.m_eventID;
	m_eventData = oeToCopy.m_eventData;
}

ObservableEvent& ObservableEvent::operator=(const ObservableEvent& oeToAssign)
{
	m_eventID = oeToAssign.m_eventID;
	m_eventData = oeToAssign.m_eventData;
	return *this;
}

ObservableEvent::~ObservableEvent(void)
{
	try
	{
	}
	CATCH_AND_LOG_ALL_EXCEPTIONS("ELI16391");
}

const EventData& ObservableEvent::getEventData() const
{
	return m_eventData;
}

const EventID& ObservableEvent::getEventID() const
{
	return m_eventID;
}

void ObservableEvent::addEventData(const string& strDataName, const ValueTypePair& valueTypePair)
{
	m_eventData.add(strDataName, valueTypePair);
}
