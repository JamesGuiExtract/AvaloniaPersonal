#pragma once

#include <vector>
#include <string>

using namespace std;

//-------------------------------------------------------------------------------------------------
// Constants
//-------------------------------------------------------------------------------------------------

class RedactionUISettings
{
public:
	RedactionUISettings(void);
	RedactionUISettings(RedactionUISettings &rUISettings);
	~RedactionUISettings(void);

	RedactionUISettings &operator =(RedactionUISettings &rUISettings);

	bool getReviewAllPages();
	void setReviewAllPages(bool bReviewAllPages);
	
	bool getAlwaysOutputImage();
	void setAlwaysOutputImage( bool bAlwaysOutputImage );

	string getOutputImageName();
	void setOutputImageName(const string strOutputImageName);

	bool getAlwaysOutputMeta();
	void setAlwaysOutputMeta(bool bAlwaysOutputMeta);

	string getMetaOutputName();
	void setMetaOutputName(const string strMetaOutputName);

	// added as per [p16 #2833] - JDS 01/28/2008
	// get / set whether the user is required to specify the redaction types
	bool getRequireRedactionTypes();
	void setRequireRedactionTypes(bool bRequireRedactionTypes);

	void Load ( IStream *pStream);
	void Save ( IStream *pStream);

	// Get / set whether or not existing annotations will be saved in 
	// the output file
	void setCarryForwardAnnotations( bool bCarryForward );
	bool getCarryForwardAnnotations();

	// Get / set whether or not redactions will be saved as annotations 
	// in the output file
	void setApplyRedactionsAsAnnotations( bool bApply );
	bool getApplyRedactionsAsAnnotations();

	// Get/set whether to collect feedback
	void setCollectFeedback(bool bCollectFeedback);
	bool getCollectFeedback();

	// Get/set the types of documents from which to collect feedback
	void setFeedbackCollectOption(UCLID_REDACTIONCUSTOMCOMPONENTSLib::EFeedbackCollectOption 
		eFeedbackCollectOption);
	UCLID_REDACTIONCUSTOMCOMPONENTSLib::EFeedbackCollectOption getFeedbackCollectOption();

	// Get/set the folder in which to store feedback data
	void setFeedbackDataFolder(string strFeedbackDataFolder);
	string getFeedbackDataFolder();

	// Get/set whether to collect the original image as a part of the feedback data
	void setCollectFeedbackImage(bool bCollectFeedbackImage);
	bool getCollectFeedbackImage();

	// Get/set whether to use the original feedback image filenames (true) or generate a unique 
	// filename for each image.
	void setFeedbackOriginalFilenames(bool bFeedbackOriginalFilenames);
	bool getFeedbackOriginalFilenames();

	// Get/set the input ID Shield data file to use for verification
	string getInputDataFile();
	void setInputDataFile(const string& strInputDataFile);

	// Get/set whether to use the previous redacted image as input (true) or the original image
	// as input (false)
	bool getInputRedactedImage();
	void setInputRedactedImage(bool bInputRedactedImage);

	// Get/set whether to require the verifier to specify exemption codes for all redactions 
	// before continuing to the next document.
	bool getRequireExemptionCodes();
	void setRequireExemptionCodes(bool bRequireExemptionCodes);

	// Get/set the text to print on output redactions
	string getRedactionText();
	void setRedactionText(const string& strRedactionText);

	// Get/set the border color of output redactions
	COLORREF getBorderColor();
	void setBorderColor(COLORREF crBorderColor);

	// Get/set the fill color of output redactions
	COLORREF getFillColor();
	void setFillColor(COLORREF crFillColor);

	// Get/set the font
	LOGFONT getFont();
	void setFont(const LOGFONT &lgFont);

	// Get/set the font size in points
	int getFontSize();
	void setFontSize(int iFontSize);

private:
	bool m_bReviewAllPages;
	bool m_bAlwaysOutputImage;
	string m_strOutputImageName;
	bool m_bAlwaysOutputMeta;
	string m_strMetaOutputName;

	// ID Shield input data file
	string m_strInputDataFile;

	// Use redacted image as input
	bool m_bInputRedactedImage;

	// Flags to indicate use of annotations
	bool	m_bCarryForwardAnnotations;
	bool	m_bApplyRedactionsAsAnnotations;

	// feedback collection options
	bool m_bCollectFeedback;
	UCLID_REDACTIONCUSTOMCOMPONENTSLib::EFeedbackCollectOption m_eFeedbackCollectOption;
	string m_strFeedbackDataFolder;
	bool m_bCollectFeedbackImage;
	bool m_bFeedbackOriginalFilenames;

	// require users to specify redaction type [p16 #2833]
	bool m_bRequireRedactionTypes;

	// Require users to specify exemption code
	bool m_bRequireExemptionCodes;

	// Text to print on redaction
	string m_strRedactionText;

	// Redaction color options
	COLORREF m_crBorderColor;
	COLORREF m_crFillColor;

	// Font options
	LOGFONT m_lgFont;
	int m_iFontSize; // in points

	// Sets properties to their default state
	void clear();
};
