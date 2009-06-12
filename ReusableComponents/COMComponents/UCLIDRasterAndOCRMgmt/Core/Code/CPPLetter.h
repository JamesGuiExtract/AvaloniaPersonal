#pragma once

// This class is used to represent a spatial letter.
// The data members are ordered to be in the same format as the buffer
// used by ISpatialString::GetLetterArray and SetLetterArray.  The Idea
// is that the void* used by those two methods can be typecast to a Letter* 
// (array of letters) and everything should work correctly

#define LTR_ITALIC			0x01
#define LTR_BOLD			0x02
#define LTR_SANSSERIF		0x04
#define LTR_SERIF			0x08
#define LTR_PROPORTIONAL	0x10
#define LTR_UNDERLINE		0x20
#define LTR_SUPERSCRIPT		0x40
#define LTR_SUBSCRIPT		0x80

class CPPLetter
{
public:
	CPPLetter() :
	m_usGuess1(-1), m_usGuess2(-1), m_usGuess3(-1), 
	m_usTop(-1), m_usLeft(-1), m_usRight(-1), m_usBottom(-1), 
	m_usPageNumber(-1),
	m_bIsEndOfParagraph(false),
	m_bIsEndOfZone(false),
	m_bIsSpatial(false),
	m_ucFontSize(0),
	m_ucCharConfidence(100),
	m_ucFont(0)
	{
	}

	CPPLetter(unsigned short usGuess1,
			  unsigned short usGuess2,
			  unsigned short usGuess3,
			  unsigned short usTop,
			  unsigned short usBottom,
			  unsigned short usLeft,
			  unsigned short usRight,
			  unsigned short usPageNumber,
			  bool bIsEndOfParagraph,
			  bool bIsEndOfZone,
			  bool bIsSpatial,
			  unsigned char ucFontSize,
			  unsigned char ucCharConfidence,
			  unsigned char ucFont)
	 : m_usGuess1(usGuess1), 
	   m_usGuess2(usGuess2), 
	   m_usGuess3(usGuess3), 
	   m_usTop(usTop), 
	   m_usBottom(usBottom), 
	   m_usLeft(usLeft), 
	   m_usRight(usRight), 
	   m_usPageNumber(usPageNumber),
	   m_bIsEndOfParagraph(bIsEndOfParagraph),
	   m_bIsEndOfZone(bIsEndOfZone),
	   m_bIsSpatial(bIsSpatial),
	   m_ucFontSize(ucFontSize),
	   m_ucCharConfidence(ucCharConfidence),
	   m_ucFont(ucFont)
	{
	}
	
	//////////////
	// Methods
	//////////////
	inline bool isItalic() const { return (m_ucFont & LTR_ITALIC) != 0; }
	inline bool isBold() const { return (m_ucFont & LTR_BOLD) != 0; }
	inline bool isSansSerif() const { return (m_ucFont & LTR_SANSSERIF) != 0; }
	inline bool isSerif() const { return (m_ucFont & LTR_SERIF) != 0; }
	inline bool isProportional() const { return (m_ucFont & LTR_PROPORTIONAL) != 0; }
	inline bool isUnderline() const { return (m_ucFont & LTR_UNDERLINE) != 0; }
	inline bool isSuperScript() const { return (m_ucFont & LTR_SUPERSCRIPT) != 0; }
	inline bool isSubScript() const { return (m_ucFont & LTR_SUBSCRIPT) != 0; }

	inline void setFlag(unsigned char flag, bool bSet)
	{
		if(bSet)
		{
			m_ucFont |= flag;
		}
		else
		{
			m_ucFont &= (flag ^ 0xFF);
		}
	}

	inline void setItalic(bool bSet) { setFlag(LTR_ITALIC, bSet); }
	inline void setBold(bool bSet) { setFlag(LTR_BOLD, bSet); }
	inline void setSansSerif(bool bSet) { setFlag(LTR_SANSSERIF, bSet); }
	inline void setSerif(bool bSet) { setFlag(LTR_SERIF, bSet); }
	inline void setProportional(bool bSet) { setFlag(LTR_PROPORTIONAL, bSet); }
	inline void setUnderline(bool bSet) { setFlag(LTR_UNDERLINE, bSet); }
	inline void setSuperScript(bool bSet) { setFlag(LTR_SUPERSCRIPT, bSet); }
	inline void setSubScript(bool bSet) { setFlag(LTR_SUBSCRIPT, bSet); }
	//////////////
	// Variables
	//////////////
	// The possible characters that this letter could be
	unsigned short m_usGuess1;
	unsigned short m_usGuess2;
	unsigned short m_usGuess3;
	
	// The spatialBoundaries of the letter
	unsigned short m_usTop;
	unsigned short m_usLeft;
	unsigned short m_usRight;
	unsigned short m_usBottom;
	
	// max number of pages per document is limited to 65535
	// The page on which this character lies
	unsigned short m_usPageNumber;

	// true if this is the last character in a paragraph
	bool m_bIsEndOfParagraph;

	// true if this is the last character in a zone
	bool m_bIsEndOfZone;

	// True if this charcater has spatial information
	// i.e. is a "Spatial Letter"
	bool m_bIsSpatial;

	// This is the font size (in points) of the letter 
	unsigned char m_ucFontSize;

	// This is the font size (in points) of the letter 
	unsigned char m_ucCharConfidence;

	unsigned char m_ucFont;
};
