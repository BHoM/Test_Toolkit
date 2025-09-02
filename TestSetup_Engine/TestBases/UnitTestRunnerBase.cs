using BH.Engine.Base;
using BH.Engine.Test;
using BH.oM.Base.Attributes;
using BH.oM.Base.Debugging;
using BH.oM.Data.Library;
using BH.oM.Test.NUnit;
using BH.oM.Test.Results;
using BH.oM.Test.UnitTests;
using BH.Tests.Setup.TestBases;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace BH.Tests.Setup
{
    public abstract class UnitTestRunnerBase : BaseTestBase
    {
        public UnitTestRunnerBase() : base("TestData", typeof(object[])) { }

        /***************************************************/

        [TestCaseSource("TestData")]
        [Description("Runs a datadriven unit datatest.")]
        public void RunDatadrivenUnitTest(string fileName, MethodBase method, TestData data)
        {
            if (method == null || data == null)
            {
                Assert.Multiple(() =>
                {
                    Assert.That(method, Is.Not.Null, "Test method is null!");
                    Assert.That(data, Is.Not.Null, "Test data is null!");
                    List<Event> events;
                    if (m_DeserialisationEvents.TryGetValue(fileName, out events))
                    {
                        string error = "";
                        string warning = "";
                        foreach (Event e in events.GroupBy(x => new { x.Type, x.Message}).Select(x => x.First()))
                        {
                            if (e.Type == EventType.Error)
                                error += e.Message + "\n";
                            else
                                warning += e.Message + "\n";
                        }

                        if (error != "")
                            Assert.Fail("Errors raised during deserialisation of the unit tests:\n" + error);
                        if (warning != "")
                            Assert.Warn("Warnings raised during deserialisation of the unit tests:\n" + warning);
                    }
                });
            }
            else
            {
                List<Event> events;
                if (m_DeserialisationEvents.TryGetValue(fileName, out events))
                {
                    string warning = "";
                    foreach (Event e in events.GroupBy(x => new { x.Type, x.Message }).Select(x => x.First()))
                    {
                        warning += e.Message + "\n";
                    }
                    if (warning != "")
                        Assert.Warn("Warnings raised during deserialisation of the unit tests:\n" + warning);
                }
            }

            TestResult result = BH.Engine.UnitTest.Compute.CheckTest(method, data, -1);

            Assert.That(result.Status, Is.EqualTo(oM.Test.TestStatus.Pass), $"The ut did not pass {result.FullMessage(3, oM.Test.TestStatus.Error)}");

            //Console.WriteLine(result.FullMessage());
        }

        /***************************************************/

        [Description("Extracts all the unittest datasets from the relative folder in the currently executing repo and deserialises the content of the file and returns an IEnumerable with filename, method and one peice of testdata for each UnitTest and each TestData in the dataset.")]
        public static IEnumerable<object[]> GetTestDataInRelativeFolder(string folder)
        {
            string dataFolder = Path.Combine(Query.CurrentDatasetsUTFolder(), folder);
            foreach (var item in Query.GetFiles(dataFolder, "*.json", true))
            {
                foreach (var test in GetTestData(item))
                {
                    yield return test;
                }
            }
        }

        /***************************************************/


        [Description("Deserialises the content of the file and returns an IEnumerable with filename, method and one peice of testdata for each UnitTest and each TestData in the dataset.")]
        public static IEnumerable<object[]> GetTestData(string fileName)
        {
            string fileNameNoPath = fileName;
            BH.Engine.Base.Compute.ClearCurrentEvents();
            try
            {
                if (!string.IsNullOrEmpty(fileName))
                {
                    fileNameNoPath = fileName.Replace(Query.CurrentDatasetsUTFolder(), "");
                    StreamReader sr = new StreamReader(fileName);
                    string line = sr.ReadToEnd();
                    sr.Close();

                    Dataset ds = (Dataset)BH.Engine.Serialiser.Convert.FromJson(line);
                    return GetTestData(fileNameNoPath, ds);
                }
            }
            catch (Exception e)
            {
                BH.Engine.Base.Compute.RecordError(e, "Failed to deserialise dataset");
                return new List<object[]> { new object[] { fileNameNoPath, null, null } };
            }
            finally
            {
                m_DeserialisationEvents[fileNameNoPath] = BH.Engine.Base.Query.CurrentEvents();
            }

            return new List<object[]> { new object[] { fileNameNoPath, null, null } };
        }

        /***************************************************/

        [Description("Returns an IEnumerable with filename, method and one peice of testdata for each UnitTest and each TestData in the dataset.")]
        public static IEnumerable<object[]> GetTestData(string fileName, Dataset testDataSet)
        {
            if (testDataSet != null)
            {
                List<UnitTest> unitTests = testDataSet.Data.OfType<UnitTest>().ToList();

                foreach (UnitTest test in unitTests)
                {
                    foreach (TestData data in test.Data)
                    {
                        yield return new object[] { fileName, test.Method, data };
                    }
                }
            }

        }

        /***************************************************/

        private static Dictionary<string, List<Event>> m_DeserialisationEvents = new Dictionary<string, List<Event>>();

    }
}
