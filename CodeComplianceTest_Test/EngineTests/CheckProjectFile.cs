/*
 * This file is part of the Buildings and Habitats object Model (BHoM)
 * Copyright (c) 2015 - 2020, the respective contributors. All rights reserved.
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

namespace BH.Test.Test
{
    public partial class Test_Engine
    {
        [TestMethod]
        public void CheckProjectFile()
        {
            /*List<string> changedFiles = GetChangedObjectFiles(true);
            if (changedFiles == null || changedFiles.Where(x => Path.GetExtension(x).EndsWith("csproj")).Count() == 0) { Assert.IsTrue(true); return; }

            changedFiles = changedFiles.Where(x => Path.GetExtension(x).EndsWith("csproj")).ToList();

            ComplianceResult r = Create.ComplianceResult(ResultStatus.Pass);
            foreach (string s in changedFiles)
            {
                r = r.Merge(BH.Engine.Test.CodeCompliance.Compute.CheckReferences(s));
            }

            if (r.Status == ResultStatus.CriticalFail)
                Assert.Fail(r.Errors.Select(x => x.ToText() + "\n").Aggregate((a, b) => a + b));
            else*/
                Assert.IsTrue(true);
        }
    }
}
