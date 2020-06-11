using Extract.Testing.Utilities;
using NUnit.Framework;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using UCLID_COMUTILSLib;

namespace Extract.Utilities.Parsers.Test
{
    /// <summary>
    /// Class for testing the ConfigSettings class
    /// </summary>
    [TestFixture]
    [Category("Automatic")]
    public class TestRichExtractor
    {
        #region Constants

        #endregion Constants

        #region Overhead Methods

        /// <summary>
        /// Setup method to initialize the testing environment.
        /// </summary>
        [OneTimeSetUp]
        public static void Setup()
        {
            GeneralMethods.TestSetup();
        }

        #endregion Overhead Methods

        #region Tests

        /// <summary>
        /// Tests pulling the 'plain text' out of some complex rich text from Davidson TN (Nashville)
        /// </summary>
        [Test]
        public static void ExtractPlaintextFromNashvilleRtf()
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

            // RichTextExtractor uses newline-only.
            // I think this is to avoid multi-byte 'display characters.'
            // E.g., if \par means '\r\n' then there would be many of these two-byte chars.
            // Maybe that wouldn't a problem but '\n' is a nicer system anyway.
            string exp = @"

STATE OF TENNESSEE, COUNTY OF DAVIDSON
AFFIDAVIT
 FORGERY
T.C.A. 39-14-114

Personally appeared before me, the undersigned, [Select one] x__ Commissioner ___ Metropolitan General Sessions Judge, the prosecutor named above and made oath in due form of law that
[Select one] ___x he ___ she [Select one] __x_ personally observed ___ has probable cause to believe the defendant named above on 02/10/2006 in Davidson County did unlawfully forge a certain writing of the value of:  [Select one]  __x_ $500 or less ___ more than $500 but less than $1,000 ___ $1,000 or more but less than $10,000 ___ $10,000 or more but less than $60,000 ___ $60,000 or more with the intent to defraud or harm the victim named above and that the probable cause is as follows:      

	 The defendant went to Sun Trust Bank at 123 North Creek Blvd. /Davidson County at approx. 1100 Hrs. and presented  a false Tenn. Drivers License with the name of John D Doe Tn. DL number 123456789 and attempted to cash his payroll check  from  Whatchamacallit  Staffing . Bank Personnel  Jane  Roe noticed the defendant s I.D. as being false.



________________________________________
Prosecutor:  Michael  Dorris  0000003097
                    Goodlettsville Police Dept. 
                    105 South Main St.
                    Goodettsville,  Tennessee  37072 
                    615 742-4248 

Sworn to and subscribed before me on  .

________________________________________
   
".Replace("\r\n", "\n");

            var (_, plainText) = RichTextExtractor.GetTextPositions(inp, "Unknown", true);

            Assert.AreEqual(exp, plainText);
        }

        /// <summary>
        /// Tests behavior with badly-formed rtf when a group isn't terminated
        /// </summary>
        [Test]
        public static void TestBadRtfUnterminatedGroup()
        {
            string inp = @"{\rtf1\ansi\deff0{\fonttbl{\f0\froman Tms Rmn;}{{";

            Assert.Throws<ExtractException>(() => RichTextExtractor.GetTextPositions(inp, "Unknown", true));
        }

        /// <summary>
        /// Tests behavior with badly-formed rtf when a group isn't started
        /// </summary>
        [Test]
        public static void TestBadRtfExtraCurlyBracket()
        {
            string inp = @"{\rtf1\ansi\deff0{\fonttbl{\f0\froman Tms Rmn;}}";

            Assert.Throws<ExtractException>(() => RichTextExtractor.GetTextPositions(inp, "Unknown", true));
        }

        #endregion Tests

        #region Helper Methods

        #endregion Helper Methods

    }
}
