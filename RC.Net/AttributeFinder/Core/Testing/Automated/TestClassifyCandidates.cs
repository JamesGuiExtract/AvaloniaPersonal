using Extract.AttributeFinder.MLNet.ClassifyCandidates;
using Extract.Testing.Utilities;
using Extract.Utilities;
using Extract.Utilities.FSharp;
using Microsoft.FSharp.Core;
using Microsoft.ML;
using Moq;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Extract.AttributeFinder.Test
{
    [Category("ClassifyCandidates"), Category("Automated")]
    public class TestClassifyCandidates
    {
        private static TestFileManager<TestClassifyCandidates> _testFiles;

        [OneTimeSetUp]
        public static void Setup()
        {
            GeneralMethods.TestSetup();

            _testFiles = new();
        }

        [OneTimeTearDown]
        public static void FinalCleanup()
        {
            _testFiles.Dispose();
        }

        /// Confirm that the client/server code functions properly under heavy loads
        /// Ideally, one server process will get created to handle all the clients
        [Test]
        [Parallelizable(ParallelScope.All)]
        public static void StressServerCreationLogic([Values(1, 8, 16)] int simultaneousRequests)
        {
            // Arrange

            using TemporaryFile modelMock = new(false);
            Mock<INamedPipeServer> serverMock = new();

            // Configure the start action to run a server that creates and responds to requests on a named pipe
            serverMock.Setup(x =>
                x.Start(It.IsAny<string>()))
                .Callback<string>(pipeName =>
                {
                    Task.Run(async () =>
                    {
                        await Task.Delay(TimeSpan.FromSeconds(10));
                        NamedPipe.listenForRequests(pipeName,
                            FuncConvert.FromAction<DTO.PredictionRequest>(x => { }));
                    });
                });

            var startAction = FuncConvert.FromAction<string>(serverMock.Object.Start);

            // I tried to test this more precisely but the amount of time needed depends on the hardware and
            // how much unrelated work the CPU is doing so I gave up. Just allow a couple minutes to account
            // for possible overhead
            var timeToWaitForServer = TimeSpan.FromMinutes(2);

            Predict.ServerRequest serverRequest = new(
                modelPath: modelMock.FileName,
                serverName: "Extract.TestServer",
                serverCreator: startAction,
                timeToWaitForServerCreation: timeToWaitForServer,
                predictionRequest: new("input", "output"));

            // Act

            // Simultaneously try to send many messages
            Enumerable.Range(1, simultaneousRequests)
                .AsParallel()
                .ForAll(_ => Predict.sendRequestToServer(serverRequest));

            // Assert

            // Verify that the server was started exactly one time 
            serverMock.Verify(x => x.Start(It.IsAny<string>()), Times.Once);
        }

        /// Confirm that exceptions from the server are readable by the client
        [Test]
        [Parallelizable(ParallelScope.Self)]
        public static void ExceptionHandling()
        {
            // Arrange

            string modelFile = _testFiles.GetFile("Resources.MLNet.MLModel_Kofax_MICR_Classifier.zip");
            DTO.PredictionRequest predictionRequest = new("input", "output");

            // Act

            Exception actualException = null;
            try
            {
                Predict.predictWithMLNetQueue(modelFile, predictionRequest);
            }
            catch (Exception ex)
            {
                actualException = ex;
            }

            // Assert

            Assert.IsInstanceOf<ArgumentOutOfRangeException>(actualException);
            Assert.AreEqual("File does not exist at path: input\r\nParameter name: path", actualException.Message);
            Assert.That(Regex.IsMatch(actualException.ToString(),
                String.Join(@"[\S\s]*",
                    new string[] {
                        @"at Microsoft\.ML\.BinaryLoaderSaverCatalog\.LoadFromBinary",
                        @"MLNetQueue\\Code\\Predict\.fs",
                        @"Utilities\\FSharp\\Core\\Code\\NamedPipe\.fs",
                        @"TestClassifyCandidates\.cs"
                    })));
        }

        /// Confirm that predictions can be made
        [Test]
        [Parallelizable(ParallelScope.Self)]
        public static void TestPredicting()
        {
            // Arrange

            string modelFile = _testFiles.GetFile("Resources.MLNet.MLModel_Kofax_MICR_Classifier.zip");
            string inputDataFile = _testFiles.GetFile("Resources.MLNet.inputData.bin");
            using TemporaryFile outputDataFile = new(false);
            DTO.PredictionRequest predictionRequest = new(inputDataFile, outputDataFile.FileName);

            // Confirm the number of examples in the input data
            const int expectedNumExamples = 18;
            MLContext mlContext = new();
            using IDisposable inputData = (IDisposable)mlContext.Data.LoadFromBinary(inputDataFile);
            Assume.That(((IDataView)inputData).GetRowCount(), Is.EqualTo(expectedNumExamples));

            // Act

            Predict.predictWithMLNetQueue(modelFile, predictionRequest);

            // Assert

            using IDisposable outputData = (IDisposable)mlContext.Data.LoadFromBinary(outputDataFile.FileName);
            List<Prediction> predictions = mlContext.Data.CreateEnumerable<Prediction>((IDataView)outputData, false).ToList();

            // Output has the same number of examples as the input
            Assert.AreEqual(expectedNumExamples, predictions.Count);

            // One item is predicted to be true
            Assert.AreEqual(1, predictions.Where(p => p.Probability > 0.5).Count());
        }
    }

    public interface INamedPipeServer
    {
        public void Start(string pipeName);
    }

    public class Prediction
    {
        public float Probability { get; set; }
        public float Score { get; set; }
    }
}
