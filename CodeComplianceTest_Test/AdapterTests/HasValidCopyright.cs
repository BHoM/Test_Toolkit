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

using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using BH.oM.Test.CodeCompliance;
using BH.Engine.Test.CodeCompliance;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

using System.IO;

using BH.oM.Test;
using BH.oM.Test.Results;

namespace BH.Test.Test
{
    public partial class Test_Adapter
    {
        [TestMethod]
        public void HasValidCopyright()
        {
            List<string> changedFiles = GetChangedObjectFiles();
            if (changedFiles == null) { Assert.IsTrue(true); return; }

            TestResult r = Create.TestResult(TestStatus.Pass);
            foreach (string s in changedFiles)
            {
                StreamReader sr = new StreamReader(s);
                string file = sr.ReadToEnd();
                sr.Close();

                if (file != null)
                {
                    SyntaxTree st = BH.Engine.Test.CodeCompliance.Convert.ToSyntaxTree(file, s);
                    r = r.Merge(BH.Engine.Test.CodeCompliance.Query.HasValidCopyright((st.GetRoot() as CompilationUnitSyntax).GetLeadingTrivia(), DateTime.Now.Year, s));
                }
            }

            if (r.Status == TestStatus.Error)
                Assert.Fail(r.Information.Select(x => x as Error).Select(x => x.ToText() + "\n").Aggregate((a, b) => a + b));
            else
                Assert.IsTrue(true);
        }
    }
}

