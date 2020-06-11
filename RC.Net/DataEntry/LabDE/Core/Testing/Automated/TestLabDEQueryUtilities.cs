using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using Extract.Testing.Utilities;

namespace Extract.DataEntry.LabDE.Test
{
    /// <summary>
    /// Provides test cases for the <see cref="LabDEQueryUtilities"/> class.
    /// </summary>
    [TestFixture]
    [NUnit.Framework.Category("LabDEQueryUtilities")]
    public static class TestLabDEQueryUtilities
    {
        /// <summary>
        /// Setup method to initialize the testing environment.
        /// </summary>
        [OneTimeSetUp]
        public static void Setup()
        {
            GeneralMethods.TestSetup();
        }

        /// <summary>
        /// Cleanup after all tests have run.
        /// </summary>
        [OneTimeTearDown]
        public static void FinalCleanup()
        {
        }

        [Test, Category("ValidateFlagAgainstValueAndRange")]
        public static void TestFlagFormat01()
        {
            bool valid = LabDEQueryUtilities.ValidateFlagAgainstValueAndRange("", "2", "1.0-3.0");

            Assert.That(valid == true);
        }

        [Test, Category("ValidateFlagAgainstValueAndRange")]
        public static void TestFlagFormat01A()
        {
            bool valid = LabDEQueryUtilities.ValidateFlagAgainstValueAndRange("A", "2", "1.0-3.0");

            Assert.That(valid == false);
        }

        [Test, Category("ValidateFlagAgainstValueAndRange")]
        public static void TestFlagFormat02()
        {
            bool valid = LabDEQueryUtilities.ValidateFlagAgainstValueAndRange("", "4 . 0", "1 . 0 - 2 . 0");

            Assert.That(valid == false);
        }

        [Test, Category("ValidateFlagAgainstValueAndRange")]
        public static void TestFlagFormat02A()
        {
            bool valid = LabDEQueryUtilities.ValidateFlagAgainstValueAndRange("A", "4 . 0", "1 . 0 - 2 . 0");

            Assert.That(valid == true);
        }

        [Test, Category("ValidateFlagAgainstValueAndRange")]
        public static void TestFlagFormat03()
        {
            bool valid = LabDEQueryUtilities.ValidateFlagAgainstValueAndRange("", "<1000", ">=1000");

            Assert.That(valid == false);
        }

        [Test, Category("ValidateFlagAgainstValueAndRange")]
        public static void TestFlagFormat03A()
        {
            bool valid = LabDEQueryUtilities.ValidateFlagAgainstValueAndRange("A", "<1000", ">=1000");

            Assert.That(valid == true);
        }

        [Test, Category("ValidateFlagAgainstValueAndRange")]
        public static void TestFlagFormat04()
        {
            bool valid = LabDEQueryUtilities.ValidateFlagAgainstValueAndRange("", "2,500.0", "</=2,500.0");

            Assert.That(valid == true);
        }

        [Test, Category("ValidateFlagAgainstValueAndRange")]
        public static void TestFlagFormat04A()
        {
            bool valid = LabDEQueryUtilities.ValidateFlagAgainstValueAndRange("A", "2,500.0", "</=2,500.0");

            Assert.That(valid == false);
        }

        [Test, Category("ValidateFlagAgainstValueAndRange")]
        public static void TestFlagFormat05()
        {
            bool valid = LabDEQueryUtilities.ValidateFlagAgainstValueAndRange("", "> / = 1,000.0", "< 1,000.0");

            Assert.That(valid == false);
        }

        [Test, Category("ValidateFlagAgainstValueAndRange")]
        public static void TestFlagFormat05A()
        {
            bool valid = LabDEQueryUtilities.ValidateFlagAgainstValueAndRange("A", "> / = 1,000.0", "< 1,000.0");

            Assert.That(valid == true);
        }

        [Test, Category("ValidateFlagAgainstValueAndRange")]
        public static void TestFlagFormat06()
        {
            bool valid = LabDEQueryUtilities.ValidateFlagAgainstValueAndRange("", "10.001", "9.999-10.001");

            Assert.That(valid == true);
        }

        [Test, Category("ValidateFlagAgainstValueAndRange")]
        public static void TestFlagFormat06A()
        {
            bool valid = LabDEQueryUtilities.ValidateFlagAgainstValueAndRange("A", "10.001", "9.999-10.001");

            Assert.That(valid == false);
        }

        [Test, Category("ValidateFlagAgainstValueAndRange")]
        public static void TestFlagFormat07()
        {
            bool valid = LabDEQueryUtilities.ValidateFlagAgainstValueAndRange("", "10", ">= 10.001");

            Assert.That(valid == false);
        }

        [Test, Category("ValidateFlagAgainstValueAndRange")]
        public static void TestFlagFormat07A()
        {
            bool valid = LabDEQueryUtilities.ValidateFlagAgainstValueAndRange("A", "10", ">= 10.001");

            Assert.That(valid == true);
        }

        [Test, Category("ValidateFlagAgainstValueAndRange")]
        public static void TestFlagFormat08()
        {
            bool valid = LabDEQueryUtilities.ValidateFlagAgainstValueAndRange("", "10.001", ">= 10");

            Assert.That(valid == true);
        }

        [Test, Category("ValidateFlagAgainstValueAndRange")]
        public static void TestFlagFormat08A()
        {
            bool valid = LabDEQueryUtilities.ValidateFlagAgainstValueAndRange("A", "10.001", ">= 10");

            Assert.That(valid == false);
        }

        [Test, Category("ValidateFlagAgainstValueAndRange")]
        public static void TestFlagFormat09()
        {
            bool valid = LabDEQueryUtilities.ValidateFlagAgainstValueAndRange("", "1.020", "1.005,");

            Assert.That(valid == true);
        }

        [Test, Category("ValidateFlagAgainstValueAndRange")]
        public static void TestFlagFormat09A()
        {
            bool valid = LabDEQueryUtilities.ValidateFlagAgainstValueAndRange("A", "1.020", "1.005,");

            Assert.That(valid == true);
        }

        [Test, Category("ValidateFlagAgainstValueAndRange")]
        public static void TestFlagFormat10()
        {
            bool valid = LabDEQueryUtilities.ValidateFlagAgainstValueAndRange("", "1.020", "<=1.005-1.030");

            Assert.That(valid == true);
        }

        [Test, Category("ValidateFlagAgainstValueAndRange")]
        public static void TestFlagFormat10A()
        {
            bool valid = LabDEQueryUtilities.ValidateFlagAgainstValueAndRange("A", "1.020", "<=1.005-1.030");

            Assert.That(valid == true);
        }

        [Test, Category("ValidateFlagAgainstValueAndRange")]
        public static void TestFlagFormat11()
        {
            bool valid = LabDEQueryUtilities.ValidateFlagAgainstValueAndRange("", "unknown", "unknown");

            Assert.That(valid == true);
        }

        [Test, Category("ValidateFlagAgainstValueAndRange")]
        public static void TestFlagFormat11A()
        {
            bool valid = LabDEQueryUtilities.ValidateFlagAgainstValueAndRange("A", "unknown", "unknown");

            Assert.That(valid == true);
        }

        [Test, Category("ValidateFlagAgainstValueAndRange")]
        public static void TestFlagFormat12()
        {
            bool valid = LabDEQueryUtilities.ValidateFlagAgainstValueAndRange("", "2", "");

            Assert.That(valid == true);
        }

