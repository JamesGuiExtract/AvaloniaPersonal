#pragma once

#include <UCLIDException.h>

using namespace System;
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
	

			
			void TestSavedLine(String^ fileName, long long unixStartTime);
			String^ GetDefaultFileExceptionFullPath();
		};

	}
}