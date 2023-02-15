using BH.oM.Test.NUnit;
using BH.oM.Test.Results;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BH.Engine.Test.NUnit
{
    public static partial class Convert
    {
        public static TestResult ToTestResult(this TestRun nunitTest)
        {
            TestResult result = new TestResult();

            result.Status = ToTestStatus(nunitTest.Result);
            result.Description = $"Passed: {nunitTest.Passed}{System.Environment.NewLine}Failed: {nunitTest.Failed}{System.Environment.NewLine}";
            result.Message = $"";

            if (nunitTest.TestSuite != null)
                result.Information.Add(nunitTest.TestSuite.ToTestResult());

            return result;
        }

        public static TestResult ToTestResult(this TestSuite nunitTestSuite)
        {
            TestResult result = new TestResult();

            result.Status = ToTestStatus(nunitTestSuite.Result);
            result.Description = $"Passed: {nunitTestSuite.Passed}{System.Environment.NewLine}Failed: {nunitTestSuite.Failed}{System.Environment.NewLine}";
            result.Message = $"";

            if (nunitTestSuite.Child != null)
                result.Information.Add(nunitTestSuite.Child.ToTestResult());

            if (nunitTestSuite.TestCases.Count > 0)
                result.Information.AddRange(nunitTestSuite.TestCases.Select(x => x.ToTestResult()));

            return result;
        }

        public static TestResult ToTestResult(this TestCase nunitTestCase)
        {
            TestResult result = new TestResult();

            result.Status = ToTestStatus(nunitTestCase.Result);
            result.Description = nunitTestCase.FullName;
            result.Message = $"";

            if(nunitTestCase.Failure != null)
                result.Message += $"{nunitTestCase.Failure.Message}{System.Environment.NewLine}{nunitTestCase.Failure.Stacktrace}";

            return result;
        }
    }
}
