#include <string>

using namespace std;

class CFileProcessorsUtils
{
public:
	CFileProcessorsUtils();
	~CFileProcessorsUtils();
	// Set tags for File Processors configuration dialog
	static const std::string ChooseDocTag(HWND hwnd, long x, long y);
	// Expand tags using FAM Tag manager and expand utility function
	static const std::string ExpandTagsAndTFE(IFAMTagManager * pFAMTM, const std::string& strFile, const std::string& strSourceDocName);
	//---------------------------------------------------------------------------------------------
	// PROMISE: Returns the cleaned image name corresponding to strImageFileName
	static const string getCleanImageName(const string& strImageFileName);
	//---------------------------------------------------------------------------------------------
	// PROMISE: If the cleaned image exists returns the clean image name corresponding to 
	//          strImageFileName, otherwise returns strImageFileName.
	static const string getCleanImageNameIfExists(const string& strImageFileName);
private:
	// return IFAMTagManager pointer
	static IFAMTagManagerPtr getFAMTagManager();
};