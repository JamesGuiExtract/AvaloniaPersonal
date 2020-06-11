using Extract.Testing.Utilities;
using Extract.Utilities;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Reflection;

namespace Extract.SetOperations.Test
{
    /// <summary>
    /// Set of test methods for testing the SetOperations utility.
    /// </summary>
    [TestFixture]
    [Category("Extract SetOperations")]
    public class TestSetOperations
    {
        static string _SET_OPERATIONS_PATH = string.Empty;

        /// <summary>
        /// Setup code that must be run before any tests can execute.
        /// </summary>
        [OneTimeSetUp]
        public static void Setup()
        {
            GeneralMethods.TestSetup();

            // Find the path to SetOperations
            var assembly = Assembly.GetAssembly(typeof(TemporaryFile));
            _SET_OPERATIONS_PATH = Path.Combine(new Uri(Path.GetDirectoryName(assembly.CodeBase)).LocalPath,
                "SetOperations.exe");
        }

        #region Test Methods

        /// <summary>
        /// Tests the case insensitive union operation.
        /// </summary>
        /// <param name="listA">The list A.</param>
        /// <param name="listB">The list B.</param>
        [Test, Category("Automated")]
        public static void TestUnionCaseInsensitive([Values(
            new string[] { "red", "gReEn", "", "Blue", "1", "42", "     " },
            new string[] { "green", "blue", "Yellow", "", "\r\n", " " },
            new string[] { "1", "", "\r\n", "blue", "", "42", "165", "RED" })] string[] listA,
            [Values(new string[] { "red", "gReEn", "", "Blue", "1", "42", "     " },
            new string[] { "green", "blue", "Yellow", "", "\r\n", " " },
            new string[] { "1", "", "\r\n", "blue", "", "42", "165", "RED" })] string[] listB)
        {
            using (TemporaryFile fileA = new TemporaryFile(".lst", false),
                fileB = new TemporaryFile(".lst", false),
                fileResult = new TemporaryFile(".lst", false))
            {
                File.WriteAllLines(fileA.FileName, listA);
                File.WriteAllLines(fileB.FileName, listB);

                var setA = FillSetFromList(listA, StringComparer.OrdinalIgnoreCase);
                var setB = FillSetFromList(listB, StringComparer.OrdinalIgnoreCase);
                setA.UnionWith(setB);

                var info = new ProcessStartInfo(_SET_OPERATIONS_PATH,
                    string.Concat("\"", fileA.FileName, "\" Union \"",
                    fileB.FileName, "\" \"", fileResult.FileName, "\""));
                RunProcess(info);

                var setResult = new HashSet<string>(File.ReadAllLines(fileResult.FileName),
                    StringComparer.OrdinalIgnoreCase);

                Assert.That(setResult.SetEquals(setA));
            }
        }

        /// <summary>
        /// Tests the case sensitive union operation.
        /// </summary>
        /// <param name="listA">The list A.</param>
        /// <param name="listB">The list B.</param>
        [Test, Category("Automated")]
        public static void TestUnionCaseSensitive([Values(
            new string[] { "red", "gReEn", "", "Blue", "1", "42", "     " },
            new string[] { "green", "blue", "Yellow", "", "\r\n", " " },
            new string[] { "1", "", "\r\n", "blue", "", "42", "165", "RED" })] string[] listA,
            [Values(new string[] { "red", "gReEn", "", "Blue", "1", "42", "     " },
            new string[] { "green", "blue", "Yellow", "", "\r\n", " " },
            new string[] { "1", "", "\r\n", "blue", "", "42", "165", "RED" })] string[] listB)
        {
            using (TemporaryFile fileA = new TemporaryFile(".lst", false),
                fileB = new TemporaryFile(".lst", false),
                fileResult = new TemporaryFile(".lst", false))
            {
                File.WriteAllLines(fileA.FileName, listA);
                File.WriteAllLines(fileB.FileName, listB);

                var setA = FillSetFromList(listA, StringComparer.Ordinal);
                var setB = FillSetFromList(listB, StringComparer.Ordinal);
                setA.UnionWith(setB);

                var info = new ProcessStartInfo(_SET_OPERATIONS_PATH,
                    string.Concat("\"", fileA.FileName, "\" Union \"",
                    fileB.FileName, "\" \"", fileResult.FileName, "\" /c"));
                RunProcess(info);

                var setResult = new HashSet<string>(File.ReadAllLines(fileResult.FileName),
                    StringComparer.Ordinal);

                Assert.That(setResult.SetEquals(setA));
            }
        }

