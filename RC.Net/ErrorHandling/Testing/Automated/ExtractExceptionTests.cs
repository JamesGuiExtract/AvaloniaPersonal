using NUnit.Framework;
using System;
using System.IO;
using System.Linq;
using static System.Environment;
using Extract.Utilities;
using System.Security.Permissions;

namespace Extract.ErrorHandling.Test
{
    class TestEventArgs : EventArgs
    {
        public string TestString { get; set; }
    }

    [TestFixture()]
    public class ExtractExceptionTests
    {
        /// <summary>
        /// EliCode	"ELI28774"	
        /// Message	"Application trace: FAM Service stopped. (PerformanceProcess)"	
        /// Data: 
        ///     "Threads Stopped": "True"
        ///     "CatchID": "ELI02154"
        /// </summary>
        const string TestString = "1f00000055434c4944457863657074696f6e204f626a6563742056657273696f6e20320300000008000000454c4932383737343c0000004170706c69636174696f6e2074726163653a2046414d20536572766963652073746f707065642e2028506572666f726d616e636550726f63657373290000000000020000000f000000546872656164732053746f7070656400000000040000005472756507000000436174636849440000000008000000454c49303231353400000000";
        const string TestStringizedExceptionWithInner = "1f00000055434c4944457863657074696f6e204f626a6563742056657273696f6e20320300000008000000454c4933323431322400000046696c65206f7065726174696f6e206661696c656420616674657220726574726965732e01050600001f00000055434c4944457863657074696f6e204f626a6563742056657273696f6e20320300000008000000454c493231303631de0000005468652070726f636573732063616e6e6f7420616363657373207468652066696c652027433a5c55736572735c6c615f77696c6c69616d5f706172725c417070446174615c4c6f63616c5c54656d705c546573744f6666696365436f6e76657273696f6e5f38626266356135352d373339342d346530332d623533362d6366636335306530303163375c54657374576f7264446f63756d656e74732e56504e496e737472756374696f6e732e646f6378272062656361757365206974206973206265696e67207573656420627920616e6f746865722070726f636573732e00000000000000000005000000b3010000457874726163745f456e637279707465643a20323642303733434641463830313842324630373833314439383332423141383541413241383845393836453841313036393930364644433045423336413937313437394138313645313043424433344637373141393942313533433344393141363243433645363434353136433445374645374241344134454439423641364631414635333536303539354239423032354437314132304336443636453236434239443433313945443630373735464132423133394642424533423537343035353735414237333332323643424436393141374438443730444131373034453932433036453642383436464238343344464145423733303135443736344434393143353139313342423331364645373233444544463537453439304145463331423933333043414441353141383532323343463332324336383936414541413445373238333432453645374545353434303136323230413242443042313441423041434443433137313044343833363642323744313837343633463632324239374544333835344131383641323830354144444133463239333941374235373353010000457874726163745f456e637279707465643a20364633344239313839424434463137354630373833314439383332423141383541413241383845393836453841313036393930364644433045423336413937313437394138313645313043424433344638383746453146453642443144323834443645334433434137463434313837444533414430363133373739354635384142373342363343333543444134333238363830373537423141423745464136373637434334363746434638303046464138353442463239343237344334374639344431454139323837343644363843333531354139303835443741463245314635464445374139383145464542393835433239323339453830463039364633413033323437393441413445454539463434373941383136453130434244333446364635303644314243324631454636444134443444333030354146303342313473000000457874726163745f456e637279707465643a20323644333833443339343241434245414236344630323644324245433136393846313639354144323137363938424534303330463544314643464133354338424135414241314431364337453345423236423334384635354541443542334331a3000000457874726163745f456e637279707465643a20414139334638313735424142384645304236344630323644324245433136393842453137343842383132354145334543413534424636363835424441443441463033304635443146434641333543384241354142413144313643374533454232363742344238354533383841383443334139364437323930383139374644393242423444413230374546463841393541b3000000457874726163745f456e637279707465643a20464645333538303931424241383943324236344630323644324245433136393831463439323545363333364339363634313436443739374333454646304638413943393734374135423334364435444241324638344632343842303035444545313933334242414241303246303045423543444342423430433934433046373742433031423044453633323539393643363646413233464134434637433234350000000009000000120000004e756d626572204f6620417474656d70747300000000020000003530160000004d6178204e756d626572204f6620417474656d70747300000000020000003530080000004361746368454c490000000008000000454c493332343133090000004361746368454c49310000000008000000454c493332383736090000004361746368454c49320000000008000000454c493332383734090000004361746368454c49330000000008000000454c4932333639351100000046696c6520466f722044656c6574696f6e000000008c000000433a5c55736572735c6c615f77696c6c69616d5f706172725c417070446174615c4c6f63616c5c54656d705c546573744f6666696365436f6e76657273696f6e5f38626266356135352d373339342d346530332d623533362d6366636335306530303163375c54657374576f7264446f63756d656e74732e56504e496e737472756374696f6e732e646f6378090000004361746368454c49340000000008000000454c49323535313107000000436174636849440000000008000000454c49303231353402000000e3010000457874726163745f456e637279707465643a20434136313341314136393145374241414630373833314439383332423141383541413241383845393836453841313036393930364644433045423336413937313437394138313645313043424433344641433543383941313942413633374336434645324130453636353146424243344638303832333436443839303544373632463137443144454131333841313838363742344238354533383841383443333539314442454639333238454243353946343331354138363538433642304635303041443841394336464538424130333246364645323435453530344631463943464532413045363635314642424334364344364634364342343637393039323733453242344236453239304644353938324545393939324446393933383533393632303939313144304444303430383143353139313342423331364645373233444544463537453439304145463331423933333043414441353141383532323343463332324336383936414541413445373238333432453645374545353434303136323230413242443042313441423041434443433137313044343833363642323744313837343633463632324239333736303538374543443641383541374533433635393443394437434333334673010000457874726163745f456e637279707465643a2041353030334533353835364542413731463037383331443938333242314138354141324138384539383645384131303639393036464443304542333641393731343739413831364531304342443334463331334146433341394239413344323945333234423635353746363044353437354533303532433741424639393633353943333031444244313932463131443241313632303530323732423841414246323041463142374545313336343046433034323231314334364643314332313436394239353142394631354344444641453139373939434646414433464544383742423431333535384546333931383533463246343132373842443541343844304339373744384234353032463543424536424641334142423244443143393839413639383634454232313646343245314245463531463234303334413030453241333745304137354434344438464336433830463436364436334433414646";

