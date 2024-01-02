/*
 * This file is part of the Buildings and Habitats object Model (BHoM)
 * Copyright (c) 2015 - 2024, the respective contributors. All rights reserved.
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

using BH.oM.Base.Attributes;
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
        [Message("Method cannot contain more than one Output attribute", "HasUniqueOutputAttribute")]
        [ErrorLevel(TestStatus.Error)]
        [Path(@"([a-zA-Z0-9]+)_(Engine|Adapter)\\.*\.cs$")]
        [Path(@"([a-zA-Z0-9]+)_Engine\\Objects\\.*\.cs$", false)]
        [Path(@"([a-zA-Z0-9]+)_Tests\\.*\.cs$", false)]
        [ComplianceType("documentation")]
        public static Span HasUniqueOutputAttribute(this AttributeSyntax node)
        {
            if (node == null)
                return null;

            string name = node.Name.ToString();
            if (name != "Output" && name != "MultiOutput")
                return null;

            var method = node.Parent.Parent as BaseMethodDeclarationSyntax;
            if (method != null && method.IsPublic() && (method.IsEngineMethod() || method.IsAdapterConstructor()))
            {
                var outattrs = method.GetAttributes("Output");
                if (outattrs.Count > 0 && outattrs[0] != node)
                    return node.Span.ToSpan();

                List<AttributeSyntax> multiOutAttrs = method.GetAttributes("MultiOutput");
                if (multiOutAttrs.Where(x => x == node).Count() > 1)
                    return node.Span.ToSpan();
            }

            return null;
        }
    }
}
