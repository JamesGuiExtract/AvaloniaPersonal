#include "stdafx.h"
#include "MasterExemptionCodeList.h"

#include <COMUtils.h>
#include <cpputil.h>
#include <UCLIDException.h>

//-------------------------------------------------------------------------------------------------
MasterExemptionCodeList::MasterExemptionCodeList(const string& strExemptionDirectory)
{
	try
	{
		// Get all the xml files in the specified directory
		vector<string> xmlFiles;
		getFilesInDir(xmlFiles, strExemptionDirectory, "*.xml");

		// Add the category and codes for each xml file
		vector<string>::iterator iter = xmlFiles.begin();
		for (; iter != xmlFiles.end(); iter++)
		{
			addCategoryCodesFromXml(*iter);
		}
	}
	CATCH_DISPLAY_AND_RETHROW_ALL_EXCEPTIONS("ELI24972")
}
//-------------------------------------------------------------------------------------------------
MasterExemptionCodeList::MasterExemptionCodeList(const MasterExemptionCodeList& exemptionCodes)
{
	m_categoryToCodes = exemptionCodes.m_categoryToCodes;
}
//-------------------------------------------------------------------------------------------------
void MasterExemptionCodeList::getCategoryNames(vector<string> &rvecCategories) const
{
	// Iterate over each category
	map<string, ExemptionCategoryInfo>::const_iterator iter = m_categoryToCodes.begin();
	for (; iter != m_categoryToCodes.end(); iter++)
	{
		// Add the category name
		rvecCategories.push_back(iter->first);
	}
}
//-------------------------------------------------------------------------------------------------
void MasterExemptionCodeList::getCodesInCategory(vector<ExemptionCodeInfo> &rvecCodes, 
		const string& strCategory) const
{
	// If the category doesn't exist, we are done
	map<string, ExemptionCategoryInfo>::const_iterator itCat = m_categoryToCodes.find(strCategory);
	if (itCat == m_categoryToCodes.end())
	{
		return;
	}

	// Get all the codes in the specified category
	const vector<ExemptionCodeInfo>& vecCodes = itCat->second.m_vecCodes;
	rvecCodes.assign(vecCodes.begin(), vecCodes.end());
}
//-------------------------------------------------------------------------------------------------
string MasterExemptionCodeList::getDescription(const string& strCategory, const string& strCode) const
{
	// If the category doesn't exist, throw an exception
	map<string, ExemptionCategoryInfo>::const_iterator itCat = m_categoryToCodes.find(strCategory);
	if (itCat == m_categoryToCodes.end())
	{
		UCLIDException ue("ELI24973", "Exemption category doesn't exist.");
		ue.addDebugInfo("Exemption category", strCategory);
		throw ue;
	}

	// Iterate over all the codes in the specified category
	const vector<ExemptionCodeInfo>& vecCodes = itCat->second.m_vecCodes;
	vector<ExemptionCodeInfo>::const_iterator iter = vecCodes.begin();
	for	(; iter != vecCodes.end(); iter++)
	{
		// If we found the specified code, return its description
		if (iter->m_strCode == strCode)
		{
			return iter->m_strDescription;
		}
	}

	// If we reached this point the code wasn't found, throw an exception
	UCLIDException ue("ELI24932", "Cannot find exemption code description.");
	ue.addDebugInfo("Exemption category", strCategory);
	ue.addDebugInfo("Exemption code", strCode);
	throw ue;
}
//-------------------------------------------------------------------------------------------------
string MasterExemptionCodeList::getCategoryAbbreviation(const string& strCategory) const
{
	// If the category doesn't exist, return the empty string
	map<string, ExemptionCategoryInfo>::const_iterator itCat = m_categoryToCodes.find(strCategory);
	if (itCat == m_categoryToCodes.end())
	{
		return "";
	}

	return itCat->second.m_abbreviation;
}
//-------------------------------------------------------------------------------------------------
string MasterExemptionCodeList::getFullCategoryName(const string& strAbbreviation) const
{
	// Iterate over each category
	map<string, ExemptionCategoryInfo>::const_iterator iter = m_categoryToCodes.begin();
	for (; iter != m_categoryToCodes.end(); iter++)
	{
		// If the abbreviated name matches, return the full name
		if (iter->second.m_abbreviation == strAbbreviation)
		{
			return iter->first;
		}
	}

	// The abbreviated category wasn't found
	return "";
}
//-------------------------------------------------------------------------------------------------
void MasterExemptionCodeList::addCategoryCodesFromXml(const string& strXmlFile)
{
	// Create an xml document object
	MSXML::IXMLDOMDocumentPtr ipXmlDocument(CLSID_DOMDocument);
	ASSERT_RESOURCE_ALLOCATION("ELI24921", ipXmlDocument != NULL);

	// Ensure asynchronous processing is disabled
	ipXmlDocument->async = VARIANT_FALSE;

	// Load the XML File
	if (VARIANT_FALSE == ipXmlDocument->load( _variant_t(strXmlFile.c_str()) ))
	{
		UCLIDException ue("ELI24922", "Unable to load XML file.");
		ue.addDebugInfo("XML filename", strXmlFile);
		throw ue;
	}

	// Get the root element
	MSXML::IXMLDOMElementPtr ipCategory = ipXmlDocument->documentElement; 
	ASSERT_RESOURCE_ALLOCATION("ELI24923", ipCategory != NULL);

	// Get the root node's name
	string strRootNodeName = asString(ipCategory->nodeName);

	// Get the exemption category from the xml file
	if (strRootNodeName != "ExemptionCategory")
	{
		// Throw an exception
		UCLIDException ue("ELI24924", "Invalid root node.");
		ue.addDebugInfo("XML filename", strXmlFile);
		ue.addDebugInfo("Root node", strRootNodeName);
		throw ue;
	}

	// Get the attributes of the root node
	MSXML::IXMLDOMNamedNodeMapPtr ipCategoryAttributes = ipCategory->attributes;
	ASSERT_RESOURCE_ALLOCATION("ELI24925", ipCategoryAttributes != NULL);

	// Store the abbreviated name of this category
	ExemptionCategoryInfo category;
	category.m_abbreviation = getAttributeAsString(ipCategoryAttributes, "Name");

	// Get the exemption codes
	MSXML::IXMLDOMNodeListPtr ipExemptionList = ipCategory->getElementsByTagName("Exemption");
	ASSERT_RESOURCE_ALLOCATION("ELI24926", ipExemptionList != NULL);

	// Prepare the vector to hold the exemption codes
	long lNumExemptions = ipExemptionList->length;
	category.m_vecCodes.reserve(lNumExemptions);

	// Iterate through each exemption
	for (long i = 0; i < lNumExemptions; i++)
	{
		// Get the ith exemption
		MSXML::IXMLDOMNodePtr ipExemption(ipExemptionList->item[i]);
		ASSERT_RESOURCE_ALLOCATION("ELI24927", ipExemption != NULL);

		// Get the attributes of this exemption
		MSXML::IXMLDOMNamedNodeMapPtr ipExemptionAttributes(ipExemption->attributes);
		ASSERT_RESOURCE_ALLOCATION("ELI24928", ipExemptionAttributes != NULL);

		// Get the code, summary, and description
		string strCode = getAttributeAsString(ipExemptionAttributes, "Code");
		string strSummary = getAttributeAsString(ipExemptionAttributes, "Summary");
		string strDescription = getAttributeAsString(ipExemptionAttributes, "Description");

		// Add this exemption code information to the vector
		ExemptionCodeInfo code = {strCode, strSummary, strDescription};
		category.m_vecCodes.push_back(code);
	}

	// Add the category and exemption codes
	string strCategoryName = getFileNameWithoutExtension(strXmlFile);
	m_categoryToCodes[strCategoryName] = category;
}
//-------------------------------------------------------------------------------------------------
string MasterExemptionCodeList::getAttributeAsString(MSXML::IXMLDOMNamedNodeMapPtr ipAttributes, 
										const char* pszAttributeName) const
{
	// Get the attribute with the specified name
	MSXML::IXMLDOMNodePtr ipAttribute = ipAttributes->getNamedItem(pszAttributeName);
	ASSERT_RESOURCE_ALLOCATION("ELI24929", ipAttribute != NULL);

	// Return the text of the attribute
	return asString(ipAttribute->text);
}
//-------------------------------------------------------------------------------------------------
