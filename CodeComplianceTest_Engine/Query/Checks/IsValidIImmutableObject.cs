/*
 * This file is part of the Buildings and Habitats object Model (BHoM)
 * Copyright (c) 2015 - 2023, the respective contributors. All rights reserved.
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

using BH.Engine.Test.CodeCompliance;

using BH.oM.Test;

namespace BH.Engine.Test.CodeCompliance.Checks
{
    public static partial class Query
    {
        [Message("IImutable objects must have at least one property with only a get accessor (no set accessor).", "IsValidIImmutableObject")]
        [ErrorLevel(TestStatus.Error)]
        [Path(@"([a-zA-Z0-9]+)_?oM\\.*\.cs$")]
        [Path(@"([a-zA-Z0-9]+)_Engine\\.*\.cs$", false)]
        [Path(@"([a-zA-Z0-9]+)_Adapter\\.*\.cs$", false)]
        [Path(@"([a-zA-Z0-9]+)_UI\\.*\.cs$", false)]
        [ComplianceType("code")]
        public static Span IsValidIImmutableObject(this ClassDeclarationSyntax node)
        {
            if (node == null || node.BaseList == null || node.BaseList.Types.Where(x => x.Type.ToString().ToLower() == "iimmutable").FirstOrDefault() == null)
                return null; //Not an IImmutable object so not relevant

            List<PropertyDeclarationSyntax> properties = node.Members.Where(x => x is PropertyDeclarationSyntax).Select(x => x as PropertyDeclarationSyntax).ToList();
            properties = properties.Where(x => x.AccessorList.Accessors.Where(y => y.ToString() == "set;").Count() == 0).ToList(); //Obtain the 'get' only properties

            if (properties.Count == 0)
                return node.Span.ToSpan(); //Object has no `get` only properties, not valid to be IImmutable

            return null; //All good, object has at least one `get` only property, so valid
        }
    }
}




