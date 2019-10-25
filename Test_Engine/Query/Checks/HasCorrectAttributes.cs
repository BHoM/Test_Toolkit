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
        public static ComplianceResult HasCorrectAttributes(BaseMethodDeclarationSyntax node)
        {
            ComplianceResult result = Create.ComplianceResult(ResultStatus.Pass);
            string ns = node.IGetNamespace();
            if (ns.StartsWith("BH."))
            {
                string[] parts = ns.Split('.');
                string second = parts[1];

                if ((!node.IsConstructor() && second == "Engine") || (node.IsConstructor() && second == "Adapter"))
                {
                    var descattrs = node.GetAttributes("Description");
                    if (descattrs.Count == 0)
                    {
                        result = result.Merge(Create.ComplianceResult(ResultStatus.Fail, new List<Error> { Create.Error($"Method '{node.IGetName()}' requires a Description attribute", node.IGetIdentifier().Span.ToBHoM(), ErrorLevel.Warning) }));
                    }
                    else if (node.GetAttributes("Description").Count > 1)
                    {
                        for (int i = 1; i < descattrs.Count; i++)
                        {
                            result = result.Merge(Create.ComplianceResult(ResultStatus.Fail, new List<Error> { Create.Error($"Method '{node.IGetName()}' cannot contain more than one Description attribute", descattrs[i].Span.ToBHoM(), ErrorLevel.Warning) }));
                        }
                    }

                    var outputattrs = node.GetAttributes("Output");
                    if (outputattrs.Count == 0 && !node.HasAttribute("MultiOutput"))
                    {
                        result = result.Merge(Create.ComplianceResult(ResultStatus.Fail, new List<Error> { Create.Error($"Method '{node.IGetName()}' requires an Output attribute", node.IGetIdentifier().Span.ToBHoM(), ErrorLevel.Warning) }));
                    }
                    else if(outputattrs.Count > 1)
                    {
                        for (int i = 1; i < outputattrs.Count; i++)
                        {
                            result = result.Merge(Create.ComplianceResult(ResultStatus.Fail, new List<Error> { Create.Error($"Method '{node.IGetName()}' cannot contain more than one Output attribute - if you have more than one output consider using MultiOutput", outputattrs[i].Span.ToBHoM(), ErrorLevel.Warning) }));
                        }
                    }

                    List<AttributeListSyntax> attrib = node.InputAttributes();
                    ParameterListSyntax paramList = node.ParameterList;
                    foreach (ParameterSyntax p in paramList.Parameters)
                    {
                        AttributeListSyntax a = null;
                        foreach (AttributeListSyntax als in attrib)
                        {
                            foreach (var ab in als.Attributes)
                            {
                                foreach (var ac in ab.ArgumentList.Arguments)
                                {
                                    string acs = ac.Expression.GetFirstToken().Value.ToString();
                                    if (acs == p.Identifier.ToString())
                                        a = als;
                                }
                            }
                        }

                        if (a == null)
                            result = result.Merge(Create.ComplianceResult(ResultStatus.Fail, new List<Error> { Create.Error($"Input '{p.Identifier}' in method '{node.IGetName()}' requires an Input attribute", p.Identifier.Span.ToBHoM(), ErrorLevel.Warning) }));
                        else
                            attrib.Remove(a);
                    }

                    if (attrib.Count > 0)
                    {
                        foreach (var attr in attrib)
                        {
                            result = result.Merge(Create.ComplianceResult(ResultStatus.Fail, new List<Error> { Create.Error($"Unneccesary attribute", attr.Span.ToBHoM(), ErrorLevel.Warning) }));
                        }
                    }
                }
            }

            return result;
        }
    }
}
