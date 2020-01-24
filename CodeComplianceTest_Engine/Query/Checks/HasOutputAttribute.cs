﻿/*
 * This file is part of the Buildings and Habitats object Model (BHoM)
 * Copyright (c) 2015 - 2019, the respective contributors. All rights reserved.
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

using BH.oM.Test;
using BH.oM.Test.Attributes;
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
        [Message("Method must contain an Output or MultiOutput attribute")]
        [ErrorLevel(ErrorLevel.Warning)]
        [Path(@"([a-zA-Z0-9]+)_Engine\\.*\.cs$")]
        [Path(@"([a-zA-Z0-9]+)_Engine\\Objects\\.*\.cs$", false)]
        [IsPublic()]
        public static Span HasOutputAttribute(MethodDeclarationSyntax node)
        {
           
           
            return ((PredefinedTypeSyntax)node.ReturnType).Keyword.Kind() == SyntaxKind.VoidKeyword
                || node.HasAttribute("Output") || node.HasAttribute("MultiOutput")  ? null : node.Identifier.Span.ToBHoM();
        }

        [Message("Method must contain an Output or MultiOutput attribute")]
        [ErrorLevel(ErrorLevel.Warning)]
        [Path(@"([a-zA-Z0-9]+)_Adapter\\.*\.cs$")]
        [IsPublic()]
        public static Span HasOutputAttribute(ConstructorDeclarationSyntax node)
        {
            return node.HasAttribute("Output") || node.HasAttribute("MultiOutput")  ? null : node.Identifier.Span.ToBHoM();
        }
    }
}
