
#pragma once

#include <string>

// Data structure for each item in a List
typedef	struct tagITEMINFO
{
	int				iIndex;
	unsigned long	ulTime;
	std::string		strData;
} ITEMINFO;

// Column Heading IDs
const unsigned long	EMPTY_LIST_COLUMN =				0;
const unsigned long	TIME_LIST_COLUMN =				1;
const unsigned long	TOP_ELI_COLUMN =				2;
const unsigned long	TOP_EXCEPTION_COLUMN =			3;
const unsigned long	SERIAL_LIST_COLUMN =			4;
const unsigned long	APPLICATION_LIST_COLUMN =		5;
const unsigned long	COMPUTER_LIST_COLUMN =			6;
const unsigned long	USER_LIST_COLUMN =				7;
const unsigned long	PID_LIST_COLUMN =				8;
const unsigned long	DATA_LIST_COLUMN =				9;		// not displayed

const unsigned long	SERIAL_VALUE =			0;
const unsigned long	APPLICATION_VALUE =		1;
const unsigned long	COMPUTER_VALUE =		2;
const unsigned long	USER_VALUE =			3;
const unsigned long	PID_VALUE =				4;
const unsigned long	TIME_VALUE =			5;
const unsigned long EXCEPTION_VALUE	=		6;

//-------------------------------------------------------------------------------------------------
// Sort function declarations for each column
//-------------------------------------------------------------------------------------------------
extern int CALLBACK SerialCompareProc(LPARAM lParam1, LPARAM lParam2, LPARAM lParamSort);
extern int CALLBACK ApplicationCompareProc(LPARAM lParam1, LPARAM lParam2, LPARAM lParamSort);
extern int CALLBACK ELICompareProc(LPARAM lParam1, LPARAM lParam2, LPARAM lParamSort);
extern int CALLBACK ExceptionCompareProc(LPARAM lParam1, LPARAM lParam2, LPARAM lParamSort);
extern int CALLBACK ComputerCompareProc(LPARAM lParam1, LPARAM lParam2, LPARAM lParamSort);
extern int CALLBACK UserCompareProc(LPARAM lParam1, LPARAM lParam2, LPARAM lParamSort);
extern int CALLBACK PidCompareProc(LPARAM lParam1, LPARAM lParam2, LPARAM lParamSort);
extern int CALLBACK TimeCompareProc(LPARAM lParam1, LPARAM lParam2, LPARAM lParamSort);