        [Test()]
        public void ExtractExceptionTestNoParameters()
        { 
            var testException = new ExtractException();
            Assert.IsNotNull(testException);
            Assert.IsTrue(string.IsNullOrWhiteSpace(testException.EliCode));
            Assert.IsNotNull(testException.Resolutions);
            Assert.IsNotNull(testException.StackTraceValues);

            try
            {
                throw testException;
            }
            catch (ExtractException ee)
            {
                Assert.IsNotNull(ee.Resolutions);

                Assert.IsNotNull(ee.StackTraceValues);
                Assert.AreEqual(0, ee.StackTraceValues.Count);

                Assert.AreEqual(0, ee.StackTraceValues.Count);
                var newEE = ExtractException.LoadFromByteStream(ee.AsStringizedByteStream());
                Assert.IsTrue(ee.EliCode.Equals(newEE.EliCode));
                Assert.IsTrue(ee.Message.Equals(newEE.Message));
            }
        }

        [Test()]
        public void ExtractExceptionTest_ELICode_Message()
        {
            var testException = new ExtractException("ELITest", "Message");
            Assert.IsNotNull(testException);

            try
            {
                throw testException;
            }
            catch (ExtractException ee)
            {
                Assert.AreEqual("ELITest", ee.EliCode, "EliCode property was not set.");
                Assert.IsNotNull(ee.Resolutions);
                Assert.IsNotNull(ee.StackTraceValues);
                Assert.AreEqual("Message", ee.Message, "Message property was not set.");

                Assert.IsNull(ee.InnerException, "There should not be an inner exception");
            }
        }

