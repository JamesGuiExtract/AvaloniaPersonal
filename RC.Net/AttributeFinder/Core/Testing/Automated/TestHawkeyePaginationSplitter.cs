using Extract.AttributeFinder.Rules;
using Extract.Testing.Utilities;
using NUnit.Framework;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using UCLID_AFCORELib;

namespace Extract.AttributeFinder.Test
{
    [Category("HawkeyePaginationSplitter")]
    public class TestHawkeyePaginationSplitter
    {
        private static TestFileManager<TestRuleSetRunMode> _testFiles;

        #region Overhead

        /// <summary>
        /// Setup method to initialize the testing environment.
        /// </summary>
        [OneTimeSetUp]
        public static void Setup()
        {
            GeneralMethods.TestSetup();

            _testFiles = new TestFileManager<TestRuleSetRunMode>();

        }

        /// <summary>
        /// Cleanup after all tests have run.
        /// </summary>
        [OneTimeTearDown]
        public static void FinalCleanup()
        {
            if (_testFiles != null)
            {
                _testFiles.Dispose();
            }
        }

        #endregion Overhead

        #region ExpectedData

        [SuppressMessage("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        static readonly Dictionary<string, List<(string, string)>> _resourceNameToExpectedData = new()
        {
            {
                "Resources.Hawkeye.EmailWithSixAttachments.eml.pdf",
                new List<(string, string)>()
                {
                    ("Document", "This is the repeated email body\r\n\r\n"),
                    ("Pages", "1"),
                    ("SubFileID", "1"),
                    ("UnitID", "1"),
                    ("Document", "2"),
                    ("Pages", "2"),
                    ("SubFileID", "2"),
                    ("UnitID", "1"),
                    ("DocumentData", "IN TEE CIRCUIT COURT 07 TEE TULITI JUDICIAL CIRCUIT IN AND FOR\r\n\r\nTENNANT COUNTY CIVIL ACTION\r\n\r\n0\r\nr\r\n\r\nnig\r\n\r\n,\r\n\r\n41\r\n\r\nc.ri r,1\r\n\r\nppei=p\r\n\r\n'I)-Tt\r\n\r\n=0\r\n\r\n.-4 n1\r\n\r\nv-4201\r\nsaw=\r\n\r\n1,0 zC 41 us\r\n\r\ncrt\r\n\r\nrra\r\n-474 e)\r\n- xv\r\n\r\nTHE STATE OF FLORIDA,\r\nDEPARTMENT OF REVENUE\r\nON BEHALF OF JANE R. ROE,\r\n\r\nPlaintiff,\r\n\r\nCASE NO. CA99-1111\r\n\r\nvs.\r\n\r\nJOHN DOE,\r\n\r\n\":41\r\n\r\nDefendant.\r\n\r\nSSN: 999-11-5555\r\n\r\nSTIPULATION TO ABATE CURRENT QUILD SUPPORT\r\n\r\nPlaintiff, the State of Florida, Department of Revenue, on\r\nbehalf of Jane R. Roe, by and through the undersigned\r\nattorney, and the Defendant, John Doe, whose social security\r\nnumber is 999-11-5555, agree to the following for the abatement of ima\r\nthe current child support obligation:\r\n\r\n1. As of September 14, 1995, the minor child of the parties, 03\r\nPrecious Baby Doe, born February 1, 1979, is residing with trA\r\nthe Defendant, John Doe.\r\n\r\n2. Current child support shall be abated as of September 14,\r\n1995, until such time as the minor child is no longer residing with\r\nthe Defendant, John Doe.\r\n\r\n3. The suspension in no way relieves any arrearage owing by\r\nthe Defendant or any retroactive child support obligation due and\r\nowing by the Defendant, John Doe.\r\n\r\n"),
                    ("Document", "This is the repeated email body\r\n\r\n"),
                    ("Pages", "1"),
                    ("SubFileID", "3"),
                    ("UnitID", "2"),
                    ("Document", "3"),
                    ("Pages", "3"),
                    ("SubFileID", "4"),
                    ("UnitID", "2"),
                    ("DocumentData", "1626'6 609 me SIM $133 DKT t 1515658 1 of 2\r\n\r\nPrepared by and return to:\r\nRonald L Wltt, Attorney at Law\r\nKaki's, Reid, Venable & Witt, P.A.\r\n1400 4th Avenue West\r\nBeernardt, Florida 34205\r\n941-747-1180\r\nFile Number: 99W-341\r\n\r\nGrantee S.S. No. 777-55-3333 and 444.44-6666\r\nParcel Identification No. 4904.0000/9\r\n\r\n_Ppm Above This Line For Recording Deal\r\n\r\nWarranty Deed\r\n\r\n(STATUTORY FORM SECTION 689.02, F.S.)\r\n\r\nThis Indenture made this 5th day of January, 2000 between Jane R. Roe whose post office address is 86\r\nSummerbell Ave, Centerville, Massachusetts 02636 of the County of Barnstable, State of Massachusetts, grantor*, and\r\nJohn D. Doe and Jane W Doe, husband and wife whose post office address is 1119 9 AMognetti Road,\r\nParrish, Florida 34219 of the County of Farudale, State of Florida, grantee*,\r\n\r\nWitnesseth that said grantor, for and in consideration of the sum of TEN AND NO/100 DOLLARS ($10.00) and other\r\ngood and valuable considerations to said grantor in hand paid by said grantee, the receipt whereof is hereby acknowledged,\r\nhas granted, bargained, and sold to the said grantee, and grantee's heirs and assigns forever, the following described land,\r\nsituate, lying and being in Farndale County, Florida, to-wit:\r\n\r\nThe North 1/2 of the NW 1/4 of the SE 1/4, Section 32, Township 33 South, Range 19 East, Farudale\r\nCounty, Florida, LESS the North 15 feet AND LESS the West 40 feet described in OR Book 215,\r\npage 268, of the Public Records of Farudale County, Florida.\r\n\r\nGrantor warrants that at the time of this conveyance, the subject property is not the Grantor's\r\nhomestead within the meaning set forth in the constitution of the state of Florida, nor is it contiguous\r\nto or a part of homestead property. Grantor's residence and homestead address is: 86 Summerbell\r\nAve, Craigville, MA 02636.\r\n\r\nand said grantor does hereby fully warrant the title to said land, and will defend the same against lawful claims of all\r\npersons whomsoever.\r\n\r\n\"Grantor\" and \"Grantee\" are used for singular or plural, as context requires.\r\n\r\nIn Witness Whereof, grantor has hereunto set grantor's hand and seal the day and year first above written.\r\nSigned, sealed and aelivered in our presence.\r\n\r\nWitness Name:\r\n\r\nDoublerirre\r\n\r\n1\r\n\r\n"),
                    ("Document", "This is the repeated email body\r\n\r\n"),
                    ("Pages", "1"),
                    ("SubFileID", "5"),
                    ("UnitID", "3"),
                    ("Document", "4-7"),
                    ("Pages", "4-7"),
                    ("SubFileID", "6"),
                    ("UnitID", "3"),
                    ("DocumentData", "IA THE CIRCUIT COURT OF THE TWELFTH JUDICIAL CIRCUIT IN AND FOR\r\nANNETTE COUNTY CIVIL ACTION\r\n\r\nDOE, JOHN\r\n000-66.75555\r\n\r\nPETITIONER,\r\n\r\nVS.\r\n\r\nDOES JANE\r\n444-33-5555\r\n\r\nRESPONDENT.\r\n\r\nCASE Nab. 66616763CA\r\n\r\nFIVOrC\r\n\r\nR.71\r\n\r\nJUDGMENT BY OPERATION OF LAN\r\nCERTIFICATE m DELINQUENCY OF SUPPORT PAYMENTS\r\n\r\n(SECTION 61.181 FLORIDA STATUTES)\r\n\r\nIs SHERRI SHROPSHIRE', CLERK OF THE CIRCUIT COURT,\r\nOF AANETTE COUNTY: FLORIDA', HEREBY CERTIFY AS FOLLOWS:\r\n\r\n1. DOEs JANE FAILED TO PAY INTO\r\nTHE DEPOSITORY THE COURTORDERED SUPPORT PAYMENTS DUE\r\nIN THE AMOUNT OF $\r\n\r\n6\r\n\r\n1=11=0\r\n\r\n2. A CERTIFIED COPY OF THE SUPPORT ORDER AND ANY\r\n\r\nMODIFICATIONS THEREOF ARE ATTACHED TO THIS CERTIFICATE.\r\n\r\n3. A TIMELY NOTICE OF DELINQUENCY WAS MAIL CERTIFIED\r\nMAIL, RETURN RECEIPT REQUESTED TO DOE, JANE\r\nAT THE LAST ADDRESS KNOWN TO THE CLERK OR TO THE LAST\r\nADDRESS AVAILABLE FROM THE COURT FILE.\r\n\r\n4. THE DELINQUENCY REMAINS UNPAID AND 30 DAYS GR MORE\r\nHAVE TRANSPIRED FROM THE DUE DATE OF THE SUPPORT PAYMENT\r\nGIVING RISE TO THE DELINQUENCY. (SEE 1 AUOVE)\r\n\r\nCJ\r\n\r\n13,\r\n\r\nSECTION 61.1415)1 FLORIDA STATUTES PROVIDES THAT THE\r\n\r\nRECORDING OF THIS CERTIFICATE AND THE ATTACHMENTS THERETO\r\nEVIDENCES A FINAL JUDGMENT BY OPERATION OF LAW FOR ALL\r\nDELINQUENCIES DUE AS OF THE DATE CERTIFIED ABOVE AND ALL\r\nOTHER AMOUNTS WHICH HEREAFTER BECOME DUE AND REMAIN\r\nUNPAID, INCLUDING DELINQUENCIES WHICA MAY HAVE EXISTED\r\n\r\nPRIOR Ti; THAT DATE, TOGETHER WITH ALL APPLICABLE COSTS:\r\nFEES AND INTEREST, WHICH HAS THE FULL FORCE' EFFECT AND\r\nATTRIBUTES OF A JUDGMENT ENTERED BY A CURT OF THIS STATE\r\nFOR WHICH SUMS LET EXECUTION ISSUE.\r\n\r\nSHERRI SHROPSHIRE\r\nCLERK OF THE CIRCUIT COURT\r\n\r\n(4-1-t-44141\r\nBY: '-\r\n\r\nDEPUTY CL\r\n\r\nCERTIFICATE OF SERVICE\r\n\r\nI CERTIFY THAT A TRUE COPY OF THIS NOTICE WAS NAILED TO\r\nTHE OBLIGOR'S LAST ADDRESS OF RECORD WITH THE UEPOSITjRY:\r\n\r\nDOE, JANE\r\n1212 Yo Driver West 00000\r\nBeernardt, FL 34205\r\n\r\nANTS TO THE JBLIGEEIS LAST ADDRESS OF RECORD WITH THE DEPOSIT # y:\r\n\r\nDOE, JOHN\r\n011 VERMONT AVENUE\r\nBEERNARDT, FL 00j\r\n\r\nBY U.S. MAIL THIS / DAY OF 16-4# 19\r\n\r\nSHERRI SHROPSHIRE\r\nCLERK OF THE CIRCUIT COURT\r\n\r\n3Y:\r\n\r\nIN THE CIRCUIT COURT OF THE TWELFTH JUDICIAL CIRCUIT\r\n\r\nIN AND FOR ANNETTE COUNTY, FLORIDA\r\n\r\nIN RE: THE MARRIAGE OF\r\n\r\nJOHN DOE,\r\n\r\nFormer Husband,\r\n\r\nand Case No. CA 83-1076\r\n\r\nJANE DOE,\r\n\r\nWife.\r\n\r\nORDER ON REPORT OF GENERAL MASTER\r\nTHIS CAUSE came on to be heard upon the Report of\r\n\r\n)1(449,\r\n\r\nthe General Master dated\r\n\r\nand\r\n\r\nthe Court having considered the findings and recommendations\r\ntherein and being otherwise fully advised in the premises,\r\nand there being not timely filed exceptions, it is there\r\nupon\r\n\r\nORDERED AND ADJUDGED that:\r\nThe Report of the General Master be and the same is\r\nhereby ratified and approved, subject to timely filed\r\nexceptions.\r\n\r\n1. Primary residential responsibility of the\r\nparties' minor child, LITTLE JANE DOE, shall be\r\nmodified from that of the former wife to that of the former\r\nhusband. The former wife shall have reasonable rights of\r\nvisitation, if the parties cannot agree as to reasonable\r\n\r\nvisitation then the Twelfth Judicial Circuit Guidelines\r\nshall apply.\r\n\r\n2. The former wife shall pay to the former husband\r\nbeginning on August 14, 1992 the sum of $50 per week for\r\nchild support. This child support shall continue until the\r\nminor child reaches the age of eighteen, marries, dies or\r\nbecomes otherwise emancipated. An Income Deduction Order\r\nshall be entered immediately with the child support being\r\npaid to the Clerk of Court through the Depository plus the\r\nClerk's fee.\r\n\r\nORDERED at Beernardt, Annette County, Florida this\r\n\r\n:V day ofCCICI2C7{2--: , 1992.\r\n\r\nC.0 inCI\r\n7r.:1\r\n\r\nr-P1\r\n\r\nZ\r\n\r\nrm\r\n\r\nC-D\r\n\r\n77_\r\nrri\r\n\r\nrr,\r\n\r\nCC1\r\n\r\nCIRCUIT JUDGE\r\n\r\ncc: Steven G. Lavely, Esq.\r\n\r\nWilliam H. Meeks, Jr., Esq.\r\n\r\nSTATE OF FL OPIQ/1., CQUNTY OF AlVINI TTY\r\n\r\n7.14; io waif)? live lkycrogaing le army\r\n\r\ncorract copy of duce and\r\n\r\n'\r\n\r\npp tile in y office\r\nWitness my hand and of seal Prig day\r\n\r\n8\r\n\r\n74,\r\n\r\n777\r\n4-1\r\n\r\n"),
                    ("Document", "This is the repeated email body\r\n\r\n"),
                    ("Pages", "1"),
                    ("SubFileID", "7"),
                    ("UnitID", "4"),
                    ("Document", "8"),
                    ("Pages", "8"),
                    ("SubFileID", "8"),
                    ("UnitID", "4"),
                    ("DocumentData", "LI,. 9 2 LI,. 8 2 3 2 2\r\n\r\n1872\r\n\r\nDepartment of the Treasury - Internal Revenue Service\r\n\r\nNotice of Federal Tax Lien\r\n\r\nForm 668 (Y)(c)\r\n\r\n(Rev. February 2004)\r\n\r\nFor Optional Use by Recording Office\r\n\r\nSerial Number\r\nArea:\r\n\r\nSMALL BUSINESS/SELF EMPLOYED AREA #2\r\nLien Unit Phone: (800) 829-3903\r\n\r\n282523806\r\n\r\nAs provided by section 6321, 6322, and 6323 of the Internal Revenue\r\nCode, we are giving a notice that taxes (including interest and penalties)\r\nhave been assessed against the following-named taxpayer. We have made\r\na demand for payment of this liability, but it remains unpaid. Therefore,\r\nthere is a lien in favor of the United States on all property and rights to\r\nproperty belonging to this taxpayer for the amount of these taxes, and\r\nadditional penalties, interest, and costs that may accrue.\r\n\r\nName of Taxpayer JOHN DOE\r\n\r\nResidence\r\n\r\n10010 ROAD RD\r\n\r\nCOLUMBIA STA, OH 44028\r\n\r\nIMPORTANT RELEASE INFORMATION: For each assessment listed below,\r\nunless notice of the lien is refiled by the date given in column {e), this notice shall,\r\non the day following such date, operate as a certificate of release as defined\r\nin IRC 6325(a).\r\n\r\nf4\r\n\r\nUnpiultdance\r\nof Assessment\r\n\r\n(f)\r\n\r\nLast Day for\r\nRefilmg\r\n\r\n(e)\r\n\r\nDate of\r\nAssessment\r\n\r\n(d)\r\n\r\nTax Period\r\nEnding\r\n\r\n(b)\r\n\r\nIdentifying Number\r\n\r\n(c)\r\n\r\nKind of Tax\r\n\r\n(a)\r\n\r\n06/19/2012\r\n06/25/2013\r\n12/14/2015\r\n\r\n05/20/2002\r\n05/26/2003\r\n11/14/2005\r\n\r\n3222.42\r\n4130.85\r\n6067.99\r\n\r\n12/31/2001\r\n12/31/2002\r\n12/31/2004\r\n\r\nXXX - XX - 4444\r\nXXX -XX- 4444\r\nXXX- XX - 4444\r\n\r\n1040\r\n1040\r\n1040\r\n\r\nPlace of Filing\r\n\r\nRecorder of Lorain County\r\nLorain County\r\nElyria, OH 44035\r\n\r\n13421.26\r\nTotal\r\n\r\nDETROIT, MI , on this,\r\nThis notice was prepared and signed at\r\n\r\n04th April\r\n\r\nday of\r\n\r\n2006.\r\nthe\r\n\r\nH.\r\n\r\nTitle\r\nACS\r\n\r\n(800) 829-3903\r\n\r\nSignature\r\n\r\n22-00-0008\r\n\r\nfor REGINA OWENS\r\n\r\n(NOTE: Certificate of officer authorized by law to take acknowledgment is not essential to the validity of Notice of Federal Tax lien\r\nRev. Rul. 71-466, 1971 - 2 C.B. 409) Form 668(Y)(c) (Rev. 2-2004)\r\n\r\nCAT. NO 60025X\r\n\r\nPart I - Kept By Recording Office\r\n\r\n"),
                    ("Document", "This is the repeated email body\r\n\r\n"),
                    ("Pages", "1"),
                    ("SubFileID", "9"),
                    ("UnitID", "5"),
                    ("Document", "9"),
                    ("Pages", "9"),
                    ("SubFileID", "10"),
                    ("UnitID", "5"),
                    ("DocumentData", "Filed at Ohio Secretary of State 01/17/2002 09:00 AM FILE# OH00043424946\r\n\r\nM11110111111111M1111111111\r\n\r\nRECEIVED\r\nJAN j 7 2002\r\n\r\n8E013E6%94%mm\r\n\r\n111111101111111111=1111\r\n\r\nUCC FINANCING STATEMENT\r\n\r\nRECEIVED\r\nDEC 2 62001\r\n\r\nsECRETillivoF:sTAIE\r\n\r\nFOLLOW INSTRUCTIONS (front and back) CAREFULLY\r\nA. NAME & PHONE OF CONTACT AT FILER /optional]\r\n\r\nB. SEND ACKNOWLEDGMENT To: (Name and Address)\r\n\r\nE flydelFindlay Area Credit thikill\r\n\r\nlii9d/ S77 RI,- /,,,2)\r\n\r\nl/b/644-(--/ o/420 s58`.7e)\r\n\r\nL.\r\n\r\nTHE ABOVE SPACE IS FOR FILING OFFICE USE ONLY\r\n\r\n1. DEBTOR'S EXACT FULL LEGAL NAME - mean 0 y2 oikotw name (1e or 1b1 - do not abbreviate or combine names\r\n\r\n114. ORGANIZATION'S NAME\r\n\r\nOR\r\n\r\nIP. INDIVIDUAL'S LAST NAME\r\n\r\npo\r\n\r\nIAILING ADD ESS\r\n\r\nFIRST NAME\r\n\r\n'r(\r\n\r\nMIDDLE NAME\r\n\r\nSUFFIX\r\n\r\n1c. MAILING ADDRESS\r\n\r\n-.7- PARK ST\r\n\r\n73EDPORD\r\n\r\nPOSTAL CODE\r\n\r\n.5-53- 6,\r\n\r\nId. TAXID fk SSN OR EIN AMYL. INFO RE Da TYPE OF tIRGANIZATtOta I II. JURISDICTION OF ORGANIZATION i1gt. ORGANIZATIONAL ID tot anxopionaq\r\n\r\na3 .....Lic....63-131 ORMIZATION\r\n\r\nI I\r\n2. ADDITIONAL DEBTORS EXACT FULL. LEGAL NAME - meet colyboe debtor name (23 or 211)- do not abbreviate or combos names\r\n\r\nSTATE\r\nCOUNTRY\r\n\r\nIt\r\n2a. ORGANIZATION'S NAME\r\nOR\r\n\r\nFIRST NAME\r\n\r\nNN\r\n\r\n2b. INDIVIDUAL'S LAST NAME\r\n\r\nMIDDLE NAME\r\n\r\nSUFFIX\r\n\r\nL1OE\r\n\r\n20. MAILING ADDRESS\r\n\r\nPARK ST\r\n\r\n2c1. TAX ID SSN OR EIN ADM. INFO RE r2.. TYPE OF ORGANIZATION\r\n11,23 \" ORsCrIZATION\r\n\r\n_\r\n\r\nQTY13ED-F6TD\r\n\r\nPO ODE 1COUNTRY\r\nSTATE\r\n\r\nC)I\r\n\r\n2r. JuRisoartoNoFoRtWzcnom\r\n\r\n2g. ORGANIZATIONAL ID N. d onyloptonei)\r\n\r\nCINONE\r\n3. SECURED PARTY'S NAME (or NAME of TOTAL ASSIGNEE of ASSIGNOR SIP) 'ow °mule sewed party name (3a or 3b)\r\n\r\n3a. ORGANIZATION'S NAME\r\n\r\nae4RN/ a)1,e.,4hK,\r\n\r\nR\r\n\r\n3b. tNowtothocsLasr NAME\r\n\r\nMIDDLE NAME\r\n\r\nSUFFIX\r\nFIRST NAME\r\n\r\n3c. MAILING ADDRESS\r\n\r\niq go /\r\n\r\nSTATE 'POSTAL CODE\r\n0-15\r\n\r\nCOUNTRY\r\n144/Cad<\r\n\r\nO\r\n\r\n..s-3)c(c.)\r\n\r\n4. This FINANCING STATEMENT covers 311 foliovong oaiiatorai\r\n\r\nSimplicity Riding Mower\r\n\r\n5 ALTERNATIvE DESIGNATION apohcablal ILESSEE,LESSOR(CONSIGNEE CONSIGNOR I ISAiLEE.8AILOR F IsELLER:suYER I LAG LIEN I_ I NON-UCC F/LING\r\n\r\nDebtor 1\r\nAll Debtors\r\n2\r\n\r\n8 OPTIONAL FILER REFERENCE DATA\r\n\r\n"),
                    ("Document", "This is the repeated email body\r\n\r\n"),
                    ("Pages", "1"),
                    ("SubFileID", "11"),
                    ("UnitID", "6"),
                    ("Document", "10"),
                    ("Pages", "10"),
                    ("SubFileID", "12"),
                    ("UnitID", "6"),
                    ("DocumentData", "BOOK 773 eAcf..97\r\n\r\nTrustee shall deliver to the purchaser Trustee's deed conveying the Property with special warranty of title. The\r\nrecitals in the Trustee's deed shall be prima facie evidence of the truth of the statements made therein. Trustee shall apply\r\nthe proceeds of the sale in the following order: (a)to all expenses of the sale, including, but not limited to, Trustee's fees of\r\n5% of the gross sale price and reasonable attorney's fees; (b) to the discharge of all taxes, levies, and assessments on the\r\nProperty, if any, as provided by applicable law; (e) to all sums secured by this Security Instrument; and (d) any excess to\r\nthe person or persons legally entitled to it. Trustee shall not be required to take possession of the Property prior to the sale\r\nthereof or to deliver possession of the Property to the purchaser at the sale.\r\n\r\n22. Release. Upon payment of all sums secured by this Security Instrument, Lender shall request Trustee to release\r\nthis Security Instrument and shall surrender all notes evidencing debt secured by this Security Instrument to Trustee. Trustee\r\nshall release this Security Instrument without charge to Borrower. Borrower shall pay any recordation costs.\r\n\r\n23. Substitute Trustee. Lender, at its option, may from time to time remove Trustee and appoint a successor trustee\r\nto any Trustee appointed hereunder. Without conveyance of the Property, the successor trusteeshall succeed to all the title,\r\npower and duties conferred upon Triutee herein and by applicable law.\r\n\r\n24. Identification of Note. The Note is identified by a certificate an the Note executed by any Notary Public who\r\ncertifies an acknowledgment hereto.\r\n\r\n25. Riders to this Security Instrument. If one or more riders are executed by Borrower and recorded together\r\nwith this Security Instrument, the covenants and agreements of each such rider shall be incorporated into and shall amend\r\nand supplement the covenants and agreements of this Security Instrument as if the rider(s) were a part of this Security Instrument\r\n(Check applicable box(es))\r\n\r\n\r\n\r\n0 Adjustable Rate Rider D Condominium Rider 0 1-4 Family Rider\r\n\r\n0 Graduated Payment Rider ID Planned Unit Development Rider D Biweekly Payment Rider\r\nEl Balloon Rider 0 Rate Improvement Rider 0 Second Home Rider\r\nOthe(s) EaPecifYl El Assignment of Rents E Residential Rider\r\nNOTICE: THE DEBT SECURED HEREBY IS SUBJECT TO CALL IN FULL OR THE TERMS THEREOF BEING\r\nMODIFIED IN THE EVENT OF SALE OR CONVEYANCE OF THE PROPERTY CONVEYED.\r\n\r\nBY SIGNING BELOW, Borrower accepts and agrees to the terms and covenants contained in this Security Instrument\r\nand in any rider(s) executed by Borrower and recorded with it.\r\n\r\nWitnesses:\r\n\r\n(Se\r\nC....John Doe Borro al)wer\r\n\r\nSocial Security Number. 4/4\r\n\r\n4221ae4C( Borrower\r\n\r\n(Seal)\r\n\r\nSocial Security Number . .46..19.88\r\n\r\n[Spats Balavo This Use For AsIonnollegmoral\r\n\r\nCity of Fredericksburg County ss:\r\nSTATE OF VIRGINIA,\r\n\r\nThe foregoing instrument was acknowledged before me this June 27, 1991\r\n\r\n(Date)\r\nby John Doe and Jane Doe\r\n\r\nJuly 14% ({'gfn Acknowledging) itef,\"\r\nMy Commission Expires\r\n\r\nNOTARY PUBLIC\r\nVIRGINIA: In th,e,Clerk's Office the Circuit Court of the\r\nCounty of (-,:i'lre.:444er\r\non the pr?9 day of i).t9,te4.-11..... , 19 .1/. .\r\nat o'clock ... M., this d was presented and with\r\nCertificatmitred to record an. d\r\n\r\nest: .. . Clerk\r\n\r\nRIM1 7041 IMO (page 6 of 6 pages)\r\n\r\nSTATE TAX S 40 61,\r\nLOCAL TAX $\r\nCLERK TAX E / #\r\n\r\nTOTAL S /.6 °")
                }
            },
            {
                "Resources.Hawkeye.HtmlBodyWithNoAttachments.eml.pdf",
                new List<(string, string)>()
                {
                    ("Document", "1-2"),
                    ("Pages", "1-2"),
                    ("SubFileID", "1"),
                    ("UnitID", "1"),
                    ("DocumentData", "Hi Nat,\r\nWhen people think of AI and machine learning, there are usually visions of\r\nmassive data sets that require incredible computing power to be able to process.\r\nRecently, several organizations have been taking on more shallow learning\r\nprojects, allowing them to draw insight from a relatively smaller data set.\r\nLearn more about this and its implications in our latest blog.\r\n\r\nHEALTHYDATA BLOG:\r\nIn healthcare, there are so many regulations and technologies to keep up with. Follow us\r\nand get up-to-date info about everything from MACRA and MIPS to EMR news, clinical\r\nbest practices and QAPI program initiatives.\r\n\r\nRECENT ARTICLES:\r\no Hospital Budget Systems Are Harming Innovation\r\no Federal Healthcare IT Budget Increases\r\no Bad Habits in Data Entry\r\n\r\nExtract Systems 8517 Excelsior Drive Suite 400 Madison, WI 53717 USA\r\n\r\nUnsubscribe from all future emails")
                }
            },
            {
                "Resources.Hawkeye.Example05.tif",
                new List<(string, string)>()
                {
                    ("Document", "1-3"),
                    ("Pages", "1-3"),
                    ("SubFileID", "1"),
                    ("UnitID", "1"),
                    ("DocumentData", "3\r\n\r\n1111111111111111111111111111111111111111111111111\r\n\r\nFRESNO County Recorder\r\nRobert C, Werner\r\nDOC 1000-0123456\r\n\r\nRECORDING REQUESTED BY\r\nFirst American Title Company\r\n\r\nAND WHEN RECORDED MAIL TO:\r\nJohn Doe\r\n123 Non Road\r\nSan Marino, CA 91108\r\n\r\nAcct 5-First American Title Insurance Company\r\nFriday, JAN 02, 2004 08:00:00\r\n\r\nTtl Pd $15,00 Nbr-0001350971\r\n\r\njzg/R2/1-3\r\n\r\nSpace Above This Line for Recorder's Use Only\r\n\r\nA.P.N.: 123-123-12 File No.: 1004-1075450 (RK)\r\n\r\nINTERSPOUSAL TRANSFER GRANT DEED\r\n\r\n(Excluded from Reappraisal under California Constitution Article 13A 1 et seq.)\r\n\r\nThe Undersigned Grantor(s) declare(s): DOCUMENTARY TRANSFER TAX $).00; CITY TRANSFER TAX $;\r\nThis conveyance is solely between spouses and establishes the sole and separate property of a spouse and is\r\nEXEMPT from the imposition of the Documentary Transfer Tax pursuant to 11911 of the Revenue and Taxation\r\nCode.\r\n\r\nThis is an Interspousal Transfer and not a change in ownership under Section 63 of the Revenue and Taxation\r\nCode, and transfer by Grantor(s) is excluded from reappraisal as a creation, transfer, solely between the spouses\r\nof any co-owner's interest.\r\n\r\nFOR A VALUABLE CONSIDERATION, receipt of which is hereby acknowledged, Jane Doe, wife of grantee\r\nherein\r\n\r\nhereby GRANTS to John Doe, a married man as his sole and separate property\r\nthe following described property in the City of FRESNO, County of FRESNO, State of California:\r\n\r\nAttached hereto as Exhibit A.\r\n\r\nIt is the express intent of the Grantor, being the spouse of the Grantee, to convey all right, title and\r\ninterest of the Grantor, community or otherwise, in and to the herein described property to the\r\nGrantee as his/her sole and separate property.\r\n\r\nDated: 12/2W_2003\r\n\r\nMail Tax Statments To: SAME AS ABOVE\r\n\r\n/\r\n\r\n0\r\n\r\nA.P.N.: 123-123-12 File No.: 1004-1075450 (RK)\r\n\r\n(6 Aieelef\r\n\r\ntGi\r\n\r\n}\r\n} ss.\r\n\r\n}\r\n\r\nSTATE OF\r\nCOUNTY OF\r\n\r\nbefore\r\n\r\nOn t i #\r\n\r\ne,eot_4( 4-. j 9-03 ,\r\nme, --Kityc-e,(jese, 4,4-0,:ez, personally\r\n\r\nappeared d(i/rti ,a6 - ,\r\n-personally knownto_mP (or proved to me on the basis of satisfactory evidence) to be the person4 whose\r\nname( is/are subscribed to the within instrument and acknowledged to me that,he/she/Iney executed tj-ie same\r\ninjais/her/their authorized capacity(ies) and that-his/her/their signature(K on the instrument the person(s) or the\r\nentity upon behalf of which the person(s) acted, executed the instrument.\r\n\r\nThis area for official\r\nnotarial seal\r\n\r\nWITNESS my hand and official seal.\r\n\r\nSignature\r\n\r\n(\"0\"-I litUL\r\nMy Commission Expires: il1/47/ /7/ 2 0.6\r\n\r\nTERESA ROSE MARTINEZ\r\nCommission # 1384821\r\nNotary Public - California t\r\nLos Angeles County\r\nMy Comm. Expires Nov 17, 2006\r\n\r\nPage 2\r\n\r\nInterspousal Transfer Grant Deed - continued File No.: 1004-1075450 (RK)\r\nA.P.N.: 123-123-12\r\n\r\nEXHIBIT A\r\n\r\nLOT 12 OF EGGERS COLONY, IN THE CITY OF FRESNO, COUNTY OF FRESNO, STATE OF CALIFORNIA,\r\nACCORDING TO THE MAP THEREOF RECORDED IN BOOK 1 PAGE 23 OF PLATS, FRESNO COUNTY\r\nRECORDS.\r\n\r\n3")
                }
            },
            {
                "Resources.Hawkeye.Example05.tif.pdf",
                new List<(string, string)>()
                {
                    ("Document", "1-3"),
                    ("Pages", "1-3"),
                    ("SubFileID", "1"),
                    ("UnitID", "1"),
                    ("DocumentData", "3\r\n\r\n1111111111111111111111111111111111111111111111111\r\n\r\nFRESNO County Recorder\r\nRobert C, Werner\r\nDOC 1000-0123456\r\n\r\nRECORDING REQUESTED BY\r\nFirst American Title Company\r\n\r\nAND WHEN RECORDED MAIL TO:\r\nJohn Doe\r\n123 Non Road\r\nSan Marino, CA 91108\r\n\r\nAcct 5-First American Title Insurance Company\r\nFriday, JAN 02, 2004 08:00:00\r\n\r\nTtl Pd $15,00 Nbr-0001350971\r\n\r\njzg/R2/1-3\r\n\r\nSpace Above This Line for Recorder's Use Only\r\n\r\nA.P.N.: 123-123-12 File No.: 1004-1075450 (RK)\r\n\r\nINTERSPOUSAL TRANSFER GRANT DEED\r\n\r\n(Excluded from Reappraisal under California Constitution Article 13A 1 et seq.)\r\n\r\nThe Undersigned Grantor(s) declare(s): DOCUMENTARY TRANSFER TAX $).00; CITY TRANSFER TAX $;\r\nThis conveyance is solely between spouses and establishes the sole and separate property of a spouse and is\r\nEXEMPT from the imposition of the Documentary Transfer Tax pursuant to 11911 of the Revenue and Taxation\r\nCode.\r\n\r\nThis is an Interspousal Transfer and not a change in ownership under Section 63 of the Revenue and Taxation\r\nCode, and transfer by Grantor(s) is excluded from reappraisal as a creation, transfer, solely between the spouses\r\nof any co-owner's interest.\r\n\r\nFOR A VALUABLE CONSIDERATION, receipt of which is hereby acknowledged, Jane Doe, wife of grantee\r\nherein\r\n\r\nhereby GRANTS to John Doe, a married man as his sole and separate property\r\nthe following described property in the City of FRESNO, County of FRESNO, State of California:\r\n\r\nAttached hereto as Exhibit A.\r\n\r\nIt is the express intent of the Grantor, being the spouse of the Grantee, to convey all right, title and\r\ninterest of the Grantor, community or otherwise, in and to the herein described property to the\r\nGrantee as his/her sole and separate property.\r\n\r\nDated: 12/2W_2003\r\n\r\nMail Tax Statments To: SAME AS ABOVE\r\n\r\n/\r\n\r\n0\r\n\r\nA.P.N.: 123-123-12 File No.: 1004-1075450 (RK)\r\n\r\n(6 Aieelef\r\n\r\ntGi\r\n\r\n}\r\n} ss.\r\n\r\n}\r\n\r\nSTATE OF\r\nCOUNTY OF\r\n\r\nbefore\r\n\r\nOn t i #\r\n\r\ne,eot_4( 4-. j 9-03 ,\r\nme, --Kityc-e,(jese, 4,4-0,:ez, personally\r\n\r\nappeared d(i/rti ,a6 - ,\r\n-personally knownto_mP (or proved to me on the basis of satisfactory evidence) to be the person4 whose\r\nname( is/are subscribed to the within instrument and acknowledged to me that,he/she/Iney executed tj-ie same\r\ninjais/her/their authorized capacity(ies) and that-his/her/their signature(K on the instrument the person(s) or the\r\nentity upon behalf of which the person(s) acted, executed the instrument.\r\n\r\nThis area for official\r\nnotarial seal\r\n\r\nWITNESS my hand and official seal.\r\n\r\nSignature\r\n\r\n(\"0\"-I litUL\r\nMy Commission Expires: il1/47/ /7/ 2 0.6\r\n\r\nTERESA ROSE MARTINEZ\r\nCommission # 1384821\r\nNotary Public - California t\r\nLos Angeles County\r\nMy Comm. Expires Nov 17, 2006\r\n\r\nPage 2\r\n\r\nInterspousal Transfer Grant Deed - continued File No.: 1004-1075450 (RK)\r\nA.P.N.: 123-123-12\r\n\r\nEXHIBIT A\r\n\r\nLOT 12 OF EGGERS COLONY, IN THE CITY OF FRESNO, COUNTY OF FRESNO, STATE OF CALIFORNIA,\r\nACCORDING TO THE MAP THEREOF RECORDED IN BOOK 1 PAGE 23 OF PLATS, FRESNO COUNTY\r\nRECORDS.\r\n\r\n3")
                }
            }
        };

        #endregion

        /// <summary>
        /// Confirm that the expected hierarchy is generated from emails converted to PDFs and also from normal TIF/PDF files
        /// </summary>
        [Test, Category("Automated")]
        [TestCaseSource(nameof(_resourceNameToExpectedData))]
        [SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures")]
        public static void TestSplitDocument(KeyValuePair<string, List<(string, string)>> testCaseData)
        {
            // Arrange
            string inputResource = testCaseData.Key;
            List<(string, string)> expectedFlattened = testCaseData.Value;
            string imageFile = _testFiles.GetFile(inputResource);
            string ussFile = _testFiles.GetFile(inputResource + ".uss");

            AFDocumentClass inputDoc = new();
            inputDoc.Text.LoadFrom(ussFile, false);

            // Act
            AFDocument outputDoc = HawkeyePaginationSplitter.SplitDocument(inputDoc);

            // Assert
            var actualFlattened = AttributeMethods.EnumerateDepthFirst(outputDoc.Attribute.SubAttributes)
                .Select(attribute => (attribute.Name, attribute.Value.String))
                .ToList();

            CollectionAssert.AreEqual(expectedFlattened, actualFlattened);
        }
    }
}
