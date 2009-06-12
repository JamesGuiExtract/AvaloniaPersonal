// ParcelIDIF.cpp : Implementation of CParcelIDIF
#include "stdafx.h"
#include "ParcelIDFinder.h"
#include "ParcelIDIF.h"

#include <string>
#include <vector>
using namespace std;

/////////////////////////////////////////////////////////////////////////////
// CParcelIDIF

STDMETHODIMP CParcelIDIF::InterfaceSupportsErrorInfo(REFIID riid)
{
	static const IID* arr[] = 
	{
		&IID_IParcelIDIF,
		&IID_IInputFinder,
		&IID_ICategorizedComponent
	};
	for (int i=0; i < sizeof(arr) / sizeof(arr[0]); i++)
	{
		if (InlineIsEqualGUID(*arr[i],riid))
			return S_OK;
	}
	return S_FALSE;
}

// ICategorizedComponent
STDMETHODIMP CParcelIDIF::GetComponentDescription(BSTR * pbstrComponentDescription)
{
	if (pbstrComponentDescription == NULL)
		return E_POINTER;
	
	// Provide a description
	*pbstrComponentDescription = ::SysAllocString(L"Highlight Parcel ID" );
	return S_OK;
}

// IInputFinder
STDMETHODIMP CParcelIDIF::ParseString(BSTR strInput, IIUnknownVector **ippTokenPositions)
{
	if (ippTokenPositions == NULL)
		return E_POINTER;
	
	// Create vector for interim storage of positions
	CComPtr<IIUnknownVector> ipUnknownVec;
	HRESULT hr = ipUnknownVec.CoCreateInstance(__uuidof(IUnknownVector));
	if (FAILED(hr))
	{
		return E_FAIL;
	}
	
	// Create local copy of string for parsing
	_bstr_t	bstrInput( strInput );
	string	strTemp( bstrInput );
	
	// Create vector for starting positions of parcel IDs
	vector<int>	vecStartPositions;
	
	////////////////////////
	// Do the actual parsing
	////////////////////////
	
	// Get the string size
	int		iLen = strTemp.size();
	
	// Search for each dash in the string
	int		iStartPos = 0;
	int		iPos = 0;
	bool	bDone = false;
	while (!bDone)
	{
		// Find the next dash
		iPos = strTemp.find( '-', iStartPos );
		
		// Check for no more dashes
		if (iPos == -1)
		{
			bDone = true;
			break;
		}
		
		// Check for too early
		if (iPos < 4)
		{
			// Update the start position for the next search
			iStartPos = iPos + 1;
			continue;
		}
		
		// Check for too late
		if (iLen - iPos < 5)
		{
			bDone = true;
			break;
		}
		
		// Check for four digits before the dash
		// and 4 digits after the dash
		bool	bFound = false;
		if (isdigit( strTemp[iPos - 1] ) &&
			isdigit( strTemp[iPos - 2] ) &&
			isdigit( strTemp[iPos - 3] ) &&
			isdigit( strTemp[iPos - 4] ) &&
			isdigit( strTemp[iPos + 1] ) &&
			isdigit( strTemp[iPos + 2] ) &&
			isdigit( strTemp[iPos + 3] ) &&
			isdigit( strTemp[iPos + 4] ))
		{
			bFound = true;
		}
		
		// Store starting position in vector
		if (bFound)
		{
			vecStartPositions.push_back( iPos - 4 );
		}
		
		// Update the start position for the next search
		iStartPos = iPos + 1;
		
		// Have we finished searching?
		if (iLen - iStartPos < 5)
		{
			bDone = true;
		}
	}
	
	///////////////////////////////////
	// Store IToken objects into vector
	///////////////////////////////////
	for (unsigned long n = 0; n < vecStartPositions.size(); n++)
	{
		// Create IToken object
		CComPtr<IToken> ipToken;
		HRESULT hr = ipToken.CoCreateInstance(__uuidof(Token));
		if (FAILED(hr))
		{
			return E_FAIL;
		}
		
		// Store position information into the object
		hr = ipToken->InitToken( 
			vecStartPositions[n], 
			vecStartPositions[n] + 8, 
			_bstr_t("Parcel ID"), 
			_bstr_t(strTemp.substr( vecStartPositions[n], 9 ).c_str()) );
		if (FAILED(hr))
		{
			return E_FAIL;
		}
		
		// Add object to vector
		ipUnknownVec->PushBack( ipToken );
	}
	
	// Provide IUnknownVector to caller
	*ippTokenPositions = ipUnknownVec.Detach();
	return S_OK;
}
