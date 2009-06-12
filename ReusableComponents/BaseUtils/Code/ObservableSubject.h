#ifndef OBSERVABLE_SUBJECT_HPP
#define OBSERVABLE_SUBJECT_HPP

#ifndef BASE_UTIL_H
#include "BaseUtils.h"
#endif
#ifndef OBSERVER_HPP
#include "Observer.h"
#endif
#ifndef OBSERVABLE_EVENT_HPP
#include "ObservableEvent.h"
#endif
#ifndef VALUETYPEPAIR_HPP
#include "ValueTypePair.h"
#endif

#include <vector>
using namespace std;

class EXPORT_BaseUtils ObservableSubject
{
public:
	virtual ~ObservableSubject() {}
	void addObserver(Observer* pObserver);
	void removeObserver(Observer* pObserver); 

protected:
    vector<Observer*> vecObservers;
    void notifyObservers(const ObservableEvent& oeEvent);
};

#endif //  OBSERVABLE_SUBJECT_HPP