        [Test, Category("ValidateFlagAgainstValueAndRange")]
        public static void TestFlagFormat12A()
        {
            bool valid = LabDEQueryUtilities.ValidateFlagAgainstValueAndRange("A", "2", "");

            Assert.That(valid == true);
        }

        [Test, Category("ValidateFlagAgainstValueAndRange")]
        public static void TestFlagFormat13()
        {
            bool valid = LabDEQueryUtilities.ValidateFlagAgainstValueAndRange("", "", "1-2");

            Assert.That(valid == true);
        }

        [Test, Category("ValidateFlagAgainstValueAndRange")]
        public static void TestFlagFormat13A()
        {
            bool valid = LabDEQueryUtilities.ValidateFlagAgainstValueAndRange("A", "", "1-2");

            Assert.That(valid == true);
        }

        [Test, Category("ValidateFlagAgainstValueAndRange")]
        public static void TestFlagRangeOverlaps01()
        {
            bool valid = LabDEQueryUtilities.ValidateFlagAgainstValueAndRange("", "0-2", "3-6");

            Assert.That(valid == false);
        }

        [Test, Category("ValidateFlagAgainstValueAndRange")]
        public static void TestFlagRangeOverlaps01A()
        {
            bool valid = LabDEQueryUtilities.ValidateFlagAgainstValueAndRange("A", "0-2", "3-6");

            Assert.That(valid == true);
        }

        [Test, Category("ValidateFlagAgainstValueAndRange")]
        public static void TestFlagRangeOverlaps01H()
        {
            bool valid = LabDEQueryUtilities.ValidateFlagAgainstValueAndRange("H", "0-2", "3-6");

            Assert.That(valid == false);
        }

        [Test, Category("ValidateFlagAgainstValueAndRange")]
        public static void TestFlagRangeOverlaps01L()
        {
            bool valid = LabDEQueryUtilities.ValidateFlagAgainstValueAndRange("L", "0-2", "3-6");

            Assert.That(valid == true);
        }

        [Test, Category("ValidateFlagAgainstValueAndRange")]
        public static void TestFlagRangeOverlaps02()
        {
            bool valid = LabDEQueryUtilities.ValidateFlagAgainstValueAndRange("", "0-3", "3-6");

            Assert.That(valid == true);
        }

        [Test, Category("ValidateFlagAgainstValueAndRange")]
        public static void TestFlagRangeOverlaps02A()
        {
            bool valid = LabDEQueryUtilities.ValidateFlagAgainstValueAndRange("A", "0-3", "3-6");

            Assert.That(valid == true);
        }

        [Test, Category("ValidateFlagAgainstValueAndRange")]
        public static void TestFlagRangeOverlaps02H()
        {
            bool valid = LabDEQueryUtilities.ValidateFlagAgainstValueAndRange("H", "0-3", "3-6");

            Assert.That(valid == false);
        }

        [Test, Category("ValidateFlagAgainstValueAndRange")]
        public static void TestFlagRangeOverlaps02L()
        {
            bool valid = LabDEQueryUtilities.ValidateFlagAgainstValueAndRange("L", "0-3", "3-6");

            Assert.That(valid == true);
        }

        [Test, Category("ValidateFlagAgainstValueAndRange")]
        public static void TestFlagRangeOverlaps03()
        {
            bool valid = LabDEQueryUtilities.ValidateFlagAgainstValueAndRange("", "0-4", "3-6");

            Assert.That(valid == true);
        }

        [Test, Category("ValidateFlagAgainstValueAndRange")]
        public static void TestFlagRangeOverlaps03A()
        {
            bool valid = LabDEQueryUtilities.ValidateFlagAgainstValueAndRange("A", "0-4", "3-6");

            Assert.That(valid == true);
        }

        [Test, Category("ValidateFlagAgainstValueAndRange")]
        public static void TestFlagRangeOverlaps03H()
        {
            bool valid = LabDEQueryUtilities.ValidateFlagAgainstValueAndRange("H", "0-4", "3-6");

            Assert.That(valid == false);
        }

        [Test, Category("ValidateFlagAgainstValueAndRange")]
        public static void TestFlagRangeOverlaps03L()
        {
            bool valid = LabDEQueryUtilities.ValidateFlagAgainstValueAndRange("L", "0-4", "3-6");

            Assert.That(valid == true);
        }

        [Test, Category("ValidateFlagAgainstValueAndRange")]
        public static void TestFlagRangeOverlaps04()
        {
            bool valid = LabDEQueryUtilities.ValidateFlagAgainstValueAndRange("", "4-5", "3-6");

            Assert.That(valid == true);
        }

        [Test, Category("ValidateFlagAgainstValueAndRange")]
        public static void TestFlagRangeOverlaps04A()
        {
            bool valid = LabDEQueryUtilities.ValidateFlagAgainstValueAndRange("A", "4-5", "3-6");

            Assert.That(valid == false);
        }

        [Test, Category("ValidateFlagAgainstValueAndRange")]
        public static void TestFlagRangeOverlaps04H()
        {
            bool valid = LabDEQueryUtilities.ValidateFlagAgainstValueAndRange("H", "4-5", "3-6");

            Assert.That(valid == false);
        }

        [Test, Category("ValidateFlagAgainstValueAndRange")]
        public static void TestFlagRangeOverlaps04L()
        {
            bool valid = LabDEQueryUtilities.ValidateFlagAgainstValueAndRange("L", "4-5", "3-6");

            Assert.That(valid == false);
        }

        [Test, Category("ValidateFlagAgainstValueAndRange")]
        public static void TestFlagRangeOverlaps05()
        {
            bool valid = LabDEQueryUtilities.ValidateFlagAgainstValueAndRange("", "5-8", "3-6");

            Assert.That(valid == true);
        }

        [Test, Category("ValidateFlagAgainstValueAndRange")]
        public static void TestFlagRangeOverlaps05A()
        {
            bool valid = LabDEQueryUtilities.ValidateFlagAgainstValueAndRange("A", "5-8", "3-6");

            Assert.That(valid == true);
        }

        [Test, Category("ValidateFlagAgainstValueAndRange")]
        public static void TestFlagRangeOverlaps05H()
        {
            bool valid = LabDEQueryUtilities.ValidateFlagAgainstValueAndRange("H", "5-8", "3-6");

            Assert.That(valid == true);
        }

        [Test, Category("ValidateFlagAgainstValueAndRange")]
        public static void TestFlagRangeOverlaps05L()
        {
            bool valid = LabDEQueryUtilities.ValidateFlagAgainstValueAndRange("L", "5-8", "3-6");

            Assert.That(valid == false);
        }

        [Test, Category("ValidateFlagAgainstValueAndRange")]
        public static void TestFlagRangeOverlaps06()
        {
            bool valid = LabDEQueryUtilities.ValidateFlagAgainstValueAndRange("", "6-8", "3-6");

            Assert.That(valid == true);
        }

        [Test, Category("ValidateFlagAgainstValueAndRange")]
        public static void TestFlagRangeOverlaps06A()
        {
            bool valid = LabDEQueryUtilities.ValidateFlagAgainstValueAndRange("A", "6-8", "3-6");

            Assert.That(valid == true);
        }

        [Test, Category("ValidateFlagAgainstValueAndRange")]
        public static void TestFlagRangeOverlaps06H()
        {
            bool valid = LabDEQueryUtilities.ValidateFlagAgainstValueAndRange("H", "6-8", "3-6");

            Assert.That(valid == true);
        }

