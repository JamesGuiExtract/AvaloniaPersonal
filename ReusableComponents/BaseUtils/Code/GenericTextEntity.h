#pragma once

#include "BaseUtils.h"

#include <string>
#include <Windows.h>

using namespace std;

class Point;

class EXPORT_BaseUtils GenericTextEntity
{
public:
	GenericTextEntity(const std::string& strGenericText="");
	~GenericTextEntity();

	// these two mehods return strGenericText, passed to the
	// constructor.  Derived classes may (and probably should) call setText()
	// and getText() as appropriate from the overridden methods.
	virtual const std::string& getText();
	virtual void setText(const std::string& strNewText);

	// default implementation returns RGB(0,0,0), and the setColor() call
	// is ignored.
	virtual COLORREF getColor();
	virtual void setColor(const COLORREF& newColor,
		bool bUseLayerColorIfCADBased = true);

	// default implementation compares this obect to another GenericTextEntity
	// object
	virtual bool isEqualTo(const GenericTextEntity* pGenericTextEntity);
	
	// default implementation returns false
	virtual bool isCADBased();

	// following functions are expected to be overridden in derived 
	// text entity classes that are CAD-based.  The default implementation
	// throw's a "not implemented" exception
	virtual Point getCharacterLocation(int iPositionInString);
	virtual void zoom();
	virtual void setInsertionPoint(const Point& insertionPoint);
	virtual Point getCenter();
	virtual void setCenter(const Point& center);
	virtual GenericTextEntity* clone();
	virtual void eraseFromDB();
	virtual std::string getLayer();
	virtual void setLayer(const std::string& strNewLayer);
	virtual void addToDB();

	std::string getSourceUNCPath(void) const {return m_strSourceFileName;}
	void setSourceUNCPath(const std::string& strFileName) {m_strSourceFileName = strFileName;}

protected:
	std::string strGenericText;
	std::string m_strSourceFileName;	// fully qualified file name
};