        [Test()]
        public void ExtractExceptionTest_ELICode_Message_InnerException()
        {
            var innerException = new ExtractException("ELIInner", "InnerException");
            Assert.IsNotNull(innerException);
            var testException = new ExtractException("ELITest", "Message", innerException);
            Assert.IsNotNull(testException);

            try
            {
                throw testException;
            }
            catch (ExtractException ee)
            {
                Assert.AreEqual("ELITest", ee.EliCode, "EliCode property was not set.");
                Assert.IsNotNull(ee.Resolutions);
                Assert.IsNotNull(ee.StackTraceValues);
                Assert.AreEqual("Message", ee.Message, "Message property was not set.");

                Assert.IsNotNull(ee.InnerException, "There should be an inner exception");
                Assert.AreEqual("ELIInner", ((ExtractException)ee.InnerException)?.EliCode);
                Assert.AreEqual("InnerException", ((ExtractException)ee.InnerException)?.Message);
            }
        }

        [Test()]
        public void AddDebugDataTestEventArgs()
        {
            var testException = new ExtractException("ELITest", "Message");
            var testEventArgs = new TestEventArgs()
            {
                TestString = "TestArg"
            };

            Assert.DoesNotThrow(()=> testException.AddDebugData("TestValue", testEventArgs));
            string testString = null;
            Assert.DoesNotThrow(() => testString = (string) testException.Data["TestValue.TestString"]);
            Assert.AreEqual(testEventArgs.TestString, testString);

            // can this be streamed
            ExtractException streamedException = null;
                
            Assert.DoesNotThrow(() => streamedException = ExtractException.LoadFromByteStream(testException.AsStringizedByteStream()));
            Assert.IsNotNull(streamedException);

            Assert.AreEqual(testEventArgs.TestString, (string)streamedException.Data["TestValue.TestString"]);
        }

        [Test()]
        public void AddDebugDataTestString()
        {
            var testException = new ExtractException("ELITest", "Message");
            testException.AddDebugData("TestName", "TestValue");

            Assert.AreEqual("TestValue", (string)testException.Data["TestName"]);

            ExtractException streamedException = null;
            Assert.DoesNotThrow(() => streamedException = ExtractException.LoadFromByteStream(testException.AsStringizedByteStream()));
            Assert.AreEqual("TestValue", (string) streamedException.Data["TestName"]);
        }

        [Test()]
        public void AddDebugDataTestValueType()
        {
            var testException = new ExtractException("ELITest", "Message");
            Assert.Multiple(() =>
            {
                Assert.DoesNotThrow(() =>
                {
                    string dataName = null;
                    testException.AddDebugData(dataName, "Test data name of null");
                });
                Assert.AreEqual(0, testException.Data.Count);

                Assert.DoesNotThrow(() =>
                {
                    string data = null;
                    testException.AddDebugData("TestNull", data);
                });
                Assert.AreEqual("<null>", (string)testException.Data["TestNull"]);

                Assert.DoesNotThrow(() => testException.AddDebugData("TestInt", (int)10), "Should be able to add int");
                Assert.DoesNotThrow(() => testException.AddDebugData("TestInt64", (Int64)100), "Should be able to add Int64");
                Assert.DoesNotThrow(() => testException.AddDebugData("TestUInt32", (UInt32)10), "Should be able to add UInt32");
                Assert.DoesNotThrow(() => testException.AddDebugData("TestDouble", (double)10.5), "Should be able to add double");
                Assert.DoesNotThrow(() => testException.AddDebugData("TestBoolean", true), "Should be able to add boolean");
                var dateTime = DateTime.Now;
                Assert.DoesNotThrow(() => testException.AddDebugData("TestDateTime", dateTime), "Should be able to add DateTime");
                var guid = Guid.NewGuid();
                Assert.DoesNotThrow(() => testException.AddDebugData("TestGuid", guid), "Should be able to add Guid");
                
                ExtractException streamedException = null;
                Assert.DoesNotThrow(() => streamedException = ExtractException.LoadFromByteStream(testException.AsStringizedByteStream()));

                Assert.AreEqual((int)10, (int)streamedException.Data["TestInt"], "TestInt should be 10");
                Assert.AreEqual((Int64)100, (Int64)streamedException.Data["TestInt64"], "TestInt64 should be 100");
                Assert.AreEqual((UInt32)10, (UInt32)streamedException.Data["TestUInt32"], "TestUInt32 should be 10");
                Assert.AreEqual((double)10.5, (double)streamedException.Data["TestDouble"], "TestDouble should be 10.5");
                Assert.AreEqual(true, (Boolean)streamedException.Data["TestBoolean"], "TestBoolean should be true");
                Assert.AreEqual(dateTime, (DateTime)streamedException.Data["TestDateTime"], $"TestDateTime should be {dateTime:G}");
                Assert.AreEqual(guid, (Guid)streamedException.Data["TestGuid"], $"Guid should be {guid}");
            });
        }

