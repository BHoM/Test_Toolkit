/*
 * This file is part of the Buildings and Habitats object Model (BHoM)
 * Copyright (c) 2015 - 2025, the respective contributors. All rights reserved.
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
using BH.oM.Test.CodeCompliance.Attributes;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using BH.oM.Test;

namespace BH.Engine.Test.CodeCompliance.Checks
{
    public static partial class Query
    {

        [Message("Class (object) name must match file name", "ObjectNameMatchesFileName")]
        [ErrorLevel(TestStatus.Error)]
        [Path(@"([a-zA-Z0-9]+)(_?oM|_Tests)\\.*\.cs$")]
        [Path(@"([a-zA-Z0-9]+)_Engine\\.*\.cs$", false)]
        [Path(@"([a-zA-Z0-9]+)_Adapter\\.*\.cs$", false)]
        [Path(@"([a-zA-Z0-9]+)_UI\\.*\.cs$", false)]
        [ComplianceType("code")]
        public static Span ObjectNameMatchesFileName(this ClassDeclarationSyntax node)
        {
            if (node == null)
                return null;

            string filePath = node.SyntaxTree.FilePath;
            if (!string.IsNullOrEmpty(filePath))
            {
                string filename = System.IO.Path.GetFileNameWithoutExtension(filePath);
                if (node.IGetName() != filename)
                    return node.Identifier.Span.ToSpan();
            }

            return null;
        }
    }
}






