using Extract.Redaction;
using Extract.Testing.Utilities;
using Extract.Utilities;
using Extract.Utilities.Parsers;
using NUnit.Framework;
using System;
using System.IO;
using System.Linq;
using System.Text;
using UCLID_AFCORELib;
using UCLID_AFVALUEFINDERSLib;
using UCLID_RASTERANDOCRMGMTLib;

namespace Extract.FileActionManager.FileProcessors.Test
{
    /// <summary>
    /// Provides unit test cases for the <see cref="CreateRedactedTextTask"/>.
    /// </summary>
    [TestFixture]
    [Category("CreateRedactedTextTask")]
    public class TestCreateRedactedTextTask
    {
        #region Constants

        const string RTF_HEADER = @"\rtf1\ansi\ansicpg1252\deff0\nouicompat\deflang1033";

        #endregion Constants

        #region Fields

        #endregion Fields

        #region Overhead Methods

        /// <summary>
        /// Setup method to initialize the testing environment.
        /// </summary>
        [TestFixtureSetUp]
        public static void Setup()
        {
            GeneralMethods.TestSetup();
        }

        /// <summary>
        /// Cleanup after all tests have run.
        /// </summary>
        [TestFixtureTearDown]
        public static void FinalCleanup()
        {
        }

        #endregion Overhead Methods

        #region Tests

        /// <summary>
        /// Tests redacting plain text
        /// </summary>
        [Test]
        public static void RedactPlainTextFromRasterZone()
        {
            string inp = "Test redacting a thing: [RedactMe]";
            string exp = "Test redacting a thing: XXXXXXXXXX";

            (byte[] inputBytes, SpatialString displayString) = GetRedactTaskInputFromString(inp);

            int redactionStartIdx = 24;
            int redactionLength = 10;
            var redactionZones = new[]
            {
                new RasterZoneClass
                {
                    StartX = redactionStartIdx,
                    EndX = redactionStartIdx + redactionLength,
                    StartY = 0,
                    EndY = 0,
                    Height = 1
                }
            };

            var task = new CreateRedactedTextTask
            {
                TaskSettings = new CreateRedactedTextSettings(true, false, new string[0], RedactionMethod.ReplaceCharacters, CharacterClass.All, "X", false, 0, null, null, null, null)
            };

            byte[] redactedBytes = task.RedactBytes(inputBytes, displayString, redactionZones, false);
            string redactedString = GetRedactedStringFromBytes(redactedBytes);

            Assert.AreEqual(exp, redactedString);
        }

        /// <summary>
        /// Tests redacting plain text using a rule to get the redaction zones
        /// </summary>
        [Test]
        public static void RedactPlainTextFromRulesOutput()
        {
            string inp = "Test redacting a thing: [RedactMe]";
            string exp = "Test redacting a thing: XXXXXXXXXX";

            (byte[] inputBytes, SpatialString displayString) = GetRedactTaskInputFromString(inp);
            var doc = new AFDocumentClass { Text = displayString };

            var rule = new RegExprRuleClass { Pattern = @"\[RedactMe\]" };
            var redactionZones =
                rule.ParseText(doc, null)
                .ToIEnumerable<IAttribute>()
                .SelectMany(attr => attr.Value.GetOCRImageRasterZones().ToIEnumerable<RasterZone>());

            var task = new CreateRedactedTextTask
            {
                TaskSettings = new CreateRedactedTextSettings(true, false, new string[0], RedactionMethod.ReplaceCharacters, CharacterClass.All, "X", false, 0, null, null, null, null)
            };

            byte[] redactedBytes = task.RedactBytes(inputBytes, displayString, redactionZones, false);
            string redactedString = GetRedactedStringFromBytes(redactedBytes);

            Assert.AreEqual(exp, redactedString);
        }

