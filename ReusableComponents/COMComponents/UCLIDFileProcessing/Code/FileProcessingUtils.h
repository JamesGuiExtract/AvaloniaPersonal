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
};