        [Test, Category("ValidateFlagAgainstValueAndRange")]
        public static void TestFlagRangeOverlaps06L()
        {
            bool valid = LabDEQueryUtilities.ValidateFlagAgainstValueAndRange("L", "6-8", "3-6");

            Assert.That(valid == false);
        }

        [Test, Category("ValidateFlagAgainstValueAndRange")]
        public static void TestFlagRangeOverlaps07()
        {
            bool valid = LabDEQueryUtilities.ValidateFlagAgainstValueAndRange("", "7-8", "3-6");

            Assert.That(valid == false);
        }

        [Test, Category("ValidateFlagAgainstValueAndRange")]
        public static void TestFlagRangeOverlaps07A()
        {
            bool valid = LabDEQueryUtilities.ValidateFlagAgainstValueAndRange("A", "7-8", "3-6");

            Assert.That(valid == true);
        }

        [Test, Category("ValidateFlagAgainstValueAndRange")]
        public static void TestFlagRangeOverlaps07H()
        {
            bool valid = LabDEQueryUtilities.ValidateFlagAgainstValueAndRange("H", "7-8", "3-6");

            Assert.That(valid == true);
        }

        [Test, Category("ValidateFlagAgainstValueAndRange")]
        public static void TestFlagRangeOverlaps07L()
        {
            bool valid = LabDEQueryUtilities.ValidateFlagAgainstValueAndRange("L", "7-8", "3-6");

            Assert.That(valid == false);
        }

        [Test, Category("ValidateFlagAgainstValueAndRange")]
        public static void TestFlagRangeOverlaps08()
        {
            bool valid = LabDEQueryUtilities.ValidateFlagAgainstValueAndRange("", "0-8", "3-6");

            Assert.That(valid == true);
        }

        [Test, Category("ValidateFlagAgainstValueAndRange")]
        public static void TestFlagRangeOverlaps08A()
        {
            bool valid = LabDEQueryUtilities.ValidateFlagAgainstValueAndRange("A", "0-8", "3-6");

            Assert.That(valid == true);
        }

        [Test, Category("ValidateFlagAgainstValueAndRange")]
        public static void TestFlagRangeOverlaps08H()
        {
            bool valid = LabDEQueryUtilities.ValidateFlagAgainstValueAndRange("H", "0-8", "3-6");

            Assert.That(valid == true);
        }

        [Test, Category("ValidateFlagAgainstValueAndRange")]
        public static void TestFlagRangeOverlaps08L()
        {
            bool valid = LabDEQueryUtilities.ValidateFlagAgainstValueAndRange("L", "0-8", "3-6");

            Assert.That(valid == true);
        }

        [Test, Category("ValidateFlagAgainstValueAndRange")]
        public static void TestFlagRangeOverlaps09()
        {
            bool valid = LabDEQueryUtilities.ValidateFlagAgainstValueAndRange("", "0-4", "30-60");

            Assert.That(valid == false);
        }

        [Test, Category("ValidateFlagAgainstValueAndRange")]
        public static void TestFlagRangeOverlaps09A()
        {
            bool valid = LabDEQueryUtilities.ValidateFlagAgainstValueAndRange("A", "0-4", "30-60");

            Assert.That(valid == true);
        }

        [Test, Category("ValidateFlagAgainstValueAndRange")]
        public static void TestFlagRangeOverlaps09H()
        {
            bool valid = LabDEQueryUtilities.ValidateFlagAgainstValueAndRange("H", "0-4", "30-60");

            Assert.That(valid == false);
        }

        [Test, Category("ValidateFlagAgainstValueAndRange")]
        public static void TestFlagRangeOverlaps09L()
        {
            bool valid = LabDEQueryUtilities.ValidateFlagAgainstValueAndRange("L", "0-4", "30-60");

            Assert.That(valid == true);
        }

        [Test, Category("ValidateFlagAgainstValueAndRange")]
        public static void TestFlagRangeOverlaps10()
        {
            bool valid = LabDEQueryUtilities.ValidateFlagAgainstValueAndRange("", "50-80", "3-6");

            Assert.That(valid == false);
        }

        [Test, Category("ValidateFlagAgainstValueAndRange")]
        public static void TestFlagRangeOverlaps10A()
        {
            bool valid = LabDEQueryUtilities.ValidateFlagAgainstValueAndRange("A", "50-80", "3-6");

            Assert.That(valid == true);
        }

        [Test, Category("ValidateFlagAgainstValueAndRange")]
        public static void TestFlagRangeOverlaps10H()
        {
            bool valid = LabDEQueryUtilities.ValidateFlagAgainstValueAndRange("H", "50-80", "3-6");

            Assert.That(valid == true);
        }

        [Test, Category("ValidateFlagAgainstValueAndRange")]
        public static void TestFlagRangeOverlaps10L()
        {
            bool valid = LabDEQueryUtilities.ValidateFlagAgainstValueAndRange("L", "50-80", "3-6");

            Assert.That(valid == false);
        }

        [Test, Category("ValidateFlagAgainstValueAndRange")]
        public static void TestFlagPosNeg01()
        {
            bool valid = LabDEQueryUtilities.ValidateFlagAgainstValueAndRange("", "pos", "pos");

            Assert.That(valid == true);
        }

        [Test, Category("ValidateFlagAgainstValueAndRange")]
        public static void TestFlagPosNeg01A()
        {
            bool valid = LabDEQueryUtilities.ValidateFlagAgainstValueAndRange("A", "pos", "pos");

            Assert.That(valid == false);
        }

        [Test, Category("ValidateFlagAgainstValueAndRange")]
        public static void TestFlagPosNeg02()
        {
            bool valid = LabDEQueryUtilities.ValidateFlagAgainstValueAndRange("", "pos", "neg");

            Assert.That(valid == false);
        }

        [Test, Category("ValidateFlagAgainstValueAndRange")]
        public static void TestFlagPosNeg02A()
        {
            bool valid = LabDEQueryUtilities.ValidateFlagAgainstValueAndRange("A", "pos", "neg");

            Assert.That(valid == true);
        }

        [Test, Category("ValidateFlagAgainstValueAndRange")]
        public static void TestFlagPosNeg03()
        {
            bool valid = LabDEQueryUtilities.ValidateFlagAgainstValueAndRange("", "neg", "pos");

            Assert.That(valid == false);
        }

        [Test, Category("ValidateFlagAgainstValueAndRange")]
        public static void TestFlagPosNeg03A()
        {
            bool valid = LabDEQueryUtilities.ValidateFlagAgainstValueAndRange("A", "neg", "pos");

            Assert.That(valid == true);
        }

        [Test, Category("ValidateFlagAgainstValueAndRange")]
        public static void TestFlagPosNeg04()
        {
            bool valid = LabDEQueryUtilities.ValidateFlagAgainstValueAndRange("", "neg", "neg");

            Assert.That(valid == true);
        }

        [Test, Category("ValidateFlagAgainstValueAndRange")]
        public static void TestFlagPosNeg04A()
        {
            bool valid = LabDEQueryUtilities.ValidateFlagAgainstValueAndRange("A", "neg", "neg");

            Assert.That(valid == false);
        }

