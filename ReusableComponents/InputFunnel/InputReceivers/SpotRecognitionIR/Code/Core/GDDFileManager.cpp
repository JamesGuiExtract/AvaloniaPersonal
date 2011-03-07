#include "stdafx.h"
#include "GDDFileManager.h"

#include "SpotRecognitionDlg.h"

#include <UCLIDException.h>
#include <StringTokenizer.h>
#include <CommentedTextFileReader.h>
#include <cpputil.h>

#include <fstream>
#include <io.h>

#ifdef _DEBUG
#undef THIS_FILE
static char THIS_FILE[]=__FILE__;
#define new DEBUG_NEW
#endif

using namespace std;

//--------------------------------------------------------------------------------------------------
GDDFileManager::GDDFileManager(SpotRecognitionDlg *pSpotRecDlg)
: m_pSpotRecognitionDlg(pSpotRecDlg), 
  m_strGDDFileName(""),
  m_strImageFileName(""), 
  m_dScaleFactor(0.0)
{
}
//--------------------------------------------------------------------------------------------------
void GDDFileManager::openGDDFile(const string& strGDDFileName)
{
	m_strGDDFileName = strGDDFileName;
	m_strImageFileName = "";

	// check for existence of the file
	if (!isFileOrFolderValid( strGDDFileName ))
	{
		UCLIDException uclidException("ELI03218", "Input file doesn't exist.");
		uclidException.addDebugInfo("Input File Name", strGDDFileName);
		throw uclidException;
	}

	// check if it's a gdd file, i.e. has extension as .gdd
	if (!sIsGDDFile(strGDDFileName))
	{
		UCLIDException uclidException("ELI03217", "Input file is not a GDD file.");
		uclidException.addDebugInfo("Input File Name", strGDDFileName);
		throw uclidException;
	}

	// create ifstream to open the file first
	ifstream ifs(strGDDFileName.c_str());
	
	// read in the gdd file line by line
	// # is the comment symbol, and we want to skip any empty lines
	CommentedTextFileReader fileReader(ifs, "#", true);
		
	long nCurrentLine = 1;

	try
	{
		string strLine("");

		EInfoType eInfoType = kImageName;

		do
		{
			// read each line
			strLine = fileReader.getLineText();
			if (!strLine.empty())
			{
				// process current line, eInfoType will get 
				// set in processLine()
				processLine(eInfoType, strLine);
				nCurrentLine++;
			}
		}
		while(!ifs.eof());

		m_pSpotRecognitionDlg->m_UCLIDGenericDisplayCtrl.setDocumentModified(FALSE);
	}
	catch (UCLIDException &ue)
	{	
		m_pSpotRecognitionDlg->m_UCLIDGenericDisplayCtrl.setDocumentModified(FALSE);
		// add line# of the gdd file where problem occurs.
		ue.addDebugInfo("Error at Line#", nCurrentLine);
		// if the gdd file is corrupted, display it and prompt user if 
		// they wish to open the image file only
		throw ue;
	}
}
//--------------------------------------------------------------------------------------------------
void GDDFileManager::saveAs(const string& strGDDFileName)
{
	m_strGDDFileName = strGDDFileName;
	m_strImageFileName = "";

	// open the gdd file for writing
	CStdioFile writeFile;
	CFileException fileException;
	if( !writeFile.Open(strGDDFileName.c_str(), CFile::modeCreate | CFile::modeWrite, &fileException))
	{
		m_pSpotRecognitionDlg->m_UCLIDGenericDisplayCtrl.setDocumentModified(FALSE);

		UCLIDException uclidException("ELI03226", "Failed to open GDD file for writing.");
		uclidException.addDebugInfo("GDD File", strGDDFileName);

		TCHAR szError[1024];
		fileException.GetErrorMessage(szError, 1024);
		
		CString zError;
		zError.Format ("Error Message:%s",szError);

		uclidException.addDebugInfo("Error", (string)zError);
		throw uclidException;
	}

	// alway overwrite if the file exists.
	writeFile.SeekToBegin();

	EInfoType eInfoType = kImageName;
	while (eInfoType < kSpecializedCircleEntityData && eInfoType >= kImageName)
	{
		string strLine(retrieveLine(eInfoType));
		// add a carriage return at the end of each line text
		strLine += "\n";
		writeFile.WriteString(strLine.c_str());
	}

	// close the file
	writeFile.Close();
	waitForFileToBeReadable(strGDDFileName);

	m_pSpotRecognitionDlg->m_UCLIDGenericDisplayCtrl.setDocumentModified(FALSE);
}