        /// <summary>
        /// Tests redacting text that isn't only low-ASCII codes
        /// https://extract.atlassian.net/browse/ISSUE-12345
        /// </summary>
        [Test]
        public static void RedactExtendedASCIIText()
        {
            string inp =
                "Couples can save themselves a lot of heartache – not to mention, financial strain – by having a simple conversation abou" +
                "t what’s in and out of bounds when it comes to their relationship, says nationally recognized intimacy expert Sheri Meye" +
                "rs, Psy.D. <http://fao.r.mailjet.com/link/01nz/o6jpk57/1/FgGyS9HMELW9-msc4GBBAg/aHR0cDovL3d3dy5jaGF0dGluZ29yY2hlYXRpbmcu" +
                "Y29tLw>  (media appearances include “The Steve Harvey Show,” CNN, CBS’s “Live from the Couch,” ABC News, KTLA 5 and more";
            string exp =
                "Couples can save themselves a lot of heartache – not to mention, financial strain – by having a simple conversation abou" +
                "t what’s in and out of bounds when it comes to their relationship, says nationally recognized intimacy expert XXXXXXXXXX" +
                "XX, Psy.D. <XXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXX" +
                "XXXXXX>  (media appearances include “The XXXXXXXXXXXX Show,” CNN, CBS’s “Live from the Couch,” ABC News, KTLA 5 and more";

            (byte[] inputBytes, SpatialString displayString) = GetRedactTaskInputFromString(inp);
            var doc = new AFDocumentClass { Text = displayString };

            var rule = new RegExprRuleClass { Pattern = @"Steve\sHarvey|\bhttp[^\s>]+|Sheri\sMeyers" };
            var redactionZones =
                rule.ParseText(doc, null)
                .ToIEnumerable<IAttribute>()
                .SelectMany(attr => attr.Value.GetOCRImageRasterZones().ToIEnumerable<RasterZone>());

            var task = new CreateRedactedTextTask
            {
                TaskSettings = new CreateRedactedTextSettings(true, false, new string[0], RedactionMethod.ReplaceCharacters, CharacterClass.All, "X", false, 0, null, null, null, null)
            };

            byte[] redactedBytes = task.RedactBytes(inputBytes, displayString, redactionZones, false);
            string redactedString = GetRedactedStringFromBytes(redactedBytes);

            Assert.AreEqual(exp, redactedString);
        }

        /// <summary>
        /// Tests redacting some simple rich text
        /// </summary>
        [Test]
        public static void RedactSimpleRichTextFromRulesOutput()
        {
            string inp = "{" + RTF_HEADER + "Test redacting a thing: [RedactMe]" + "}";
            string exp = "{" + RTF_HEADER + "Test redacting a thing: XXXXXXXXXX" + "}";

            (byte[] inputBytes, SpatialString displayString) = GetRedactTaskInputFromRTFString(inp);
            var doc = new AFDocumentClass { Text = displayString };

            var rule = new RegExprRuleClass { Pattern = @"\[RedactMe\]" };
            var redactionZones =
                rule.ParseText(doc, null)
                .ToIEnumerable<IAttribute>()
                .SelectMany(attr => attr.Value.GetOCRImageRasterZones().ToIEnumerable<RasterZone>());

            var task = new CreateRedactedTextTask
            {
                TaskSettings = new CreateRedactedTextSettings(true, false, new string[0], RedactionMethod.ReplaceCharacters, CharacterClass.All, "X", false, 0, null, null, null, null)
            };

            byte[] redactedBytes = task.RedactBytes(inputBytes, displayString, redactionZones, true);
            string redactedString = GetRedactedStringFromBytes(redactedBytes);

            Assert.AreEqual(exp, redactedString);
        }