        [Test, Category("ValidateFlagAgainstValueAndRange")]
        public static void TestFlagPosNeg05()
        {
            bool valid = LabDEQueryUtilities.ValidateFlagAgainstValueAndRange("", "positive", "unknown");

            Assert.That(valid == true);
        }

        [Test, Category("ValidateFlagAgainstValueAndRange")]
        public static void TestFlagPosNeg05A()
        {
            bool valid = LabDEQueryUtilities.ValidateFlagAgainstValueAndRange("A", "positive", "unknown");

            Assert.That(valid == true);
        }

        [Test, Category("ValidateFlagAgainstValueAndRange")]
        public static void TestFlagPosNeg06()
        {
            bool valid = LabDEQueryUtilities.ValidateFlagAgainstValueAndRange("", "unknown", "negative");

            Assert.That(valid == true);
        }

        [Test, Category("ValidateFlagAgainstValueAndRange")]
        public static void TestFlagPosNeg06A()
        {
            bool valid = LabDEQueryUtilities.ValidateFlagAgainstValueAndRange("A", "unknown", "negative");

            Assert.That(valid == true);
        }

        [Test, Category("ValidateFlagAgainstValueAndRange")]
        public static void TestFlagPosNeg07()
        {
            bool valid = LabDEQueryUtilities.ValidateFlagAgainstValueAndRange("", "0", "positive");

            Assert.That(valid == false);
        }

        [Test, Category("ValidateFlagAgainstValueAndRange")]
        public static void TestFlagPosNeg07A()
        {
            bool valid = LabDEQueryUtilities.ValidateFlagAgainstValueAndRange("A", "0", "positive");

            Assert.That(valid == true);
        }

        [Test, Category("ValidateFlagAgainstValueAndRange")]
        public static void TestFlagPosNeg08()
        {
            bool valid = LabDEQueryUtilities.ValidateFlagAgainstValueAndRange("", "1", "POS");

            Assert.That(valid == true);
        }

        [Test, Category("ValidateFlagAgainstValueAndRange")]
        public static void TestFlagPosNeg08A()
        {
            bool valid = LabDEQueryUtilities.ValidateFlagAgainstValueAndRange("A", "1", "POS");

            Assert.That(valid == false);
        }

        [Test, Category("ValidateFlagAgainstValueAndRange")]
        public static void TestFlagPosNeg09()
        {
            bool valid = LabDEQueryUtilities.ValidateFlagAgainstValueAndRange("", "0", "neg");

            Assert.That(valid == true);
        }

        [Test, Category("ValidateFlagAgainstValueAndRange")]
        public static void TestFlagPosNeg09A()
        {
            bool valid = LabDEQueryUtilities.ValidateFlagAgainstValueAndRange("A", "0", "neg");

            Assert.That(valid == false);
        }

        [Test, Category("ValidateFlagAgainstValueAndRange")]
        public static void TestFlagPosNeg10()
        {
            bool valid = LabDEQueryUtilities.ValidateFlagAgainstValueAndRange("", "1", "NEGATIVE");

            Assert.That(valid == false);
        }

        [Test, Category("ValidateFlagAgainstValueAndRange")]
        public static void TestFlagPosNeg10A()
        {
            bool valid = LabDEQueryUtilities.ValidateFlagAgainstValueAndRange("A", "1", "NEGATIVE");

            Assert.That(valid == true);
        }

        [Test, Category("ValidateFlagAgainstValueAndRange")]
        public static void TestFlagOr01()
        {
            bool valid = LabDEQueryUtilities.ValidateFlagAgainstValueAndRange("", "59", "> OR = 60");

            Assert.That(valid == false);
        }

        [Test, Category("ValidateFlagAgainstValueAndRange")]
        public static void TestFlagOr02()
        {
            bool valid = LabDEQueryUtilities.ValidateFlagAgainstValueAndRange("L", "59", "> OR = 60");

            Assert.That(valid == true);
        }

        [Test, Category("ValidateFlagAgainstValueAndRange")]
        public static void TestFlagOr03()
        {
            bool valid = LabDEQueryUtilities.ValidateFlagAgainstValueAndRange("", "61", "< OR = 60");

            Assert.That(valid == false);
        }

        [Test, Category("ValidateFlagAgainstValueAndRange")]
        public static void TestFlagOr04()
        {
            bool valid = LabDEQueryUtilities.ValidateFlagAgainstValueAndRange("H", "61", "< OR = 60");

            Assert.That(valid == true);
        }

        [Test, Category("ValidateFlagAgainstValueAndRange")]
        public static void TestFlagOr05()
        {
            bool valid = LabDEQueryUtilities.ValidateFlagAgainstValueAndRange("", "> OR = 60", "< 60");

            Assert.That(valid == false);
        }

        [Test, Category("ValidateFlagAgainstValueAndRange")]
        public static void TestFlagOr06()
        {
            bool valid = LabDEQueryUtilities.ValidateFlagAgainstValueAndRange("", "< OR = 60", "> 60");

            Assert.That(valid == false);
        }

        [Test, Category("ValidateFlagAgainstValueAndRange")]
        public static void TestFlagOr07()
        {
            bool valid = LabDEQueryUtilities.ValidateFlagAgainstValueAndRange("H", "> OR = 60", "< 60");

            Assert.That(valid == true);
        }

        [Test, Category("ValidateFlagAgainstValueAndRange")]
        public static void TestFlagOr08()
        {
            bool valid = LabDEQueryUtilities.ValidateFlagAgainstValueAndRange("", "> OR = 60", "< OR = 60");

            Assert.That(valid == true);
        }

        [Test, Category("ValidateFlagAgainstValueAndRange")]
        public static void TestFlagOr09()
        {
            bool valid = LabDEQueryUtilities.ValidateFlagAgainstValueAndRange("A", "59", ">OR=60");

            Assert.That(valid == true);
        }

        [Test, Category("ValidateFlagAgainstValueAndRange")]
        public static void TestFlagOr10()
        {
            bool valid = LabDEQueryUtilities.ValidateFlagAgainstValueAndRange("A", "60", ">OR=60");

            Assert.That(valid == false);
        }

        [Test, Category("ValidateFlagAgainstValueAndRange")]
        public static void TestFlagOr11()
        {
            bool valid = LabDEQueryUtilities.ValidateFlagAgainstValueAndRange("", ">OR=60", "<OR=70");

            Assert.That(valid == true);
        }

        [Test, Category("ValidateFlagAgainstValueAndRange")]
        public static void TestFlagOr12()
        {
            bool valid = LabDEQueryUtilities.ValidateFlagAgainstValueAndRange("H", ">OR=60", "<OR=70");

            Assert.That(valid == true);
        }

        [Test, Category("ValidateFlagAgainstValueAndRange")]
        public static void TestFlagOr13()
        {
            bool valid = LabDEQueryUtilities.ValidateFlagAgainstValueAndRange("L", ">OR=60", "<OR=70");

            Assert.That(valid == false);
        }

        [Test, Category("ValidateFlagAgainstValueAndRange")]
        public static void TestGreaterThanRangeBias01()
        {
            bool valid = LabDEQueryUtilities.ValidateFlagAgainstValueAndRange("", ">2", "2");

            Assert.That(valid == false);
        }

        [Test, Category("ValidateFlagAgainstValueAndRange")]
        public static void TestGreaterThanRangeBias01A()
        {
            bool valid = LabDEQueryUtilities.ValidateFlagAgainstValueAndRange("A", ">2", "2");

            Assert.That(valid == true);
        }

