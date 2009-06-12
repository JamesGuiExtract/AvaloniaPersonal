
#include "stdafx.h"
#include "ObservableSubject.h"

///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
void ObservableSubject::addObserver(Observer* pObserver)
{
	vecObservers.push_back(pObserver);
}

///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
void ObservableSubject::removeObserver(Observer* pObserver)
{
	vector<Observer*>::iterator iter;
	for (iter = vecObservers.begin(); iter != vecObservers.end(); iter++)
	{
		if (*iter == pObserver)
		{
			vecObservers.erase(iter);
			break;
		}
	}
}

///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
void ObservableSubject::notifyObservers(const ObservableEvent& oeEvent)
{
	vector<Observer*>::const_iterator iter;
	for (iter = vecObservers.begin(); iter != vecObservers.end(); iter++)
	{
		(*iter)->onEventFromSubject(this, oeEvent);
	}
}


