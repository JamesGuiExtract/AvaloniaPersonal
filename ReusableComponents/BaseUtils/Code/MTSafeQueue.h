#pragma once

#include "UCLIDException.h"
//#include "Win32Semaphore.h"
//#include "Win32Mutex.h"

#include <deque>
#include <vector>
#include <afxmt.h>

template <class T>
class MTSafeQueue
{
public:
	//---------------------------------------------------------------------------------------------
	void push(const T& item)
	{
		// protect access to the queue
		CSingleLock lock( &m_lock, TRUE );
		
		// push new item on to the queue 
		m_qItems.push_back(item);
	}
	//---------------------------------------------------------------------------------------------
	void push(const std::vector<T>& vecItems)
	{
		// protect access to the queue
		CSingleLock lock( &m_lock, TRUE );
		
		int i;
		for(i = 0; i < vecItems.size(); i++)
		{
			// push new item on to the queue 
			m_qItems.push_back(vecItems[i]);
		}
	}
	//---------------------------------------------------------------------------------------------
	void peek(T& rItem)
	{
		// protect access to the queue
		CSingleLock lock( &m_lock, TRUE );
		
		// ensure that the queue is not empty
		if (m_qItems.size() <= 0)
		{
			throw UCLIDException("ELI09025", "Cannot peek an empty queue!");
		}

		// return the top item in the queue and pop the queue
		rItem = m_qItems.front();
	}
	//---------------------------------------------------------------------------------------------
	void getTopAndPop(T& rItem)
	{
		// protect access to the queue
		CSingleLock lock( &m_lock, TRUE );

		// ensure that the queue is not empty
		if (m_qItems.size() <= 0)
		{
			throw UCLIDException("ELI19359", "Cannot pop an empty queue!");
		}

		// return the top item in the queue and pop the queue
		rItem = m_qItems.front();
		m_qItems.pop_front();
	}
	//---------------------------------------------------------------------------------------------
	unsigned long getSize()
	{
		// protect access to the queue
		CSingleLock lock( &m_lock, TRUE );

		return m_qItems.size();
	}
	//---------------------------------------------------------------------------------------------
	void clear()
	{
		// protect access to the queue
		CSingleLock lock( &m_lock, TRUE );
	//	while(m_qItems.size() > 0)
	//	{
	//		m_qItems.pop_front();
	//	}
		m_qItems.clear();
	}
	//---------------------------------------------------------------------------------------------
	void remove(const T& item, bool bRemoveAll = true)
	{
		CSingleLock lock( &m_lock, TRUE );
		int i;
		for(i = 0; i < m_qItems.size(); i++)
		{
			const T& tmp = m_qItems[i];
			if( tmp == item)
			{
				m_qItems.erase(m_qItems.begin() + i);
				i--;
				if(!bRemoveAll)
				{
					break;
				}
			}
		}
	}

private:
	std::deque<T> m_qItems;

	CMutex m_lock;
};