        [Test, Category("ValidateFlagAgainstValueAndRange")]
        public static void TestGreaterThanRangeBias01H()
        {
            bool valid = LabDEQueryUtilities.ValidateFlagAgainstValueAndRange("H", ">2", "2");

            Assert.That(valid == true);
        }

        [Test, Category("ValidateFlagAgainstValueAndRange")]
        public static void TestGreaterThanRangeBias01L()
        {
            bool valid = LabDEQueryUtilities.ValidateFlagAgainstValueAndRange("L", ">2", "2");

            Assert.That(valid == false);
        }

        [Test, Category("ValidateFlagAgainstValueAndRange")]
        public static void TestGreaterThanRangeBias02()
        {
            bool valid = LabDEQueryUtilities.ValidateFlagAgainstValueAndRange("", "2", ">2");

            Assert.That(valid == false);
        }

        [Test, Category("ValidateFlagAgainstValueAndRange")]
        public static void TestGreaterThanRangeBias02A()
        {
            bool valid = LabDEQueryUtilities.ValidateFlagAgainstValueAndRange("A", "2", ">2");

            Assert.That(valid == true);
        }

        [Test, Category("ValidateFlagAgainstValueAndRange")]
        public static void TestGreaterThanRangeBias02H()
        {
            bool valid = LabDEQueryUtilities.ValidateFlagAgainstValueAndRange("H", "2", ">2");

            Assert.That(valid == false);
        }

        [Test, Category("ValidateFlagAgainstValueAndRange")]
        public static void TestGreaterThanRangeBias02L()
        {
            bool valid = LabDEQueryUtilities.ValidateFlagAgainstValueAndRange("L", "2", ">2");

            Assert.That(valid == true);
        }

        [Test, Category("ValidateFlagAgainstValueAndRange")]
        public static void TestGreaterThanRangeBias03()
        {
            bool valid = LabDEQueryUtilities.ValidateFlagAgainstValueAndRange("", ">=2", "2");

            Assert.That(valid == true);
        }

        [Test, Category("ValidateFlagAgainstValueAndRange")]
        public static void TestGreaterThanRangeBias03A()
        {
            bool valid = LabDEQueryUtilities.ValidateFlagAgainstValueAndRange("A", ">=2", "2");

            Assert.That(valid == true);
        }

        [Test, Category("ValidateFlagAgainstValueAndRange")]
        public static void TestGreaterThanRangeBias03H()
        {
            bool valid = LabDEQueryUtilities.ValidateFlagAgainstValueAndRange("H", ">=2", "2");

            Assert.That(valid == true);
        }

        [Test, Category("ValidateFlagAgainstValueAndRange")]
        public static void TestGreaterThanRangeBias03L()
        {
            bool valid = LabDEQueryUtilities.ValidateFlagAgainstValueAndRange("L", ">=2", "2");

            Assert.That(valid == false);
        }

        [Test, Category("ValidateFlagAgainstValueAndRange")]
        public static void TestGreaterThanRangeBias04()
        {
            bool valid = LabDEQueryUtilities.ValidateFlagAgainstValueAndRange("", "2", ">=2");

            Assert.That(valid == true);
        }

        [Test, Category("ValidateFlagAgainstValueAndRange")]
        public static void TestGreaterThanRangeBias04A()
        {
            bool valid = LabDEQueryUtilities.ValidateFlagAgainstValueAndRange("A", "2", ">=2");

            Assert.That(valid == false);
        }

        [Test, Category("ValidateFlagAgainstValueAndRange")]
        public static void TestGreaterThanRangeBias04H()
        {
            bool valid = LabDEQueryUtilities.ValidateFlagAgainstValueAndRange("H", "2", ">=2");

            Assert.That(valid == false);
        }

        [Test, Category("ValidateFlagAgainstValueAndRange")]
        public static void TestGreaterThanRangeBias04L()
        {
            bool valid = LabDEQueryUtilities.ValidateFlagAgainstValueAndRange("L", "2", ">=2");

            Assert.That(valid == false);
        }

        [Test, Category("ValidateFlagAgainstValueAndRange")]
        public static void TestGreaterThanRangeBias05()
        {
            bool valid = LabDEQueryUtilities.ValidateFlagAgainstValueAndRange("", ">2", ">2");

            Assert.That(valid == true);
        }

        [Test, Category("ValidateFlagAgainstValueAndRange")]
        public static void TestGreaterThanRangeBias05A()
        {
            bool valid = LabDEQueryUtilities.ValidateFlagAgainstValueAndRange("A", ">2", ">2");

            Assert.That(valid == false);
        }

        [Test, Category("ValidateFlagAgainstValueAndRange")]
        public static void TestGreaterThanRangeBias05H()
        {
            bool valid = LabDEQueryUtilities.ValidateFlagAgainstValueAndRange("H", ">2", ">2");

            Assert.That(valid == false);
        }

        [Test, Category("ValidateFlagAgainstValueAndRange")]
        public static void TestGreaterThanRangeBias05L()
        {
            bool valid = LabDEQueryUtilities.ValidateFlagAgainstValueAndRange("L", ">2", ">2");

            Assert.That(valid == false);
        }

        [Test, Category("ValidateFlagAgainstValueAndRange")]
        public static void TestGreaterThanRangeBias06()
        {
            bool valid = LabDEQueryUtilities.ValidateFlagAgainstValueAndRange("", ">2", ">=2");

            Assert.That(valid == true);
        }

        [Test, Category("ValidateFlagAgainstValueAndRange")]
        public static void TestGreaterThanRangeBias06A()
        {
            bool valid = LabDEQueryUtilities.ValidateFlagAgainstValueAndRange("A", ">2", ">=2");

            Assert.That(valid == false);
        }

        [Test, Category("ValidateFlagAgainstValueAndRange")]
        public static void TestGreaterThanRangeBias06H()
        {
            bool valid = LabDEQueryUtilities.ValidateFlagAgainstValueAndRange("H", ">2", ">=2");

            Assert.That(valid == false);
        }

        [Test, Category("ValidateFlagAgainstValueAndRange")]
        public static void TestGreaterThanRangeBias06L()
        {
            bool valid = LabDEQueryUtilities.ValidateFlagAgainstValueAndRange("L", ">2", ">=2");

            Assert.That(valid == false);
        }

        [Test, Category("ValidateFlagAgainstValueAndRange")]
        public static void TestGreaterThanRangeBias07()
        {
            bool valid = LabDEQueryUtilities.ValidateFlagAgainstValueAndRange("", ">=2", ">2");

            Assert.That(valid == true);
        }

        [Test, Category("ValidateFlagAgainstValueAndRange")]
        public static void TestGreaterThanRangeBias07A()
        {
            bool valid = LabDEQueryUtilities.ValidateFlagAgainstValueAndRange("A", ">=2", ">2");

            Assert.That(valid == true);
        }

        [Test, Category("ValidateFlagAgainstValueAndRange")]
        public static void TestGreaterThanRangeBias07H()
        {
            bool valid = LabDEQueryUtilities.ValidateFlagAgainstValueAndRange("H", ">=2", ">2");

            Assert.That(valid == false);
        }

        [Test, Category("ValidateFlagAgainstValueAndRange")]
        public static void TestGreaterThanRangeBias07L()
        {
            bool valid = LabDEQueryUtilities.ValidateFlagAgainstValueAndRange("L", ">=2", ">2");

            Assert.That(valid == true);
        }

