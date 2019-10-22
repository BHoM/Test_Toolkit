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

namespace Test_Test
{
    [TestClass]
    public class UnitTest1
    {
        private TestContext testContextInstance;

        public TestContext TestContext
        {
            get { return testContextInstance; }
            set { testContextInstance = value; }
        }

        [TestMethod]
        public void TestMethod1()
        {
            string projectName = TestContext.Properties["projectName"].ToString();

            if (projectName == "null") Assert.IsTrue(true);

            string projectSplit = "";
            if (projectName.EndsWith("_Toolkit"))
                projectSplit = projectSplit.Split('_')[0];
            else
                projectSplit = projectName;

            string cwd = Directory.GetCurrentDirectory();
            Console.WriteLine(cwd);

            string projectOM = null;
            string projectEngine = null;
            string projectAdapter = null;
            string projectUI = null;

            if(TestContext.Properties.Contains("oMName"))
                projectOM = Path.Combine("..", "..", projectName, TestContext.Properties["oMName"].ToString());
            if (TestContext.Properties.Contains("engineName"))
                projectEngine = Path.Combine("..", "..", projectName, TestContext.Properties["engineName"].ToString());
            if (TestContext.Properties.Contains("adapterName"))
                projectAdapter = Path.Combine("..", "..", projectName, TestContext.Properties["adapterName"].ToString());
            if (TestContext.Properties.Contains("uiName"))
                projectUI = Path.Combine("..", "..", projectName, TestContext.Properties["uiName"].ToString());

            
            List<string> oMFiles = Directory.EnumerateFiles(projectOM, "*.cs", SearchOption.AllDirectories).ToList();

            ComplianceResult result = null;
            foreach(string s in oMFiles)
            {
                StreamReader sr = new StreamReader(s);

                SyntaxTree st = BH.Engine.Test.Convert.ToSyntaxTree(sr.ReadLine(), s);
                result = result.Merge(st.GetFileRoot().RunChecks());

                sr.Close();
            }

            if (result.Status == ResultStatus.Pass)
                Assert.IsTrue(true); //Pass test
            else
            {
                Assert.Fail(result.Errors.Select(x => x.Message).ToString());
                Assert.IsTrue(false); //Fail test
            }
        }
    }
}
