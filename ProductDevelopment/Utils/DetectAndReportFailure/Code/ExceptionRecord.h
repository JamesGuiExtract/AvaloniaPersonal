
#pragma once

#include <UPI.h>
#include <UCLIDException.h>

// each logged exception that this dialog is notified of is time stamped and
// kept together with the time stamp in this class
class ExceptionRecord
{
public:
	// ctor, assignment operator and copy ctor
	ExceptionRecord(const std::string& strStringizedException, const UPI& upi);
	ExceptionRecord(const ExceptionRecord& recordToCopy);
	ExceptionRecord& operator=(const ExceptionRecord& recordToAssign);

	// methods to access member variables
	const time_t getTime() const;
	const UCLIDException& getException() const;
	const UPI& getUPI() const;

private:
	time_t m_time;
	UCLIDException m_ex;
	UPI m_UPI;
};