        [Test, Category("ValidateFlagAgainstValueAndRange")]
        public static void TestGreaterThanRangeBias08()
        {
            bool valid = LabDEQueryUtilities.ValidateFlagAgainstValueAndRange("", ">=2", ">=2");

            Assert.That(valid == true);
        }

        [Test, Category("ValidateFlagAgainstValueAndRange")]
        public static void TestGreaterThanRangeBias08A()
        {
            bool valid = LabDEQueryUtilities.ValidateFlagAgainstValueAndRange("A", ">=2", ">=2");

            Assert.That(valid == false);
        }

        [Test, Category("ValidateFlagAgainstValueAndRange")]
        public static void TestGreaterThanRangeBias08H()
        {
            bool valid = LabDEQueryUtilities.ValidateFlagAgainstValueAndRange("H", ">=2", ">=2");

            Assert.That(valid == false);
        }

        [Test, Category("ValidateFlagAgainstValueAndRange")]
        public static void TestGreaterThanRangeBias08L()
        {
            bool valid = LabDEQueryUtilities.ValidateFlagAgainstValueAndRange("L", ">=2", ">=2");

            Assert.That(valid == false);
        }

        [Test, Category("ValidateFlagAgainstValueAndRange")]
        public static void TestLessThanRangeBias01()
        {
            bool valid = LabDEQueryUtilities.ValidateFlagAgainstValueAndRange("", "<2", "2");

            Assert.That(valid == false);
        }

        [Test, Category("ValidateFlagAgainstValueAndRange")]
        public static void TestLessThanRangeBias01A()
        {
            bool valid = LabDEQueryUtilities.ValidateFlagAgainstValueAndRange("A", "<2", "2");

            Assert.That(valid == true);
        }

        [Test, Category("ValidateFlagAgainstValueAndRange")]
        public static void TestLessThanRangeBias01H()
        {
            bool valid = LabDEQueryUtilities.ValidateFlagAgainstValueAndRange("H", "<2", "2");

            Assert.That(valid == false);
        }

        [Test, Category("ValidateFlagAgainstValueAndRange")]
        public static void TestLessThanRangeBias01L()
        {
            bool valid = LabDEQueryUtilities.ValidateFlagAgainstValueAndRange("L", "<2", "2");

            Assert.That(valid == true);
        }

        [Test, Category("ValidateFlagAgainstValueAndRange")]
        public static void TestLessThanRangeBias02()
        {
            bool valid = LabDEQueryUtilities.ValidateFlagAgainstValueAndRange("", "2", "<2");

            Assert.That(valid == false);
        }

        [Test, Category("ValidateFlagAgainstValueAndRange")]
        public static void TestLessThanRangeBias02A()
        {
            bool valid = LabDEQueryUtilities.ValidateFlagAgainstValueAndRange("A", "2", "<2");

            Assert.That(valid == true);
        }

        [Test, Category("ValidateFlagAgainstValueAndRange")]
        public static void TestLessThanRangeBias02H()
        {
            bool valid = LabDEQueryUtilities.ValidateFlagAgainstValueAndRange("H", "2", "<2");

            Assert.That(valid == true);
        }

        [Test, Category("ValidateFlagAgainstValueAndRange")]
        public static void TestLessThanRangeBias02L()
        {
            bool valid = LabDEQueryUtilities.ValidateFlagAgainstValueAndRange("L", "2", "<2");

            Assert.That(valid == false);
        }

        [Test, Category("ValidateFlagAgainstValueAndRange")]
        public static void TestLessThanRangeBias03()
        {
            bool valid = LabDEQueryUtilities.ValidateFlagAgainstValueAndRange("", "<=2", "2");

            Assert.That(valid == true);
        }

        [Test, Category("ValidateFlagAgainstValueAndRange")]
        public static void TestLessThanRangeBias03A()
        {
            bool valid = LabDEQueryUtilities.ValidateFlagAgainstValueAndRange("A", "<=2", "2");

            Assert.That(valid == true);
        }

        [Test, Category("ValidateFlagAgainstValueAndRange")]
        public static void TestLessThanRangeBias03H()
        {
            bool valid = LabDEQueryUtilities.ValidateFlagAgainstValueAndRange("H", "<=2", "2");

            Assert.That(valid == false);
        }

        [Test, Category("ValidateFlagAgainstValueAndRange")]
        public static void TestLessThanRangeBias03L()
        {
            bool valid = LabDEQueryUtilities.ValidateFlagAgainstValueAndRange("L", "<=2", "2");

            Assert.That(valid == true);
        }

        [Test, Category("ValidateFlagAgainstValueAndRange")]
        public static void TestLessThanRangeBias04()
        {
            bool valid = LabDEQueryUtilities.ValidateFlagAgainstValueAndRange("", "2", "<=2");

            Assert.That(valid == true);
        }

        [Test, Category("ValidateFlagAgainstValueAndRange")]
        public static void TestLessThanRangeBias04A()
        {
            bool valid = LabDEQueryUtilities.ValidateFlagAgainstValueAndRange("A", "2", "<=2");

            Assert.That(valid == false);
        }

        [Test, Category("ValidateFlagAgainstValueAndRange")]
        public static void TestLessThanRangeBias04H()
        {
            bool valid = LabDEQueryUtilities.ValidateFlagAgainstValueAndRange("H", "2", "<=2");

            Assert.That(valid == false);
        }

        [Test, Category("ValidateFlagAgainstValueAndRange")]
        public static void TestLessThanRangeBias04L()
        {
            bool valid = LabDEQueryUtilities.ValidateFlagAgainstValueAndRange("L", "2", "<=2");

            Assert.That(valid == false);
        }

        [Test, Category("ValidateFlagAgainstValueAndRange")]
        public static void TestLessThanRangeBias05()
        {
            bool valid = LabDEQueryUtilities.ValidateFlagAgainstValueAndRange("", "<2", "<2");

            Assert.That(valid == true);
        }

        [Test, Category("ValidateFlagAgainstValueAndRange")]
        public static void TestLessThanRangeBias05A()
        {
            bool valid = LabDEQueryUtilities.ValidateFlagAgainstValueAndRange("A", "<2", "<2");

            Assert.That(valid == false);
        }

        [Test, Category("ValidateFlagAgainstValueAndRange")]
        public static void TestLessThanRangeBias05H()
        {
            bool valid = LabDEQueryUtilities.ValidateFlagAgainstValueAndRange("H", "<2", "<2");

            Assert.That(valid == false);
        }

        [Test, Category("ValidateFlagAgainstValueAndRange")]
        public static void TestLessThanRangeBias05L()
        {
            bool valid = LabDEQueryUtilities.ValidateFlagAgainstValueAndRange("L", "<2", "<2");

            Assert.That(valid == false);
        }

        [Test, Category("ValidateFlagAgainstValueAndRange")]
        public static void TestLessThanRangeBias06()
        {
            bool valid = LabDEQueryUtilities.ValidateFlagAgainstValueAndRange("", "<2", "<=2");

            Assert.That(valid == true);
        }

        [Test, Category("ValidateFlagAgainstValueAndRange")]
        public static void TestLessThanRangeBias06A()
        {
            bool valid = LabDEQueryUtilities.ValidateFlagAgainstValueAndRange("A", "<2", "<=2");

            Assert.That(valid == false);
        }

