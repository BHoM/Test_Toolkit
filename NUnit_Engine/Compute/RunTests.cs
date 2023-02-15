﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Text;

using BH.oM.Test.NUnit;
using NUnit.Engine;
using System.Xml.Serialization;
using BH.oM.Base.Attributes;
using System.Xml;

namespace BH.Engine.Test.NUnit
{
    public static partial class Compute
    {
        [Description("Runs a set of NUnit tests from a given DLL which contains code set up with the NUnit framework.")]
        [Input("filePath", "A full file path to the DLL file which contains the NUnit tests.")]
        [Output("testRun", "The NUnit test result from running the DLL.")]
        public static TestRun RunTests(string filePath)
        {
            if (string.IsNullOrEmpty(filePath) || !File.Exists(filePath))
                return null;

            ITestEngine testEngine = TestEngineActivator.CreateInstance();
            var package = new TestPackage(filePath);
            var testRunner = testEngine.GetRunner(package);
            var testResult = testRunner.Run(null, TestFilter.Empty);

            XmlDocument xmlDoc = new XmlDocument();
            xmlDoc.LoadXml(testResult.OuterXml);

            XmlSerializer ser = new XmlSerializer(typeof(TestRun));
            TestRun result = null;
            using (XmlNodeReader reader = new XmlNodeReader(xmlDoc))
            {
                result = ser.Deserialize(reader) as TestRun;
            }
            return result;
        }
    }
}
