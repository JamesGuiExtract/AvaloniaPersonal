#include <string>

class CFileProcessingUtils
{
public:
	CFileProcessingUtils();
	~CFileProcessingUtils();
	// Set tags for File Processors configuration dialog
	static const std::string ChooseDocTag(HWND hwnd, long x, long y);
	// Expand tags using FAM Tag manager and expand utility function
	static const std::string ExpandTagsAndTFE(UCLID_FILEPROCESSINGLib::IFAMTagManager *pFAMTM, const std::string &strFile, const std::string &strSourceDocName);

private:
	// return IFAMTagManager pointer
	static UCLID_FILEPROCESSINGLib::IFAMTagManagerPtr getFAMTagManager();
};