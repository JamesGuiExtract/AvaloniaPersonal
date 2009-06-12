#include "stdafx.h"
#include "GenericTextEntity.h"
#include "Point.h"
#include "UCLIDException.h"

GenericTextEntity::GenericTextEntity(const string& strGenericText)
:strGenericText(strGenericText)
{
}

GenericTextEntity::~GenericTextEntity()
{
	try
	{
	}
	CATCH_AND_LOG_ALL_EXCEPTIONS("ELI16383");
}

bool GenericTextEntity::isCADBased()
{
	return false;
}

Point GenericTextEntity::getCharacterLocation(int /*iPositionInString*/)
{
	if (isCADBased())
	{
		throw UCLIDException("ELI00713", "Default class behavior not overridden!");
	}
	else
	{
		throw UCLIDException("ELI00714", 
			"GenericTextEntity::getCharacterLocation() can only be called on CAD based text entity objects!");
	}
}

void GenericTextEntity::zoom()
{
	if (isCADBased())
		throw UCLIDException("ELI00724", "Default class behavior not overridden!");
	else
		throw UCLIDException("ELI00723", "GenericTextEntity::zoom() can only be called on CAD based text entity objects!");
}

void GenericTextEntity::setInsertionPoint(const Point& /*insertionPoint*/)
{
	if (isCADBased())
	{
		throw UCLIDException("ELI00720", "Default class behavior not overridden!");
	}
	else
	{
		throw UCLIDException("ELI00719", 
			"GenericTextEntity::setInsertionPoint() can only be called on CAD based text entity objects!");
	}
}

Point GenericTextEntity::getCenter()
{
	if (isCADBased())
		throw UCLIDException("ELI00718", "Default class behavior not overridden!");
	else
		throw UCLIDException("ELI00717", "GenericTextEntity::getCenter() can only be called on CAD based text entity objects!");
}

void GenericTextEntity::setCenter(const Point& /*center*/)
{
	if (isCADBased())
	{
		throw UCLIDException("ELI00716", "Default class behavior not overridden!");
	}
	else
	{
		throw UCLIDException("ELI00715", "GenericTextEntity::setCenter() can only be called on CAD based text entity objects!");
	}
}

GenericTextEntity* GenericTextEntity::clone()
{
	if (isCADBased())
		throw UCLIDException("ELI00725", "Default class behavior not overridden!");
	else
		throw UCLIDException("ELI00726", "GenericTextEntity::clone() can only be called on CAD based text entity objects!");
}

void GenericTextEntity::eraseFromDB()
{
	if (isCADBased())
		throw UCLIDException("ELI00727", "Default class behavior not overridden!");
	else
		throw UCLIDException("ELI00728", "GenericTextEntity::eraseFromDB() can only be called on CAD based text entity objects!");
}

string GenericTextEntity::getLayer()
{
	if (isCADBased())
		throw UCLIDException("ELI00729", "Default class behavior not overridden!");
	else
		throw UCLIDException("ELI00730", "GenericTextEntity::getLayer() can only be called on CAD based text entity objects!");
}

void GenericTextEntity::setLayer(const string& /*strNewLayer*/)
{
	if (isCADBased())
	{
		throw UCLIDException("ELI00731", "Default class behavior not overridden!");
	}
	else
	{
		throw UCLIDException("ELI00733", "GenericTextEntity::setLayer() can only be called on CAD based text entity objects!");
	}
}

void GenericTextEntity::addToDB()
{
	if (isCADBased())
		throw UCLIDException("ELI00734", "Default class behavior not overridden!");
	else
		throw UCLIDException("ELI00735", "GenericTextEntity::addToDB() can only be called on CAD based text entity objects!");
}

void GenericTextEntity::setColor(const COLORREF& /*newColor*/,
								 bool /*bUseLayerColorIfCADBased*/)
{
	// this call is ignored in the default implementation
}

COLORREF GenericTextEntity::getColor()
{
	return RGB(0, 0, 0);
}

bool GenericTextEntity::isEqualTo(const GenericTextEntity *pGenericTextEntity)
{
	return strGenericText == pGenericTextEntity->strGenericText;
}

void GenericTextEntity::setText(const string& strNewText)
{
	strGenericText = strNewText;
}

const string& GenericTextEntity::getText()
{
	return strGenericText;
}
