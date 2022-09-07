#pragma once

#include <UCLIDException.h>

using namespace System;
using namespace System::Collections;
using namespace NUnit::Framework;

namespace Extract
{
	namespace Test
	{
		[TestFixture()]
		ref class ExceptionLoggerTest
		{

		public:

			[OneTimeSetUp]
			static void Setup()
			{
			}

			[OneTimeTearDown]
			static void FinalCleanup()
			{

			}

			[Test]
			[Category("Automated")]
			void LogTest();
			
			[Test]
			[Category("Automated")]
			void LogWithFileName();
			[Test]
			[Category("Automated")]
			void LogWithFileNameEmpty();

			[Test]
			[Category("Automated")]
			void LogWithFileNameCharPtr();

			[Test]
			[Category("Automated")]
			void LogWithFileNameCharPtrNull();

			[Test]
			[Category("Automated")]
			void LoadExtractExceptionInUclidException();
			
			[Test]
			[Category("Automated")]
			void LoadUclidExecptionInExtractException();
				
		private:
			void TestSavedLine(String^ fileName, long long unixStartTime);
			String^ GetDefaultFileExceptionFullPath();

			typedef Generic::Dictionary<int, DictionaryEntry>::ValueCollection::Enumerator DictionaryEnumerator;
			DictionaryEnumerator^ Find(String^ value, DictionaryEnumerator^ start)
			{
				DictionaryEnumerator^ found = start;
				while (found->MoveNext())
				{
					if (found->Current.Key->Equals(value))
						return found;
					
				};
				return nullptr;
			}
		};
	}
}