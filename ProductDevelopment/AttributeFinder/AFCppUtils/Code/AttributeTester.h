#pragma once

#include "AFCppUtils.h"

#include <vector>
#include <set>

using namespace std;

// Represents an interface for objects that test attributes for a particular condition
//
// To use:
// 1) Iterate through attributes to test, calling test method with the attribute name and value
// 2) Continue iterating until the test method returns true (short circuiting evaluation) 
//    or until you run out of attributes to iterate.
// 3) Call getResult() to get the final result of the test
class EXPORT_AFCppUtils IAttributeTester abstract
{
public:
	virtual ~IAttributeTester();

	// Returns the result of the testing on all the attributes tested so far.
	virtual bool getResult() const = 0;

	// Resets the attribute tester to its initial state
	virtual void reset() = 0;

	// Returns true if no further testing is necessary and testing should short circuit evaluation.
	virtual bool test(const string& strName, const string& strValue) = 0;

protected:

	// Returns true if the specified attribute name is a metadata attribute, false otherwise.
	bool isMetadataName(const string& strName);
};

// Represents the ORing of a series of attribute testers
class EXPORT_AFCppUtils AttributeTester : IAttributeTester
{
public:
	AttributeTester();
	~AttributeTester();

	// Adds the specified attribute tester to the collection. 
	// Note: AttributeTester will delete pTester when AttributeTester goes out of scope.
	void addTester(IAttributeTester* pTester);

	// Returns the result of the testing on all the attributes tested so far.
	bool getResult() const;

	// Resets the attribute tester to its initial state
	void reset();

	// Returns true if no further testing is necessary and testing should short circuit.
	bool test(const string& strName, const string& strValue);

private:

	// A vector of the attribute testers to use
	vector<IAttributeTester*> m_vecTesters;

	// A vector of the attribute testers that have short-circuited 
	// evaluation and no longer need to be tested
	vector<IAttributeTester*> m_vecShortcircuitedTesters;
};

// Tests whether an attribute matches a particular set of data types
class EXPORT_AFCppUtils DataTypeAttributeTester : public IAttributeTester
{
public:

	DataTypeAttributeTester(const set<string>& setDataTypes, bool bInitialResult=false,
		bool bCaseSensitive = false);
	virtual ~DataTypeAttributeTester();

	// Returns the result of the testing on all the attributes tested so far.
	bool getResult() const;

	// Resets the attribute tester to its initial state
	virtual void reset();

	// Returns true if no further testing is necessary and testing should short circuit.
	virtual bool test(const string& strName, const string& strValue) = 0;

protected:

	// Result of the current evaluation
	bool m_bResult;

	// Indicates whether the attribute testing should be case sensitive
	bool m_bCaseSensitive;

	// The set of data types against which we are evaluating
	set<string> m_setDataTypes;
};

// Tests whether an attribute does not match any specified data types
class EXPORT_AFCppUtils NoneDataTypeAttributeTester : public DataTypeAttributeTester
{
public:
	NoneDataTypeAttributeTester(const set<string>& setDataTypes);

	// Resets the attribute tester to its initial state
	void reset();

	// Returns true if no further testing is necessary and testing should short circuit.
	bool test(const string& strName, const string& strValue);
};

// Tests whether an attribute matches at least one specified data type
class EXPORT_AFCppUtils AnyDataTypeAttributeTester : public DataTypeAttributeTester
{
public:
	AnyDataTypeAttributeTester(const set<string>& setDataTypes);

	// Returns true if no further testing is necessary and testing should short circuit.
	bool test(const string& strName, const string& strValue);
};

// Tests whether an attribute matches at least one of each specified data type
class EXPORT_AFCppUtils OneOfEachDataTypeAttributeTester : public DataTypeAttributeTester
{
public:
	OneOfEachDataTypeAttributeTester(const set<string>& setDataTypes);

	// Resets the attribute tester to its initial state
	void reset();

	// Returns true if no further testing is necessary and testing should short circuit.
	bool test(const string& strName, const string& strValue);

private:

	// The set of data types that have found at least one match
	set<string> m_setMatchedDataTypes;
};

// Tests whether an attribute matches at least one specified data type and no others
class EXPORT_AFCppUtils OnlyAnyDataTypeAttributeTester : public DataTypeAttributeTester
{
public:
	OnlyAnyDataTypeAttributeTester(const set<string>& setDataTypes);
	
	// Returns true if no further testing is necessary and testing should short circuit.
	bool test(const string& strName, const string& strValue);
};

// Tests whether an attribute matches a particular set of document types
class EXPORT_AFCppUtils DocTypeAttributeTester : public IAttributeTester
{
public:
	DocTypeAttributeTester(const set<string>& setDocTypes);
	~DocTypeAttributeTester();

	// Returns the result of the testing on all the attributes tested so far.
	bool getResult() const;

	// Resets the attribute tester to its initial state
	void reset();

	// Returns true if no further testing is necessary and testing should short circuit.
	bool test(const string& strName, const string& strValue);

private:

	// Result of the current evaluation
	bool m_bResult;

	// Counts the number of DocType attributes found
	int m_iDocTypeCount;

	// The set of document types we are looking for
	set<string> m_setDocTypes;
};