        [Test, Category("ValidateFlagAgainstValueAndRange")]
        public static void TestLessThanRangeBias06H()
        {
            bool valid = LabDEQueryUtilities.ValidateFlagAgainstValueAndRange("H", "<2", "<=2");

            Assert.That(valid == false);
        }

        [Test, Category("ValidateFlagAgainstValueAndRange")]
        public static void TestLessThanRangeBias06L()
        {
            bool valid = LabDEQueryUtilities.ValidateFlagAgainstValueAndRange("L", "<2", "<=2");

            Assert.That(valid == false);
        }

        [Test, Category("ValidateFlagAgainstValueAndRange")]
        public static void TestLessThanRangeBias07()
        {
            bool valid = LabDEQueryUtilities.ValidateFlagAgainstValueAndRange("", "<=2", "<2");

            Assert.That(valid == true);
        }

        [Test, Category("ValidateFlagAgainstValueAndRange")]
        public static void TestLessThanRangeBias07A()
        {
            bool valid = LabDEQueryUtilities.ValidateFlagAgainstValueAndRange("A", "<=2", "<2");

            Assert.That(valid == true);
        }

        [Test, Category("ValidateFlagAgainstValueAndRange")]
        public static void TestLessThanRangeBias07H()
        {
            bool valid = LabDEQueryUtilities.ValidateFlagAgainstValueAndRange("H", "<=2", "<2");

            Assert.That(valid == true);
        }

        [Test, Category("ValidateFlagAgainstValueAndRange")]
        public static void TestLessThanRangeBias07L()
        {
            bool valid = LabDEQueryUtilities.ValidateFlagAgainstValueAndRange("L", "<=2", "<2");

            Assert.That(valid == false);
        }

        [Test, Category("ValidateFlagAgainstValueAndRange")]
        public static void TestLessThanRangeBias08()
        {
            bool valid = LabDEQueryUtilities.ValidateFlagAgainstValueAndRange("", "<=2", "<=2");

            Assert.That(valid == true);
        }

        [Test, Category("ValidateFlagAgainstValueAndRange")]
        public static void TestLessThanRangeBias08A()
        {
            bool valid = LabDEQueryUtilities.ValidateFlagAgainstValueAndRange("A", "<=2", "<=2");

            Assert.That(valid == false);
        }

        [Test, Category("ValidateFlagAgainstValueAndRange")]
        public static void TestLessThanRangeBias08H()
        {
            bool valid = LabDEQueryUtilities.ValidateFlagAgainstValueAndRange("H", "<=2", "<=2");

            Assert.That(valid == false);
        }

        [Test, Category("ValidateFlagAgainstValueAndRange")]
        public static void TestLessThanRangeBias08L()
        {
            bool valid = LabDEQueryUtilities.ValidateFlagAgainstValueAndRange("L", "<=2", "<=2");

            Assert.That(valid == false);
        }

        [Test, Category("ValidateFlagAgainstValueAndRange")]
        public static void TestOpposingRangeBias01()
        {
            bool valid = LabDEQueryUtilities.ValidateFlagAgainstValueAndRange("", ">2", "<2");

            Assert.That(valid == false);
        }

        [Test, Category("ValidateFlagAgainstValueAndRange")]
        public static void TestOpposingRangeBias01A()
        {
            bool valid = LabDEQueryUtilities.ValidateFlagAgainstValueAndRange("A", ">2", "<2");

            Assert.That(valid == true);
        }

        [Test, Category("ValidateFlagAgainstValueAndRange")]
        public static void TestOpposingRangeBias01H()
        {
            bool valid = LabDEQueryUtilities.ValidateFlagAgainstValueAndRange("H", ">2", "<2");

            Assert.That(valid == true);
        }

        [Test, Category("ValidateFlagAgainstValueAndRange")]
        public static void TestOpposingRangeBias01L()
        {
            bool valid = LabDEQueryUtilities.ValidateFlagAgainstValueAndRange("L", ">2", "<2");

            Assert.That(valid == false);
        }

        [Test, Category("ValidateFlagAgainstValueAndRange")]
        public static void TestOpposingRangeBias02()
        {
            bool valid = LabDEQueryUtilities.ValidateFlagAgainstValueAndRange("", ">2", "<=2");

            Assert.That(valid == false);
        }

        [Test, Category("ValidateFlagAgainstValueAndRange")]
        public static void TestOpposingRangeBias02A()
        {
            bool valid = LabDEQueryUtilities.ValidateFlagAgainstValueAndRange("A", ">2", "<=2");

            Assert.That(valid == true);
        }

        [Test, Category("ValidateFlagAgainstValueAndRange")]
        public static void TestOpposingRangeBias02H()
        {
            bool valid = LabDEQueryUtilities.ValidateFlagAgainstValueAndRange("H", ">2", "<=2");

            Assert.That(valid == true);
        }

        [Test, Category("ValidateFlagAgainstValueAndRange")]
        public static void TestOpposingRangeBias02L()
        {
            bool valid = LabDEQueryUtilities.ValidateFlagAgainstValueAndRange("L", ">2", "<=2");

            Assert.That(valid == false);
        }

        [Test, Category("ValidateFlagAgainstValueAndRange")]
        public static void TestOpposingRangeBias03()
        {
            bool valid = LabDEQueryUtilities.ValidateFlagAgainstValueAndRange("", "<2", ">2");

            Assert.That(valid == false);
        }

        [Test, Category("ValidateFlagAgainstValueAndRange")]
        public static void TestOpposingRangeBias03A()
        {
            bool valid = LabDEQueryUtilities.ValidateFlagAgainstValueAndRange("A", "<2", ">2");

            Assert.That(valid == true);
        }

        [Test, Category("ValidateFlagAgainstValueAndRange")]
        public static void TestOpposingRangeBias03H()
        {
            bool valid = LabDEQueryUtilities.ValidateFlagAgainstValueAndRange("H", "<2", ">2");

            Assert.That(valid == false);
        }

        [Test, Category("ValidateFlagAgainstValueAndRange")]
        public static void TestOpposingRangeBias03L()
        {
            bool valid = LabDEQueryUtilities.ValidateFlagAgainstValueAndRange("L", "<2", ">2");

            Assert.That(valid == true);
        }

        [Test, Category("ValidateFlagAgainstValueAndRange")]
        public static void TestOpposingRangeBias04()
        {
            bool valid = LabDEQueryUtilities.ValidateFlagAgainstValueAndRange("", "<2", ">=2");

            Assert.That(valid == false);
        }

        [Test, Category("ValidateFlagAgainstValueAndRange")]
        public static void TestOpposingRangeBias04A()
        {
            bool valid = LabDEQueryUtilities.ValidateFlagAgainstValueAndRange("A", "<2", ">=2");

            Assert.That(valid == true);
        }

        [Test, Category("ValidateFlagAgainstValueAndRange")]
        public static void TestOpposingRangeBias04H()
        {
            bool valid = LabDEQueryUtilities.ValidateFlagAgainstValueAndRange("H", "<2", ">=2");

            Assert.That(valid == false);
        }

        [Test, Category("ValidateFlagAgainstValueAndRange")]
        public static void TestOpposingRangeBias04L()
        {
            bool valid = LabDEQueryUtilities.ValidateFlagAgainstValueAndRange("L", "<2", ">=2");

            Assert.That(valid == true);
        }

