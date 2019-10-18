/*
 * This file is part of the Buildings and Habitats object Model (BHoM)
 * Copyright (c) 2015 - 2019, the respective contributors. All rights reserved.
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

using BH.oM.Test;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BH.Engine.Test.Checks
{
    public static partial class Query
    {
        public static ComplianceResult HasCorrectAttributes(BaseMethodDeclarationSyntax node, CodeContext ctx)
        {
            ComplianceResult result = Create.ComplianceResult(ResultStatus.Pass);
            if (ctx != null && !string.IsNullOrWhiteSpace(ctx.Namespace) && ctx.Namespace.StartsWith("BH."))
            {
                string[] parts = ctx.Namespace.Split('.');
                string second = parts[1];

                if ((!node.IsConstructor() && second == "Engine") || (node.IsConstructor() && second == "Adapter"))
                {
                    if (node.HasAttribute("Description") && node.GetAttributes("Description").Count > 1)
                        result = result.Merge(Create.ComplianceResult(ResultStatus.CriticalFail, new List<Error> { Create.Error($"Method '{node.IGetName()}' cannot contain more than one Description attribute", node.Span.ToBHoM()) }));
                    else if(!node.HasAttribute("Description"))
                        result = result.Merge(Create.ComplianceResult(ResultStatus.CriticalFail, new List<Error> { Create.Error($"Method '{node.IGetName()}' requires a Description attribute", node.Span.ToBHoM()) }));
                    if (node.HasAttribute("Output") && node.GetAttributes("Output").Count > 1)
                        result = result.Merge(Create.ComplianceResult(ResultStatus.CriticalFail, new List<Error> { Create.Error($"Method '{node.IGetName()}' cannot contain more than one Output attribute - if you have more than one output consider using MultiOutput", node.Span.ToBHoM()) }));
                    else if(!node.HasAttribute("Output") && !node.HasAttribute("MultiOutput"))
                        result = result.Merge(Create.ComplianceResult(ResultStatus.CriticalFail, new List<Error> { Create.Error($"Method '{node.IGetName()}' requires an Output attribute", node.Span.ToBHoM()) }));

                    if(!node.HasAttribute("Input") && node.ParameterList != null)
                        result = result.Merge(Create.ComplianceResult(ResultStatus.CriticalFail, new List<Error> { Create.Error($"Method '{node.IGetName()}' requires an Input attribute", node.Span.ToBHoM()) }));
                    else if(node.HasAttribute("Input"))
                    {
                        List<AttributeListSyntax> attrib = node.InputAttributes();
                        ParameterListSyntax paramList = node.ParameterList;
                        foreach(ParameterSyntax p in paramList.Parameters)
                        {
                            AttributeListSyntax a = null;// attrib.Where(x => x.Attributes.Where(y => y.ArgumentList.Arguments.Count >= 1 && y.ArgumentList.Arguments[0].ToString() == p.Identifier.ToString()).ToList().Count > 0).FirstOrDefault();

                            foreach(AttributeListSyntax als in attrib)
                            {
                                foreach(var ab in als.Attributes)
                                {
                                    foreach(var ac in ab.ArgumentList.Arguments)
                                    {
                                        string acs = ac.Expression.GetFirstToken().Value.ToString();
                                        if (acs == p.Identifier.ToString())
                                            a = als;
                                    }
                                }
                            }

                            if (a == null)
                                result = result.Merge(Create.ComplianceResult(ResultStatus.CriticalFail, new List<Error> { Create.Error($"Input '{p.Identifier}' in method '{node.IGetName()}' requires an Input attribute", node.Span.ToBHoM()) }));
                            else
                                attrib.Remove(a);
                        }

                        if(attrib.Count > 0)
                            result = result.Merge(Create.ComplianceResult(ResultStatus.CriticalFail, new List<Error> { Create.Error($"Method '{node.IGetName()}' contains unnecessary Input attribute(s)", node.Span.ToBHoM()) }));
                    }
                }
            }

            return result;
        }
    }
}
