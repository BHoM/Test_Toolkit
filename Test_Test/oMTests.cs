using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using System.IO;
using System.Collections.Generic;
using System.Linq;

using BH.oM.Test;
using BH.Engine.Test;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace BH.Test.Test
{
    [TestClass]
    public class oMTests
    {
        private TestContext testContextInstance;

        public TestContext TestContext
        {
            get { return testContextInstance; }
            set { testContextInstance = value; }
        }

        private List<string> GetAllObjectFiles()
        {
            string projectName = TestContext.Properties["projectName"].ToString();

            if (projectName == "null") return null;

            string projectOM = null;

            if (TestContext.Properties.Contains("oMName"))
                projectOM = Path.Combine("..", "..", projectName, TestContext.Properties["oMName"].ToString());

            if (projectOM == null) return null;

            return Directory.EnumerateFiles(projectOM, "*.cs", SearchOption.AllDirectories).ToList();
        }

        private List<string> GetChangedObjectFiles()
        {
            string projectName = TestContext.Properties["projectName"].ToString();

            if (projectName == "null") return null;

            string projectSplit = "";
            if (projectName.EndsWith("_Toolkit"))
                projectSplit = projectName.Split('_')[0];
            else
                projectSplit = projectName;

            string build = Environment.GetEnvironmentVariable("BUILD_SOURCESDIRECTORY").ToString();

            string pathToOM = Path.Combine(build, "PRTestFiles", projectName, projectSplit + "_oM");
            return Directory.EnumerateFiles(pathToOM, "*.cs", SearchOption.AllDirectories).ToList();
        }

        [TestMethod]
        public void TestObjectCompliance()
        {
            List<string> changedFiles = GetChangedObjectFiles();
            if (changedFiles == null) { Assert.IsTrue(true); return; }

            ComplianceResult r = Create.ComplianceResult(ResultStatus.Pass);
            foreach (string s in changedFiles)
            {
                StreamReader sr = new StreamReader(s);
                string file = sr.ReadToEnd();
                sr.Close();

                if(file != null)
                {
                    SyntaxTree st = BH.Engine.Test.Convert.ToSyntaxTree(file, s);
                    r = r.Merge(Compute.Check(Query.AllChecks().Where(x => x.Name == "IsPublic").FirstOrDefault(), st.GetFileRoot()));
                }
            }

            string message = "";
            foreach (Error e in r.Errors)
                message += e.Message + "\n";

            if (r.Status == ResultStatus.CriticalFail)
                Assert.Fail(message);
            else
                Assert.IsTrue(true);
             
        }
    }
}
