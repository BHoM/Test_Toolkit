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
        [Message("Input documentation attributes should be in the same order as the method input parameters.", "InputAttributesAreInOrder")]
        [ErrorLevel(TestStatus.Warning)]
        [Path(@"([a-zA-Z0-9]+)_(Engine|Adapter)\\.*\.cs$")]
        [Path(@"([a-zA-Z0-9]+)_Tests\\.*\.cs$", false)]     //NUnit style projects
        [Path(@"([a-zA-Z0-9]+)_Test\\.*\.cs$", false)]      //Verification projects
        [ComplianceType("documentation")]
        public static Span InputAttributesAreInOrder(this BaseMethodDeclarationSyntax node)
        {
            if (node == null)
                return null;

            var attributes = node.AttributeLists;
            var parameters = node.ParameterList;

            List<string> paramNames = parameters.Parameters.Select(x => x.Identifier.Text).ToList();
            List<string> attributeParamNames = attributes.SelectMany(x => x.Attributes.Select(y =>
            {
                if ((y.Name.ToString() == "Input" && y.ArgumentList.Arguments.Count >= 2) || (y.Name.ToString() == "InputFromProperty" && y.ArgumentList.Arguments.Count >= 1))
                    return y.ArgumentList.Arguments[0].Expression.GetFirstToken().Value.ToString();
                else
                    return null;
            }).Where(y => !string.IsNullOrEmpty(y))).ToList();

            int lastFoundIndex = -1;
            bool inOrder = true;

            for(int x = 0; x < paramNames.Count; x++)
            {
                if (x >= attributeParamNames.Count)
                    break;

                int indexOfAttribute = attributeParamNames.IndexOf(paramNames[x]);
                if (indexOfAttribute != -1)
                {
                    //Check if the current index is no more than 1 away from the last found index
                    if ((lastFoundIndex + 1) != indexOfAttribute)
                    {
                        //Last index + 1 is not equal to x - means the input attributes aren't in order
                        inOrder = false;
                        break;
                    }
                }

                lastFoundIndex = x;
            }

            if (!inOrder)
            {
                //Input attributes are not in consecutive order
                return node.Span.ToSpan();
            }

            return null;
        }
    }
}


