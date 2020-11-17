﻿/*
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

using BH.oM.Test.CodeCompliance;
using BH.oM.Test.CodeCompliance.Attributes;
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
        [Message("Methods implementing a PreviousVersion versioning attribute should implement it for the current version of development. Methods with PreviousVersion attributes from previous milestones should have those attributes removed", "HasValidPreviousVersionAttribute")]
        [ErrorLevel(ErrorLevel.Error)]
        [Path(@"([a-zA-Z0-9]+)_Engine\\.*\.cs$")]
        [Path(@"([a-zA-Z0-9]+)_Engine\\Objects\\.*\.cs$", false)]
        [IsPublic()]
        [ComplianceType("documentation")]
        public static Span HasValidPreviousVersionAttribute(this MethodDeclarationSyntax node)
        {
            if (node.IsDeprecated() || !node.HasAttribute("PreviousVersion"))
                return null; //Don't care about deprecated methods or if the method does not have the attribute at all

            List<AttributeSyntax> multiOutAttrs = node.GetAttributes("PreviousVersion");

            if (multiOutAttrs.Count > 1)
                return node.Identifier.Span.ToSpan();

            string currentVersion = "4.0"; //Update each milestone
            string givenVersion = multiOutAttrs.First().ArgumentList.Arguments[0].Expression.GetFirstToken().Value.ToString();

            if (givenVersion != currentVersion)
                return node.Identifier.Span.ToSpan();

            return null; //All ok
        }

        [Message("Methods implementing a PreviousVersion versioning attribute should implement it for the current version of development. Methods with PreviousVersion attributes from previous milestones should have those attributes removed", "HasValidPreviousVersionAttribute")]
        [ErrorLevel(ErrorLevel.Error)]
        [Path(@"([a-zA-Z0-9]+)_Adapter\\.*\.cs$")]
        [IsPublic()]
        [ComplianceType("documentation")]
        public static Span HasValidPreviousVersionAttribute(this ConstructorDeclarationSyntax node)
        {
            if (node.IsDeprecated() || !node.HasAttribute("PreviousVersion"))
                return null; //Don't care about deprecated methods or if the method does not have the attribute at all

            List<AttributeSyntax> multiOutAttrs = node.GetAttributes("PreviousVersion");

            if (multiOutAttrs.Count > 1)
                return node.Span.ToSpan();

            string currentVersion = "4.0"; //Update each milestone
            string givenVersion = multiOutAttrs.First().ArgumentList.Arguments[0].Expression.GetFirstToken().Value.ToString();

            if (givenVersion != currentVersion)
                return node.Span.ToSpan();

            return null; //All ok
        }
    }
}
