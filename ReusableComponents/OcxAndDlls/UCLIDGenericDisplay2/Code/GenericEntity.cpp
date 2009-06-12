//==================================================================================================
//
// COPYRIGHT (c) 2000 UCLID SOFTWARE, LLC., IN PUBLISHED AND UNPUBLISHED WORKS
// ALL RIGHTS RESERVED.
//
// FILE:	GenericEntity.cpp
//
// PURPOSE:	This is an implementation file for GenericEntity() class.
//			Where the GenericEntity() class has been declared as base class.
//			The code written in this file makes it possible to implement the various
//			application methods in the user interface.
// NOTES:	
//
// AUTHORS:	Segu Prasad, M.Srinivasa Rao
//
//==================================================================================================
// GenericEntity.cpp : implementation file
//
#include "stdafx.h"
#include "GenericEntity.h"
#include "GenericDisplayView.h"
#include "GenericDisplayFrame.h"
#include "UCLIDException.h"

#include <cmath>

//////////////////////////////////////////////////////////////////////////////
//	GenericEntity message handlers

GenericEntity::GenericEntity(unsigned long id)
 : m_ulID(id), m_bSelected(FALSE)
{
	// Black color for line
	m_color = 0x00000000;

	// Make the entity visible
	m_bVisible = 1;

	m_bVisibleAsInGddFile = 1;

	m_bVisibilityChanged = false;

	// assign default value
	m_pGenericDisplayCtrl = NULL;

	m_ulPageNumber = 0;
}
//==================================================================================================
GenericEntity::GenericEntity(unsigned long id, EntityAttributes& EntAttr, COLORREF Color)
	: m_ulID(id)
{
	//attributes	= EntAttr;
	m_color		= Color;
}
//==================================================================================================
GenericEntity::~GenericEntity()
{
	try
	{
	}
	CATCH_AND_LOG_ALL_EXCEPTIONS("ELI16452");
}
//==================================================================================================
unsigned long GenericEntity::getID()
{
	return m_ulID;
}
//==================================================================================================
const EntityAttributes& GenericEntity::getAttributes() const
{
	return m_attributes;
}
//==================================================================================================
int GenericEntity::modifyAttribute(const string& strAttributeName, const string& strNewValue)
{
	//	find the iterator of the given attribute name
	STR2STR::iterator itAttFind = m_attributes.find(strAttributeName);
	
	//	modify the attribute value for the given attribute name
	if(itAttFind != m_attributes.end())
	{
		// if the new attribute is same as the existing return false
		if (m_attributes[strAttributeName] == strNewValue)
			return 0;

		m_attributes[strAttributeName] = strNewValue;
		return 1;
	}
	else
	{
		// strAttributeName is not defined for this entity.So raise an exception. Moreover
		// as the attribute Visible is added while saving to the file based on m_visible 
		// exclude it from checking
		if (_strcmpi (strAttributeName.c_str(), "Visible") != 0)
		{ 
			return -1;
		}
		return 0;
	}
}
//==================================================================================================
void GenericEntity::addAttribute(const string& strAttributeName, const string& strValue)
{
	if (_strcmpi(strAttributeName.c_str(), "page") == 0)
	{
		// TESTTHIS cast to unsigned long
		m_ulPageNumber = (unsigned long)atof(strValue.c_str());
	}

	//	check for the existance of the given attribute
	STR2STR::iterator itAttFind = m_attributes.find(strAttributeName);

	//	add the attribute name and value
	if(itAttFind == m_attributes.end())
		m_attributes.insert(STR2STR::value_type(strAttributeName, strValue));
}
//==================================================================================================
bool GenericEntity::deleteAttribute(const string& strAttributeName)
{
	//	check for the existance of the given attribute
	STR2STR::iterator itAttFind = m_attributes.find(strAttributeName);
	
	//	delete the given attribute name and value
	if(itAttFind != m_attributes.end())
	{
		m_attributes.erase(itAttFind);
		return true;
	}
	else
	{
		// the attribute to delete is not defined for this entity. 
		// throw an exception if the attribute is not 'visible'
		if (_strcmpi (strAttributeName.c_str(), "Visible") == 0)
		{
			AfxThrowOleDispatchException (0, "ELI90030: The visible attribute can not be deleted");
		}
	}
	return false;
}
//==================================================================================================
COLORREF GenericEntity::getColor() 
{
	return m_color;
}
//==================================================================================================
void GenericEntity::setColor(COLORREF newColor)
{
	m_color = newColor;
}
//==================================================================================================
double GenericEntity::incrementAngle(double dRadius)
{
	double h		= 0.3;				//fixed value
	double h1		= dRadius - h;		// h+h1	= Radius
	double dAngRad	= 0;

	dAngRad			= acos(h1/dRadius);

	// return caluculated increment angle
	return dAngRad;
}
//==================================================================================================
int GenericEntity::getAttributeString(CString &zAttrStr, BOOL bFlag)
{
	char pszBuf1[8];
	char pszBuf2[3];

	CString zTempStr;

	//	get the description
	string strType = getDesc();

	//	set the data to pszBuf1 and pszBuf2 as per the flag
	if(bFlag == 1)
	{
		strcpy_s(pszBuf1, "/");
		strcpy_s(pszBuf2, ":");
	}

	if(bFlag == 0)
	{
		strcpy_s(pszBuf1, "\n\n");	
		strcpy_s(pszBuf2, "=");
	}

	//	remove all the previous data from the string
	zAttrStr.Empty();

	// set the page number
	zAttrStr.Format("Page%s%d", pszBuf2, m_ulPageNumber);
	//	set the TYPE and value 
	zTempStr.Format("%sType%s%s", pszBuf1, pszBuf2, strType.c_str());
	//	concatenate the two strings
	zAttrStr += zTempStr;
	//	set Visible attribute and value
	zTempStr.Format("%sVisible%s%d", pszBuf1, pszBuf2, m_bVisible);

	//	concatenate the two strings
	zAttrStr += zTempStr;

	//	iterator for the map
	STR2STR::iterator itmap;

	//	get the first user defined attribute
	itmap = m_attributes.begin();

	//	check for the end of attributes list
	while(itmap != m_attributes.end())
	{
		//	copy the present user defined attribute name and value into temporary string
		zTempStr.Format("%s%s%s%s", pszBuf1, (itmap->first).c_str(), pszBuf2, (itmap->second).c_str());

		//concatenate the present user defined attribute name and value string to the previous string
		zAttrStr += zTempStr;

		//	get the iterator for the next attribute
		itmap++;
	}

	//	return attributes string final length
	return zAttrStr.GetLength();
}
//==================================================================================================
void GenericEntity::setAttributesFromFile(CString zAttrFileStr)
{
	CString zTemp;
	CString zName;
	CString zValue;
	int iDelimiter = 0;
	CStringArray azAttrStr;

	for(int i = 0; i < zAttrFileStr.GetLength(); i++)
	{

		if(zAttrFileStr[i] == '/' || i == (zAttrFileStr.GetLength() - 1))
		{
			//	add string into array
			azAttrStr.Add(zTemp);

			//	remove data from temporary string
			zTemp.Empty();
		}
		else
			//	add characters to the temporary string
			zTemp += zAttrFileStr[i];
	}

	for(int j = 0; j <= azAttrStr.GetUpperBound(); j++)
	{
		//	find the position of ':'
		int iDelimiter = azAttrStr[j].Find(':');

		//	get the data for attribute name and value
		zName = azAttrStr[j].Mid(0, iDelimiter);
		zValue = azAttrStr[j].Mid(iDelimiter + 1, azAttrStr[j].GetLength() - iDelimiter - 1);
	
		// get the page number which is the first attrib from the entity data
		if(j == 0)
		{
			// TESTTHIS cast to unsigned long
			m_ulPageNumber = (unsigned long)atof(zValue);
		}

		if(j == 2)
		{
			// visible attribute data
			if(zValue[0] == '1')
			{
				m_bVisible = TRUE;
				m_bVisibleAsInGddFile = TRUE;
			}
			else
			{
				m_bVisible = FALSE;
				m_bVisibleAsInGddFile = FALSE;
			}
		}
		else if(j > 2)
		{
			// user defined attributes
			string strName;
			string strValue;
			
			// replacing the attribute string with \n and \r
			zName.Replace("\\x0A","\\n");
			zName.Replace("\\x0D","\\r");
			zValue.Replace("\\x0A","\\n");
			zValue.Replace("\\x0D","\\r");

			//	assign the user defined attribute name and value to the string
			strName.assign(zName);
			strValue.assign(zValue);

			//	add the attribute name and value
			addAttribute(strName, strValue);
		}
	}
}
//==================================================================================================
BOOL GenericEntity::checkForEntAttr(CString zStrAttr)
{
	CString zTemp;
	CString zName;
	CString zValue;

	CStringArray azAttrStr;

	for(int i = 0; i < zStrAttr.GetLength(); i++)
	{
		//	check for \n
		if(zStrAttr[i] == 'n' && zStrAttr[i-1] == '\\')
			continue;

		if(zStrAttr[i] == '\\' || i == (zStrAttr.GetLength() - 1))
		{
			if(i == (zStrAttr.GetLength() - 1))
				//	add characters to the temporary string
				zTemp += zStrAttr[i];

			if(zTemp.IsEmpty() == FALSE)
				//	add string into array
				azAttrStr.Add(zTemp);

			//	remove data from temporary string
			zTemp.Empty();
		}
		else
			//	add characters to the temporary string
			zTemp += zStrAttr[i];
	}

	CArray<int, int> aFlag;

	for(int j = 0; j < azAttrStr.GetSize(); j++)
	{
		//	find the position of ':'
		int iDelimiter = azAttrStr[j].Find('=');

		//	get the data for attribute name and value
		zName = azAttrStr[j].Mid(0, iDelimiter);
		zValue = azAttrStr[j].Mid(iDelimiter + 1, azAttrStr[j].GetLength() - iDelimiter - 1);

		//	check for type
		if(zName.Compare("Type") == 0)
			if(zValue.Compare(getDesc().c_str()) == 0)
			{
				aFlag.Add(1);
				continue;
			}
			else
			{
				aFlag.Add(0);
				continue;
			}
		
		//	check for Visible attribute
		if(zName.Compare("Visible") == 0)
		{
			bool bFlag;
			CString zHidden = "The Value for Visible attribute is not valid!";

			if(zValue[0] == '1')
				bFlag = 1;
			else if(zValue[0] == '0')
				bFlag = 0;
			else
				return FALSE; 

			if(m_bVisible == bFlag)
				aFlag.Add(1);
			else
				aFlag.Add(0);

			continue;
		}
		
		string strName;
		string strValue;

		//	assign the user defined attribute name and value to the string
		strName.assign(LPCTSTR(zName));
		strValue.assign(LPCTSTR(zValue));

		//	find the iterator of the given attribute name
		STR2STR::iterator itAttFind = m_attributes.find(strName);

		//	check for attributes
		if(itAttFind == m_attributes.end())
			aFlag.Add(0);
		else if(m_attributes[strName] != strValue)
			aFlag.Add(0);
		else
			aFlag.Add(1);
	}	

	//	return the results
	for(int m = 0; m < azAttrStr.GetSize(); m++)
		if(aFlag[m] == 0)
			return FALSE;

	return TRUE;
}
//==================================================================================================
double GenericEntity::getDistanceBetweenPointAndLine (Point point, Point lineStpt, Point lineEndPt)
{
	double dDistance;

	double dX  = point.dXPos;
	double dY  = point.dYPos;

	double dX1 = lineStpt.dXPos;
	double dY1 = lineStpt.dYPos;
	double dX2 = lineEndPt.dXPos;
	double dY2 = lineEndPt.dYPos;

	// The following algorithm is followed to calculate the distance.Let us assume that A and B
	// are the two end points of the line. We need to calculate the distance between the 
	// point 'P' and the line 'AB'. point 'N' is the normal projected on to the line AB. 
	// Let us calculate a parameter, dPointParam which indicates N's position along AB.
	//
	//            A ____________________N_________________ B
	//									|
	//									|
	//									|
	//									|
	//									|Normal
	//									|
	//									|
	//									|
	//									* 
	//								    P
	//
	//					AP dot AB						(Px-Ax)(Bx-Ax) + (Py-Ay)(By-Ay)
	//  dPointParam = ------------- which expands to   ----------------------------------
	//					||AB||^2								Square ofLineLength 
	//
	//
	//
	//
	//  Now, dPointParam has following meaning:
	//  
	//  dPointParam = 0			N = A
	//  dPointParam = 1			N = B
	//  dPointParam < 0			N is on the backward extension of AB
	//  dPointParam > 1			N is on the forward extension of AB
	//  
	//  0 < dPointParam < 1		N is interior to AB or N lies on AB
	//
	//
	//  So, calculate the dPointParam and if it is greater than 0 and less than 1, then 
	// calculate the distance of the normal. Othewise the distance will be the minimum of
	// distances between point and A and distance between point and B.


	double dPointParam = 0.0;

	double dLength = sqrt ((dX2 -dX1) * (dX2 -dX1) + (dY2 - dY1) * (dY2 - dY1));

	dPointParam = ((dX-dX1) * (dX2-dX1) + (dY - dY1) * (dY2 - dY1)) / pow(dLength,2);
	// first check whether point falls within the extents of the entity
	if (dPointParam > 0 && dPointParam < 1)
	{
		// Find the equation of a line 
		// caclulate the slope of a line
		double dDeltaX = abs(dX2 - dX1);
		double dDeltaY = abs(dY2 - dY1);
		double dLnSlope = dDeltaY/dDeltaX;
		//double dPerpSlope = (-1.0) *(1/dLnSlope);pow
		
		// calculate A, B and C
		double dA, dB, dC;
		
		if(dX2 == dX1)	
		{
			dA = 1;
			dB = 0;
			dC = -dX1;
		}
		else
		{
			dA = dY1 - dY2;
			dB = dX2 - dX1;
			dC = (dX1*dY2) - (dX2*dY1);
		}
		
		dDistance = abs((dA*dX) + (dB*dY) + dC ) / sqrt((dA * dA) + (dB * dB));
	}
	else
	{
		// the point is not on the line entity
		// so calculate the distance between start and end point of the line 
		// and assign the distance whichever is less
		double stPtDistance  = sqrt((dX-dX1)*(dX-dX1) + (dY-dY1)*(dY-dY1)) ;
		double endPtDistance = sqrt((dX-dX2)*(dX-dX2) + (dY-dY2)*(dY-dY2)) ;
	
		// assign the distance with minimum 
		dDistance = min (stPtDistance, endPtDistance);
	}

	return dDistance;
}
//==================================================================================================
void GenericEntity::setPage(unsigned long ulPageNumber)
{
	m_ulPageNumber = ulPageNumber;
}
//==================================================================================================
bool GenericEntity::checkPageNumber()
{
	if(m_ulPageNumber == m_pGenericDisplayCtrl->getCurrentPageNumber())
		return true;
	else
		return false;
}
//==================================================================================================