        [Test()]
        [Ignore("Encryption not implemented")]
        public void AddDebugDataTestEventArgsEncrypt()
        {
            Assert.Fail();
        }

        [Test()]
        [Ignore("Encryption not implemented")]
        public void AddDebugDataTestStringEncrypt()
        {
            Assert.Fail();
        }

        [Test()]
        [Ignore("Encryption not implemented")]
        public void AddDebugDataTestValueTypeEncrypt()
        {
            Assert.Fail();
        }

        [Test()]
        public void AsStringizedByteStreamTest()
        {
            var innerException = new ExtractException("ELIInner", "InnerException");
            Assert.IsNotNull(innerException);
            var testException = new ExtractException("ELITest", "Message", innerException);
            Assert.IsNotNull(testException);

            try
            {
                throw testException;
            }
            catch (ExtractException ee)
            {
                var newEE = ExtractException.LoadFromByteStream(ee.AsStringizedByteStream());
                Assert.IsTrue(ee.EliCode.Equals(newEE.EliCode), "ELiCodes should match");
                Assert.IsTrue(ee.Message.Equals(newEE.Message), "Message should match");
                Assert.AreEqual(ee.StackTraceValues.Count, newEE.StackTraceValues.Count, "Number of StackTraceValues should match");
                Assert.IsTrue(ee.StackTraceValues.SequenceEqual(newEE.StackTraceValues), "StackTraceValues should match");
                Assert.IsNotNull(ee.InnerException, "There should be an inner exception");
                Assert.AreEqual("ELIInner", ((ExtractException)ee.InnerException)?.EliCode);
                Assert.AreEqual("InnerException", ((ExtractException)ee.InnerException)?.Message);
                Assert.AreEqual(ee.InnerException.Message, newEE.InnerException.Message, "Message of the inner exceptions should match");
                Assert.AreEqual(((ExtractException)ee.InnerException).EliCode, ((ExtractException)newEE.InnerException).EliCode
                    , "Eli code of the InnerExceptions should match");
            }
        }

        [Test()]
        public void CreateLogStringTest()
        {
            // Not a good way to test, most critical is the number of , and that the hex exception string is there
            var testException = new ExtractException("ELITest", "Message");
            Assert.IsNotNull(testException);

            string logString = testException.CreateLogString();
            var items = logString.Split(',');
            Assert.AreEqual(7, items.Length);
            Assert.IsTrue(items[6].Equals(testException.AsStringizedByteStream()), "Exception string portion of log string should be Stringized byte stream of exception");
        }

        [Test()]
        [Category("Interactive")]
        [Ignore("Display not implemented")]
        public void DisplayTest()
        {
            Assert.Fail();
        }

