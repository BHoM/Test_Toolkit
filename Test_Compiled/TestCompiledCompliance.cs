using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using BH.Engine.Reflection;
using System.Reflection;
using System.Collections.Generic;
using System.Linq;

namespace BH.Test.TestCompiled
{
    [TestClass]
    public partial class TestCompiledCompliance
    {
        private TestContext testContextInstance;

        public TestContext TestContext
        {
            get { return testContextInstance; }
            set { testContextInstance = value; }
        }

        private string GetBuildFolder()
        {
            string projectName = TestContext.Properties["projectName"].ToString();

            if (projectName == "null") return null;

            string projectSplit = "";
            if (projectName.EndsWith("_Toolkit"))
                projectSplit = projectName.Split('_')[0];
            else
                projectSplit = projectName;

            string build = Environment.GetEnvironmentVariable("BUILD_SOURCESDIRECTORY").ToString();

            return System.IO.Path.Combine(build, projectName, "Build");
        }
    }
}
