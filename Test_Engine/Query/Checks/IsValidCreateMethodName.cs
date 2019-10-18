/*
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
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BH.Engine.Test.Checks
{
    public static partial class Query
    {
        public static ComplianceResult IsValidCreateMethodName(MethodDeclarationSyntax node)
        {
            if(node.IsPublic() && node.IGetNamespace().StartsWith("BH.Engine") && node.IGetClass() == "Create")
            {
                string name = node.Identifier.Text;
                var type = node.ReturnType;
                if (type is QualifiedNameSyntax) type = ((QualifiedNameSyntax)type).Right;

                string returntype = type.ToString();
                var match = System.Text.RegularExpressions.Regex.Match(returntype, $"^I?{name}(<.+>)?$");

                if (!match.Success)
                {
                    return Create.ComplianceResult(ResultStatus.CriticalFail, new List<Error> {
                        Create.Error($"Invalid Create method name: Method '{node.Identifier}' name must have the same name as its return type", node.Identifier.Span.ToBHoM())
                    });
                }
            }
            return Create.ComplianceResult(ResultStatus.Pass);
        }

    }
}
