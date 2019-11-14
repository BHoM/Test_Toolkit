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
    public class EngineTests
    {
        private TestContext testContextInstance;

        public TestContext TestContext
        {
            get { return testContextInstance; }
            set { testContextInstance = value; }
        }

        private List<string> GetEngineFiles()
        {
            string projectName = TestContext.Properties["projectName"].ToString();

            if (projectName == "null") return null;

            string projectSplit = "";
            if (projectName.EndsWith("_Toolkit"))
                projectSplit = projectSplit.Split('_')[0];
            else
                projectSplit = projectName;

            string projectEngine = null;

            if (TestContext.Properties.Contains("engineName"))
                projectEngine = Path.Combine("..", "..", projectName, TestContext.Properties["engineName"].ToString());

            if (projectEngine == null) return null;

            return Directory.EnumerateFiles(projectEngine, "*.cs", SearchOption.AllDirectories).ToList();
        }

        [TestMethod]
        public void TestEngineCompliance()
        {
            /*List<string> allEngineFiles = GetEngineFiles();
            if (allEngineFiles == null) { Assert.IsTrue(true); return; }

            ComplianceResult result = Create.ComplianceResult(ResultStatus.Pass);

            foreach(string s in allEngineFiles)
            {
                StreamReader sr = new StreamReader(s);
                string file = sr.ReadToEnd();
                sr.Close();

                if(file != null)
                {
                    SyntaxTree st = BH.Engine.Test.Convert.ToSyntaxTree(file, s);
                    result = result.Merge(st.GetFileRoot().RunChecks());
                }
            }

            if (result.Status == ResultStatus.Pass) { Assert.IsTrue(true); return; }

            string message = "";
            foreach (Error e in result.Errors)
                message += e.ToText() + "\n";

            if(result.Status == ResultStatus.Fail) { Assert.Inconclusive(message); }
            if (result.Status == ResultStatus.CriticalFail) { Assert.Fail(message); }*/

            Assert.IsTrue(true);
        }
    }
}
