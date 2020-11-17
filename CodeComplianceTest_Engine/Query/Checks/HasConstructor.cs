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

using BH.oM.Reflection.Attributes;
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
        [Message("BHoM objects should not contain a constructor unless they are implementing the IImmutable interface. If the object is implementing the IImutable interface then it must contain a constructor.", "HasConstructor")]
        [Path(@"([a-zA-Z0-9]+)_?oM\\.*\.cs$")]
        [Path(@"([a-zA-Z0-9]+)_Engine\\.*\.cs$", false)]
        [Path(@"([a-zA-Z0-9]+)_Adapter\\.*\.cs$", false)]
        [Path(@"([a-zA-Z0-9]+)_UI\\.*\.cs$", false)]
        [ComplianceType("code")]
        [ErrorLevel(ErrorLevel.Error)]
        [Output("A span that represents where this error resides or null if there is no error")]
        public static Span HasConstructor(this ClassDeclarationSyntax node)
        {
            if (node.HasAConstructor() && (node.BaseList != null && node.BaseList.Types.Where(x => x.Type.ToString().ToLower() == "iimmutable").FirstOrDefault() == null))
                return node.Members.Where(x => x.IsConstructor()).FirstOrDefault().Span.ToSpan(); //Has a constructor but isn't implementing the Immutable interface, this is not good

            if (!node.HasAConstructor() && (node.BaseList != null && node.BaseList.Types.Where(x => x.Type.ToString().ToLower() == "iimmutable").FirstOrDefault() != null))
                return node.Span.ToSpan(); //Has no constructor, but is implementing the Immutable interface, this is not good

            return null;
        }
    }
}

