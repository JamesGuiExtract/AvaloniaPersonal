
#include "stdafx.h"
#include "ExceptionRecord.h"

//-------------------------------------------------------------------------------------------------
// ExceptionRecord class
//-------------------------------------------------------------------------------------------------
ExceptionRecord::ExceptionRecord(const string& strStringizedException, const UPI& upi)
: m_UPI(upi)
{
	// create the exception object
	m_ex.createFromString("ELI11875", strStringizedException);

	// store the current time as the time associated with the exception
	time(&m_time);
}
//-------------------------------------------------------------------------------------------------
ExceptionRecord::ExceptionRecord(const ExceptionRecord& recordToCopy)
: m_UPI(recordToCopy.m_UPI)
{
	// assign the object to copy to this object
	*this = recordToCopy;
}
//-------------------------------------------------------------------------------------------------
ExceptionRecord& ExceptionRecord::operator=(const ExceptionRecord& recordToAssign)
{
	// copy the member variables
	m_time = recordToAssign.m_time;
	m_ex = recordToAssign.m_ex;
	m_UPI = recordToAssign.m_UPI;

	return *this;
}
//-------------------------------------------------------------------------------------------------
const time_t ExceptionRecord::getTime() const
{
	return m_time;
}
//-------------------------------------------------------------------------------------------------
const UCLIDException& ExceptionRecord::getException() const
{
	return m_ex;
}
//-------------------------------------------------------------------------------------------------
const UPI& ExceptionRecord::getUPI() const
{
	return m_UPI;
}
//-------------------------------------------------------------------------------------------------

