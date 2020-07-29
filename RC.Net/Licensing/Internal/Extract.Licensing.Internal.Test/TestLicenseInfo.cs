// NUnit 3 tests
// See documentation : https://github.com/nunit/docs/wiki/NUnit-Documentation
using Extract.Testing.Utilities;
using NUnit.Framework;
using System;
using System.IO;
using System.Linq;

namespace Extract.Licensing.Internal.Test
{
    [TestFixture]
    public class TestLicenseInfo
    {
        // User string from voyager
        const string UserString = "3E487F6493664191F06714D99535699D879E324A942391CB6F7D5922D705947E";
        const string LicenseString = "64C6164E598AB18EC60FA89FC0893CD1692E2BA59FB2581FC579461F6E37A3BD1080CF55179871FAD97D39C8D8EDCF5D9189CBB41A47FDF42D339708B78EBEC757CF15B1D3EE2D437AEF9D863C2C79A81AA1663BE4CBD668B899F7DAECDD7DF6B431D52B3630318A6B2A1A446EB0C83FE886852AF604DE516415785B96661F70A72ED55696247E9757314B8233C158924F394880D78CD9DB3CF0E571C69FA44082742D8E8FEFA3FC76AA404C7AE7A909CAD03FEABE1D79D0C075E90DA26C347CAFFC67AE837895DD3C6CEC5B0A718379EE5183AE4CB1D893F88527FA46E6C964AC272CB5077358E9A746AF3020A2094EE9C1771C539A237E329622551D505E9CDB008256D67B9526A07AB5E48B004D8B11685F4FC1BD641D90F577666A94C78DCDC47CCE42FDBAFE709CBBB2B7D5240EBE3797D1DA9E823B0DB4F997186B5253F3146231F9D173FEFDD3E894A6A820B33D95BB637B66B396DB88FED3F3A4822BA69D581BAB013BEF167184C0D95708AA20FE868788EFDFEA199D89CD723501169560586DAC1D233E352825940208643E6850CCCE7AE5098ECAFF9A8290C7B9B0D5DEC81A9D78A4A15855A409472E724B8BD85021849CD867096037722F10FCED44905A7BF4F2636DA3CCB019DCD354AC28F87A8B765B088041D899F8C4B6286E7C692BB834B860E3456D07ABAB33A94ED7BF29D07D7E8F511CDA398AE47A0B8B400265B43F018958F398A5FBD5F618837C78A6F1A29ED8DEA9DCDB694EE018B501CDE15BADE86EFAB7BBE36642B7E4E193DE19C43D8FD2D5BAEA83E412F97A7D505A4E0129AE48B0B315FA750D2662D77D30943417958AD1ED54D32D81635B6B7B9FD5A0D5A6D802EC1BC513EEE38C13B831AE6750C9A239CE6AABEEE689E5DDDC8B65BF8B9DA763EAD60E63DA14520D43BED9827B60430E642F6D61B76DC7329C502A6F18BFB729A3870AC77463506FA11D467D4588B1E7152AEFDAEAEC2BEA9B887CDB6F9A0FB08EC9F256C4AAA95F8E560D795F91497AC4C6084944B0CED980E4A9191118186BF5C976867243D4E7334A24D680A517E7F33DF30FC4652B232F019D7DC17095EB5DBCCD339205D086D7BF29D07D7E8F511CDA398AE47A0B8BEC7A196F40B16F8838498E96D676B16B747542C4A4BD55F3A6791CCE97C5793D2E527BA39678C53A93B2757CAE6BE2190728177C058A93F7E08568550D0A1799343DDBBFE562D313D047198AE624400139864A421D738A82A59F97E07818872F2D6EA4FEB5C23F4C930D6ADA248D0EC5F69AB95E390DB5DF283A0B8EC042713BA684CCA00286E73D30937953A44A7AC5269DA17DD2FBFD1BA38400F4918661338DDF35F5C354C3BCD306A3041FCDC8760D9D971634036D6C8B9722090FFD9DCB53D7F39DA104678E56EFCEBE6249BA76D58AD6C2B81EF1567E9093DAFC62B0944E0735600B9EE869EB7775379E0BFF0E48041DB1D39E756B1000FC1DC52E7191AB7AE244EB03BAA68F14547C8DA2005AD7BF29D07D7E8F511CDA398AE47A0B8B03080EA61299952714FF8A06DC4EFFF6F403CE5F01A117BD43D5CE617C39C7169AE0AFBEBF64A710A47FF7A20ACF72CA401804110F8FF3A96013CA12001B7FB3D2E9A510CE7E2E8599E9B92E9190C96072B8338E21481FE2F00D5B9FFA562F32EFEAA52028B1320F8F2F850B24D0B7C07E801C710273726F4DA9966204F3406E9B27BF18A455F942D6DFF8340538CD18C9D95CA4C8F0A34A38DFB621E82F4F06B155FD22AB9FBEB7A80E676B1D3573C3BA9E505BD535E2716E722C7256040D9FB016DC12A50A15FA965FC72564CD0209F323E8C3EEC800DD27002E153A945DB1D86C28B3BBD405D6E5CFF6A2EEC463FD245E53A4AAD0A807B72DC9CF05643E667E51310F9F3572E0F00DCE61BD1A9B76D7BF29D07D7E8F511CDA398AE47A0B8B918B595E451160A7909A0103D0F9D9C78A3B00D6052CF7F0D10CD98A1F95DD74DEA2730BF11B94CF02775C784638EB79F26ADA4C1D12144EAF80CA0596C00A94CAB540E9DDCC682628EEDE0A7F3D3077C41690D2B5C1665284CF4F1D3EFBDAC7067A317C7DB2AA6D61BC40F9AA3C1F7891CD2B4CC5350341FDDB717737DE9E5D49F4942D93578537C740DF493C576A741ED19CB019B105B97CA6CF7A35C923333FF833DC8C8E019CBEF829ED1EEACF29773ACE5703D8A3148E794317E99D8D9883A2EAD6F98966EDDD15BE71E7BDC8C3C7B4678D9B86F3627FF19B67CE539F14E934296F103BCCC272C766B9A2CB657ED316557726B8E9241F7AFCB4BD2820C7BE187B004E111EA8663A322B576AD805D7BF29D07D7E8F511CDA398AE47A0B8B5B413D2A0E38170F29B28BC6BE6358B6FE6115F378FA420E5D3F3E0C52BB5104B73DD314175CD5E34DBCE2E6D330630F586CB6C72236EFFB3AED87176CB9AB31870A92DD859BAAF3A815DB5D317AE69A3CF24A6946FCBABA28930E5E08C52FDF16FC9BAC8E62A289787E295734AAD58D209CB6CCDC6948DC0B7F8C03BB4565A9D6BAF215AF032193E884BE7CB90036B84D3FB3DC2BF24D4DD8F6B2A34BF76A94D8AA511E9C5D7213E6D86B0EF1A523F94876C848E4452DAB63DA3EE9C900769C1B7FD37804A8CFE233194DF2710A27D71065033D2A1BEB61B8ABB9D6A3571243518E02C585FB8A8F1A8654E0F1FDFC9FA666ED0D53991900DC091EAB8586E99F336F7B769656DFA5C702AFF087C7AB07D7BF29D07D7E8F511CDA398AE47A0B8BD40A3E9CA0FF246E730EDB0237CE29F79D0094A9FE4088DC589B001145C5602110C8097AF076A2F7FE9F3BA82B8E8C392CC99CF459AE42A3BBD307E524A1B02DA71B7F001989393E9BD0C59041724FF9020D257B9841BF9472878F5DBE10D59CB7C574E6C71C0B63A3A452D4BBCBA394A1F1980ABF5A1502008624BB0A590DFD92F64A46F5C63F5BECB16400082BBD4258BBF526D1B0F566D9E77A35163E3493DDC9F0C976B4053D337684859BAF48D6B0F04B32BC9C44222CBFB85205616A1BD97F309F4F0A3548332877852035CB01B438F11E6D80D99BF421B9EA59B776454FA099A2D53C1C2CE238552F25CE135DE44F29683A69A2526DF80B51E76E8988EBA584AE0300835699282DA769122702";
        readonly uint[] ComponentsLicensedInLicenseString = new uint[]
        {
            1,
            2,
            3,
            4,
            5,
            6,
            10,
            21,
            22,
            23,
            24,
            25,
            26,
            27,
            28,
            29,
            44,
            45,
            46,
            47,
            48,
            49,
            50,
            51,
            52,
            70,
            72,
            73,
            76,
            77,
            78,
            79,
            80,
            81,
            82,
            83,
            84,
            85,
            87,
            88,
            89,
            90,
            91,
            92,
            93,
            101,
            102,
            103,
            110
        };
        const UInt32 ComponentBase = 7000;

