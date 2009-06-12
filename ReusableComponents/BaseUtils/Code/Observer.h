#ifndef OBSERVER_HPP
#define OBSERVER_HPP

#ifndef BASE_UTIL_H
#include "BaseUtils.h"
#endif
#ifndef VALUETYPEPAIR_HPP
#include "ValueTypePair.h"
#endif
#ifndef OBSERVABLE_EVENT_HPP
#include "ObservableEvent.h"
#endif

#include <map>
#include <list>
using namespace std;

class ObservableSubject;

class EXPORT_BaseUtils Observer
{
  public:
    virtual void onEventFromSubject(ObservableSubject *pSubject, const ObservableEvent& oeEvent) = 0;
};

#endif // OBSERVER_HPP