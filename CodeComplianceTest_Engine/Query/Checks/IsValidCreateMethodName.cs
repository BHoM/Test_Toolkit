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
using BH.oM.Test.Attributes;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace BH.Engine.CodeComplianceTest.Checks
{
    public static partial class Query
    {
        [Message("Invalid Create method name: Method name must start with or end with the same name as its return type")]
        [Path(@"([a-zA-Z0-9]+)_Engine\\Create\\.*\.cs$")]
        [IsPublic()]
        public static Span IsValidCreateMethodName(MethodDeclarationSyntax node)
        {
            string name = node.Identifier.Text;
            var type = node.ReturnType;
            if (type is QualifiedNameSyntax) type = ((QualifiedNameSyntax)type).Right;
            string returnType = type.ToString();

            if (!name.StartsWith(returnType) && !name.EndsWith(returnType))
            {
                List<string> returnSplit = CamelCaseSplit(name);
                List<string> nameSplit = CamelCaseSplit(name);

                if (returnSplit[0] == "List") // Takes care of the case of lists
                {
                    returnSplit = returnSplit.Skip(1).ToList();
                    returnSplit[returnSplit.Count - 1] += 's';
                }

                if (returnSplit.First() != nameSplit.First() && returnSplit.Last() != nameSplit.Last())
                    return node.Identifier.Span.ToBHoM(); //Name is not compliant
            }

            return null;
        }

        private static List<string> CamelCaseSplit(string input)
        {
            List<string> values = new List<string>();
            int pos = 0;
            foreach (Match m in Regex.Matches(input, "[A-Z]"))
            {
                if (m.Index > 0)
                    values.Add(input.Substring(pos, m.Index - pos));
                pos = m.Index + m.Length - 1;
            }
            values.Add(input.Substring(pos));

            return values;
        }

    }
}