        [Test()]
        public void LogTestNoParam()
        {
            var testException = new ExtractException("ELITest", "Message", null);
            Assert.IsNotNull(testException);

            UseDefaultUEX((fileName) =>
            {
                Assert.DoesNotThrow(() => testException.Log(), "Log method should not throw exception.");

                // Load the text file
                var lines = File.ReadLines(fileName).ToList();
                Assert.AreEqual(1, lines.Count);

                var items = lines[0].Split(',');
                Assert.AreEqual(7, items.Length);
                Assert.IsTrue(items[6].Equals(testException.AsStringizedByteStream()), "Exception string portion of log string should be Stringized byte stream of exception");
            });
        }

        [Test()]
        public void LogTestFileName()
        {
            var testException = new ExtractException("ELITest", "Message", null);
            Assert.IsNotNull(testException);

            UseTemporyUEX((fileName) =>
            {
                Assert.DoesNotThrow(() => testException.Log(fileName), "Log method should not throw exception.");

                // Load the text file
                var lines = File.ReadLines(fileName).ToList();
                Assert.AreEqual(1, lines.Count);

                var items = lines[0].Split(',');
                Assert.AreEqual(7, items.Length);
                Assert.IsTrue(items[6].Equals(testException.AsStringizedByteStream()), "Exception string portion of log string should be Stringized byte stream of exception");
            });
        }

        [Test()]
        [Ignore("Log with force local not implemented")]
        public void LogTestFileNameForceLocal()
        {
            Assert.Fail();
        }

        [Test()]
        public void LogTestMachineUserTimeProcessIdAppNameNoRemote()
        {
            var testException = new ExtractException("ELITest", "Message", null);
            Assert.IsNotNull(testException);

            UseDefaultUEX((fileName) =>
            {
                var time = DateTime.Now;
                testException.Log("TestMachine", "TestUser", (int)time.ToUnixTime(), 1010, "TestApplication", false);
                
                // Load the text file
                var lines = File.ReadLines(fileName).ToList();
                Assert.AreEqual(1, lines.Count);
                
                var items = lines[0].Split(',');
                Assert.AreEqual(7, items.Length);

                Assert.Multiple(() =>
                {
                    Assert.IsTrue(string.IsNullOrEmpty(items[0]), "SerialNumber should be empty");
                    Assert.IsTrue(items[1].Equals("TestApplication"), "ApplicationName should be 'TestApplication");
                    Assert.IsTrue(items[2].Equals("TestMachine"), "Machine Name should be 'TestMachine'");
                    Assert.IsTrue(items[3].Equals("TestUser"), "User should be 'TestUser");
                    Assert.AreEqual(1010, int.Parse(items[4]), "ProcessID should be 1010");
                    Assert.AreEqual((int)time.ToUnixTime(), int.Parse(items[5]), $"Time should be {((int)time.ToUnixTime()).ToString()}");
                    Assert.IsTrue(items[6].Equals(testException.AsStringizedByteStream()), "Exception string portion of log string should be Stringized byte stream of exception");
                });
            });
            
        }

        [Test]
        public void ConvertUCLIDExceptionGeneratedSringWithInner()
        {
            ExtractException testException = null;
            Assert.DoesNotThrow(() =>
            {
                testException = ExtractException.LoadFromByteStream(TestStringizedExceptionWithInner);
            });
            Assert.That(testException?.InnerException, Is.Not.Null);
            Assert.That(testException.EliCode.Equals("ELI32412"));
            Assert.That(testException?.InnerException is ExtractException);
            var inner = testException?.InnerException as ExtractException;
            Assert.IsTrue(inner?.EliCode.Equals("ELI21061"));
        }