        /// <summary>
        /// Tests the case insensitive intersection operation.
        /// </summary>
        /// <param name="listA">The list A.</param>
        /// <param name="listB">The list B.</param>
        [Test, Category("Automated")]
        public static void TestIntersectCaseInsensitive([Values(
            new string[] { "red", "gReEn", "", "Blue", "1", "42", "     " },
            new string[] { "green", "blue", "Yellow", "", "\r\n", " " },
            new string[] { "1", "", "\r\n", "blue", "", "42", "165", "RED" })] string[] listA,
            [Values(new string[] { "red", "gReEn", "", "Blue", "1", "42", "     " },
            new string[] { "green", "blue", "Yellow", "", "\r\n", " " },
            new string[] { "1", "", "\r\n", "blue", "", "42", "165", "RED" })] string[] listB)
        {
            using (TemporaryFile fileA = new TemporaryFile(".lst", false),
                fileB = new TemporaryFile(".lst", false),
                fileResult = new TemporaryFile(".lst", false))
            {
                File.WriteAllLines(fileA.FileName, listA);
                File.WriteAllLines(fileB.FileName, listB);

                var setA = FillSetFromList(listA, StringComparer.OrdinalIgnoreCase);
                var setB = FillSetFromList(listB, StringComparer.OrdinalIgnoreCase);
                setA.IntersectWith(setB);

                var info = new ProcessStartInfo(_SET_OPERATIONS_PATH,
                    string.Concat("\"", fileA.FileName, "\" Intersect \"",
                    fileB.FileName, "\" \"", fileResult.FileName, "\""));
                RunProcess(info);

                var setResult = new HashSet<string>(File.ReadAllLines(fileResult.FileName),
                    StringComparer.OrdinalIgnoreCase);

                Assert.That(setResult.SetEquals(setA));
            }
        }

        /// <summary>
        /// Tests the case sensitive intersect operation.
        /// </summary>
        /// <param name="listA">The list A.</param>
        /// <param name="listB">The list B.</param>
        [Test, Category("Automated")]
        public static void TestIntersectCaseSensitive([Values(
            new string[] { "red", "gReEn", "", "Blue", "1", "42", "     " },
            new string[] { "green", "blue", "Yellow", "", "\r\n", " " },
            new string[] { "1", "", "\r\n", "blue", "", "42", "165", "RED" })] string[] listA,
            [Values(new string[] { "red", "gReEn", "", "Blue", "1", "42", "     " },
            new string[] { "green", "blue", "Yellow", "", "\r\n", " " },
            new string[] { "1", "", "\r\n", "blue", "", "42", "165", "RED" })] string[] listB)
        {
            using (TemporaryFile fileA = new TemporaryFile(".lst", false),
                fileB = new TemporaryFile(".lst", false),
                fileResult = new TemporaryFile(".lst", false))
            {
                File.WriteAllLines(fileA.FileName, listA);
                File.WriteAllLines(fileB.FileName, listB);

                var setA = FillSetFromList(listA, StringComparer.Ordinal);
                var setB = FillSetFromList(listB, StringComparer.Ordinal);
                setA.IntersectWith(setB);

                var info = new ProcessStartInfo(_SET_OPERATIONS_PATH,
                    string.Concat("\"", fileA.FileName, "\" Intersect \"",
                    fileB.FileName, "\" \"", fileResult.FileName, "\" /c"));
                RunProcess(info);

                var setResult = new HashSet<string>(File.ReadAllLines(fileResult.FileName),
                    StringComparer.Ordinal);

                Assert.That(setResult.SetEquals(setA));
            }
        }

