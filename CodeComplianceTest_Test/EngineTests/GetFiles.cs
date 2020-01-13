using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using System.IO;
using System.Collections.Generic;
using System.Linq;

using System.Text;
using System.Threading.Tasks;

namespace BH.Test.Test
{
    [TestClass]
    public partial class Test_Engine
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

            if (TestContext.Properties.Contains("engineName"))
                projectOM = Path.Combine("..", "..", projectName, TestContext.Properties["engineName"].ToString());

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

            string pathToOM = Path.Combine(build, "PRTestFiles", projectName, projectSplit + "_Engine");
            return Directory.EnumerateFiles(pathToOM, "*.cs", SearchOption.AllDirectories).ToList();
        }
    }
}