        [OneTimeSetUp]
        public static void Setup()
        {
            GeneralMethods.TestSetup();
        }

        [OneTimeTearDown]
        public static void FinalCleanup()
        {
        }

        [Test]
        [Category("Licensing")]
        [Category("Automated")]
        [Category("Internal")]
        public void TestCreateCodeAndFromCode()
        {
            LicenseInfo licenseInfo = CreateLicenseInfo();

            licenseInfo.ComponentIDToInfo[1] = new ComponentInfo();
            licenseInfo.ComponentIDToInfo[2] = new ComponentInfo(DateTime.UtcNow.AddDays(30));

            var licenseCode = licenseInfo.CreateCode();

            LicenseInfo fromCode = new LicenseInfo(licenseCode);

            Assert.IsTrue(fromCode.Equals(licenseInfo));
        }

        [Test]
        [Category("Licensing")]
        [Category("Automated")]
        [Category("Internal")]
        public void TestLicenseStringDecode()
        {
            var licenseInfo = new LicenseInfo(LicenseString);
            Assert.AreEqual(licenseInfo.UserComputerName.ToUpper(), "VOYAGER");
            Assert.AreEqual(licenseInfo.UserMACAddress.ToUpper(), "3417EBA0BF7D");
            Assert.AreEqual(licenseInfo.UserSerialNumber, (UInt32)182269668);

            // Test the licensed components
            var licensedComponents = licenseInfo.ComponentIDToInfo
                .Where(kvp => kvp.Value.PermanentLicense)
                .Select(kvp => kvp.Key)
                .OrderBy(k => k);

            Assert.IsTrue(licensedComponents
                .SequenceEqual(ComponentsLicensedInLicenseString
                    .Select(v => v + ComponentBase)
                .OrderBy(v => v)));
        }

