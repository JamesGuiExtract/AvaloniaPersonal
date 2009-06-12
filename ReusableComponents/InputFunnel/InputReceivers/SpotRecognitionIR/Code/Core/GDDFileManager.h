#pragma once

#include <string>
#include <vector>
#include <map>

class SpotRecognitionDlg;

class GDDFileManager
{
public:
	GDDFileManager(SpotRecognitionDlg *pSpotRecDlg);
	// get current open GDD file name
	const std::string& getCurrentGDDFileName() const {return m_strGDDFileName;}
	// Load the gdd file and open it in SpotRecDlg
	void openGDDFile(const std::string& strGDDFileName);
	// save current file opened in SpotRecDlg into the gdd file
	void saveAs(const std::string& strGDDFileName);
	
	// static methods
	static std::string sGetImageNameFromGDDFile(const std::string& strPathName);
	static bool sIsGDDFile(const std::string& strFileName);

private:
	//*********************************
	// Member variables
	//*********************************
	// info type for the image, for example, image file name, line entity, 
	// text entity, zone entity, circle, etc.
	enum EInfoType {kNoType=0, kImageName, kScaleFactor, kBaseRotation, kNumOfLineEntity,
					kLineEntityData, kNumOfCurveEntity, kCurveEntityData, kNumOfTextEntity,
					kTextEntityData, kNumOfZoneEntity, kZoneEntityData, 
					kNumOfSpecializedCircleEntity, kSpecializedCircleEntityData};

	// access all methods and variable from SpotRecognitionDlg
	SpotRecognitionDlg *m_pSpotRecognitionDlg;

	// GDD file to open
	std::string m_strGDDFileName;

	// underlining image file name
	std::string m_strImageFileName;

	// scale factor
	double m_dScaleFactor;

	// number of entities
	long m_nNumOfEntities;

	std::vector<unsigned long> m_vecEntityIDs;

	//*********************************
	// helper functions
	//*********************************
	// get entity attributes from the string
	std::map<std::string, std::string> getEntityAttributesFromString(const std::string& strColor);

	// get entity color from the string
	COLORREF getEntityColorFromString(const std::string& strColor);

	// parse entity data and put them to the image
	void parseEntityData(EInfoType eInfoType, const std::string& strEntityData);

	// process each line text
	// eInfoType is passed by reference, which means it'll get changed according to
	// the actual info processed from current line text. 
	// For example, if the current line is at type of kNumOfLineEntity, 
	// as this line gets processed, the actually number for line entity is 0,
	// therefore, the next line will be the number for curve entity, eInfoType
	// shall be set to kNumOfCurveEntity.
	void processLine(EInfoType &eInfoType, const std::string& strLine);

	// retrieve all entity ids that is of current entity type, and put them in the vec.
	void queryEntityIDs(EInfoType eInfoType, std::vector<unsigned long>& vecEntityIDs);

	// get entity data from current image
	CString retrieveEntityData(unsigned long nEntityID);

	// retrieve each line of string from Generic Display in order to 
	// write to the gdd file.
	// eInfoType shall be set properly inside this method implementation
	CString retrieveLine(EInfoType &eInfoType);
};