        /// <summary>
        /// Tests redacting all 'plain text' of some complex rich text from Davidson TN (Nashville)
        /// </summary>
        [Test]
        public static void RedactComplexRichTextFromRulesOutput()
        {
            string inp =
                @"{\rtf1\ansi\deff0{\fonttbl{\f0\froman Tms Rmn;}{\f1\froman Times New Roman;}{\f2\froman Times;}}{\colortbl\red0\green0\b" +
                @"lue0;\red0\green0\blue255;\red0\green255\blue255;\red0\green255\blue0;\red255\green0\blue255;\red255\green0\blue0;\red25" +
                @"5\green255\blue0;\red255\green255\blue255;\red0\green0\blue127;\red0\green127\blue127;\red0\green127\blue0;\red127\green" +
                @"0\blue127;\red127\green0\blue0;\red127\green127\blue0;\red127\green127\blue127;\red192\green192\blue192;}{\info{\creatim" +
                @"\yr1998\mo12\dy14\hr9\min30\sec50}{\printim\yr1999\mo1\dy12\hr4\min37\sec41}{\version1}{\vern262367}}\paperw12240\paperh" +
                @"15840\margl59\margr0\margt119\margb432\deftab720\pard\ql\li720\fi0\ri1784{\f0\fs24\cf0\up0\dn0\par}\pard\ql\li720\fi0\ri" +
                @"1784{\f0\fs24\cf0\up0\dn0\par}\pard\qc\li720\fi0\ri1784{\f0\fs24\cf0\up0\dn0 \loch\af0 STATE OF TENNESSEE, COUNTY OF DAV" +
                @"IDSON}{\par}\pard\qc\li720\fi0\ri1784{\f0\fs24\cf0\up0\dn0 \loch\af0 AFFIDAVIT}{\par}\pard\qc\li720\fi0\ri1784{\field{\*" +
                @"\fldinst{\f0\fs20\cf0\up0\dn0 QUALIFIER_TYPE}}{\fldrslt{\f0\fs20\cf0\up0\dn0 }}}{\f0\fs24\cf0\up0\dn0 \loch\af0  }{\b\f0" +
                @"\fs24\cf0\up0\dn0 \loch\af0 FORGERY}{\par}\pard\qc\li720\fi0\ri1784{\b\f0\fs24\cf0\up0\dn0 \loch\af0 T.C.A. 39-14-114}{\" +
                @"par}\pard\ql\li720\fi0\ri1784{\f0\fs24\cf0\up0\dn0\par}\pard\qj\li720\fi0\ri1784{\f0\fs24\cf0\up0\dn0 \loch\af0 Personal" +
                @"ly appeared before me, the undersigned, }{\b\f0\fs24\cf0\up0\dn0 \loch\af0 [Select one] }{\f0\fs24\cf0\up0\dn0 \loch\af0" +
                @" x__ Commissioner ___ Metropolitan }\qj{\f0\fs24\cf0\up0\dn0 \loch\af0 General Sessions Judge, the prosecutor named abov" +
                @"e and made oath in due form of law that}{\par}\pard\qj\li720\fi0\ri1784{\b\f0\fs24\cf0\up0\dn0 \loch\af0 [Select one]}{\" +
                @"f0\fs24\cf0\up0\dn0 \loch\af0  ___x he ___ she }{\b\f0\fs24\cf0\up0\dn0 \loch\af0 [Select one] }{\f0\fs24\cf0\up0\dn0 \l" +
                @"och\af0 __x_ personally observed ___ has probable cause to }\qj{\f0\fs24\cf0\up0\dn0 \loch\af0 believe the defendant nam" +
                @"ed above on }{\field{\*\fldinst{\f0\fs20\cf0\up0\dn0 OFFENSE_DT_FRM}}{\fldrslt{\f0\fs20\cf0\up0\dn0 02/10/2006}}}{\f0\fs" +
                @"24\cf0\up0\dn0 \loch\af0  in Davidson County did unlawfully forge a certain }\qj{\f0\fs24\cf0\up0\dn0 \loch\af0 writing " +
                @"of the value of:  }{\b\f0\fs24\cf0\up0\dn0 \loch\af0 [Select one]  }{\f0\fs24\cf0\up0\dn0 \loch\af0 __x_ $500 or less __" +
                @"_ more than $500 but less than $1,000 }\qj{\f0\fs24\cf0\up0\dn0 \loch\af0 ___ $1,000 or more but less than $10,000 ___ $" +
                @"10,000 or more but less than $60,000 ___ $60,000 }\qj{\f0\fs24\cf0\up0\dn0 \loch\af0 or more with the intent to defraud " +
                @"or harm the victim named above and that }{\b\i\f1\fs28\cf0\up0\dn0 \loch\af1 the probable cause }\qj{\b\i\f1\fs28\cf0\up" +
                @"0\dn0 \loch\af1 is as follows}{\f1\fs24\cf0\up0\dn0 \loch\af1 :      }{\par}\pard\qj\li720\fi0\ri1784{\f1\fs24\cf0\up0\d" +
                @"n0\par}\pard\qj\li720\fi0\ri1784{\f1\fs24\cf0\up0\dn0\tab}{\f1\fs24\cf0\up0\dn0 \loch\af1  The defendant went to Sun Tru" +
                @"st Bank at 123 North Creek Blvd. /Davidson County at }\qj{\f1\fs24\cf0\up0\dn0 \loch\af1 approx. 1100 Hrs. and presented" +
                @"  a false Tenn. Drivers License with the name of John D Doe }\qj{\f1\fs24\cf0\up0\dn0 \loch\af1 Tn. DL number 123456789 " +
                @"and attempted to cash his payroll check  from  Whatchamacallit  Staffing . }\qj{\f1\fs24\cf0\up0\dn0 \loch\af1 Bank Pers" +
                @"onnel  Jane  Roe noticed the defendant s I.D. as being false.}{\par}\pard\qj\li720\fi0\ri1784{\f0\fs24\cf0\up0\dn0\par}\" +
                @"pard\qj\li720\fi0\ri1784{\f0\fs24\cf0\up0\dn0\par}\pard\qj\li720\fi0\ri1784{\f0\fs24\cf0\up0\dn0\par}\pard\qj\li720\fi0\" +
                @"ri1784{\f0\fs24\cf0\up0\dn0 \loch\af0 ________________________________________}{\par}\pard\qj\li720\fi0\ri1784{\f0\fs24\" +
                @"cf0\up0\dn0 \loch\af0 Prosecutor:  }{\field{\*\fldinst{\f0\fs20\cf0\up0\dn0 PROSECUTOR_FIRST_NAME}}{\fldrslt{\f0\fs20\cf" +
                @"0\up0\dn0 Michael}}}{\f0\fs24\cf0\up0\dn0 \loch\af0  }{\field{\*\fldinst{\f0\fs20\cf0\up0\dn0 PROSECUTOR_MIDDLE_NAME}}{\" +
                @"fldrslt{\f0\fs20\cf0\up0\dn0 }}}{\f0\fs24\cf0\up0\dn0 \loch\af0  }{\field{\*\fldinst{\f0\fs20\cf0\up0\dn0 PROSECUTOR_LAS" +
                @"T_NAME}}{\fldrslt{\f0\fs20\cf0\up0\dn0 Dorris}}}{\f0\fs24\cf0\up0\dn0 \loch\af0  }{\field{\*\fldinst{\f0\fs20\cf0\up0\dn" +
                @"0 PROSECUTOR_SUFFIX}}{\fldrslt{\f0\fs20\cf0\up0\dn0 }}}{\f0\fs20\cf0\up0\dn0 \loch\af0  }{\field{\*\fldinst{\f0\fs20\cf0" +
                @"\up0\dn0 ENF_OFCR_EMPLOYEE_NUM}}{\fldrslt{\f0\fs20\cf0\up0\dn0 0000003097}}}{\par}\pard\qj\li720\fi0\ri1784{\f0\fs24\cf0" +
                @"\up0\dn0 \loch\af0                     }{\field{\*\fldinst{\f0\fs20\cf0\up0\dn0 PROS_ADDRESS_LINE1_TXT}}{\fldrslt{\f0\fs" +
                @"20\cf0\up0\dn0 Goodlettsville Police Dept.}}}{\f0\fs20\cf0\up0\dn0 \loch\af0  }{\field{\*\fldinst{\f0\fs20\cf0\up0\dn0 S" +
                @"TATION_ASSIGNED}}{\fldrslt{\f0\fs20\cf0\up0\dn0 }}}{\par}\pard\qj\li720\fi0\ri1784{\f0\fs24\cf0\up0\dn0 \loch\af0       " +
                @"              }{\field{\*\fldinst{\f0\fs20\cf0\up0\dn0 PROS_ADDRESS_LINE2_TXT}}{\fldrslt{\f0\fs20\cf0\up0\dn0 105 South " +
                @"Main St.}}}{\par}\pard\qj\li720\fi0\ri1784{\f0\fs24\cf0\up0\dn0 \loch\af0                     }{\field{\*\fldinst{\f0\fs" +
                @"20\cf0\up0\dn0 PROS_CITY_NAME}}{\fldrslt{\f0\fs20\cf0\up0\dn0 Goodettsville}}}{\f0\fs24\cf0\up0\dn0 \loch\af0 ,  }{\fiel" +
                @"d{\*\fldinst{\f0\fs20\cf0\up0\dn0 PROS_STATE_NAME}}{\fldrslt{\f0\fs20\cf0\up0\dn0 Tennessee}}}{\f0\fs24\cf0\up0\dn0 \loc" +
                @"h\af0   }{\field{\*\fldinst{\f0\fs20\cf0\up0\dn0 PROS_POSTAL_CD}}{\fldrslt{\f0\fs20\cf0\up0\dn0 37072}}}{\f0\fs24\cf0\up" +
                @"0\dn0 \loch\af0  }{\field{\*\fldinst{\f0\fs20\cf0\up0\dn0 PROS_POSTAL_CD_EXT}}{\fldrslt{\f0\fs20\cf0\up0\dn0 }}}{\par}\p" +
                @"ard\qj\li720\fi0\ri1784{\f0\fs24\cf0\up0\dn0 \loch\af0                     }{\field{\*\fldinst{\f0\fs20\cf0\up0\dn0 PROS" +
                @"_PHONE_PREFIX_NUM}}{\fldrslt{\f0\fs20\cf0\up0\dn0 615}}}{\f0\fs24\cf0\up0\dn0 \loch\af0  }{\field{\*\fldinst{\f0\fs20\cf" +
                @"0\up0\dn0 PROS_BASE_PHONE_NUM}}{\fldrslt{\f0\fs20\cf0\up0\dn0 742-4248}}}{\f0\fs24\cf0\up0\dn0 \loch\af0  }{\field{\*\fl" +
                @"dinst{\f0\fs20\cf0\up0\dn0 PROS_EXTENSION_NUM}}{\fldrslt{\f0\fs20\cf0\up0\dn0 }}}{\par}\pard\qj\li720\fi0\ri1784{\f0\fs2" +
                @"4\cf0\up0\dn0\par}\pard\qj\li720\fi0\ri1784{\f0\fs24\cf0\up0\dn0 \loch\af0 Sworn to and subscribed before me on }{\field" +
                @"{\*\fldinst{\f0\fs20\cf0\up0\dn0 DATE_SWORN}}{\fldrslt{\f0\fs20\cf0\up0\dn0 }}}{\f0\fs24\cf0\up0\dn0 \loch\af0  }{\field" +
                @"{\*\fldinst{\f0\fs20\cf0\up0\dn0 TIME_SWORN}}{\fldrslt{\f0\fs20\cf0\up0\dn0 }}}{\f0\fs20\cf0\up0\dn0 \loch\af0 .}{\par}\" +
                @"pard\qj\li720\fi0\ri1784{\f0\fs24\cf0\up0\dn0\par}\pard\qj\li720\fi0\ri1784{\f0\fs24\cf0\up0\dn0 \loch\af0 _____________" +
                @"___________________________}{\par}\pard\qj\li720\fi0\ri1784{\field{\*\fldinst{\f0\fs20\cf0\up0\dn0 COMSNR_FIRST_NM}}{\fl" +
                @"drslt{\f0\fs20\cf0\up0\dn0 }}}{\f0\fs24\cf0\up0\dn0 \loch\af0  }{\field{\*\fldinst{\f0\fs20\cf0\up0\dn0 COMSNR_MDL_NM}}{" +
                @"\fldrslt{\f0\fs20\cf0\up0\dn0 }}}{\f0\fs24\cf0\up0\dn0 \loch\af0  }{\field{\*\fldinst{\f0\fs20\cf0\up0\dn0 COMSNR_LAST_N" +
                @"M}}{\fldrslt{\f0\fs20\cf0\up0\dn0 }}}{\f0\fs24\cf0\up0\dn0 \loch\af0  }{\field{\*\fldinst{\f0\fs20\cf0\up0\dn0 COMSNR_SU" +
                @"FFIX}}{\fldrslt{\f0\fs20\cf0\up0\dn0 }}}{\par}\pard\qj\li720\fi0\ri1784}";

            string exp =
                @"{\rtf1\ansi\deff0{\fonttbl{\f0\froman Tms Rmn;}{\f1\froman Times New Roman;}{\f2\froman Times;}}{\colortbl\red0\green0\b" +
                @"lue0;\red0\green0\blue255;\red0\green255\blue255;\red0\green255\blue0;\red255\green0\blue255;\red255\green0\blue0;\red25" +
                @"5\green255\blue0;\red255\green255\blue255;\red0\green0\blue127;\red0\green127\blue127;\red0\green127\blue0;\red127\green" +
                @"0\blue127;\red127\green0\blue0;\red127\green127\blue0;\red127\green127\blue127;\red192\green192\blue192;}{\info{\creatim" +
                @"\yr1998\mo12\dy14\hr9\min30\sec50}{\printim\yr1999\mo1\dy12\hr4\min37\sec41}{\version1}{\vern262367}}\paperw12240\paperh" +
                @"15840\margl59\margr0\margt119\margb432\deftab720\pard\ql\li720\fi0\ri1784{\f0\fs24\cf0\up0\dn0\par}\pard\ql\li720\fi0\ri" +
                @"1784{\f0\fs24\cf0\up0\dn0\par}\pard\qc\li720\fi0\ri1784{\f0\fs24\cf0\up0\dn0 \loch\af0 XXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXX" +
                @"XXXXX}{\par}\pard\qc\li720\fi0\ri1784{\f0\fs24\cf0\up0\dn0 \loch\af0 XXXXXXXXX}{\par}\pard\qc\li720\fi0\ri1784{\field{\*" +
                @"\fldinst{\f0\fs20\cf0\up0\dn0 QUALIFIER_TYPE}}{\fldrslt{\f0\fs20\cf0\up0\dn0 }}}{\f0\fs24\cf0\up0\dn0 \loch\af0 X}{\b\f0" +
                @"\fs24\cf0\up0\dn0 \loch\af0 XXXXXXX}{\par}\pard\qc\li720\fi0\ri1784{\b\f0\fs24\cf0\up0\dn0 \loch\af0 XXXXXXXXXXXXXXXX}{\" +
                @"par}\pard\ql\li720\fi0\ri1784{\f0\fs24\cf0\up0\dn0\par}\pard\qj\li720\fi0\ri1784{\f0\fs24\cf0\up0\dn0 \loch\af0 XXXXXXXX" +
                @"XXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXX}{\b\f0\fs24\cf0\up0\dn0 \loch\af0 XXXXXXXXXXXXX}{\f0\fs24\cf0\up0\dn0 \loch\af0" +
                @" XXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXX}\qj{\f0\fs24\cf0\up0\dn0 \loch\af0 XXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXX" +
                @"XXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXX}{\par}\pard\qj\li720\fi0\ri1784{\b\f0\fs24\cf0\up0\dn0 \loch\af0 XXXXXXXXXXXX}{\" +
                @"f0\fs24\cf0\up0\dn0 \loch\af0 XXXXXXXXXXXXXXXXX}{\b\f0\fs24\cf0\up0\dn0 \loch\af0 XXXXXXXXXXXXX}{\f0\fs24\cf0\up0\dn0 \l" +
                @"och\af0 XXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXX}\qj{\f0\fs24\cf0\up0\dn0 \loch\af0 XXXXXXXXXXXXXXXXXXXXXXXXX" +
                @"XXXXXXXXXXXX}{\field{\*\fldinst{\f0\fs20\cf0\up0\dn0 OFFENSE_DT_FRM}}{\fldrslt{\f0\fs20\cf0\up0\dn0 02/10/2006}}}{\f0\fs" +
                @"24\cf0\up0\dn0 \loch\af0 XXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXX}\qj{\f0\fs24\cf0\up0\dn0 \loch\af0 XXXXXXXX" +
                @"XXXXXXXXXXXXXXXXXX}{\b\f0\fs24\cf0\up0\dn0 \loch\af0 XXXXXXXXXXXXXX}{\f0\fs24\cf0\up0\dn0 \loch\af0 XXXXXXXXXXXXXXXXXXXX" +
                @"XXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXX}\qj{\f0\fs24\cf0\up0\dn0 \loch\af0 XXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXX" +
                @"XXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXX}\qj{\f0\fs24\cf0\up0\dn0 \loch\af0 XXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXX" +
                @"XXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXX}{\b\i\f1\fs28\cf0\up0\dn0 \loch\af1 XXXXXXXXXXXXXXXXXXX}\qj{\b\i\f1\fs28\cf0\up" +
                @"0\dn0 \loch\af1 XXXXXXXXXXXXX}{\f1\fs24\cf0\up0\dn0 \loch\af1 XXXXXXX}{\par}\pard\qj\li720\fi0\ri1784{\f1\fs24\cf0\up0\d" +
                @"n0\par}\pard\qj\li720\fi0\ri1784{\f1\fs24\cf0\up0\dn0XXXX}{\f1\fs24\cf0\up0\dn0 \loch\af1 XXXXXXXXXXXXXXXXXXXXXXXXXXXXXX" +
                @"XXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXX}\qj{\f1\fs24\cf0\up0\dn0 \loch\af1 XXXXXXXXXXXXXXXXXXXXXXXXXXXXXXX" +
                @"XXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXX}\qj{\f1\fs24\cf0\up0\dn0 \loch\af1 XXXXXXXXXXXXXXXXXXXXXXXX" +
                @"XXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXX}\qj{\f1\fs24\cf0\up0\dn0 \loch\af1 XXXXXXXXX" +
                @"XXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXX}{\par}\pard\qj\li720\fi0\ri1784{\f0\fs24\cf0\up0\dn0\par}\" +
                @"pard\qj\li720\fi0\ri1784{\f0\fs24\cf0\up0\dn0\par}\pard\qj\li720\fi0\ri1784{\f0\fs24\cf0\up0\dn0\par}\pard\qj\li720\fi0\" +
                @"ri1784{\f0\fs24\cf0\up0\dn0 \loch\af0 XXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXX}{\par}\pard\qj\li720\fi0\ri1784{\f0\fs24\" +
                @"cf0\up0\dn0 \loch\af0 XXXXXXXXXXXXX}{\field{\*\fldinst{\f0\fs20\cf0\up0\dn0 PROSECUTOR_FIRST_NAME}}{\fldrslt{\f0\fs20\cf" +
                @"0\up0\dn0 Michael}}}{\f0\fs24\cf0\up0\dn0 \loch\af0 X}{\field{\*\fldinst{\f0\fs20\cf0\up0\dn0 PROSECUTOR_MIDDLE_NAME}}{\" +
                @"fldrslt{\f0\fs20\cf0\up0\dn0 }}}{\f0\fs24\cf0\up0\dn0 \loch\af0 X}{\field{\*\fldinst{\f0\fs20\cf0\up0\dn0 PROSECUTOR_LAS" +
                @"T_NAME}}{\fldrslt{\f0\fs20\cf0\up0\dn0 Dorris}}}{\f0\fs24\cf0\up0\dn0 \loch\af0 X}{\field{\*\fldinst{\f0\fs20\cf0\up0\dn" +
                @"0 PROSECUTOR_SUFFIX}}{\fldrslt{\f0\fs20\cf0\up0\dn0 }}}{\f0\fs20\cf0\up0\dn0 \loch\af0 X}{\field{\*\fldinst{\f0\fs20\cf0" +
                @"\up0\dn0 ENF_OFCR_EMPLOYEE_NUM}}{\fldrslt{\f0\fs20\cf0\up0\dn0 0000003097}}}{\par}\pard\qj\li720\fi0\ri1784{\f0\fs24\cf0" +
                @"\up0\dn0 \loch\af0 XXXXXXXXXXXXXXXXXXXX}{\field{\*\fldinst{\f0\fs20\cf0\up0\dn0 PROS_ADDRESS_LINE1_TXT}}{\fldrslt{\f0\fs" +
                @"20\cf0\up0\dn0 Goodlettsville Police Dept.}}}{\f0\fs20\cf0\up0\dn0 \loch\af0 X}{\field{\*\fldinst{\f0\fs20\cf0\up0\dn0 S" +
                @"TATION_ASSIGNED}}{\fldrslt{\f0\fs20\cf0\up0\dn0 }}}{\par}\pard\qj\li720\fi0\ri1784{\f0\fs24\cf0\up0\dn0 \loch\af0 XXXXXX" +
                @"XXXXXXXXXXXXXX}{\field{\*\fldinst{\f0\fs20\cf0\up0\dn0 PROS_ADDRESS_LINE2_TXT}}{\fldrslt{\f0\fs20\cf0\up0\dn0 105 South " +
                @"Main St.}}}{\par}\pard\qj\li720\fi0\ri1784{\f0\fs24\cf0\up0\dn0 \loch\af0 XXXXXXXXXXXXXXXXXXXX}{\field{\*\fldinst{\f0\fs" +
                @"20\cf0\up0\dn0 PROS_CITY_NAME}}{\fldrslt{\f0\fs20\cf0\up0\dn0 Goodettsville}}}{\f0\fs24\cf0\up0\dn0 \loch\af0 XXX}{\fiel" +
                @"d{\*\fldinst{\f0\fs20\cf0\up0\dn0 PROS_STATE_NAME}}{\fldrslt{\f0\fs20\cf0\up0\dn0 Tennessee}}}{\f0\fs24\cf0\up0\dn0 \loc" +
                @"h\af0 XX}{\field{\*\fldinst{\f0\fs20\cf0\up0\dn0 PROS_POSTAL_CD}}{\fldrslt{\f0\fs20\cf0\up0\dn0 37072}}}{\f0\fs24\cf0\up" +
                @"0\dn0 \loch\af0 X}{\field{\*\fldinst{\f0\fs20\cf0\up0\dn0 PROS_POSTAL_CD_EXT}}{\fldrslt{\f0\fs20\cf0\up0\dn0 }}}{\par}\p" +
                @"ard\qj\li720\fi0\ri1784{\f0\fs24\cf0\up0\dn0 \loch\af0 XXXXXXXXXXXXXXXXXXXX}{\field{\*\fldinst{\f0\fs20\cf0\up0\dn0 PROS" +
                @"_PHONE_PREFIX_NUM}}{\fldrslt{\f0\fs20\cf0\up0\dn0 615}}}{\f0\fs24\cf0\up0\dn0 \loch\af0 X}{\field{\*\fldinst{\f0\fs20\cf" +
                @"0\up0\dn0 PROS_BASE_PHONE_NUM}}{\fldrslt{\f0\fs20\cf0\up0\dn0 742-4248}}}{\f0\fs24\cf0\up0\dn0 \loch\af0 X}{\field{\*\fl" +
                @"dinst{\f0\fs20\cf0\up0\dn0 PROS_EXTENSION_NUM}}{\fldrslt{\f0\fs20\cf0\up0\dn0 }}}{\par}\pard\qj\li720\fi0\ri1784{\f0\fs2" +
                @"4\cf0\up0\dn0\par}\pard\qj\li720\fi0\ri1784{\f0\fs24\cf0\up0\dn0 \loch\af0 XXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXX}{\field" +
                @"{\*\fldinst{\f0\fs20\cf0\up0\dn0 DATE_SWORN}}{\fldrslt{\f0\fs20\cf0\up0\dn0 }}}{\f0\fs24\cf0\up0\dn0 \loch\af0 X}{\field" +
                @"{\*\fldinst{\f0\fs20\cf0\up0\dn0 TIME_SWORN}}{\fldrslt{\f0\fs20\cf0\up0\dn0 }}}{\f0\fs20\cf0\up0\dn0 \loch\af0 X}{\par}\" +
                @"pard\qj\li720\fi0\ri1784{\f0\fs24\cf0\up0\dn0\par}\pard\qj\li720\fi0\ri1784{\f0\fs24\cf0\up0\dn0 \loch\af0 XXXXXXXXXXXXX" +
                @"XXXXXXXXXXXXXXXXXXXXXXXXXXX}{\par}\pard\qj\li720\fi0\ri1784{\field{\*\fldinst{\f0\fs20\cf0\up0\dn0 COMSNR_FIRST_NM}}{\fl" +
                @"drslt{\f0\fs20\cf0\up0\dn0 }}}{\f0\fs24\cf0\up0\dn0 \loch\af0 X}{\field{\*\fldinst{\f0\fs20\cf0\up0\dn0 COMSNR_MDL_NM}}{" +
                @"\fldrslt{\f0\fs20\cf0\up0\dn0 }}}{\f0\fs24\cf0\up0\dn0 \loch\af0 X}{\field{\*\fldinst{\f0\fs20\cf0\up0\dn0 COMSNR_LAST_N" +
                @"M}}{\fldrslt{\f0\fs20\cf0\up0\dn0 }}}{\f0\fs24\cf0\up0\dn0 \loch\af0 X}{\field{\*\fldinst{\f0\fs20\cf0\up0\dn0 COMSNR_SU" +
                @"FFIX}}{\fldrslt{\f0\fs20\cf0\up0\dn0 }}}{\par}\pard\qj\li720\fi0\ri1784}";

            (byte[] inputBytes, SpatialString displayString) = GetRedactTaskInputFromRTFString(inp);
            var doc = new AFDocumentClass { Text = displayString };

            var rule = new RegExprRuleClass { Pattern = @"[\S\s]+" };
            var redactionZones =
                rule.ParseText(doc, null)
                .ToIEnumerable<IAttribute>()
                .SelectMany(attr => attr.Value.GetOCRImageRasterZones().ToIEnumerable<RasterZone>());

            var task = new CreateRedactedTextTask
            {
                TaskSettings = new CreateRedactedTextSettings(true, false, new string[0], RedactionMethod.ReplaceCharacters, CharacterClass.All, "X", false, 0, null, null, null, null)
            };

            byte[] redactedBytes = task.RedactBytes(inputBytes, displayString, redactionZones, true);
            string redactedString = GetRedactedStringFromBytes(redactedBytes);

            Assert.AreEqual(exp, redactedString);
        }

