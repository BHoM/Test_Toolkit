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
        [Message("Input parameter requires a matching Input attribute", "IsInputAttributePresent")]
        [ErrorLevel(ErrorLevel.Warning)]
        [ComplianceType("documentation")]
        public static Span IsInputAttributePresent(this ParameterSyntax node)
        {
            var method = node.Parent.Parent as BaseMethodDeclarationSyntax;
            
            if (method != null && method.IsPublic() && (method.IsEngineMethod() || method.IsAdapterConstructor()) && !method.IsDeprecated())
            {
                foreach (var ab in method.InputAttributes())
                {
                    if ((ab.Name.ToString() == "Input" && ab.ArgumentList.Arguments.Count >= 2) || (ab.Name.ToString() == "InputFromProperty" && ab.ArgumentList.Arguments.Count >= 1))
                    {
                        string paramname = ab.ArgumentList.Arguments[0].Expression.GetFirstToken().Value.ToString();
                        if (paramname == node.Identifier.ToString())
                            return null;
                    }
                }
                return node.Identifier.Span.ToSpan();
            }
            return null;
        }
    }
}


