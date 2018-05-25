#include <string>

using namespace std;

class CFileProcessingUtils
{
public:
	CFileProcessingUtils();
	~CFileProcessingUtils();

	// Expand tags using FAM Tag manager and expand utility function
	static const string ExpandTagsAndTFE(UCLID_FILEPROCESSINGLib::IFAMTagManager *pFAMTM, 
		const string &strFile, const string &strSourceDocName);

	// Set the action status inside the combo box
	static void addStatusInComboBox(CComboBox& comboStatus);

	// Creates an IFileProcessingDB instance in a MTA COM server. Having a MTA instance is important
	// to avoiding deadlocks: https://extract.atlassian.net/browse/ISSUE-12328
	__declspec(dllexport) static UCLID_FILEPROCESSINGLib::IFileProcessingDBPtr createMTAFileProcessingDB();

	// Creates an IProgressStatus instance in a MTA COM server. Having an MTA instance is necessary
	// to allow schema updates from the FAMDBAdmin https://extract.atlassian.net/browse/ISSUE-15385
	__declspec(dllexport) static IProgressStatusPtr createMTAProgressStatus();
};