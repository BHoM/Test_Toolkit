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

using BH.oM.Test.CodeCompliance;
using BH.oM.Test.CodeCompliance.Attributes;
using Microsoft.CodeAnalysis.CSharp;
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
        [Message("Methods returning a type of Output<t1, ..., tn> should have a matching number of MultiOutput attributes.", "HasValidMultiOutputAttributes")]
        [ErrorLevel(TestStatus.Error)]
        [Path(@"([a-zA-Z0-9]+)_Engine\\.*\.cs$")]
        [Path(@"([a-zA-Z0-9]+)_Engine\\Objects\\.*\.cs$", false)]
        [Path(@"([a-zA-Z0-9]+)_Tests\\.*\.cs$", false)]
        [IsPublic()]
        [ComplianceType("documentation")]
        public static Span HasValidMultiOutputAttributes(this MethodDeclarationSyntax node)
        {
            if (node == null)
                return null;

            bool isvoid = false;
            if (node.ReturnType is PredefinedTypeSyntax)
                isvoid = ((PredefinedTypeSyntax)node.ReturnType).Keyword.Kind() == SyntaxKind.VoidKeyword;

            if (isvoid || node.IsDeprecated() || node.HasOutputAttribute() != null)
                return null; //Don't care about void return types or deprecated methods or methods with no output attribute at all

            string returnType = node.ReturnType.ToString();

            if (!returnType.StartsWith("Output<"))
                return null; //Not a multi output return type

            returnType = returnType.Substring(7);//Trim the 'Output<' from the string
            returnType = returnType.Substring(0, returnType.Length - 1); //Trim the final '>' from the string

            List<string> returnOptions = new List<string>();
            int split = 0;
            string builtString = "";
            foreach (char x in returnType)
            {
                if (x == '<')
                    split++;

                if(x == '>')
                    split--;

                if (x == ',' && split == 0)
                {
                    //Either splitting at a comma, or end of the string
                    returnOptions.Add(builtString);
                    builtString = "";
                }
                else
                    builtString += x;
            }
            returnOptions.Add(builtString); //Add the last built string that wasn't separated by a comma

            List<AttributeSyntax> multiOutAttrs = node.GetAttributes("MultiOutput");

            if (returnOptions.Count != multiOutAttrs.Count)
                return node.Identifier.Span.ToSpan();
            else
                return null;
        }
    }
}