        [Test]
        [Category("Licensing")]
        [Category("Automated")]
        [Category("Internal")]
        public void TestUserLicenseStringDecode()
        {
            var licenseInfo = new LicenseInfo();
            licenseInfo.UserString = UserString;
            Assert.AreEqual(licenseInfo.UserComputerName.ToUpper(), "VOYAGER");
            Assert.AreEqual(licenseInfo.UserMACAddress.ToUpper(), "3417EBA0BF7D");
            Assert.AreEqual(licenseInfo.UserSerialNumber, (UInt32)182269668);
        }

        [Test]
        [Category("Licensing")]
        [Category("Automated")]
        [Category("Internal")]
        [TestCase(true, Description ="Permanent License")]
        [TestCase(false, Description = "Expiring License")]
        public void TestCreateLicenseFile(bool permanentLicense)
        {
            DateTime expireDate = DateTime.Now.AddDays(30);
            var licenseInfo = CreateLicenseInfo();

            licenseInfo.UseSerialNumber = true;
            licenseInfo.UserString = UserString;

            // add components
            foreach (uint i in ComponentsLicensedInLicenseString)
            {
                licenseInfo.ComponentIDToInfo[i + ComponentBase] = (permanentLicense)
                    ? new ComponentInfo()
                    : new ComponentInfo(expireDate);
            }

            using (var temporaryFile = new Extract.Utilities.TemporaryFile(false))
            {
                licenseInfo.SaveToFile(temporaryFile.FileName);
                var licenseInfoFromFile = new LicenseInfo();
                licenseInfoFromFile.LoadFromFile(temporaryFile.FileName);
                Assert.AreEqual(licenseInfo, licenseInfoFromFile);
            }
        }