        [Test, Category("Automated"), Category("Exceptions")]
        public void LoadFromByteStreamTest()
        {
            // This is a simple case
            Assert.DoesNotThrow(() =>ExtractException.LoadFromByteStream(TestString), "Stringized Exception should be convertible");

            ExtractException convertedException = ExtractException.LoadFromByteStream(TestString);
            Assert.AreEqual( "ELI28774", convertedException.EliCode, "Loaded ELICode is incorrect");
            Assert.AreEqual("Application trace: FAM Service stopped. (PerformanceProcess)", convertedException.Message, "Loaded message is incorrect");
            Assert.AreEqual(2, convertedException.Data.Count, "Should be 2 Data items");

            Assert.AreEqual(convertedException.Data["Threads Stopped"], "True");
            Assert.AreEqual(convertedException.Data["CatchID"], "ELI02154");
        }
        
        [Test]
        public void RenameLogFile()
        {
            // get a temporary file name;
            using var uexFile = new TemporaryFile("uex", false);
            var testException = new ExtractException("ELITest", "Message", null);
            
            testException.Log(uexFile.FileName);
            var NewFileName = string.Empty;
            try
            {
                NewFileName = testException.RenameLogFile(uexFile.FileName, false, "", false);
            }
            finally
            {
                Assert.IsTrue(File.Exists(uexFile.FileName));
                var lines = File.ReadAllLines(uexFile.FileName);
                Assert.AreEqual(1, lines.Length);
                var savedException = ExtractException.LoadFromByteStream(lines.First().Split(',').Last());
                Assert.IsTrue("ELI53578".Equals(savedException.EliCode));
                
                Assert.IsTrue(File.Exists(NewFileName));
                if (!string.IsNullOrWhiteSpace(NewFileName))
                {
                    File.Delete(NewFileName);
                }
            }
            Assert.DoesNotThrow(()=>testException.RenameLogFile("", false, "", false));
            Assert.Throws<ExtractException>(() => testException.RenameLogFile("", false, "", true));

        }
        
        [Test]
        public void RenameLogFileUserRenamed()
        {
            // get a temporary file name;
            using var uexFile = new TemporaryFile("uex", false);
            var testException = new ExtractException("ELITest", "Message", null);

            testException.Log(uexFile.FileName);
            var NewFileName = string.Empty;
            try
            {
                NewFileName = testException.RenameLogFile(uexFile.FileName, true, "User renamed", false);
            }
            finally
            {
                Assert.IsTrue(File.Exists(uexFile.FileName));
                var lines = File.ReadAllLines(uexFile.FileName);
                Assert.AreEqual(1, lines.Length);
                var savedException = ExtractException.LoadFromByteStream(lines.First().Split(',').Last());
                Assert.IsTrue("ELI53579".Equals(savedException.EliCode));

                Assert.IsTrue(File.Exists(NewFileName));
                if (!string.IsNullOrWhiteSpace(NewFileName))
                {
                    File.Delete(NewFileName);
                }
            }
            Assert.DoesNotThrow(() => testException.RenameLogFile("", true, "User renamed", false));
            Assert.Throws<ExtractException>(() => testException.RenameLogFile("", true, "User renamed", true));
        }

        internal void UseDefaultUEX(Action<string> action)
        {
            string logPath = GetFolderPath(SpecialFolder.CommonApplicationData);
            string fileName = Path.Combine(logPath, "Extract Systems\\LogFiles\\ExtractException.uex");
            string tempRename = fileName + ".tmp";
            bool fileRenamed = false;

            try
            {
                if (File.Exists(tempRename))
                    File.Delete(tempRename);
                if (File.Exists(fileName))
                {
                    File.Move(fileName, tempRename);
                    fileRenamed = true; ;
                }

                action(fileName);
            }
            finally
            {
                if (fileRenamed)
                {
                    File.Delete(fileName);
                    File.Move(tempRename, fileName);
                }
            }
        }

        internal void UseTemporyUEX(Action<string> action)
        {
            string fileName = Path.GetTempFileName();
            FileInfo fileInfo = new FileInfo(fileName);
            fileInfo.Attributes = FileAttributes.Temporary;

            try
            {
                if (File.Exists(fileName))
                {
                    File.Delete(fileName);
                }

                action(fileName);
            }
            finally
            {
                File.Delete(fileName);
            }
        }
    }
}