        [Test, Category("ValidateFlagAgainstValueAndRange")]
        public static void TestOpposingRangeBias05()
        {
            bool valid = LabDEQueryUtilities.ValidateFlagAgainstValueAndRange("", ">=2", "<2");

            Assert.That(valid == false);
        }

        [Test, Category("ValidateFlagAgainstValueAndRange")]
        public static void TestOpposingRangeBias05A()
        {
            bool valid = LabDEQueryUtilities.ValidateFlagAgainstValueAndRange("A", ">=2", "<2");

            Assert.That(valid == true);
        }

        [Test, Category("ValidateFlagAgainstValueAndRange")]
        public static void TestOpposingRangeBias05H()
        {
            bool valid = LabDEQueryUtilities.ValidateFlagAgainstValueAndRange("H", ">=2", "<2");

            Assert.That(valid == true);
        }

        [Test, Category("ValidateFlagAgainstValueAndRange")]
        public static void TestOpposingRangeBias05L()
        {
            bool valid = LabDEQueryUtilities.ValidateFlagAgainstValueAndRange("L", ">=2", "<2");

            Assert.That(valid == false);
        }

        [Test, Category("ValidateFlagAgainstValueAndRange")]
        public static void TestOpposingRangeBias06()
        {
            bool valid = LabDEQueryUtilities.ValidateFlagAgainstValueAndRange("", ">=2", "<=2");

            Assert.That(valid == true);
        }

        [Test, Category("ValidateFlagAgainstValueAndRange")]
        public static void TestOpposingRangeBias06A()
        {
            bool valid = LabDEQueryUtilities.ValidateFlagAgainstValueAndRange("A", ">=2", "<=2");

            Assert.That(valid == true);
        }

        [Test, Category("ValidateFlagAgainstValueAndRange")]
        public static void TestOpposingRangeBias06H()
        {
            bool valid = LabDEQueryUtilities.ValidateFlagAgainstValueAndRange("H", ">=2", "<=2");

            Assert.That(valid == true);
        }

        [Test, Category("ValidateFlagAgainstValueAndRange")]
        public static void TestOpposingRangeBias06L()
        {
            bool valid = LabDEQueryUtilities.ValidateFlagAgainstValueAndRange("L", ">=2", "<=2");

            Assert.That(valid == false);
        }

        [Test, Category("ValidateFlagAgainstValueAndRange")]
        public static void TestOpposingRangeBias07()
        {
            bool valid = LabDEQueryUtilities.ValidateFlagAgainstValueAndRange("", "<=2", ">2");

            Assert.That(valid == false);
        }

        [Test, Category("ValidateFlagAgainstValueAndRange")]
        public static void TestOpposingRangeBias07A()
        {
            bool valid = LabDEQueryUtilities.ValidateFlagAgainstValueAndRange("A", "<=2", ">2");

            Assert.That(valid == true);
        }

        [Test, Category("ValidateFlagAgainstValueAndRange")]
        public static void TestOpposingRangeBias07H()
        {
            bool valid = LabDEQueryUtilities.ValidateFlagAgainstValueAndRange("H", "<=2", ">2");

            Assert.That(valid == false);
        }

        [Test, Category("ValidateFlagAgainstValueAndRange")]
        public static void TestOpposingRangeBias07L()
        {
            bool valid = LabDEQueryUtilities.ValidateFlagAgainstValueAndRange("L", "<=2", ">2");

            Assert.That(valid == true);
        }

        [Test, Category("ValidateFlagAgainstValueAndRange")]
        public static void TestOpposingRangeBias08()
        {
            bool valid = LabDEQueryUtilities.ValidateFlagAgainstValueAndRange("", "<=2", ">=2");

            Assert.That(valid == true);
        }

        [Test, Category("ValidateFlagAgainstValueAndRange")]
        public static void TestOpposingRangeBias08A()
        {
            bool valid = LabDEQueryUtilities.ValidateFlagAgainstValueAndRange("A", "<=2", ">=2");

            Assert.That(valid == true);
        }

        [Test, Category("ValidateFlagAgainstValueAndRange")]
        public static void TestOpposingRangeBias08H()
        {
            bool valid = LabDEQueryUtilities.ValidateFlagAgainstValueAndRange("H", "<=2", ">=2");

            Assert.That(valid == false);
        }

        [Test, Category("ValidateFlagAgainstValueAndRange")]
        public static void TestOpposingRangeBias08L()
        {
            bool valid = LabDEQueryUtilities.ValidateFlagAgainstValueAndRange("L", "<=2", ">=2");

            Assert.That(valid == true);
        }

        [Test, Category("ValidateFlagAgainstValueAndRange")]
        public static void TestOpposingRangeBias09()
        {
            bool valid = LabDEQueryUtilities.ValidateFlagAgainstValueAndRange("", ">2", "<=2.0");

            Assert.That(valid == false);
        }

        [Test, Category("ValidateFlagAgainstValueAndRange")]
        public static void TestOpposingRangeBias09A()
        {
            bool valid = LabDEQueryUtilities.ValidateFlagAgainstValueAndRange("A", ">2", "<=2.0");

            Assert.That(valid == true);
        }

        [Test, Category("ValidateFlagAgainstValueAndRange")]
        public static void TestOpposingRangeBias09H()
        {
            bool valid = LabDEQueryUtilities.ValidateFlagAgainstValueAndRange("H", ">2", "<=2.0");

            Assert.That(valid == true);
        }

        [Test, Category("ValidateFlagAgainstValueAndRange")]
        public static void TestOpposingRangeBias09L()
        {
            bool valid = LabDEQueryUtilities.ValidateFlagAgainstValueAndRange("L", ">2", "<=2.0");

            Assert.That(valid == false);
        }

        [Test, Category("ValidateFlagAgainstValueAndRange")]
        public static void TestOpposingRangeBias10()
        {
            bool valid = LabDEQueryUtilities.ValidateFlagAgainstValueAndRange("", "<2.0", ">=2");

            Assert.That(valid == false);
        }

        [Test, Category("ValidateFlagAgainstValueAndRange")]
        public static void TestOpposingRangeBias10A()
        {
            bool valid = LabDEQueryUtilities.ValidateFlagAgainstValueAndRange("A", "<2.0", ">=2");

            Assert.That(valid == true);
        }

        [Test, Category("ValidateFlagAgainstValueAndRange")]
        public static void TestOpposingRangeBias10H()
        {
            bool valid = LabDEQueryUtilities.ValidateFlagAgainstValueAndRange("H", "<2.0", ">=2");

            Assert.That(valid == false);
        }

        [Test, Category("ValidateFlagAgainstValueAndRange")]
        public static void TestOpposingRangeBias10L()
        {
            bool valid = LabDEQueryUtilities.ValidateFlagAgainstValueAndRange("L", "<2.0", ">=2");

            Assert.That(valid == true);
        }

        [Test, Category("ValidateFlagAgainstValueAndRange")]
        public static void TestInvalidValue()
        {
            bool valid = LabDEQueryUtilities.ValidateFlagAgainstValueAndRange("", "23. .4", "12.0-14");

            Assert.That(valid == true);
        }

        [Test, Category("ValidateFlagAgainstValueAndRange")]
        public static void TestInvalidRange()
        {
            bool valid = LabDEQueryUtilities.ValidateFlagAgainstValueAndRange("L", "1.2", "0.02.0-6.9");

            Assert.That(valid == true);
        }
    }
}
