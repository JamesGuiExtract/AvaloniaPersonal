//==================================================================================================
//
// COPYRIGHT (c) 2008 EXTRACT SYSTEMS, LLC., IN PUBLISHED AND UNPUBLISHED WORKS
// ALL RIGHTS RESERVED.
//
// FILE:	SetOp.cpp
//
// PURPOSE:	To perform specified set operations on specified files of lists. [p13 #4938]
//
// AUTHORS:	Jeff Shergalis
//
// REVISION: Modified on 03/20/2008 as per [p13 #4939] 
//			 Modified to allow multiple lists and complex expressions e.g. ((a+b)-(c*d))*e
//
//==================================================================================================

#include "stdafx.h"
#include "SetOp.h"

#include <UCLIDException.h>
#include <UCLIDExceptionDlg.h>
#include <LicenseMgmt.h>
#include <ComponentLicenseIDs.h>
#include <cpputil.h>
#include <StringTokenizer.h>

#include <string>
#include <set>
#include <map>
#include <stack>
#include <vector>
#include <fstream>
#include <algorithm>
#include <iterator>

using namespace std;

#ifdef _DEBUG
#define new DEBUG_NEW
#endif

DEFINE_LICENSE_MGMT_PASSWORD_FUNCTION;

//--------------------------------------------------------------------------------------------------
// Constants
//--------------------------------------------------------------------------------------------------
const char gcUNION_OPERATOR = '+';
const char gcINTERSECTION_OPERATOR = '*';
const char gcCOMPLEMENT_OPERATOR = '-';
const char gcGROUP_BEGIN = '(';
const char gcGROUP_END = ')';

const string gstrOPERATOR_SYMBOLS = "+*-";
const string gstrGROUP_SYMBOLS = "()";
const string gstrVARIABLE_FILENAME_DELIMETER = "=";

//--------------------------------------------------------------------------------------------------
// Global variables
//--------------------------------------------------------------------------------------------------
map<string, set<string>*> g_mapVarAndList;

//--------------------------------------------------------------------------------------------------
// CSetOpApp
//--------------------------------------------------------------------------------------------------
BEGIN_MESSAGE_MAP(CSetOpApp, CWinApp)
	ON_COMMAND(ID_HELP, &CWinApp::OnHelp)
END_MESSAGE_MAP()

//--------------------------------------------------------------------------------------------------
// CSetOpApp construction
//--------------------------------------------------------------------------------------------------
CSetOpApp::CSetOpApp()
{
}

//--------------------------------------------------------------------------------------------------
// The one and only CSetOpApp object
//--------------------------------------------------------------------------------------------------
CSetOpApp theApp;