//**************************************************************************************************
// helper functions
//**************************************************************************************************
//--------------------------------------------------------------------------------------------------
map<string, string> GDDFileManager::getEntityAttributesFromString(const std::string& strAttributes)
{
	map<string, string> mapAttributeToValue;
	// each attribute is speperated by '/'
	// now we have a list of attributes
	vector<string> vecAttributes;
	StringTokenizer::sGetTokens(strAttributes, '/', vecAttributes);

	if (vecAttributes.empty())
	{
		// invalid attribute string
		UCLIDException ue("ELI05799", "Invalid attribute string.");
		ue.addDebugInfo("Attribute String", strAttributes);
		throw ue;
	}

	for (unsigned int ui = 0; ui < vecAttributes.size(); ui++)
	{
		// skip the second and third attributes since they are Entity type and visiblity
		if (ui < 1 || ui > 2)
		{
			// string for each attribute consists two parts: name of the attribute, value.
			// They are separated by ":"
			vector<string> vecValues;
			StringTokenizer::sGetTokens(vecAttributes[ui], ':', vecValues);
			// first item is the name for the attribute, second is the value
			mapAttributeToValue[vecValues[0]] = vecValues[1];
		}
	}

	return mapAttributeToValue;
}
//--------------------------------------------------------------------------------------------------
COLORREF GDDFileManager::getEntityColorFromString(const std::string& strColor)
{
	vector<string> vecRGB;
	StringTokenizer::sGetTokens(strColor, '.', vecRGB);
	if (vecRGB.size() != 3)
	{
		UCLIDException uclidException("ELI03320", "Invalid color code string.");
		uclidException.addDebugInfo("Color code string", strColor);
		throw uclidException;
	}
	long nRed = asLong(vecRGB[0]);
	long nGreen = asLong(vecRGB[1]);
	long nBlue = asLong(vecRGB[2]);

	return RGB(nRed, nGreen, nBlue);
}
//--------------------------------------------------------------------------------------------------
void GDDFileManager::parseEntityData(EInfoType eInfoType, const string& strEntityData)
{
	// first parse the string into a vector of strings
	vector<string> vecEntityData;
	StringTokenizer::sGetTokens(strEntityData, ',', vecEntityData);

	// first item must be the color for the entity
	COLORREF color = getEntityColorFromString(vecEntityData[0]);
	// second item must be the entity attributes
	// Before getting entity attributes from the string, 
	// make sure the string make send to C++ syntex, i.e. new line character, tab, hex notion,
	// etc, shall all be properly converted to C++ readable.
	string strEntityAttribute(vecEntityData[1]);
	convertNormalStringToCppString(strEntityAttribute);
	map<string, string> mapAttributeToValue = getEntityAttributesFromString(strEntityAttribute);

	// get the page that the entity is on
	long nPageNum = asUnsignedLong(mapAttributeToValue["Page"]);

	long nEntityID = 0;
	vector<string> vecTempHolder;
	switch(eInfoType)
	{
	case kLineEntityData:
		{
			// third item in vecEntityData must be the line start position
			StringTokenizer::sGetTokens(vecEntityData[2], ':', vecTempHolder);
			if (vecTempHolder.size() != 2)
			{
				UCLIDException uclidException("ELI03312", "Invalid line entity start position.");
				uclidException.addDebugInfo("Start position string", vecEntityData[2]);
				throw uclidException;
			}
			double dStartX = asDouble(vecTempHolder[0]);
			double dStartY = asDouble(vecTempHolder[1]);
			// fourth item in vecEntityData must be the line end position
			StringTokenizer::sGetTokens(vecEntityData[3], ':', vecTempHolder);
			if (vecTempHolder.size() != 2)
			{
				UCLIDException uclidException("ELI03313", "Invalid line entity end position.");
				uclidException.addDebugInfo("End position string", vecEntityData[3]);
				throw uclidException;
			}
			double dEndX = asDouble(vecTempHolder[0]);
			double dEndY = asDouble(vecTempHolder[1]);
			// add a line entity
			nEntityID = m_pSpotRecognitionDlg->m_UCLIDGenericDisplayCtrl.addLineEntity(
												dStartX, dStartY, dEndX, dEndY, nPageNum);
		}
		break;
	case kCurveEntityData:
		{
			// Curve center
			StringTokenizer::sGetTokens(vecEntityData[2], ':', vecTempHolder);
			if (vecTempHolder.size() != 2)
			{
				UCLIDException uclidException("ELI03314", "Invalid curve center.");
				uclidException.addDebugInfo("Center string", vecEntityData[2]);
				throw uclidException;
			}
			double dCenterX = asDouble(vecTempHolder[0]);
			double dCenterY = asDouble(vecTempHolder[1]);
			// radious
			double dRaious = asDouble(vecEntityData[3]);
			// start angle
			double dStartAng = asDouble(vecEntityData[4]);
			// end angle
			double dEndAng = asDouble(vecEntityData[5]);
			// add a curve entity
			nEntityID = m_pSpotRecognitionDlg->m_UCLIDGenericDisplayCtrl.addCurveEntity(
						dCenterX, dCenterY, dRaious, dStartAng, dEndAng, nPageNum);
		}
		break;
	case kTextEntityData:
		{
			// text insertion point
			StringTokenizer::sGetTokens(vecEntityData[2], ':', vecTempHolder);
			if (vecTempHolder.size() != 2)
			{
				UCLIDException uclidException("ELI03315", "Invalid text insertion point.");
				uclidException.addDebugInfo("Insertion point string", vecEntityData[2]);
				throw uclidException;
			}
			double dInsertPosX = asDouble(vecTempHolder[0]);
			double dInsertPosY = asDouble(vecTempHolder[1]);
			// actual text is at fourth
			// text aligment
			long nAligment = asLong(vecEntityData[4]);
			// text rotation
			double dRotation = asDouble(vecEntityData[5]);
			// text height
			double dHeight = asDouble(vecEntityData[6]);
			// text font is at sixth

			// add the text entity
			nEntityID = m_pSpotRecognitionDlg->m_UCLIDGenericDisplayCtrl.addTextEntity(
						dInsertPosX, dInsertPosY, vecEntityData[3].c_str(), (short)nAligment, 
						dRotation, dHeight, vecEntityData[7].c_str(), nPageNum);
		}
		break;
	case kZoneEntityData:
		{
			// zone start point
			StringTokenizer::sGetTokens(vecEntityData[2], ':', vecTempHolder);
			if (vecTempHolder.size() != 2)
			{
				UCLIDException uclidException("ELI03316", "Invalid zone start point.");
				uclidException.addDebugInfo("Zone start point string", vecEntityData[2]);
				throw uclidException;
			}
			long nStartX = asLong(vecTempHolder[0]);
			long nStartY = asLong(vecTempHolder[1]);
			// zone end point
			StringTokenizer::sGetTokens(vecEntityData[3], ':', vecTempHolder);
			if (vecTempHolder.size() != 2)
			{
				UCLIDException uclidException("ELI03317", "Invalid zone end point.");
				uclidException.addDebugInfo("Zone end point string", vecEntityData[3]);
				throw uclidException;
			}
			long nEndX = asLong(vecTempHolder[0]);
			long nEndY = asLong(vecTempHolder[1]);
			// zone height
			long nHeight = asLong(vecEntityData[4]);
			
			// TODO: read these from the file
			BOOL bFill = TRUE;
			BOOL bBorderVisible = FALSE;
			COLORREF borderColor = RGB(0, 0, 0);

			// add the zone entity
			nEntityID = m_pSpotRecognitionDlg->m_UCLIDGenericDisplayCtrl.addZoneEntity(
						nStartX, nStartY, nEndX, nEndY, nHeight, nPageNum, bFill, 
						bBorderVisible, borderColor);
		}
		break;
	case kSpecializedCircleEntityData:
		{
			// center point
			StringTokenizer::sGetTokens(vecEntityData[2], ':', vecTempHolder);
			if (vecTempHolder.size() != 2)
			{
				UCLIDException uclidException("ELI03318", "Invalid spcialized circle center point.");
				uclidException.addDebugInfo("Center point string", vecEntityData[2]);
				throw uclidException;
			}
			double dCenterX = asDouble(vecTempHolder[0]);
			double dCenterY = asDouble(vecTempHolder[1]);
			// radious percentage
			long nPercentRadious = asLong(vecEntityData[3]);

			// add the specialized circle
			nEntityID = m_pSpotRecognitionDlg->m_UCLIDGenericDisplayCtrl.addSpecializedCircleEntity(
						dCenterX, dCenterY, nPercentRadious, nPageNum);
		}
		break;
	default:
		{
			UCLIDException uclidException("ELI03220", "Invalid entity type.");
			uclidException.addDebugInfo("eInfoType", eInfoType);
			throw uclidException;
		}
		break;
	}

	// set current entity color
	m_pSpotRecognitionDlg->m_UCLIDGenericDisplayCtrl.setEntityColor(nEntityID, color);

	map<string, string>::iterator iter = mapAttributeToValue.begin();
	for (; iter != mapAttributeToValue.end(); iter++)
	{
		// finally, add attributes to the created entity
		m_pSpotRecognitionDlg->m_UCLIDGenericDisplayCtrl.addEntityAttribute(
								nEntityID, iter->first.c_str(), iter->second.c_str());
	}
}
//--------------------------------------------------------------------------------------------------
void GDDFileManager::processLine(EInfoType &eInfoType, const string& strLine)
{
	// trim off the empty space
	CString zTemp(strLine.c_str());
	zTemp.TrimRight(" \t");
	string strToProcess((LPCTSTR)zTemp);

	try
	{
		switch(eInfoType)
		{
		case kImageName:
			{
				// line 1 is the image file name with relative path to current gdd file
				m_strImageFileName = ::getAbsoluteFileName(m_strGDDFileName, strToProcess);
				// set eInfoType to kScaleFactor
				eInfoType = static_cast<EInfoType>(eInfoType + 1);
			}
			break;
		case kScaleFactor:
			{
				m_dScaleFactor = asDouble(strToProcess);
				eInfoType = static_cast<EInfoType>(eInfoType + 1);
			}
			break;
		case kBaseRotation:
			{
				// now we can set image, set scale factor and set base rotation 
				m_pSpotRecognitionDlg->m_UCLIDGenericDisplayCtrl.setImage(m_strImageFileName.c_str());
				// base rotation format string is "page1RotationAngle,page2RotationAngle,page3RotationAngle,...."
				// base rotation shall be per-page base
				vector<string> vecBaseRotations;
				StringTokenizer::sGetTokens(strToProcess, ',', vecBaseRotations);
				// if no delimiter is found and the input is not empty,
				// assume there's only one token
				long nSize = vecBaseRotations.size();
				for (int i=1; i<=nSize; i++)
				{
					m_pSpotRecognitionDlg->m_UCLIDGenericDisplayCtrl.setBaseRotation(i, asDouble(vecBaseRotations[i-1]));
				}
				
				m_pSpotRecognitionDlg->m_UCLIDGenericDisplayCtrl.setScaleFactor(m_dScaleFactor);
				
				// set eInfoType to kNumOfLineEntity
				eInfoType = static_cast<EInfoType>(eInfoType + 1);
			}
			break;
		case kNumOfLineEntity:
		case kNumOfCurveEntity:
		case kNumOfTextEntity:
		case kNumOfZoneEntity:
		case kNumOfSpecializedCircleEntity:
			{
				m_nNumOfEntities = asUnsignedLong(strToProcess);
				
				eInfoType = static_cast<EInfoType>(eInfoType + 1);
				// if number of entity is zero, then proceed to the next entity
				if (m_nNumOfEntities == 0)
				{
					eInfoType = static_cast<EInfoType>(eInfoType + 1);
				}
			}
			break;
		case kLineEntityData:
		case kCurveEntityData:
		case kTextEntityData:
		case kZoneEntityData:
		case kSpecializedCircleEntityData:
			{
				// parse current string into entity color, attribute string and entity data (eg. start
				// and end point, height if any, text if any, etc)
				parseEntityData(eInfoType, strToProcess);
				// decrement the number as current line of text is processed
				m_nNumOfEntities--;
				
				if (m_nNumOfEntities <= 0)
				{
					// if number for current entity data is zero, proceed to the next entity
					eInfoType = static_cast<EInfoType>(eInfoType + 1);
				}
			}
			break;
		default:
			{
				UCLIDException uclidException("ELI03229", "Invalid info type is reached.");
				uclidException.addDebugInfo("eInfoType", eInfoType);
				throw uclidException;
			}
			break;
		}
	}
	catch (CException& ex) 
	{
		char pszCause[256];
		ex.GetErrorMessage(pszCause, 255);

		UCLIDException uclidException("ELI03234", "Failed to process text from GDD file.");
		uclidException.addDebugInfo("Error", pszCause);
		throw uclidException;
	}
	catch (CException* pEx) 
	{
		char pszCause[256];
		pEx->GetErrorMessage(pszCause, 255);
		pEx->Delete();

		UCLIDException uclidException("ELI03235", "Failed to process text from GDD file.");
		uclidException.addDebugInfo("Error", pszCause);
		throw uclidException;
	}

}
//--------------------------------------------------------------------------------------------------
void GDDFileManager::queryEntityIDs(EInfoType eInfoType, 
									vector<unsigned long>& vecEntityIDs)
{
	CString zEntityNameCondition("");
	switch (eInfoType)
	{
	case kNumOfLineEntity:
		zEntityNameCondition = "Type=LINE";
		break;
	case kNumOfCurveEntity:
		zEntityNameCondition = "Type=CURVE";
		break;
	case kNumOfTextEntity:
		zEntityNameCondition = "Type=TEXT";
		break;
	case kNumOfZoneEntity:
		zEntityNameCondition = "Type=ZONE";
		break;
	case kNumOfSpecializedCircleEntity:
		zEntityNameCondition = "Type=CIRCLE";
		break;
	default:
		{
			UCLIDException uclidException("ELI03230", "Invalid info type.");
			uclidException.addDebugInfo("eInfoType", eInfoType);
			throw uclidException;
		}
		break;
	}

	// get all entities' ids that satisfy the condition
	CString zLineEntitiesIDs = m_pSpotRecognitionDlg->m_UCLIDGenericDisplayCtrl.queryEntities(zEntityNameCondition);
	vector<string> vecIDsInString;
	StringTokenizer::sGetTokens((LPCTSTR)zLineEntitiesIDs, ',', vecIDsInString);

	for (unsigned int ui = 0; ui < vecIDsInString.size(); ui++)
	{
		vecEntityIDs.push_back( asUnsignedLong( vecIDsInString[ui] ) );
	}	
}
//--------------------------------------------------------------------------------------------------
CString GDDFileManager::retrieveEntityData(unsigned long nEntityID)
{
	CString cstrToReturn("");
	// get entity color
	long nColor = m_pSpotRecognitionDlg->m_UCLIDGenericDisplayCtrl.getEntityColor(nEntityID);
	cstrToReturn.Format("%d.%d.%d,", GetRValue(nColor), GetGValue(nColor), GetBValue(nColor));
	
	// get the attribute string for each entity, each attribute is separated by "\\n\\n" and "="
	string strEntityAttributes(m_pSpotRecognitionDlg->m_UCLIDGenericDisplayCtrl.getEntityAttributes(nEntityID));
	replaceVariable(strEntityAttributes, "\n\n", "/");
	replaceVariable(strEntityAttributes, "=", ":");
	// replace any ',' with hex notion so that the "," will not interfere with delimiter ","
	replaceASCIICharWithHex(',', strEntityAttributes);

	cstrToReturn += strEntityAttributes.c_str();
	cstrToReturn += ",";
	// get entity info data from each entity, for instance, start and end point, text, height, etc.
	cstrToReturn += m_pSpotRecognitionDlg->m_UCLIDGenericDisplayCtrl.getEntityInfo(nEntityID);

	return cstrToReturn;
}
//--------------------------------------------------------------------------------------------------
CString GDDFileManager::retrieveLine(EInfoType &eInfoType)
{
	CString cstrToReturn("");

	switch(eInfoType)
	{
	case kImageName:
		{
			// get the relative image file name
			string strImageFileName(m_pSpotRecognitionDlg->m_UCLIDGenericDisplayCtrl.getImageName());
			cstrToReturn = ::getRelativeFileName(m_strGDDFileName, strImageFileName).c_str();
			// set eInfoType to kScaleFactor
			eInfoType = static_cast<EInfoType>(eInfoType + 1);
		}
		break;
	case kScaleFactor:
		{
			double dScaleFactor = m_pSpotRecognitionDlg->m_UCLIDGenericDisplayCtrl.getScaleFactor();
			cstrToReturn.Format("%.1f", dScaleFactor);

			eInfoType = static_cast<EInfoType>(eInfoType + 1);
		}
		break;
	case kBaseRotation:
		{
			// base rotation shall be per-page base
			long nTotalPageNum = m_pSpotRecognitionDlg->m_UCLIDGenericDisplayCtrl.getTotalPages();
			for (int i=1; i<=nTotalPageNum; i++)
			{
				CString zBaseRotation("");
				zBaseRotation.Format("%.2f", m_pSpotRecognitionDlg->m_UCLIDGenericDisplayCtrl.getBaseRotation(i));

				if (i > 1)
				{
					cstrToReturn += ",";
				}

				cstrToReturn += zBaseRotation;
			}

			// set eInfoType to kNumOfLineEntity
			eInfoType = static_cast<EInfoType>(eInfoType + 1);
		}
		break;
	case kNumOfLineEntity:
	case kNumOfCurveEntity:
	case kNumOfTextEntity:
	case kNumOfZoneEntity:
	case kNumOfSpecializedCircleEntity:
		{
			// reset the vec for entity ids
			m_vecEntityIDs.clear();
			// get all line entities' ids 
			queryEntityIDs(eInfoType, m_vecEntityIDs);

			// assume the current entity has at least one data info
			eInfoType = static_cast<EInfoType>(eInfoType + 1);
			// if current entity has no entity data
			if (m_vecEntityIDs.size() == 0)
			{
				// set to next entity
				eInfoType = static_cast<EInfoType>(eInfoType + 1);
			}

			cstrToReturn.Format("%d", m_vecEntityIDs.size());
		}
		break;
	case kLineEntityData:
	case kCurveEntityData:
	case kTextEntityData:
	case kZoneEntityData:
	case kSpecializedCircleEntityData:
		{
			// get each entity data
			cstrToReturn = retrieveEntityData(m_vecEntityIDs.back());
			// since the entity id has been used, remove it from the vec
			m_vecEntityIDs.pop_back();

			if (m_vecEntityIDs.size() == 0)
			{
				// proceed to the next line
				eInfoType = static_cast<EInfoType>(eInfoType + 1);
			}
		}
		break;
	default:
		{
			UCLIDException uclidException("ELI03228", "Invalid info type is reached.");
			uclidException.addDebugInfo("eInfoType", eInfoType);
			throw uclidException;
		}
		break;
	}

	return cstrToReturn;
}
//--------------------------------------------------------------------------------------------------
string GDDFileManager::sGetImageNameFromGDDFile(const string& strPathName)
{
	// parse the input GDD file and determine the image name
	ifstream infile(strPathName.c_str());
	string strLine;
	
	// skip all initial comment lines, if any
	do
	{
		getline(infile, strLine);
	} 
	while (!infile.eof() && !strLine.empty() && strLine[0] == '#');

	// return the first non-comment line, which is the image name
	if (infile)
	{
		// if the image file name is not a relative path, then return the absolute path
		if (isValidFile(strLine))
		{
			return strLine;
		}
		// since the image file name in GDD file is a relative path,
		// therefore, use the current GDD file name to get the image file name
		string strDirectory = ::getDirectoryFromFullPath(strPathName) + "\\";
		strLine = strDirectory + strLine;
		return strLine;
	}
	else
	{
		UCLIDException ue("ELI08006", "Unable to determine image name from GDD file!");
		ue.addDebugInfo("strPathName", strPathName);
		throw ue;
	}
}
//--------------------------------------------------------------------------------------------------
bool GDDFileManager::sIsGDDFile(const string& strFileName)
{
	string strExtension = getExtensionFromFullPath(strFileName, true);	
	return strExtension == ".gdd";
}
//--------------------------------------------------------------------------------------------------