        /// <summary>
        /// Tests the case insensitive complement operation.
        /// </summary>
        /// <param name="listA">The list A.</param>
        /// <param name="listB">The list B.</param>
        [Test, Category("Automated")]
        public static void TestComplementCaseInsensitive([Values(
            new string[] { "red", "gReEn", "", "Blue", "1", "42", "     " },
            new string[] { "green", "blue", "Yellow", "", "\r\n", " " },
            new string[] { "1", "", "\r\n", "blue", "", "42", "165", "RED" })] string[] listA,
            [Values(new string[] { "red", "gReEn", "", "Blue", "1", "42", "     " },
            new string[] { "green", "blue", "Yellow", "", "\r\n", " " },
            new string[] { "1", "", "\r\n", "blue", "", "42", "165", "RED" })] string[] listB)
        {
            using (TemporaryFile fileA = new TemporaryFile(".lst", false),
                fileB = new TemporaryFile(".lst", false),
                fileResult = new TemporaryFile(".lst", false))
            {
                File.WriteAllLines(fileA.FileName, listA);
                File.WriteAllLines(fileB.FileName, listB);

                var setA = FillSetFromList(listA, StringComparer.OrdinalIgnoreCase);
                var setB = FillSetFromList(listB, StringComparer.OrdinalIgnoreCase);
                setA.ExceptWith(setB);

                var info = new ProcessStartInfo(_SET_OPERATIONS_PATH,
                    string.Concat("\"", fileA.FileName, "\" Complement \"",
                    fileB.FileName, "\" \"", fileResult.FileName, "\""));
                RunProcess(info);

                var setResult = new HashSet<string>(File.ReadAllLines(fileResult.FileName),
                    StringComparer.OrdinalIgnoreCase);

                Assert.That(setResult.SetEquals(setA));
            }
        }

        /// <summary>
        /// Tests the case sensitive complement operation.
        /// </summary>
        /// <param name="listA">The list A.</param>
        /// <param name="listB">The list B.</param>
        [Test, Category("Automated")]
        public static void TestComplementCaseSensitive([Values(
            new string[] { "red", "gReEn", "", "Blue", "1", "42", "     " },
            new string[] { "green", "blue", "Yellow", "", "\r\n", " " },
            new string[] { "1", "", "\r\n", "blue", "", "42", "165", "RED" })] string[] listA,
            [Values(new string[] { "red", "gReEn", "", "Blue", "1", "42", "     " },
            new string[] { "green", "blue", "Yellow", "", "\r\n", " " },
            new string[] { "1", "", "\r\n", "blue", "", "42", "165", "RED" })] string[] listB)
        {
            using (TemporaryFile fileA = new TemporaryFile(".lst", false),
                fileB = new TemporaryFile(".lst", false),
                fileResult = new TemporaryFile(".lst", false))
            {
                File.WriteAllLines(fileA.FileName, listA);
                File.WriteAllLines(fileB.FileName, listB);

                var setA = FillSetFromList(listA, StringComparer.Ordinal);
                var setB = FillSetFromList(listB, StringComparer.Ordinal);
                setA.ExceptWith(setB);

                var info = new ProcessStartInfo(_SET_OPERATIONS_PATH,
                    string.Concat("\"", fileA.FileName, "\" Complement \"",
                    fileB.FileName, "\" \"", fileResult.FileName, "\" /c"));
                RunProcess(info);

                var setResult = new HashSet<string>(File.ReadAllLines(fileResult.FileName),
                    StringComparer.Ordinal);

                Assert.That(setResult.SetEquals(setA));
            }
        }

        /// <summary>
        /// Tests that an exception is generated from a list that contains duplicates
        /// case insensitively.
        /// </summary>
        [Test, Category("Automated")]
        public static void TestDuplicatesThrowExceptionCaseInsensitive()
        {
            using (TemporaryFile fileA = new TemporaryFile(".lst", false),
                fileB = new TemporaryFile(".lst", false),
                fileResult = new TemporaryFile(".lst", false),
                fileException = new TemporaryFile(".uex", false))
            {
                var listA = new List<string>(new string[] { "red", "gReEn", "green", 
                    "Blue", "1", "42", "     " });
                var listB = new List<string>(new string[] { "green", "blue",
                    "Yellow", "", "\r\n", " " });
                File.WriteAllLines(fileA.FileName, listA);
                File.WriteAllLines(fileB.FileName, listB);

                var info = new ProcessStartInfo(_SET_OPERATIONS_PATH,
                    string.Concat("\"", fileA.FileName, "\" Union \"",
                    fileB.FileName, "\" \"", fileResult.FileName, "\" /ef \"",
                    fileException.FileName, "\""));
                RunProcess(info);

                var fileInfo = new FileInfo(fileException.FileName);
                string eliCode = "ELI31797";
                var ee = ExtractException.LoadFromFile(eliCode, fileException.FileName);
                Assert.That(fileInfo.Length > 0 && ee != null);
            }
        }

