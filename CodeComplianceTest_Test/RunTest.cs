/*
 * This file is part of the Buildings and Habitats object Model (BHoM)
 * Copyright (c) 2015 - 2022, the respective contributors. All rights reserved.
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

using BH.oM.Test.CodeCompliance;
using BH.Engine.Test.CodeCompliance;

using Microsoft.CodeAnalysis;

using BH.oM.Test;
using BH.oM.Test.Results;

namespace BH.Test.Test
{
    public static partial class Test
    {
        public static void RunTest(string name, List<string> files, string projectName)
        {
            List<string> changedFiles = files;
            if (changedFiles == null)
            { 
                Assert.IsTrue(true);
                return;
            }

            if (projectName == null)
                projectName = "";

            TestResult r = Create.TestResult(TestStatus.Pass);
            foreach (string s in changedFiles)
            {
                StreamReader sr = new StreamReader(s);
                string file = sr.ReadToEnd();
                sr.Close();

                if (file != null)
                {
                    SyntaxTree st = BH.Engine.Test.CodeCompliance.Convert.ToSyntaxTree(file, s);
                    List<System.Reflection.MethodInfo> o = Query.AllChecks().ToList();
                    foreach (var check in o.Where(x => x.Name == name))
                        r = r.Merge(check.Check(st.GetRoot()));
                }
            }

            if (r.Status == TestStatus.Error)
            {
                Dictionary<string, List<Error>> errors = r.Information.GroupBy(x => x.Message).ToDictionary(x => x.Key, x => x.Select(y => y as Error).ToList());
                string errorMessage = "";
                foreach (KeyValuePair<string, List<Error>> kvp in errors)
                {

                    errorMessage += kvp.Key + "\n";
                    errorMessage += "More information about this can be found at https://github.com/BHoM/documentation/wiki/" + kvp.Value[0].DocumentationLink + "\n";
                    foreach (Error e in kvp.Value)
                        errorMessage += $" - in {e.Location.FilePath.Substring(e.Location.FilePath.IndexOf(projectName))} at line {e.Location.Line.Start.Line}, column {e.Location.Line.Start.Column} \n";
                }

                Assert.Fail(errorMessage);
            }
            else
                Assert.IsTrue(true);
        }
    }
}


