using Extract.Testing.Utilities;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Extract.Utilities.Test
{
    [TestFixture]
    [Category("Automated")]
    [Category("Retry")]
    public class TestRetry
    {
        [OneTimeSetUp]
        public static void Setup()
        {
            GeneralMethods.TestSetup();
        }

        class CustomException : Exception { }

        // Confirm that Retry happens when the specified exception is thrown
        [Test]
        public void Test_RetryForCustomException()
        {
            // Arrange
            Queue<Func<int>> results = new(new[]
            {
                () => throw new CustomException(),
                () => 1
            });

            Retry<CustomException> retry = new(1, 0);

            // Act
            int result = retry.DoRetry(() => results.Dequeue()());

            // Assert
            Assert.AreEqual(1, result);
        }

        // Confirm that retry can be cancelled
        [Test]
        public void Test_RetryCancellation()
        {
            // Arrange
            var cancelTokenSource = new CancellationTokenSource();
            cancelTokenSource.CancelAfter(TimeSpan.FromSeconds(2));
            Retry<Exception> retry = new(10, 1000, cancelTokenSource.Token.WaitHandle);
            ExtractException result = null;

            // Act
            try
            {
                retry.DoRetry(() => throw new Exception());
            }
            catch (ExtractException ex)
            {
                result = ex;
            }

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual("Application Trace: Event signaled while waiting to retry.", result.Message);
        }
    }
}
