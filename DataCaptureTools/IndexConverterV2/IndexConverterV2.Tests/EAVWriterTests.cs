using IndexConverterV2.Models;
using IndexConverterV2.ViewModels;
using IndexConverterV2.Views;
using Moq;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IndexConverterV2.Tests
{
    [TestFixture]
    public class EAVWriterTests
    {
        //System Under Test
        EAVWriter sut;

        [SetUp]
        public void Init()
        {
            sut = new();
        }


        [Test]
        public void StripQualifiersTest()
        {
            string test = "\"test\"";
            test = sut.StripQualifiers(test, '"');

            Assert.Multiple(() =>
            {
                //Quotes should be gone
                Assert.That(test, Is.EqualTo("test"));
                test = sut.StripQualifiers(test, '"');
                //No quotes should have been present, string should be unmodified
                Assert.That(test, Is.EqualTo("test"));
            });
        }

        [Test]
        public void ReplacePercentsTest()
        {
            string[] inputs = new string[3];
            inputs[0] = "doesn't matter";
            inputs[1] = "please ignore";
            inputs[2] = "GET THIS ONE";
            string result = sut.ReplacePercents("%3", inputs, '"');
            Assert.Multiple(() =>
            {
                Assert.That(result, Is.EqualTo("GET THIS ONE"));
                result = sut.ReplacePercents("123%3after", inputs, '"');
                Assert.That(result, Is.EqualTo("123GET THIS ONEafter"));
                result = sut.ReplacePercents("%3blah%3", inputs, '"');
                Assert.That(result, Is.EqualTo("GET THIS ONEblahGET THIS ONE"));
            });
        }

    }
}
