using BH.Engine.Base;
using BH.Engine.Test;
using BH.oM.Base.Attributes;
using BH.oM.Data.Library;
using BH.oM.Test.NUnit;
using BH.oM.Test.Results;
using BH.oM.Test.UnitTests;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace BH.Tests.Setup
{
    public abstract class UnitTestRunnerBase : NUnitTest
    {
        /***************************************************/

        [TestCaseSource("TestData")]
        public void RunDatadrivenUnitTest(string fileName, MethodBase method, TestData data, Exception e)
        {
            if (e != null)
                throw e;

            Assert.That(method, Is.Not.Null);
            Assert.That(data, Is.Not.Null);

            TestResult result = BH.Engine.UnitTest.Compute.CheckTest(method, data, -1);

            Assert.That(result.Status == oM.Test.TestStatus.Pass, $"The ut did not pass {result.FullMessage(3, oM.Test.TestStatus.Error)}");
        }

        /***************************************************/

        [OneTimeSetUp]
        public void EnsureStaticMembers()
        {
            var testDataMethod = this.GetType().GetMethod("TestData", System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.Public);
            Assume.That(testDataMethod != null, "Expected static member TestData is not implemented on derrived test class");

            Assume.That(testDataMethod.ReturnType == typeof(IEnumerable<object[]>), $"Expected return type of TestData() does not match expected IEnumerable<object[]>");
        }

        /***************************************************/

        public static IEnumerable<object[]> GetTestDataInRelativeFolder(string folder)
        {
            string dataFolder = Path.Combine(Query.CurrentCiFolder(), folder);
            foreach (var item in Directory.GetFiles(dataFolder, "*.json", SearchOption.AllDirectories))
            {
                foreach (var test in GetTestData(item))
                {
                    yield return test;
                }
            }
        }

        /***************************************************/


        [Description("Executes all unit tests stored in a serialised file of test sets.")]
        [Input("fileName", "The full file path to the file containing the serialised test datasets.")]
        [Output("result", "Results from the comparison of the run data with the expected output.")]
        public static IEnumerable<object[]> GetTestData(string fileName)
        {
            string fileNameNoPath = fileName;
            try
            {
                if (!string.IsNullOrEmpty(fileName))
                {
                    fileNameNoPath = Path.GetFileName(fileName);
                    StreamReader sr = new StreamReader(fileName);
                    string line = sr.ReadToEnd();
                    sr.Close();

                    object ds = BH.Engine.Serialiser.Convert.FromJson(line);
                    //Assume.That(ds != null, "Failed to deserialialise");
                    if (ds != null)
                    {
                        Dataset testSet = ds as Dataset;
                        //Assume.That(testSet != null, $"{fileName} failed to deserialise into a dataset.");
                        if (testSet != null)
                        {
                            return GetTestData(fileNameNoPath, testSet);

                        }
                        else
                            new List<object[]> { new object[] { fileNameNoPath, null, null, null } };
                    }
                    else
                        return new List<object[]> { new object[] { fileNameNoPath, null, null, null } };
                }
            }
            catch (Exception e)
            {
                return new List<object[]> { new object[] { fileNameNoPath, null, null, e } };
            }


            return new List<object[]> { new object[] { fileNameNoPath, null, null, null } };
        }

        /***************************************************/

        [Description("Executes all unit tests in a dataset and returns a total TestResult from the execution of all testResults in the dataset.")]
        [Input("testDataSet", "The test dataset to be evaluated.")]
        [Output("results", "Results from the comparison of the run data with the expected output.")]
        public static IEnumerable<object[]> GetTestData(string fileName, Dataset testDataSet)
        {
            if (testDataSet != null)
            {
                List<UnitTest> unitTests = testDataSet.Data.OfType<UnitTest>().ToList();

                foreach (UnitTest test in unitTests)
                {
                    int i = 0;
                    foreach (TestData data in test.Data)
                    {
                        yield return new object[] { fileName, test.Method, data, null };
                    }
                }
            }

        }

        /***************************************************/



    }
}
