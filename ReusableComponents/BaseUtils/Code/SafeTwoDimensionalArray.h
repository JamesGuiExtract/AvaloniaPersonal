// SafeTwoDimensionalArray.h - Template class for declaring a dynamic two dimensional array
//
// PURPOSE: To provide a two dimensional array which can be allocated dynamically at runtime
//			and will automatically clean up its own memory when it goes out of scope.
//			Also provides some elementary bounds checking if you use (row, column) to access
//			an element of the array.
//
// AUTHOR:	Jeff Shergalis
//
// DATE:	12/11/2007
//
#pragma once

#include "stdafx.h"
#include "UCLIDException.h"

template <class ArrayType>
class SafeTwoDimensionalArray
{
public:
	//---------------------------------------------------------------------------------------------
	SafeTwoDimensionalArray(long lRows, long lColumns) :
	  m_lRows(lRows),
	  m_lColumns(lColumns)
	{
		try
		{
			// size must be greater than 0
			if (lRows <= 0 || lColumns <= 0)
			{
				UCLIDException ue("ELI18440", "Invalid array bounds!");
				ue.addDebugInfo("Rows", lRows);
				ue.addDebugInfo("Columns", lColumns);
				throw ue;
			}
			
			// allocate the array
			allocateThisArray();
		}
		CATCH_ALL_AND_RETHROW_AS_UCLID_EXCEPTION("ELI18443");
	  }
	//---------------------------------------------------------------------------------------------
	  ~SafeTwoDimensionalArray()
	  {
		  try
		  {
			  // release the memory held by this array
			  clearThisArray();
		  }
		  CATCH_AND_LOG_ALL_EXCEPTIONS("ELI18444");
	  }
	//---------------------------------------------------------------------------------------------
	  // operator overload to allow the use of array[row][column]
	  inline ArrayType* operator[](long i) { return m_pArray[i]; }
	  inline const ArrayType* operator[](long i) const { return m_pArray[i]; }
	//---------------------------------------------------------------------------------------------
	  // operator overload to give a method for accessing the array which will check the
	  // bounds of the argument
	  ArrayType operator()(long lRow, long lColumn)
	  {
		  if (lRow >= m_lRows || lColumn >= m_lColumns)
		  {
			  UCLIDException ue("ELI18445", "Array bounds violation!");
			  ue.addDebugInfo("Row requested", lRow);
			  ue.addDebugInfo("Row limit", m_lRows-1);
			  ue.addDebugInfo("Column requested", lColumn);
			  ue.addDebugInfo("Column limit", m_lColumns-1);
			  throw ue;
		  }

		  return m_pArray[lRow][lColumn];
	  }

	  const ArrayType operator()(long lRow, long lColumn) const
	  {
		  if (lRow >= m_lRows || lColumn >= m_lColumns)
		  {
			  UCLIDException ue("ELI18446", "Array bounds violation!");
			  ue.addDebugInfo("Row requested", lRow);
			  ue.addDebugInfo("Row limit", m_lRows-1);
			  ue.addDebugInfo("Column requested", lColumn);
			  ue.addDebugInfo("Column limit", m_lColumns-1);
			  throw ue;
		  }

		  return m_pArray[lRow][lColumn];
	  }
	//---------------------------------------------------------------------------------------------
	  // overloaded copy constructor to allow easy copying of s2dArrays
	  SafeTwoDimensionalArray<ArrayType> operator=(SafeTwoDimensionalArray<ArrayType> s2dArray)
	  {
		  try
		  {
			  // first clear the array
			  clearThisArray();
			  
			  // set the rows and columns
			  m_lRows = s2dArray.getRows();
			  m_lColumns = s2dArray.getColumns();

			  // now allocate the new array
			  allocateThisArray();

			  for (long i=0; i < m_lRows; i++)
			  {
				  for (long j=0; j < m_lColumns; j++)
				  {
					  m_pArray[i][j] = s2dArray[i][j];
				  }
			  }

			  return *this;
		  }
		  CATCH_ALL_AND_RETHROW_AS_UCLID_EXCEPTION("ELI18503");
	  }
	//---------------------------------------------------------------------------------------------
	inline long getRows() { return m_lRows; }
	//---------------------------------------------------------------------------------------------
	inline long getColumns() { return m_lColumns; }
	//---------------------------------------------------------------------------------------------
private:
	//---------------------------------------------------------------------------------------------
	// Variables
	//---------------------------------------------------------------------------------------------
	long m_lRows;
	long m_lColumns;
	ArrayType** m_pArray;

	//---------------------------------------------------------------------------------------------
	// Methods
	//---------------------------------------------------------------------------------------------
	// PURPOSE: To allocate this two dimensional array based on the m_lRows and m_lColumns
	//
	// REQUIRE: m_lRows and m_lColumns != 0
	void allocateThisArray()
	{
		// allocate first dimension
		m_pArray = new ArrayType*[m_lRows];
		ASSERT_RESOURCE_ALLOCATION("ELI18441", m_pArray != __nullptr);

		// loop through the first dimension and allocate the second dimension
		for (long i=0; i < m_lRows; i++)
		{
			m_pArray[i] = new ArrayType[m_lColumns];
			ASSERT_RESOURCE_ALLOCATION("ELI18442", m_pArray[i] != __nullptr);
		}
	}
	//---------------------------------------------------------------------------------------------
	// PURPOSE: To release all of the memory held by the array and to set the rows and columns
	//			to 0
	void clearThisArray()
	{
		// clear the second dimension of the array first
		for (long i=0; i < m_lRows; i++)
		{
			delete [] m_pArray[i];
		}

		// clear the first dimension of the array
		delete [] m_pArray;

		m_pArray = NULL;

		m_lRows = m_lColumns = 0;
	}
	//---------------------------------------------------------------------------------------------
};