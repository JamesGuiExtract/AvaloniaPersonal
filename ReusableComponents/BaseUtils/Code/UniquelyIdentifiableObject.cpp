#include "stdafx.h"
#include "UniquelyIdentifiableObject.h"
#include "UCLIDException.h"

#ifdef _DEBUG
#undef THIS_FILE
static char THIS_FILE[]=__FILE__;
#define new DEBUG_NEW
#endif


UniquelyIdentifiableObject::UniquelyIdentifiableObject()
{
	m_bOwner = true;
	m_pUniqueAddress = new char;
}

UniquelyIdentifiableObject::UniquelyIdentifiableObject(const UniquelyIdentifiableObject& objToCopy)
{
	m_bOwner = false;
	m_pUniqueAddress = objToCopy.m_pUniqueAddress;
}

UniquelyIdentifiableObject& UniquelyIdentifiableObject::operator=(const UniquelyIdentifiableObject& objToCopy)
{
	m_bOwner = false;
	m_pUniqueAddress = objToCopy.m_pUniqueAddress;
	return *this;
}

UniquelyIdentifiableObject::~UniquelyIdentifiableObject()
{
	try
	{
		if (m_bOwner)
		{
			delete m_pUniqueAddress;
			m_pUniqueAddress = NULL;
		}
	}
	CATCH_AND_LOG_ALL_EXCEPTIONS("ELI16414");
}

bool operator == (const UniquelyIdentifiableObject& obj1,
				  const UniquelyIdentifiableObject& obj2)
{
	return (obj1.m_pUniqueAddress == obj2.m_pUniqueAddress);
}
