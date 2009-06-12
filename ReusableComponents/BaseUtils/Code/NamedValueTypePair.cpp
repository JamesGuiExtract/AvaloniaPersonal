// NamedValueTypePair.cpp: implementation of the NamedValueTypePair class.

#include "stdafx.h"
#include "NamedValueTypePair.h"
#include "UCLIDException.h"


NamedValueTypePair::NamedValueTypePair()
{
}

NamedValueTypePair::NamedValueTypePair(
	const string& strName,
	const ValueTypePair& valueTypePair) :
	m_strName(strName),
	m_pair(valueTypePair)
{
}

NamedValueTypePair::~NamedValueTypePair()
{
	try
	{
	}
	CATCH_AND_LOG_ALL_EXCEPTIONS("ELI16390");
}


