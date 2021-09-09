using Extract.Testing.Utilities;
using Extract.Utilities.FSharp;
using Microsoft.FSharp.Core;
using NUnit.Framework;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Extract.Utilities.Test
{
    [TestFixture]
    [Category("FunctionLoader")]
    [Parallelizable(ParallelScope.All)]
    public class TestFunctionLoader
    {
        [OneTimeSetUp]
        public static void Initialize()
        {
            GeneralMethods.TestSetup();
        }

        /// Confirm that functions can be loaded from multiple threads at once
        [Test, Category("Automated")]
        public static void LoadFunctionMultithreaded()
        {
            // Make a script file where the name is a valid module name in F#
            string scriptFolder = Path.GetTempPath();
            using TemporaryFile scriptFile = new(scriptFolder, "TestFunctionLoader.LoadFunctionMultithreaded.fsx", null, false);
            string scriptPath = scriptFile.FileName;

            // Write a function
            File.WriteAllText(scriptPath, "let addOne x = x + 1");

            FSharpFunc<int, int> creator()
            {
                return FunctionLoader.LoadFunction<int>(scriptPath, "addOne", true);
            }

            // Evaluate the script many times at once to confirm no errors or hangs
            List<FSharpFunc<int, int>> funs = Enumerable.Range(0, 10)
                .AsParallel()
                .Select(_ => creator())
                .ToList();

            // Confirm that the functions work as expected
            List<int> results = funs
                .AsParallel()
                .Select((fun, i) => fun.Invoke(i))
                .ToList();

            List<int> exp = Enumerable.Range(1, results.Count).ToList();
            CollectionAssert.AreEqual(exp, results);
        }

        /// Confirm that multiple functions can be loaded from multiple threads at once
        [Test, Category("Automated")]
        public static void LoadTwoFunctionsMultithreaded()
        {
            // Make a script file where the name is a valid module name in F#
            string scriptFolder = Path.GetTempPath();
            using TemporaryFile scriptFile = new(scriptFolder, "TestFunctionLoader.LoadTwoFunctionsMultithreaded.fsx", null, false);
            string scriptPath = scriptFile.FileName;

            // Write functions
            File.WriteAllText(scriptPath, "let addOne x = x + 1\nlet addTwo x = x + 2");

            FSharpFunc<int, int>[] creator()
            {
                return FunctionLoader.LoadFunctions<int>(scriptPath, new[] { "addOne", "addTwo" }, true);
            }

            // Evaluate the script many times at once to confirm no errors or hangs
            List<FSharpFunc<int, int>[]> funs = Enumerable.Range(0, 10)
                .AsParallel()
                .Select(_ => creator())
                .ToList();

            // Confirm that the functions work as expected
            List<int[]> results = funs
                .AsParallel()
                .Select((funs, i) => funs.Select(fun => fun.Invoke(i)).ToArray())
                .ToList();

            List<int[]> exp = Enumerable.Range(0, results.Count)
                .Select(i => new[] { i + 1, i + 2 })
                .ToList();
            CollectionAssert.AreEqual(exp, results);
        }

        /// Confirm that no exception is thrown for null includeDirectories param
        [Test, Category("Automated")]
        public static void LoadFunctionNullIncludeDirectories()
        {
            // Make a script file where the name is a valid module name in F#
            string scriptFolder = Path.GetTempPath();
            using TemporaryFile scriptFile = new(scriptFolder, "TestFunctionLoader.LoadFunctionNullIncludeDirectories.fsx", null, false);
            string scriptPath = scriptFile.FileName;

            // Write a function
            File.WriteAllText(scriptPath, "let addOne x = x + 1");

            // Confirm evaluation works without error
            FSharpFunc<int, int> fun = FunctionLoader.LoadFunction<int>(scriptPath, "addOne", true, null);
        }

        /// Confirm that no exception is thrown for null includeDirectories param
        [Test, Category("Automated")]
        public static void LoadTwoFunctionsNullIncludeDirectories()
        {
            // Make a script file where the name is a valid module name in F#
            string scriptFolder = Path.GetTempPath();
            using TemporaryFile scriptFile = new(scriptFolder, "TestFunctionLoader.LoadTwoFunctionsNullIncludeDirectories.fsx", null, false);
            string scriptPath = scriptFile.FileName;

            // Write functions
            File.WriteAllText(scriptPath, "let addOne x = x + 1\nlet addTwo x = x + 2");

            // Confirm evaluation works without error
            FSharpFunc<int, int>[] funs = FunctionLoader.LoadFunctions<int>(scriptPath, new[] { "addOne", "addTwo" }, true, null);
        }
    }
}
