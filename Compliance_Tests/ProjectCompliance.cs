using BH.oM.Test.Results;
using BH.oM.Test;
using BH.Engine.Test;
using Microsoft.CodeAnalysis;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace BH.Tests.Compliace
{
    public class ProjectCompliance
    {

        [TestCaseSource(typeof(BH.Tests.Setup.Query), nameof(BH.Tests.Setup.Query.TestFiles), new object[] { "csproj" })]
        public void TestCompliance(string fileName)
        {
            TestResult result = BH.Engine.Test.CodeCompliance.Compute.CheckProjectFile(fileName);
            if (result == null)
                Assert.Fail($"{fileName}: No result returned from compliance check.");

            if (result.Status == TestStatus.Error)
                Assert.Fail($"{fileName}: {result.FullMessage(5, TestStatus.Warning)}");

            if (result.Status == TestStatus.Warning)
                Assert.Warn($"{fileName}: {result.FullMessage(5, TestStatus.Warning)}");
        }
    }
}
