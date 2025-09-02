using BH.Tests.Setup.TestBases;
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
    public class ProjectCompliance : BaseTestBase
    {
        public ProjectCompliance() : base(new List<Tuple<string, Type>>(), true)
        { 
        
        }

        [TestCaseSource("TestFiles")]
        public void TestCompliance(string fileName)
        {
            TestResult result = BH.Engine.Test.CodeCompliance.Compute.CheckProjectFile(fileName);
            if (result == null)
                Assert.Fail($"{fileName}: No result returned from compliance check.");
            if (result.Status == TestStatus.Warning)
                Assert.Warn($"{fileName}: {result.FullMessage(5, TestStatus.Warning)}");
            else
                Assert.Fail($"{fileName}: {result.FullMessage(5, TestStatus.Warning)}");

            Assert.Pass($"Passing: {fileName}");
        }
        public static IEnumerable<string> TestFiles()
        {
            var files = Setup.Query.InputParametersUpdatedFiles()?.Where(f => Path.GetExtension(f).Equals(".csproj", StringComparison.OrdinalIgnoreCase));
            if (files != null)
                return files;

            return GetCsprojFiles("");
        }

        public static IEnumerable<string> GetCsprojFiles(string folder)
        {
            if (m_testFiles.TryGetValue(folder, out List<string> files))
                return files;
            lock (m_fileLock)
            {
                if (m_testFiles.TryGetValue(folder, out files))
                    return files;

                if (files == null)
                {
                    files = Setup.Query.GetFiles(System.IO.Path.Combine(Setup.Query.CurrentRepoFolder(), folder), "*.csproj", true).ToList();
                    m_testFiles[folder] = files;
                }
                return files;
            }

        }

        private static Dictionary<string, List<string>> m_testFiles = new Dictionary<string, List<string>>();
        private static object m_fileLock = new object();
    }
}