//--------------------------------------------------------------------------------------------------
// Helper methods
//--------------------------------------------------------------------------------------------------
// PURPOSE: To validate the license of this application
void validateLicense()
{
	VALIDATE_LICENSE(gnEXTRACT_CORE_OBJECTS, "ELI20572", "SetOp");
}
//--------------------------------------------------------------------------------------------------
// PURPOSE: To display usage message to the user
void displayUsage(const string& strErrMsg)
{
	string strMessage = strErrMsg;
	strMessage += "\n";
	strMessage += "Usage:\n";
	strMessage += "-------------\n";
	strMessage += "SetOp <FileVariableList> <OperationExpression> ";
	strMessage += "<OutputFileName> [/c] [/ef <LogFileName>]\n";
	strMessage += "\nRequired Arguments:\n";
	strMessage += "-------------\n";
	strMessage += "  <FileVariableList> - a comma separated list (containing at least two items) ";
	strMessage += "of variable names and files listed with the following format\n";
	strMessage += "\t<Variable>" + gstrVARIABLE_FILENAME_DELIMETER;
	strMessage += "<FileName>\n\t  ";
	strMessage += "The variable name (consisting of letters and numbers) that will be";
	strMessage += "\n\t  used in the operation expression followed by '=' and the list file name\n";
	strMessage += "  <OperationExpression> - an expression representing the set operation to be ";
	strMessage += "performed (Example a+b)\n";
	strMessage += "  List of operations:\n";
	strMessage += "  -------------\n";
	strMessage += "\t'+' - Performs a union of two lists (will contain no duplicates).\n";
	strMessage += "\t'*' - Performs an intersection of two lists.\n";
	strMessage += "\t'-' - Performs a subtraction of two lists (also known as the set ";
	strMessage += "complement)\n";
	strMessage += "  <OutputFileName> - The name of the file in which the new list will be ";
	strMessage += "printed\n";
	strMessage += "\nOptional Arguments:\n";
	strMessage += "-------------\n";
	strMessage += "  [/c] - force a case sensitive comparison of the lists ";
	strMessage += "(default is case insensitive)\n";
	strMessage += "  [/ef <LogFileName>] - fully qualified path to a file where exceptions will be ";
	strMessage += "logged rather than displayed.\n";
	strMessage += "\nExample:\n";
	strMessage += "-------------\n";
	strMessage += "SetOp a" + gstrVARIABLE_FILENAME_DELIMETER;
	strMessage += "C:\\test1\\listA.txt,b" + gstrVARIABLE_FILENAME_DELIMETER;
	strMessage += "C:\\listB.txt a+b C:\\A_BUnion.txt /ef ";
	strMessage += "C:\\SetOp_ExceptionLog.uex\n\n";

	// display usage message
	AfxMessageBox(strMessage.c_str());
}
//--------------------------------------------------------------------------------------------------
// PURPOSE: To parse the specified list file and add each line of the file to a set.
//			If the case sensitive flag is false, all lines will be made lowercase
void getListFromFile(const string& strListFile, set<string>* psetList, bool bCaseSensitive)
{
	ASSERT_ARGUMENT("ELI21605", psetList != NULL);

	validateFileOrFolderExistence(strListFile);

	ifstream fIn(strListFile.c_str(), ios::in);
	try
	{
		try
		{
			while (!fIn.eof())
			{
				string strLine;
				getline(fIn, strLine);
				if (!strLine.empty())
				{
					if (!bCaseSensitive)
					{
						makeLowerCase(strLine);
					}

					// if the element is already in the set then this file contains
					// duplicate items in its list, throw an exception
					if (!psetList->insert(strLine).second)
					{
						UCLIDException ue("ELI20573", "Duplicate item found in list!");
						ue.addDebugInfo("DuplicateItem", strLine);
						throw ue;
					}
				}
			}

			fIn.close();
		}
		CATCH_ALL_AND_RETHROW_AS_UCLID_EXCEPTION("ELI20574");
	}
	catch(UCLIDException& ue)
	{
		if (fIn.is_open())
		{
			fIn.close();
		}

		throw ue;
	}
}
//--------------------------------------------------------------------------------------------------
// PURPOSE: To parse the command line string containing variable name and file name and
//			then populate a set containing the lines in that file and map the set of
//			lines to the variable name
void parseStringVarList(const string& strArgument, bool bCaseSensitive)
{
	StringTokenizer stSplit(gstrVARIABLE_FILENAME_DELIMETER);
	vector<string> vecTokens;

	// parse the string (should end up with 2 tokens)
	stSplit.parse(strArgument, vecTokens);
	if (vecTokens.size() != 2)
	{
		UCLIDException ue("ELI20575", "Incorrect variable and file name argument!");
		ue.addDebugInfo("Argument", strArgument);
		ue.addDebugInfo("TokenCount", (unsigned int) vecTokens.size());
		throw ue;
	}

	// set to hold the list of elements from the file
	set<string>* psetList = NULL;
	try
	{
		try
		{
			psetList = new set<string>;
			ASSERT_RESOURCE_ALLOCATION("ELI21607", psetList != NULL);

			// build the absolute path to the file
			string strFile = buildAbsolutePath(vecTokens[1]);

			// get the list from the file and place it in the set
			getListFromFile(strFile, psetList, bCaseSensitive);
			
			// insert the set into the map with the variable as the key, throw an error if there
			// is already an entry in the map
			if (!g_mapVarAndList.insert(pair<string, set<string>*>(vecTokens[0], psetList)).second)
			{
				// Clean up the new set before throwing exception
				delete psetList;
				psetList = NULL;

				UCLIDException ue("ELI20576", "Variables names must be unique!");
				ue.addDebugInfo("Variable", vecTokens[0]);
				throw ue;
			}
		}
		CATCH_ALL_AND_RETHROW_AS_UCLID_EXCEPTION("ELI21606");
	}
	catch(UCLIDException& ue)
	{
		if (psetList != NULL)
		{
			delete psetList;
			psetList = NULL;
		}

		throw ue;
	}
}
//--------------------------------------------------------------------------------------------------
// PURPOSE: To tokenize the expression string
void tokenizeExpression(const string& strOperationExpression, vector<string>& rvecTokens)
{
	string strVar("");
	size_t i(0), szLength(strOperationExpression.length());
	
	while (i < szLength)
	{
		char cVal = strOperationExpression[i];

		switch(cVal)
		{
			// case group symbol or operator
		case gcGROUP_BEGIN:
		case gcGROUP_END:
		case gcUNION_OPERATOR:
		case gcINTERSECTION_OPERATOR:
		case gcCOMPLEMENT_OPERATOR:
			{
				// check to see if there was a variable before this operator
				if (!strVar.empty())
				{
					// push the variable onto the vector
					rvecTokens.push_back(strVar);
					
					// clear the variable string so it can store the next variable
					strVar.clear();
				}

				// push the symbol onto the vector
				rvecTokens.push_back(strOperationExpression.substr(i,1));
			}
			break;

			// if it is not a group symbol or operator then assume it is part of
			// a variable name
		default:
			{
				strVar += cVal;
			}
			break;
		}

		i++;
	}

	// check for last variable
	if (!strVar.empty())
	{
		rvecTokens.push_back(strVar);
	}
}
//--------------------------------------------------------------------------------------------------
// PURPOSE: To convert an infix expression (a+b-c) to postfix expression (ab+c-)
//			This algorithm is derived from the following two articles:
//			http://www.codeproject.com/KB/scripting/jsexpressioneval.aspx	
//			http://www.faqts.com/knowledge_base/view.phtml/aid/26065/fid/585
void convertToPostfix(const vector<string>& vecInfixExpression, 
								vector<string>& rvecPostfixExpression)
{
	stack<string> stkOperators;

	// iterate through all of the tokens from the infix expression
	for (vector<string>::const_iterator it = vecInfixExpression.begin();
		it != vecInfixExpression.end(); it++)
	{
		// check for grouping or operator symbol
		if (it->find_first_of(gstrGROUP_SYMBOLS + gstrOPERATOR_SYMBOLS) != it->npos)
		{
			switch ((*it)[0])
			{
				// group begin symbol: push it on the operator stack
			case gcGROUP_BEGIN:
				stkOperators.push(*it);
				break;

				// this is the end of the group:
				// pop the operator stack until we find the beginning group symbol
				// for each operator we find that is not the beginning symbol,
				// push that operator into the expression vector
				// if the stack becomes empty before finding the begin symbol then
				// throw a mismatched grouping symbols exception
			case gcGROUP_END:
				{
					string strVal = stkOperators.top();
					stkOperators.pop();
					while (strVal.find(gcGROUP_BEGIN) == strVal.npos)
					{
						rvecPostfixExpression.push_back(strVal);
						if (stkOperators.empty())
						{
							UCLIDException ue("ELI20592", 
								"Grouping symbol mismatch, found end without matching begin");
							throw ue;
						}
						strVal = stkOperators.top();
						stkOperators.pop();
					}
				}
				break;

				// operation symbol: push it on the operator stack
			case gcUNION_OPERATOR:
			case gcINTERSECTION_OPERATOR:
			case gcCOMPLEMENT_OPERATOR:
				{
					// while there are other operators with higher or same precedence
					// on the stack, pop these operators and push them onto
					// the expression vector
					while (!stkOperators.empty() && 
						(stkOperators.top().find_first_of(gstrOPERATOR_SYMBOLS) 
						!= stkOperators.top().npos))
					{
						rvecPostfixExpression.push_back(stkOperators.top());
						stkOperators.pop();
					}

					stkOperators.push(*it);
				}
				break;

				// sanity check
			default:
				THROW_LOGIC_ERROR_EXCEPTION("ELI20593");
			}
		}
		// not an operator or grouping symbol
		else
		{
			// this is a variable, just add it to the output vector
			rvecPostfixExpression.push_back(*it);
		}
	}

	// after parsing all tokens, any operators left in the stack should be placed
	// into the expression vector
	while (!stkOperators.empty())
	{
		rvecPostfixExpression.push_back(stkOperators.top());
		stkOperators.pop();
	}
}
//--------------------------------------------------------------------------------------------------
// PURPOSE: To perform the specified operation on the specified sets and return the result
//			as a new set 
set<string>* performOperation(char cOperation, set<string>* psetA, 
							 set<string>* psetB)
{
	ASSERT_ARGUMENT("ELI21603", psetA != NULL);
	ASSERT_ARGUMENT("ELI21604", psetB != NULL);

	// set to hold the result of the operation
	set<string>* psetNewSet = NULL;

	try
	{
		try
		{
			// allocate the new set
			psetNewSet = new set<string>;
			ASSERT_RESOURCE_ALLOCATION("ELI21608", psetNewSet != NULL);

			switch(cOperation)
			{
			case gcUNION_OPERATOR:
				// new set should contain all elements of A and B (without duplicates)
				// for efficiency, look for which set is larger and copy that set,
				// then insert the smaller set into the larger
				if (psetA->size() > psetB->size())
				{
					*psetNewSet = *psetA;
					psetNewSet->insert(psetB->begin(), psetB->end());
				}
				else
				{
					*psetNewSet = *psetB;
					psetNewSet->insert(psetA->begin(), psetA->end());
				}
				break;

			case gcINTERSECTION_OPERATOR:
				{
					// compute intersection of setA with setB 
					// (will only contain items common to both)
					insert_iterator<set<string>> set_ins(*psetNewSet, psetNewSet->begin());
					set_intersection(psetA->begin(), psetA->end(), 
						psetB->begin(), psetB->end(), set_ins);
				}
				break;

			case gcCOMPLEMENT_OPERATOR:
				{
					// compute setA / setB (also known as set difference)
					insert_iterator<set<string>> set_ins(*psetNewSet, psetNewSet->begin());
					set_difference(psetA->begin(), psetA->end(),
						psetB->begin(), psetB->end(), set_ins);
				}
				break;

			default:
				// unrecognized operation
				THROW_LOGIC_ERROR_EXCEPTION("ELI20577");
				break;
			}
		}
		CATCH_ALL_AND_RETHROW_AS_UCLID_EXCEPTION("ELI21598");
	}
	catch(UCLIDException& ue)
	{
		// Clean up the new set before throwing the exception
		if (psetNewSet != NULL)
		{
			delete psetNewSet;
			psetNewSet = NULL;
		}

		throw ue;
	}

	// return the new set
	return psetNewSet;
}
//--------------------------------------------------------------------------------------------------
// PURPOSE: To evaluate the provided expression string and return the output set
set<string>* evaluateExpression(const string& strOperationExpression)
{
	// first tokenize the expression
	vector<string> vecInfixTokens;
	tokenizeExpression(strOperationExpression, vecInfixTokens);

	// the vector of tokens we will use to evaluate the expression
	vector<string> vecPostFixTokens;

	// now convert it to PostFix (RPN) notation
	convertToPostfix(vecInfixTokens, vecPostFixTokens);

	// done with infix tokens so clear the vector
	vecInfixTokens.clear();

	// result stack used for expression evaluation
	stack<set<string>*> stkResult;

	try
	{
		try
		{
			// now evaluate the postfix tokens
			for (vector<string>::iterator it = vecPostFixTokens.begin();
				it != vecPostFixTokens.end(); it++)
			{
				// check if token is an operator
				if (it->find_first_of(gstrOPERATOR_SYMBOLS) != it->npos)
				{
					// sanity check, if the stack does not contain two operands then
					// throw an exception
					if (stkResult.size() < 2)
					{
						UCLIDException ue("ELI20594", 
							"Operator with incorrect number of arguments!");
						ue.addDebugInfo("Operator", *it);
						ue.addDebugInfo("OperationExpression", strOperationExpression);
						throw ue;
					}
					else
					{
						// get the operands from the stack
						set<string>* psetB = stkResult.top();
						stkResult.pop();
						set<string>* psetA = stkResult.top();
						stkResult.pop();

						// perform the operation on the sets and push the result
						// back onto the result stack
						set<string>* psetResult = performOperation((*it)[0], psetA, psetB);

						// ensure that if setA and/or setB are no longer needed that they are
						// cleaned up (note: do not want to delete sets that are in the global map)
						bool bFoundA(false), bFoundB(false);
						for (map<string, set<string>*>::iterator it = g_mapVarAndList.begin();
							it != g_mapVarAndList.end(); it++)
						{
							if (!bFoundA && it->second == psetA)
							{
								bFoundA = true;
							}
							else if (!bFoundB && it->second == psetB)
							{
								bFoundB = true;
							}

							// if both sets have been found, no need to keep searching
							if (bFoundA && bFoundB)
							{
								break;
							}
						}
						
						// not found in the global map, then they are temporary sets
						// resulting from calculations and can be deleted
						if (!bFoundA)
						{
							delete psetA;
							psetA = NULL;
						}
						if (!bFoundB)
						{
							delete psetB;
							psetA = NULL;
						}

						// push the result onto the stack
						stkResult.push(psetResult);
					}
				}
				// check for grouping symbol
				else if (it->find_first_of(gstrGROUP_SYMBOLS) != it->npos)
				{
					// there should not be any grouping symbols left, this means there
					// was a grouping symbol mismatch
					throw UCLIDException("ELI20595", 
						"Grouping symbol mismatch, found begin without matching end");
				}
				else
				{
					// this is a variable, check for its definition then
					// get the set and add it to the stack
					map<string, set<string>*>::iterator itSet = g_mapVarAndList.find(*it);
					if (itSet == g_mapVarAndList.end())
					{
						UCLIDException ue("ELI20579", "Undefined variable in expression!");
						ue.addDebugInfo("Variable", *it);
						throw ue;
					}
					else
					{
						// variable exists add to the result stack
						stkResult.push(itSet->second);
					}
				}
			}

			// sanity check, the stack should only have one value at this time, the resulting set
			if (stkResult.size() != 1)
			{

				UCLIDException ue("ELI20596", 
					"Invalid expression - One or more operands are missing operators!");
				ue.addDebugInfo("SetsLeft", (unsigned long) stkResult.size());
				ue.addDebugInfo("OperationExpression", strOperationExpression);
				throw ue;
			}
		}
		CATCH_ALL_AND_RETHROW_AS_UCLID_EXCEPTION("ELI21602");
	}
	catch(UCLIDException& ue)
	{
		// ensure the data in the result stack is cleaned up
		// (only delete elements still in the stack that are not part
		// of the global variable map)
		while (!stkResult.empty())
		{
			// get the top set from the stack
			set<string>* pTemp = stkResult.top();

			// now search for the set in the global map
			bool bFound = false;
			for (map<string, set<string>*>::iterator it = g_mapVarAndList.begin();
				it != g_mapVarAndList.end(); it++)
			{
				if (pTemp == it->second)
				{
					// found the set, set found to true and break
					bFound = true;
					break;
				}
			}

			// not found - delete the set
			if (!bFound)
			{
				delete pTemp;
			}

			// pop this set from the stack
			stkResult.pop();
		}

		throw ue;
	}

	// get the result and pop it off the stack
	set<string>* preturn = stkResult.top();
	stkResult.pop();

	// return the result
	return preturn;
}
//--------------------------------------------------------------------------------------------------
// PURPOSE: To write a set to an output file with one set element per line
void writeOutputSet(const string& strOutputFile, set<string>* psetOutput)
{
	ASSERT_ARGUMENT("ELI21601", psetOutput != NULL);

	// open the file
	ofstream fOut(strOutputFile.c_str(), ios::out);
	try
	{
		try
		{
			// iterate through the set, writing each element and appending a newline
			for (set<string>::iterator it = psetOutput->begin(); 
				it != psetOutput->end(); it++)
			{
				fOut << *it << endl;
			}

			// close the file
			fOut.close();
			waitForFileAccess(strOutputFile, giMODE_READ_ONLY);
		}
		CATCH_ALL_AND_RETHROW_AS_UCLID_EXCEPTION("ELI20580");
	}
	catch(UCLIDException& ue)
	{
		// if an exception occurred, want to ensure the file is closed
		if (fOut.is_open())
		{
			fOut.close();
		}

		throw ue;
	}
}

