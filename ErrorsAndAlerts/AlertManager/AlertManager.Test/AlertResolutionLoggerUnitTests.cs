﻿using AlertManager.Interfaces;
using AlertManager.Models.AllDataClasses;
using AlertManager.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Extract.ErrorsAndAlerts.AlertManager.Test
{
    [TestFixture]
    internal class AlertResolutionLoggerUnitTests
    {
        private IAlertActionLogger logger = new AlertActionLogger();

        [SetUp]
        public void Init()
        {
            logger = new AlertActionLogger();
        }

        [Test]
        public void TestLogResolution()
        {
            Assert.Ignore();
        }
    }
}
