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
        [Message("Convert method name invalid: Method name must begin with either 'ITo' or 'To' or 'IFrom' or 'From' and not contain 'ToBHoM' or 'FromBHoM'", "IsValidConvertMethodName")]
        [Path(@"([a-zA-Z0-9]+)_Engine\\Convert\\.*\.cs$")]
        [Path(@"([a-zA-Z0-9]+)_Adapter\\Convert\\.*\.cs$")]
        [IsPublic()]
        [ComplianceType("code")]
        public static Span IsValidConvertMethodName(this MethodDeclarationSyntax node)
        {
            string name = node.Identifier.Text;
            return ((name.StartsWith("ITo") || name.StartsWith("To")) && !name.StartsWith("ToBHoM")) ||
                ((name.StartsWith("IFrom") || name.StartsWith("From")) && !name.StartsWith("FromBHoM")) ? null : node.Identifier.Span.ToSpan();
        }

    }
}

