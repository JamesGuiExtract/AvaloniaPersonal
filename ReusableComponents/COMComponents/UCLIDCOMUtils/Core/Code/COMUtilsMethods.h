//=================================================================================================
//
// COPYRIGHT (c) 2015 EXTRACT SYSTEMS, LLC., IN PUBLISHED AND UNPUBLISHED WORKS
// ALL RIGHTS RESERVED.
//
// FILE:	COMUtilsMehtods.h
//
// PURPOSE:	Provides generic methods that use UCLIDComUtils objects
//
// NOTES:	
//
// AUTHORS:	William Parr
//
//=================================================================================================

#pragma once

#include "ComUtilsExport.h"

#include <comdef.h>
#include <string>
#include <UCLIDException.h>

using namespace std;

// PROMISE: Clone an object depending on the parameter bWithCloneIdentifiable and whether ipObject
//			implements the ICloneIdentifiableObject interface
// Args:	strELI - ELI code of exception thrown if ipObject doesn't implement ICopyableObject
//			If ipObject is null returns null
//			If bWithCloneIdentifiableObject is true and ipObject defines the interface ICloneIdentifiableObject
//			then object returned is cloned using ICloneIdentifiableObject interface
//			Otherwise object is cloned using the ICopyableObject interface
EXPORT_UCLIDCOMUtils IUnknownPtr cloneObject(string strELI, IUnknownPtr ipObject, bool bWithCloneIdentifiableObject);

//-------------------------------------------------------------------------------------------------
namespace IUnknownVectorMethods
{
	// Map (transform) a vector of TInput to a vector of TOutput
	template <typename TInput, typename TOutput>
	UCLID_COMUTILSLib::IIUnknownVectorPtr map(UCLID_COMUTILSLib::IIUnknownVectorPtr source, function<TOutput(TInput)> mapInput)
	{
		UCLID_COMUTILSLib::IIUnknownVectorPtr transformed(CLSID_IUnknownVector);
		ASSERT_RESOURCE_ALLOCATION("ELI53340", transformed != __nullptr);

		int size = source->Size();
		for (int i = 0; i < size; i++)
		{
			TInput item = source->At(i);
			ASSERT_RESOURCE_ALLOCATION("ELI53341", item != __nullptr);

			transformed->PushBack(mapInput(item));
		}

		return transformed;
	}
	//-------------------------------------------------------------------------------------------------
	// Filter vector to contain only items where the predicate returns true
	template <typename T>
	UCLID_COMUTILSLib::IIUnknownVectorPtr filter(UCLID_COMUTILSLib::IIUnknownVectorPtr source, function<bool(T)> predicate)
	{
		UCLID_COMUTILSLib::IIUnknownVectorPtr filtered(CLSID_IUnknownVector);
		ASSERT_RESOURCE_ALLOCATION("ELI53342", filtered != __nullptr);

		int size = source->Size();
		for (int i = 0; i < size; i++)
		{
			T item = source->At(i);
			ASSERT_RESOURCE_ALLOCATION("ELI53343", item != __nullptr);

			if (predicate(item))
			{
				filtered->PushBack(item);
			}
		}

		return filtered;
	}
	//-------------------------------------------------------------------------------------------------
	// Flatten a vector of vectors of x to be a vector of x
	inline UCLID_COMUTILSLib::IIUnknownVectorPtr concat(UCLID_COMUTILSLib::IIUnknownVectorPtr source)
	{
		UCLID_COMUTILSLib::IIUnknownVectorPtr flattened(CLSID_IUnknownVector);
		ASSERT_RESOURCE_ALLOCATION("ELI53344", flattened != __nullptr);

		int size = source->Size();
		for (int i = 0; i < size; i++)
		{
			UCLID_COMUTILSLib::IIUnknownVectorPtr sub = source->At(i);
			ASSERT_RESOURCE_ALLOCATION("ELI53345", sub != __nullptr);

			flattened->Append(sub);
		}

		return flattened;
	}
	//-------------------------------------------------------------------------------------------------
	// Filter a vector so that it contains only the non-null results of the specified function (map + filter for non-null)
	template <typename TInput, typename TOutput>
	UCLID_COMUTILSLib::IIUnknownVectorPtr choose(UCLID_COMUTILSLib::IIUnknownVectorPtr source, function<TOutput(TInput)> mapInput)
	{
		UCLID_COMUTILSLib::IIUnknownVectorPtr transformed(CLSID_IUnknownVector);
		ASSERT_RESOURCE_ALLOCATION("ELI53346", transformed != __nullptr);

		int size = source->Size();
		for (int i = 0; i < size; i++)
		{
			TInput item = source->At(i);
			ASSERT_RESOURCE_ALLOCATION("ELI53347", item != __nullptr);

			TOutput mappedItem = mapInput(item);

			if (mappedItem != __nullptr)
			{
				transformed->PushBack(mappedItem);
			}
		}

		return transformed;
	}
}
//-------------------------------------------------------------------------------------------------
