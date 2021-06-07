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

using BH.oM.Test;

namespace BH.Engine.Test.CodeCompliance.Checks
{
    public static partial class Query
    {
        [Message("Previous Input Name attribute is not unique.", "PreviousInputNamesAttributeIsUnique")]
        [ErrorLevel(TestStatus.Error)]
        [Path(@"([a-zA-Z0-9]+)_(Engine|Adapter)\\.*\.cs$")]
        [ComplianceType("documentation")]
        public static Span PreviousInputNamesAttributeIsUnique(this AttributeSyntax node)
        {
            if (node == null || node.Name.ToString() != "PreviousInputNames")
                return null;

            var method = node.Parent.Parent as BaseMethodDeclarationSyntax;
            if (method != null && method.IsPublic() && (method.IsEngineMethod() || method.IsAdapterConstructor()))
            {
                string paramname = node.ArgumentList.Arguments[0].Expression.GetFirstToken().Value.ToString();
                var firstOfName = method.PreviousInputNamesAttributes().First(attr => attr.ArgumentList.Arguments[0].Expression.GetFirstToken().Value.ToString() == paramname);
                if (firstOfName != node)
                    return node.Span.ToSpan();
            }
            return null;
        }
    }
}


