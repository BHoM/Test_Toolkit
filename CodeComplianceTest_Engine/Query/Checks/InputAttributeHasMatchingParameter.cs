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
        [Message("Input attribute does not match any of the given parameters", "InputAttributeHasMatchingParameter")]
        [ErrorLevel(TestStatus.Error)]
        [Path(@"([a-zA-Z0-9]+)_(Engine|Adapter)\\.*\.cs$")]
        [Path(@"([a-zA-Z0-9]+)_Tests\\.*\.cs$", false)]
        [ComplianceType("documentation")]
        public static Span InputAttributeHasMatchingParameter(this AttributeSyntax node)
        {
            if (node == null || node.Name.ToString() != "Input" && node.Name.ToString() != "InputFromProperty")
                return null;

            var method = node.Parent.Parent as BaseMethodDeclarationSyntax;
            if (method != null && method.IsPublic() && (method.IsEngineMethod() || method.IsAdapterConstructor()))
            {
                if ((node.Name.ToString() == "Input" && node.ArgumentList.Arguments.Count >= 2) || (node.Name.ToString() == "InputFromProperty" && node.ArgumentList.Arguments.Count >= 1))
                {
                    string paramname = node.ArgumentList.Arguments[0].Expression.GetFirstToken().Value.ToString();
                    if (method.ParameterList.Parameters.Any((p) => p.Identifier.Text == paramname))
                        return null;
                    else
                        return node.ArgumentList.Arguments[0].Span.ToSpan();
                }
                return node.Span.ToSpan();
            }

            return null;
        }
    }
}


