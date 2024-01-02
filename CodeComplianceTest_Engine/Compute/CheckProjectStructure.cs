/*
 * This file is part of the Buildings and Habitats object Model (BHoM)
 * Copyright (c) 2015 - 2024, the respective contributors. All rights reserved.
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

using BH.oM.Test.CodeCompliance;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

using System.IO;

using System.Xml.Linq;

using BH.oM.Test;
using BH.oM.Test.Results;

namespace BH.Engine.Test.CodeCompliance
{
    public static partial class Compute
    {
        public static TestResult CheckProjectStructure(this string projectDirectory)
        {
            TestResult finalResult = Create.TestResult(TestStatus.Pass);

            string documentationLink = "Project-References-and-Build-Paths";

            List<string> exceptionalRepos = new List<string>
            {
                "BHoM",
                "BHoM_Engine",
                "BHoM_UI",
                "BHoM_Adapter",
            };

            string[] directoryParts = projectDirectory.Split('\\');
            string toolkit = directoryParts.Last();

            string[] toolkitParts = toolkit.Split('_');
            if(toolkitParts.Length == 1 && !exceptionalRepos.Contains(toolkit))
                finalResult = finalResult.Merge(Create.TestResult(TestStatus.Error, new List<Error> { Create.Error("Project not a valid project name. Project should end in '_Toolkit'", Create.Location(projectDirectory, Create.LineSpan(1, 1)), documentationLink) }));
            else if (toolkitParts.Length == 2 && toolkitParts[1] != "Toolkit")
                finalResult = finalResult.Merge(Create.TestResult(TestStatus.Error, new List<Error> { Create.Error("Project not a valid project name. Project should end in '_Toolkit'", Create.Location(projectDirectory, Create.LineSpan(1, 1)), documentationLink) }));
            else if(toolkitParts.Length > 1)
            {
                string[] subFolders = Directory.GetDirectories(projectDirectory);
                bool containsEngine = true;
                bool containsAdapter = true;
                bool containsObject = true;

                if (subFolders.Where(x => x.EndsWith("_Engine")).Count() > 0 && subFolders.Where(x => x.EndsWith("_Engine") && (x.Split('\\').Last()) == toolkitParts[0] + "_Engine").FirstOrDefault() == null)
                    containsEngine = false;
                if (subFolders.Where(x => x.EndsWith("_Adapter")).Count() > 0 &&subFolders.Where(x => x.EndsWith("_Adapter") && (x.Split('\\').Last()) == toolkitParts[0] + "_Adapter").FirstOrDefault() == null)
                    containsAdapter = false;
                if (subFolders.Where(x => x.EndsWith("_oM")).Count() > 0 && subFolders.Where(x => x.EndsWith("_oM") && (x.Split('\\').Last()) == toolkitParts[0] + "_oM").FirstOrDefault() == null)
                    containsObject = false;

                if (!containsObject)
                    finalResult = finalResult.Merge(Create.TestResult(TestStatus.Warning, new List<Error> { Create.Error($"If the project requires an oM, the project should be titled '{toolkitParts[0]}_oM'", Create.Location(projectDirectory, Create.LineSpan(1, 1)), documentationLink) }));
                if (!containsAdapter)
                    finalResult = finalResult.Merge(Create.TestResult(TestStatus.Warning, new List<Error> { Create.Error($"If the project requires an Adapter, the project should be titled '{toolkitParts[0]}_Adapter'", Create.Location(projectDirectory, Create.LineSpan(1, 1)), documentationLink) }));
                if (!containsEngine)
                    finalResult = finalResult.Merge(Create.TestResult(TestStatus.Error, new List<Error> { Create.Error($"If the project requires an Engine, the project should be titled '{toolkitParts[0]}_Engine'", Create.Location(projectDirectory, Create.LineSpan(1, 1)), documentationLink) }));

                List<string> allowedEngineFolders = new List<string>
                {
                    "Compute",
                    "Convert",
                    "Create",
                    "Modify",
                    "Query",
                    "Properties",
                    "obj",
                    "bin",
                };
                if (containsEngine)
                {
                    try
                    {
                        string[] engineFolders = Directory.GetDirectories(projectDirectory + "\\" + toolkitParts[0] + "_Engine");
                        if (engineFolders.Length > allowedEngineFolders.Count)
                            finalResult = finalResult.Merge(Create.TestResult(TestStatus.Error, new List<Error> { Create.Error($"The Engine project should only contain Compute, Convert, Create, Modify, and Query sub-folders", Create.Location(projectDirectory, Create.LineSpan(1, 1)), documentationLink) }));
                        else
                        {
                            foreach (string st in engineFolders)
                            {
                                string[] pr = st.Split('\\');
                                if (!allowedEngineFolders.Contains(pr.Last()))
                                    finalResult = finalResult.Merge(Create.TestResult(TestStatus.Error, new List<Error> { Create.Error($"{st} is not a valid Engine sub folder", Create.Location(projectDirectory, Create.LineSpan(1, 1)), documentationLink) }));
                            }
                        }
                    }
                    catch { }
                }
            }

            return finalResult;
        }
    }
}





