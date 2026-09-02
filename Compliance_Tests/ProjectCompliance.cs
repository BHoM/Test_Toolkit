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
        /***************************************************/
        /**** Test methods                              ****/
        /***************************************************/

        [Description("Checks a .csproj file by using the method available in CodeComplianceTest_Engine." +
                     "Potential to port the content of that method over to this file, split as individual tests, each run on the .csproj test files.")]
        [TestCaseSource(nameof(ProjectFiles))]
        public void TestCompliance(string fileName, string assemblyDescriptionOrg)
        {
            TestResult result = BH.Engine.Test.CodeCompliance.Compute.CheckProjectFile(fileName, assemblyDescriptionOrg);
            if (result == null)
                Assert.Fail($"{fileName}: No result returned from compliance check.");

            if (result.Status == TestStatus.Error)
                Assert.Fail($"{fileName}: {result.FullMessage(5, TestStatus.Warning)}");

            if (result.Status == TestStatus.Warning)
                Assert.Warn($"{fileName}: {result.FullMessage(5, TestStatus.Warning)}");
        }

        /***************************************************/
        /**** Test data methods                         ****/
        /***************************************************/

        [Description("Returns the csproj files as well as assumed link to the repository.")]
        private static IEnumerable<TestCaseData> ProjectFiles()
        {
            string organisationUrl = null;
            string currentRepo = BH.Tests.Setup.Query.CurrentRepository();
            if (currentRepo != null)
                 organisationUrl = $"https://github.com/{currentRepo}";

            foreach (var file in BH.Tests.Setup.Query.TestFilesCsproj())
                yield return new TestCaseData(new string[] { file, organisationUrl }).SetArgDisplayNames(System.IO.Path.GetFileName(file));

        }

        /***************************************************/
    }
}
