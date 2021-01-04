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

using BH.Engine.Reflection;
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
using BH.oM.Test.CodeCompliance.Attributes;

namespace BH.Engine.Test.CodeCompliance
{
    public static partial class Compute
    {
        public static ComplianceResult RunChecks(this SyntaxNode node, string checkType = null)
        {
            string path = node.SyntaxTree.FilePath;
            if (Path.GetFileName(path) == "AssemblyInfo.cs")
                return Create.ComplianceResult(ResultStatus.Pass);

            ComplianceResult finalResult = Create.ComplianceResult(ResultStatus.Pass);
            foreach(MethodInfo method in Query.AllChecks())
            {
                finalResult = finalResult.Merge(method.Check(node, checkType));
            }
            return finalResult;
        }

        public static ComplianceResult RunChecks(this string filePath, string checkType)
        {
            StreamReader sr = new StreamReader(filePath);
            string file = sr.ReadToEnd();
            sr.Close();

            ComplianceResult r = Create.ComplianceResult(ResultStatus.Pass);

            if (file != null)
            {
                SyntaxTree st = BH.Engine.Test.CodeCompliance.Convert.ToSyntaxTree(file, filePath);
                
                r = r.Merge(RunChecks(st.GetRoot(), checkType));
            }

            return r;
        }
    }
}


