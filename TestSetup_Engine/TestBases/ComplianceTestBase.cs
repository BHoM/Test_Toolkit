using BH.Engine.Base;
using BH.Engine.Test;
using BH.Engine.Test.CodeCompliance;
using BH.oM.Base.Attributes;
using BH.oM.Test;
using BH.oM.Test.CodeCompliance;
using BH.oM.Test.NUnit;
using BH.oM.Test.Results;
using BH.oM.Test.UnitTests;
using BH.Tests.Setup.TestBases;
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
    public abstract class ComplianceTestBase : BaseTestBase
    {

        public ComplianceTestBase() : base("TestFiles", typeof(string)) { }

        [TestCaseSource("TestFiles")]
        public void RunCodeCompliance(string filePath)
        {
            RunCompliance(filePath, "code");
        }

        [TestCaseSource("TestFiles")]
        public void RunDocumentationCompliance(string filePath)
        {
            RunCompliance(filePath, "documentation");
        }

        private void RunCompliance(string filePath, string checkType)
        {

            Assert.That(filePath, Is.Not.Null);
            Assert.That(checkType, Is.Not.Null);

            Assert.That(File.Exists(filePath), Is.True, "File does not exist.");

            TestResult result = filePath.RunChecks(checkType);

            Assert.Multiple(() =>
            {
                //Assert.That(result.Status == oM.Test.TestStatus.Pass, $"The ut did not pass {result.FullMessage(3, oM.Test.TestStatus.Error)}");
                if (result.Status != TestStatus.Pass)
                {
                    List<Error> errors = GroupInformation(result.Information, filePath);
                    foreach (Error error in errors)
                    {
                        string message = $"{error.Location.FilePath} line {error.Location.Line.Start.Line}:{error.Location.Line.End.Line}\n";
                        message += error.Message;
                        if (error.Status == TestStatus.Error)
                            Assert.Fail(message);
                        else
                            Assert.Warn(message);
                    }
                }

            });
        }

        public static IEnumerable<string> GetCsFiles(string folder)
        {
            List<string> testFiles = Query.InputParametersUpdatedFiles();
            if (testFiles != null)
            {
                return testFiles.Where(f => Path.GetExtension(f) == ".cs");
            }
            return Query.GetFiles(Path.Combine(Query.CurrentRepoFolder(), folder), "*.cs", true);
        }

        public static List<Error> GroupInformation(List<ITestInformation> information, string filePath)
        {
            List<Error> errors = information.OfType<Error>().ToList();

            errors = errors.GroupErrors(filePath);

            return errors;
        }
    }
}
