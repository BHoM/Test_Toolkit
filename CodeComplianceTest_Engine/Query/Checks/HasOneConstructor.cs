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
        [Message("BHoM objects should not contain multiple constructors taking inputs.", "HasOneConstructor")]
        [Path(@"([a-zA-Z0-9]+)_?oM\\.*\.cs$")]
        [Path(@"([a-zA-Z0-9]+)_Engine\\.*\.cs$", false)]
        [Path(@"([a-zA-Z0-9]+)_Adapter\\.*\.cs$", false)]
        [Path(@"([a-zA-Z0-9]+)_UI\\.*\.cs$", false)]
        [Path(@"([a-zA-Z0-9]+)_Tests\\.*\.cs$", false)]     //NUnit style projects
        [Path(@"([a-zA-Z0-9]+)_Test\\.*\.cs$", false)]      //Verification projects
        [ComplianceType("code")]
        [ErrorLevel(TestStatus.Error)]
        [Output("A span that represents where this error resides or null if there is no error")]
        public static Span HasOneConstructor(this ClassDeclarationSyntax node)
        {
            if (node == null)
                return null;

            if(node.Members.Where(x => x.IsConstructor()).Count() > 1)
            {
                List<ConstructorDeclarationSyntax> constructors = node.Members.Where(x => x.IsConstructor()).Select(x => x as ConstructorDeclarationSyntax).ToList();
                if (constructors.Where(x => x.ParameterList.Parameters.Count > 0).Count() > 1)
                    return constructors.Where(x => x.ParameterList.Parameters.Count > 0).First().Span.ToSpan(); //More than one constructor which has at least 1 input or more (accepting only an empty constructor when we have 2 constructors)
            }

            return null;
        }
    }
}





