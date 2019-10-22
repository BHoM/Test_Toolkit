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

            if (projectName.EndsWith("_Toolkit"))
                projectName = projectName.Split('_')[0];

            string cwd = Directory.GetCurrentDirectory();
            Console.WriteLine(cwd);

            string projectOM = Path.Combine("..", "..", projectName + "_Toolkit", projectName + "_oM");
            string projectEngine = Path.Combine("..", "..", projectName + "_Toolkit", projectName + "_Engine");
            string projectAdapter = Path.Combine("..", "..", projectName + "_Toolkit", projectName + "_Adapter");
            string projectUI = Path.Combine("..", "..", projectName + "_Toolkit" + projectName + "_UI");

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
