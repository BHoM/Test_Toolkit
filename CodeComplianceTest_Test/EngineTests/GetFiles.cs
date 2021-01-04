/*
 * This file is part of the Buildings and Habitats object Model (BHoM)
 * Copyright (c) 2015 - 2021, the respective contributors. All rights reserved.
 *
 * Each contributor holds copyright over their respective contributions.
 * The project versioning (Git) records all such contribution source information.
 *                                           
 *                                                                              
 * The BHoM is free software: you can redistribute it and/or modify         
 * it under the terms of the GNU Lesser General Public License as published by  
 * the Free Software Foundation, either version 3.0 of the License, or          
 * (at your option) any later version.                                          
 *                                                                              
 * The BHoM is distributed in the hope that it will be useful,              
 * but WITHOUT ANY WARRANTY; without even the implied warranty of               
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the                 
 * GNU Lesser General Public License for more details.                          
 *                                                                            
 * You should have received a copy of the GNU Lesser General Public License     
 * along with this code. If not, see <https://www.gnu.org/licenses/lgpl-3.0.html>.      
 */

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

        private List<string> GetAllObjectFiles(bool includeCsproj = false)
        {
            string projectName = TestContext.Properties["projectName"].ToString();

            if (projectName == "null") return null;

            string projectOM = null;

            if (TestContext.Properties.Contains("engineName"))
                projectOM = Path.Combine("..", "..", projectName, TestContext.Properties["engineName"].ToString());

            if (projectOM == null) return null;

            string filterFiles = "*.cs";
            if (includeCsproj)
                filterFiles += "*";

            return Directory.EnumerateFiles(projectOM, filterFiles, SearchOption.AllDirectories).ToList();
        }

        private List<string> GetChangedObjectFiles(bool includeCsproj = false)
        {
            string projectName = TestContext.Properties["projectName"].ToString();

            if (projectName == "null") return null;

            string projectSplit = "";
            if (projectName.EndsWith("_Toolkit"))
                projectSplit = projectName.Split('_')[0];
            else
                projectSplit = projectName;

            string build = Environment.GetEnvironmentVariable("BUILD_SOURCESDIRECTORY").ToString();

            string filterFiles = "*.cs";
            if (includeCsproj)
                filterFiles += "*";

            string pathToOM = Path.Combine(build, "PRTestFiles", projectName, projectSplit + "_Engine");
            return Directory.EnumerateFiles(pathToOM, filterFiles, SearchOption.AllDirectories).ToList();
        }

        private string GetProjectName()
        {
            return TestContext.Properties["projectName"].ToString();
        }
    }
}

