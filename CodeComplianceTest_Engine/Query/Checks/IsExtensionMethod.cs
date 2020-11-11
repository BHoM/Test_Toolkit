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

using BH.oM.CodeComplianceTest;
using BH.oM.CodeComplianceTest.Attributes;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BH.Engine.Test.CodeCompliance.Checks
{
    public static partial class Query
    {

        [Message("Method must be an extension method", "IsExtensionMethod")]
        [Path(@"([a-zA-Z0-9]+)_Engine\\.*\.cs$")]
        [Path(@"([a-zA-Z0-9]+)_Engine\\Compute\\.*\.cs$", false)]
        [Path(@"([a-zA-Z0-9]+)_Engine\\Create\\.*\.cs$", false)]
        [IsPublic()]
        [ComplianceType("code")]
        public static Span IsExtensionMethod(this MethodDeclarationSyntax node)
        {
            var parameters = node.ParameterList.Parameters;
            if (parameters.Count > 0)
            {
                return parameters[0].Modifiers.Any(mod => mod.Kind() == SyntaxKind.ThisKeyword) ? null : parameters[0].Span.ToSpan();
            }
            return null;
        }

    }
}

