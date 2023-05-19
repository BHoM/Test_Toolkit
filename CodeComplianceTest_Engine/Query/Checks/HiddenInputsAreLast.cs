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

using BH.oM.Test;

namespace BH.Engine.Test.CodeCompliance.Checks
{
    public static partial class Query
    {
        [Message("UIExposure for some inputs are set to hidden, but the corresponding input parameter is not last in the parameter list.", "HiddenInputsAreLast")]
        [ErrorLevel(TestStatus.Warning)]
        [Path(@"([a-zA-Z0-9]+)_(Engine|Adapter)\\.*\.cs$")]
        [Path(@"([a-zA-Z0-9]+)_Tests\\.*\.cs$", false)]
        [ComplianceType("documentation")]
        public static Span HiddenInputsAreLast(this BaseMethodDeclarationSyntax node)
        {
            if (node == null)
                return null;

            var attributes = node.AttributeLists;
            var parameters = node.ParameterList;

            List<int> hiddenParamIndexes = new List<int>();

            foreach (var attributeList in attributes)
            {
                foreach (var attribute in attributeList.Attributes)
                {
                    var exposureParam = attribute.ArgumentList.Arguments[2];
                    if (exposureParam.ToString().Split('.').Last() == "Hidden")
                    {
                        string paramName = attribute.ArgumentList.Arguments[0].Expression.GetFirstToken().Value.ToString();
                        var param = parameters.Parameters.Where(p => p.Identifier.Text == paramName).FirstOrDefault();

                        if (param != null)
                            hiddenParamIndexes.Add(parameters.Parameters.IndexOf(param));
                    }
                }
            }

            bool isConsecutive = !hiddenParamIndexes.Select((i, j) => i - j).Distinct().Skip(1).Any();

            if(!isConsecutive || hiddenParamIndexes.Last() < parameters.Parameters.Count)
            {
                //The hidden parameters have a non-hidden parameter somewhere between them, or the last index is less than the total number of inputs
                return node.Span.ToSpan();
            }

            return null;
        }
    }
}
