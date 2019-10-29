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

namespace BH.Engine.Test.Checks
{
    public static partial class Query
    {
        [Message("Namespace is incorrect based on file path")]
        [Path(@"([A-Za-z0-9]+)_(Engine|oM|Adapter|UI)\\.*\.cs$")]
        public static Span NamespaceMatchesFilePath(NamespaceDeclarationSyntax node)
        {
            string ns = node.IGetNamespace();
            string path = node.SyntaxTree.FilePath;
            MatchCollection matches = Regex.Matches(path, "([A-Za-z0-9]+)_(oM|Engine|Adapter|UI)");
            if (matches.Count > 0)
            {
                Match match = matches[matches.Count - 1];
                string rootNs = $"BH.{match.Groups[2]}.{match.Groups[1]}";
                if (!ns.StartsWith(rootNs))
                {
                    return node.Name.Span.ToBHoM();
                }
            }

            return null;
        }



    }
}
