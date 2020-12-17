using Extract.Testing.Utilities;
using NUnit.Framework;
using System;
using System.IO;
using System.Linq;

namespace Extract.Utilities.Test
{
    /// <summary>
    /// Test SystemMethods class
    /// </summary>
    [TestFixture]
    [Category("FileSystemMethods")]
    class TestFileSystemMethods
    {
        /// <summary>
        /// Initializes the test fixture for testing these methods
        /// </summary>
        [OneTimeSetUp]
        public static void Initialize()
        {
            GeneralMethods.TestSetup();
        }

        /// <summary>
        /// Performs post test execution cleanup.
        /// </summary>
        [OneTimeTearDown]
        public static void Cleanup()
        {
        }

        #region GetRandomFileNameWithoutExtension

        /// <summary>
        /// Confirm that filename does not have an extension
        /// </summary>
        [Test, Category("Automated")]
        public static void RandomFileNameHasNoExtension()
        {
            StringAssert.DoesNotContain(".", FileSystemMethods.GetRandomFileNameWithoutExtension());
        }

        /// <summary>
        /// Confirm that unique filenames are generated
        /// </summary>
        [Test, Category("Automated")]
        public static void RandomFileNameIsRandom()
        {
            var count = 10000;
            var names =
                Enumerable.Range(1, count)
                .AsParallel()
                .Select(i => FileSystemMethods.GetRandomFileNameWithoutExtension())
                .ToList();
            Assert.AreEqual(count, names.Distinct().Count());
        }

        #endregion

        #region GetTemporaryFolder

        [Test, Category("Automated")]
        public static void GetTemporaryFolder_DefaultParams()
        {
            DirectoryInfo tempFolder = null;
            try
            {
                tempFolder = FileSystemMethods.GetTemporaryFolder();
                Assert.That(tempFolder.Exists);

                // Make sure there is no extension (could be suspicious-looking)
                StringAssert.DoesNotContain(".", tempFolder.Name);

                StringAssert.AreEqualIgnoringCase(Path.GetTempPath(), tempFolder.Parent.FullName + @"\");
            }
            finally
            {
                if (tempFolder.Exists)
                {
                    tempFolder.Delete();
                }
            }
        }

        [Test, Category("Automated")]
        public static void GetTemporaryFolder_NonExistentParent()
        {
            DirectoryInfo parentFolder = null;
            try
            {
                parentFolder = new DirectoryInfo(Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString()));
                // This will fail because parent dir doesn't exist
                Assert.Throws<ExtractException>(() => FileSystemMethods.GetTemporaryFolder(parentFolder.FullName));

                // The parent dir still doesn't exist
                Assert.That(!parentFolder.Exists);

                // Will succeed with createParent = true
                var tempFolder = FileSystemMethods.GetTemporaryFolder(parentFolder.FullName, createParentFolder: true);
                Assert.That(tempFolder.Exists);
            }
            finally
            {
                if (parentFolder.Exists)
                {
                    parentFolder.Delete(true);
                }
            }
        }

        [Test, Category("Automated")]
        public static void GetTemporaryFolder_ExistentParent()
        {
            DirectoryInfo parentFolder = null;
            try
            {
                parentFolder = new DirectoryInfo(Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString()));
                parentFolder.Create();
                var tempFolder = FileSystemMethods.GetTemporaryFolder(parentFolder.FullName);
                Assert.That(tempFolder.Exists);
                tempFolder.Delete();

                // createParent flag does nothing if dir already exists
                tempFolder = FileSystemMethods.GetTemporaryFolder(parentFolder.FullName, createParentFolder: true);
                Assert.That(tempFolder.Exists);
            }
            finally
            {
                if (parentFolder.Exists)
                {
                    parentFolder.Delete(true);
                }
            }
        }

        #endregion

        #region GetTemporaryFolderName

        [Test, Category("Automated")]
        public static void GetTemporaryFolderName()
        {
            string parentFolder = null;
            try
            {
                parentFolder = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
                // This will fail because parent dir doesn't exist
                Assert.Throws<ExtractException>(() => FileSystemMethods.GetTemporaryFolderName(parentFolder));

                // The parent dir still doesn't exist
                Assert.That(!Directory.Exists(parentFolder));

                Directory.CreateDirectory(parentFolder);
                var tempFolder = FileSystemMethods.GetTemporaryFolderName(parentFolder);
                Assert.That(Directory.Exists(tempFolder));
            }
            finally
            {
                if (Directory.Exists(parentFolder))
                {
                    Directory.Delete(parentFolder, true);
                }
            }
        }

        #endregion
    }
}
