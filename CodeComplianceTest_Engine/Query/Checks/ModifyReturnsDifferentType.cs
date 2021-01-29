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

using BH.oM.Test.CodeCompliance;
using BH.oM.Test.CodeCompliance.Attributes;
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

        [Message("Modify methods should return void, or their return type should be different to the input type of their first parameter", "ModifyReturnsDifferentType")]
        [Path(@"([a-zA-Z0-9]+)_Engine\\Modify\\.*\.cs$")]
        [IsPublic()]
        [ComplianceType("code")]
        public static Span ModifyReturnsDifferentType(this MethodDeclarationSyntax node)
        {
            if (node.ReturnType.ToString().ToLower() == "void")
                return null;

            ParameterSyntax param = node.ParameterList.Parameters.FirstOrDefault();
            if (param == null || param.Type.IsEquivalentTo(node.ReturnType))
                return node.ReturnType.Span.ToSpan(); //The return type matches the input type which is not valid for Modify methods, they should be returning void instead

            return null;
        }

    }
}