        #endregion Tests

        #region Helper Methods

        private static (byte[] inputBytes, SpatialString displayString) GetRedactTaskInputFromString(string inp)
        {
            Encoding encoding = Encoding.GetEncoding("windows-1252"); // Legacy code is not UTF-aware; text file bytes are interpreted (displayed in USS file viewer, e.g.) using this codepage
            byte[] inputBytes = encoding.GetBytes(inp);

            using (var tmpFile = new TemporaryFile(".txt", false))
            {
                File.WriteAllText(tmpFile.FileName, inp, encoding);

                SpatialString displayString = new SpatialStringClass();
                displayString.LoadFrom(tmpFile.FileName, false);

                return (inputBytes, displayString);
            }
        }

        private static (byte[] inputBytes, SpatialString displayString) GetRedactTaskInputFromRTFString(string inp)
        {
            Encoding encoding = Encoding.GetEncoding("windows-1252");
            byte[] inputBytes = encoding.GetBytes(inp);

            using (var tmpFile = new TemporaryFile(".rtf", false))
            {
                File.WriteAllBytes(tmpFile.FileName, inputBytes);

                SpatialString displayString = new SpatialStringClass();
                displayString.LoadFrom(tmpFile.FileName, false);

                return (inputBytes, displayString);
            }
        }

        private static string GetRedactedStringFromBytes(byte[] redactedBytes)
        {
            Encoding encoding = Encoding.GetEncoding("windows-1252");
            string redactedString = encoding.GetString(redactedBytes);

            return redactedString;
        }

        #endregion Helper Methods
    }
}