        [Test]
        [Category("Licensing")]
        [Category("Automated")]
        [Category("Internal")]
        // Only first test case will pass so have disabled one of them
        // TODO: Fix so both test cases can be enabled and they pass.
        [TestCase(false, Description = "Expiring License")]
        //[TestCase(true, Description ="Permanent License")]
        public void TestCreateLicenseFileWithLoadLicenseFiles(bool permanentLicense)
        {
            DateTime expireDate = DateTime.Now.AddDays(30).ToLocalTime().Date;

            var licenseInfo = CreateLicenseInfo();

            licenseInfo.UserString = UserString;

            licenseInfo.ComponentIDToInfo[999 + ComponentBase] = (permanentLicense)
                ? new ComponentInfo()
                : new ComponentInfo(expireDate);

            using (var temporaryFile = new Extract.Utilities.TemporaryFile(null, "TestLicense.lic", null, false))
            {
                licenseInfo.SaveToFile(temporaryFile.FileName);

                var licenseValues = Enum.GetValues(typeof(LicenseIdName)).Cast<LicenseIdName>();
                try
                {
                    // Since it may have been initialized UnlicenseAll items
                    LicenseUtilities.UnlicenseAll();
                    var licensed = licenseValues.Where(l => LicenseUtilities.IsLicensed(l));
                    Assert.AreEqual(0, licensed.Count());

                    LicenseUtilities.LoadLicenseFilesFromFolder(Path.GetDirectoryName(temporaryFile.FileName),
                                                                0,
                                                                new MapLabel());
                    licensed = licenseValues.Where(l => LicenseUtilities.IsLicensed(l));
                    Assert.AreEqual(1, licensed.Count());

                    Assert.IsTrue(LicenseUtilities.IsLicensed(LicenseIdName.TestComponent));
                    if (permanentLicense)
                    {
                        Assert.IsFalse(LicenseUtilities.IsTemporaryLicense(LicenseIdName.TestComponent));
                    }
                    else
                    {
                        Assert.IsTrue(LicenseUtilities.IsTemporaryLicense(LicenseIdName.TestComponent));
                        Assert.AreEqual(expireDate, LicenseUtilities.GetExpirationDate(LicenseIdName.TestComponent));
                    }
                }
                finally
                {
                    // reload the licenses from the default folder
                    LicenseUtilities.UnlicenseAll();
                    //LicenseUtilities.LoadLicenseFilesFromFolder(0, new MapLabel());
                }
            }
        }

        [Test]
        [Category("Licensing")]
        [Category("Automated")]
        [Category("Internal")]
        public void TestCopyFrom()
        {
            var licenseInfo = CreateLicenseInfo();

            licenseInfo.UseSerialNumber = true;
            licenseInfo.UserString = UserString;

            // add components
            foreach (uint i in ComponentsLicensedInLicenseString)
            {
                licenseInfo.ComponentIDToInfo[i + ComponentBase] = new ComponentInfo();
            }

            var newLicenseInfo = new LicenseInfo();
            newLicenseInfo.CopyFrom(licenseInfo);
            Assert.AreEqual(licenseInfo, newLicenseInfo);
        }

        private LicenseInfo CreateLicenseInfo()
        {
            LicenseInfo licenseInfo = new LicenseInfo();
            licenseInfo.IssuerName = "John Smith";
            licenseInfo.LicenseeName = "Jane Doe";
            licenseInfo.OrganizationName = "Widget Inc.";
            licenseInfo.UseComputerName = false;
            licenseInfo.UseMACAddress = false;
            licenseInfo.UseSerialNumber = false;
            return licenseInfo;
        }
    }
}
