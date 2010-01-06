#include <string>

using namespace std;

class CFileProcessingUtils
{
public:
	CFileProcessingUtils();
	~CFileProcessingUtils();
	// Set tags for File Processors configuration dialog
	static const string ChooseDocTag(HWND hwnd, long x, long y, bool bIncludeSourceDocName=true);
	// Expand tags using FAM Tag manager and expand utility function
	static const string ExpandTagsAndTFE(UCLID_FILEPROCESSINGLib::IFAMTagManager *pFAMTM, 
		const string &strFile, const string &strSourceDocName);

private:
	// return IFAMTagManager pointer
	static UCLID_FILEPROCESSINGLib::IFAMTagManagerPtr getFAMTagManager();
};