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
        [Message("Files cannot contain more than one namespace", "HasSingleNamespace")]
        [Path(@"([a-zA-Z0-9]+)(_?oM|_(Engine|UI|Adapter|Tests))\\.*\.cs$")]
        [Path(@"([a-zA-Z0-9]+)_Engine\\Objects\\.*\.cs$", false)]
        [ComplianceType("code")]
        [ErrorLevel(TestStatus.Error)]
        public static Span HasSingleNamespace(this NamespaceDeclarationSyntax node)
        {
            if (node == null || !node.SyntaxTree.HasCompilationUnitRoot)
                return null;

            CompilationUnitSyntax root = node.SyntaxTree.GetRoot() as CompilationUnitSyntax;
            if (root.Members.OfType<NamespaceDeclarationSyntax>().FirstOrDefault() != node)
                return node.Name.Span.ToSpan();

            return null;
        }

    }
}






