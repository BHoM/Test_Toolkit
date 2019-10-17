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
        public static ComplianceResult IsValidModifyMethod(MethodDeclarationSyntax node, CodeContext ctx)
        {
            ComplianceResult result = Create.ComplianceResult(ResultStatus.Pass);

            if (ctx.Namespace.StartsWith("BH.Engine") && ctx.Class == "Modify")
            {
                List<LocalDeclarationStatementSyntax> statements = node.Body.Statements.Select(x => x as LocalDeclarationStatementSyntax).ToList();
                if(statements.Count > 0)
                {
                    if (statements[0] == null)
                        result = result.Merge(Create.ComplianceResult(ResultStatus.CriticalFail, new List<Error> { Create.Error($"Invalid Modify method: Modify method '{node.Identifier}' does not contain a suitable cloning statement in its first line. Cloning input objects are required to ensure upstream objects are not changed inadvertantly", node.Span.ToBHoM()) }));
                    else
                    {
                        string firstStatement = statements[0].ToString();
                        if (!firstStatement.Contains("DeepClone") && !firstStatement.Contains("ShallowClone") && !firstStatement.Contains("Copy"))
                            result = result.Merge(Create.ComplianceResult(ResultStatus.CriticalFail, new List<Error> { Create.Error($"Invalid Modify method: Modify method '{node.Identifier}' does not contain a suitable cloning statement in its first line. Cloning input objects are required to ensure upstream objects are not changed inadvertantly", node.Span.ToBHoM()) }));
                    }
                }
            }

            return result;
        }

    }
}
