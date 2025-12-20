/*
 * This file is part of the Buildings and Habitats object Model (BHoM)
 * Copyright (c) 2015 - 2026, the respective contributors. All rights reserved.
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
        [Message("UIExposure for input is set to hidden, but the corresponding input parameter does not contain a default value.", "UIExposureHasDefaultValue")]
        [ErrorLevel(TestStatus.Error)]
        [Path(@"([a-zA-Z0-9]+)_(Engine|Adapter)\\.*\.cs$")]
        [Path(@"([a-zA-Z0-9]+)_Tests\\.*\.cs$", false)]     //NUnit style projects
        [Path(@"([a-zA-Z0-9]+)_Test\\.*\.cs$", false)]      //Verification projects
        [ComplianceType("documentation")]
        public static Span UIExposureHasDefaultValue(this AttributeSyntax node)
        {
            if (node == null || node.Name.ToString() != "Input")
                return null;

            var method = node.Parent.Parent as BaseMethodDeclarationSyntax;
            if (method != null && method.IsPublic() && (method.IsEngineMethod() || method.IsAdapterConstructor()))
            {
                if ((node.Name.ToString() == "Input" && node.ArgumentList.Arguments.Count >= 3))
                {
                    var exposureParam = node.ArgumentList.Arguments[2];
                    if (exposureParam.ToString().Split('.').Last() == "Hidden")
                    {
                        string paramName = node.ArgumentList.Arguments[0].Expression.GetFirstToken().Value.ToString();
                        var param = method.ParameterList.Parameters.Where(p => p.Identifier.Text == paramName).FirstOrDefault();
                        if (param != null)
                        {
                            if (param.Default == null)
                                return param.Span.ToSpan();

                            if (param.Default.Value == null)
                                return param.Span.ToSpan();
                        }
                    }
                }
            }

            return null;
        }
    }
}



