#ifndef OBSERVABLE_EVENT_HPP
#define OBSERVABLE_EVENT_HPP

#ifndef BASE_UTIL_H
#include "BaseUtils.h"
#endif
#ifndef EVENT_DATA_HPP
#include "EventData.h"
#endif
#ifndef UNIQUE_ID_HPP
#include "EventID.h"
#endif

class EXPORT_BaseUtils ObservableEvent
{
public:
	ObservableEvent(const EventID& eventID);
	ObservableEvent(const ObservableEvent& oeToCopy);
	virtual ~ObservableEvent(void);
	
	const EventID& getEventID() const;
	const EventData& getEventData() const;

	ObservableEvent& operator = (const ObservableEvent& oeToAssign);

protected:

	void addEventData(const string& strDataName, const ValueTypePair& valueTypePair);

private:
	// event data associated with this event
	EventData m_eventData;
	EventID	m_eventID;
};

#endif // OBSERVABLE_EVENT_HPP
