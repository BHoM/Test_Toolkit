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

        private List<string> GetObjectFiles()
        {
            string projectName = TestContext.Properties["projectName"].ToString();

            if (projectName == "null") return null;

            string projectSplit = "";
            if (projectName.EndsWith("_Toolkit"))
                projectSplit = projectSplit.Split('_')[0];
            else
                projectSplit = projectName;

            string projectOM = null;

            if (TestContext.Properties.Contains("oMName"))
                projectOM = Path.Combine("..", "..", projectName, TestContext.Properties["oMName"].ToString());

            if (projectOM == null) return null;

            return Directory.EnumerateFiles(projectOM, "*.cs", SearchOption.AllDirectories).ToList();
        }

        [TestMethod]
        public void TestObjectCompliance()
        {
            List<string> allObjectFiles = GetObjectFiles();
            if (allObjectFiles == null) { Assert.IsTrue(true); return; }

            ComplianceResult result = Create.ComplianceResult(ResultStatus.Pass);

            foreach (string s in allObjectFiles)
            {
                StreamReader sr = new StreamReader(s);
                string file = sr.ReadToEnd();
                sr.Close();

                if (file != null)
                {
                    SyntaxTree st = BH.Engine.Test.Convert.ToSyntaxTree(file, s);
                    result = result.Merge(st.GetFileRoot().IIsCompliant());
                }
            }

            if (result.Status == ResultStatus.Pass) { Assert.IsTrue(true); return; }

            string message = "";
            foreach (Error e in result.Errors)
                message += e.ToText() + "\n";

            if (result.Status == ResultStatus.Fail) { Assert.Inconclusive(message); }
            if (result.Status == ResultStatus.CriticalFail) { Assert.Fail(message); }
        }
    }
}
