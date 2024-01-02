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
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using BH.oM.Test;

namespace BH.Engine.Test.CodeCompliance.Checks
{
    public static partial class Query
    {

        [Message("A Create method return type must either match its file name, (e.g. Engine/Create/Panel.cs returning a type of Panel), OR the file must sit in a sub-folder of Create which matches the return type (e.g. Engine/Create/Panel/MyNewPanel.cs).", "IsValidCreateMethod")]
        [Path(@"([a-zA-Z0-9]+)_Engine\\Create\\.*\.cs$")]
        [IsPublic()]
        [ComplianceType("code")]
        [ErrorLevel(TestStatus.Error)]
        public static Span IsValidCreateMethod(this MethodDeclarationSyntax node)
        {
            if (node == null)
                return null;

            string filePath = node.SyntaxTree.FilePath;
            var type = node.ReturnType;
            if (type is QualifiedNameSyntax)
                type = ((QualifiedNameSyntax)type).Right;
            
            string returnType = type.ToString();
            string fileName = "";

            if(!string.IsNullOrEmpty(filePath))
            {
                fileName = System.IO.Path.GetFileNameWithoutExtension(filePath);
                if(Regex.Match(returnType, $"((List|IEnumerable)<)?I?{fileName}(<.*>)?>?$").Success)
                    return null; //File name matches return type exactly so this is valid. IsValidCreateMethodName will check if the method name matches the file name
            }

            //If file name does not exactly match the return type then we need to check if the return type is in a sub-folder in create

            List<string> pathSplit = filePath.Split(System.IO.Path.DirectorySeparatorChar).ToList();
            int createIndex = pathSplit.IndexOf("Create");
            if (createIndex == -1)
                return node.Identifier.Span.ToSpan(); //Evidently this create method isn't working for some reason - even though it should but this is as a protection/precaution

            try
            {
                if (Regex.Match(returnType, $"((List|IEnumerable)<)?I?{pathSplit[createIndex + 1]}(<.*>)?>?$").Success || Regex.Match(returnType, $"((List|IEnumerable)<)?I?{pathSplit[createIndex + 2]}(<.*>)?>?$").Success)
                    return null; //The folder path after the 'Create' folder matches the return type so this is valid. IsValidCreateMethodName will check if the method name matches the file name
            }
            catch
            { 
                //In case createIndex + 1 || createIndex + 2 result in an out of bounds error - it means the check has failed and something isn't compliant so can pass through to returning the span
            }

            return node.Identifier.Span.ToSpan(); //Create method file (name/path) and return type do not match as required
        }
    }
}