//--------------------------------------------------------------------------------------------------
// CSetOpApp initialization
//--------------------------------------------------------------------------------------------------
BOOL CSetOpApp::InitInstance()
{
	CWinApp::InitInstance();

	// string to hold the name of the exception log
	string strExceptionLog("");

	// pointer for the resultant set
	set<string>* psetOutput = NULL;

	try
	{
		try
		{
			// Setup exception handling
			UCLIDExceptionDlg exceptionDlg;
			UCLIDException::setExceptionHandler( &exceptionDlg );

			// init license management 
			LicenseManagement::sGetInstance().loadLicenseFilesFromFolder(LICENSE_MGMT_PASSWORD);

			// check license
			validateLicense();

			// check the number of arguments
			if (__argc == 2)
			{
				displayUsage("");
				return FALSE;
			}
			// minimum number of arguments is 3 <FileVariableList> 
			// <OperationExpression> and <OutputFile>
			// the maximum number is 6 [/c] [/ef <FileName>]
			else if (__argc < 4 || __argc > 7)
			{
				displayUsage("Incorrect number of arguments on the command line.\n");
				return FALSE;
			}

			// get the required command line arguments
			string strVarList(__argv[1]);

			// the operation string specified on the command line
			string strOperations(__argv[2]);

			string strOutputFile = buildAbsolutePath(__argv[3]);

			bool bCaseSensitive = false;

			// process optional command line arguments
			for (int i=4; i < __argc; i++)
			{
				string strArg(__argv[i]);
				makeLowerCase(strArg);

				// check for case sensitive flag
				if (strArg == "/c")
				{
					bCaseSensitive = true;
				}
				// check for log exceptions flag
				else if (strArg == "/ef")
				{
					// if log exceptions there should be a file name as next argument
					if (i+1 < __argc)
					{
						strExceptionLog = __argv[++i];
					}
					else
					{
						displayUsage("Missing log file name after parameter /ef!\n");
						return FALSE;
					}
				}
				else
				{
					// invalid command line parameter
					string strErrMessage = string(__argv[i]) + " is an unrecognized parameter.\n";
					displayUsage(strErrMessage);
					return FALSE;
				}
			}

			// tokenize the file variable list by the comma
			StringTokenizer stTokens(',');
			vector<string> vecVarAndList;
			stTokens.parse(strVarList, vecVarAndList);

			if (vecVarAndList.size() < 2)
			{
				string strErrorMessage = "Incorrect file list specified!\nPlease specify at least "
					"two files!\n";
				displayUsage(strErrorMessage);
				return FALSE;
			}
			else
			{
				for (vector<string>::iterator it = vecVarAndList.begin();
					it != vecVarAndList.end(); it++)
				{
					parseStringVarList(*it, bCaseSensitive);
				}
			}

			// parse and perform the operations specified
			psetOutput = evaluateExpression(strOperations);

			// write the new set to the output file
			writeOutputSet(strOutputFile, psetOutput);

			// clean up the final set
			if (psetOutput != NULL)
			{
				delete psetOutput;
				psetOutput = NULL;
			}

			// clean up the global map
			for (map<string, set<string>*>::iterator it = g_mapVarAndList.begin();
				it != g_mapVarAndList.end(); it++)
			{
				delete it->second;
				it->second = NULL;
			}
		}
		CATCH_ALL_AND_RETHROW_AS_UCLID_EXCEPTION("ELI20581");
	}
	catch(UCLIDException& ue)
	{
		// clean up resultant set
		if (psetOutput != NULL)
		{
			delete psetOutput;
			psetOutput = NULL;
		}

		// clean up the global map
		for (map<string, set<string>*>::iterator it = g_mapVarAndList.begin();
			it != g_mapVarAndList.end(); it++)
		{
			if (it->second != NULL)
			{
				delete it->second;
				it->second = NULL;
			}
		}

		// check for exception log file
		if (strExceptionLog.empty())
		{
			ue.display();
		}
		else
		{
			ue.log(strExceptionLog);
		}
	}

	// Since the dialog has been closed, return FALSE so that we exit the
	//  application, rather than start the application's message pump.
	return FALSE;
}
//--------------------------------------------------------------------------------------------------
