using BH.oM.Test.NUnit;
using NUnit.Framework;
using BH.Engine.Test.NUnit;
//using BH.oM.Test.Results;
using Newtonsoft.Json;
using Shouldly;

namespace NUnit_TestRunner_Tests
{
    public class TestsRunner_Tests
    {
        [Test]
        [Description("Uses the test runner in BH.Engine.Test.NUnit.Compute.RunTests " +
            "to run the tests written in the NUnit_Engine_Tests project and verifies that there are no errors.")]
        public void Run_NUnit_Engine_Tests()
        {
            string? solutionPath = Directory.GetParent(AppDomain.CurrentDomain.BaseDirectory)?.Parent?.Parent?.Parent?.Parent?.FullName;
            if (solutionPath == null)
                Assert.Fail("Could not find solution directory path.");

            string NUnit_Engine_Tests_path = Path.Combine(solutionPath, "NUnit_Engine_Tests\\bin\\Debug\\net6.0\\NUnit_Engine_Tests.dll");
            
            TestRun testRunResult = Compute.RunTests(NUnit_Engine_Tests_path);
            Console.WriteLine(JsonConvert.SerializeObject(testRunResult, Formatting.Indented));

            //TestResult testResult = testRunResult.ToTestResult();
            //testResult.ShouldNotBeNull();
            //Console.WriteLine(JsonConvert.SerializeObject(testResult, Formatting.Indented));

            testRunResult.Failed.ShouldBe(0);
        }
    }
}