        /// <summary>
        /// Tests that an exception is generated from a list that contains duplicates
        /// case sensitively.
        /// </summary>
        [Test, Category("Automated")]
        public static void TestDuplicatesThrowExceptionCaseSensitive()
        {
            using (TemporaryFile fileA = new TemporaryFile(".lst", false),
                fileB = new TemporaryFile(".lst", false),
                fileResult = new TemporaryFile(".lst", false),
                fileException = new TemporaryFile(".uex", false))
            {
                var listA = new List<string>(new string[] { "red", "gReEn", "green", 
                    "Blue", "1", "42", "     " });
                var listB = new List<string>(new string[] { "green", "blue",
                    "Yellow", "", "\r\n", " ", "Yellow" });
                File.WriteAllLines(fileA.FileName, listA);
                File.WriteAllLines(fileB.FileName, listB);

                var info = new ProcessStartInfo(_SET_OPERATIONS_PATH,
                    string.Concat("\"", fileA.FileName, "\" Union \"",
                    fileB.FileName, "\" \"", fileResult.FileName, "\" /c /ef \"",
                    fileException.FileName, "\""));
                RunProcess(info);

                var fileInfo = new FileInfo(fileException.FileName);
                string eliCode = "ELI31798";
                var ee = ExtractException.LoadFromFile(eliCode, fileException.FileName);
                Assert.That(fileInfo.Length > 0 && ee != null);
            }
        }

        /// <summary>
        /// Tests that an exception is not generated from a list that contains duplicates
        /// case insensitively when the ignore duplicates flag is specified.
        /// </summary>
        [Test, Category("Automated")]
        public static void TestIgnoreDuplicatesDoNotThrowExceptionCaseInsensitive()
        {
            using (TemporaryFile fileA = new TemporaryFile(".lst", false),
                fileB = new TemporaryFile(".lst", false),
                fileResult = new TemporaryFile(".lst", false),
                fileException = new TemporaryFile(".uex", false))
            {
                var listA = new List<string>(new string[] { "red", "gReEn", "green", 
                    "Blue", "1", "42", "     " });
                var listB = new List<string>(new string[] { "green", "blue",
                    "Yellow", "", "\r\n", " " });
                File.WriteAllLines(fileA.FileName, listA);
                File.WriteAllLines(fileB.FileName, listB);

                var info = new ProcessStartInfo(_SET_OPERATIONS_PATH,
                    string.Concat("\"", fileA.FileName, "\" Union \"",
                    fileB.FileName, "\" \"", fileResult.FileName, "\" /i /ef \"",
                    fileException.FileName, "\""));
                RunProcess(info);

                var fileInfo = new FileInfo(fileException.FileName);
                Assert.That(fileInfo.Length == 0);
            }
        }

        /// <summary>
        /// Tests that an exception is not generated from a list that contains duplicates
        /// case sensitively when the ignore duplicates flag is specified.
        /// </summary>
        [Test, Category("Automated")]
        public static void TestIgnoreDuplicatesDoNotThrowExceptionCaseSensitive()
        {
            using (TemporaryFile fileA = new TemporaryFile(".lst", false),
                fileB = new TemporaryFile(".lst", false),
                fileResult = new TemporaryFile(".lst", false),
                fileException = new TemporaryFile(".uex", false))
            {
                var listA = new List<string>(new string[] { "red", "gReEn", "green", 
                    "Blue", "1", "42", "     " });
                var listB = new List<string>(new string[] { "green", "blue",
                    "Yellow", "", "\r\n", " ", "Yellow" });
                File.WriteAllLines(fileA.FileName, listA);
                File.WriteAllLines(fileB.FileName, listB);


                var info = new ProcessStartInfo(_SET_OPERATIONS_PATH,
                    string.Concat("\"", fileA.FileName, "\" Union \"",
                    fileB.FileName, "\" \"", fileResult.FileName, "\" /c /i /ef \"",
                    fileException.FileName, "\""));
                RunProcess(info);

                var fileInfo = new FileInfo(fileException.FileName);
                Assert.That(fileInfo.Length == 0);
            }
        }

        #endregion Test Methods

        #region Helper Methods

        /// <summary>
        /// Fills the set from list.
        /// </summary>
        /// <param name="list">The list.</param>
        /// <param name="comparer">The comparer.</param>
        /// <returns></returns>
        static HashSet<string> FillSetFromList(IEnumerable<string> list,
            StringComparer comparer)
        {
            var set = new HashSet<string>(comparer);
            foreach (var line in list)
            {
                if (!string.IsNullOrWhiteSpace(line))
                {
                    set.Add(line);
                }
            }

            return set;
        }

        static void RunProcess(ProcessStartInfo info)
        {
            info.UseShellExecute = false;
            info.CreateNoWindow = true;
            using (var process = new Process())
            {
                process.StartInfo = info;
                process.Start();
                process.WaitForExit();
            }
        }


        #endregion Helper Methods
    }
}
