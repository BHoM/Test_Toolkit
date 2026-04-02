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

            if (string.IsNullOrEmpty(filePath))
                return node.Identifier.Span.ToSpan();

            //Split the path, including the filename.
            //Split by dot to get rid of extension in .cs file.
            //Reverse to start with file and walk backwards
            List<string> pathSplit = filePath.Split(System.IO.Path.DirectorySeparatorChar).Select(x => x.Split('.').First()).Reverse().ToList();

            foreach (string returnType in ReturnTypeCandidates(node))
            {
                foreach (string path in pathSplit)
                {
                    if (path == "Create")   //Loop until we reach the create folder
                        break;

                    if (Regex.Match(returnType, $"((List|IEnumerable)<)?I?{path}(<.*>)?>?$").Success)
                        return null; //Name of file or folder matches return type exactly so this is valid. IsValidCreateMethodName will check if the method name matches the file name
                }
            }
            return node.Identifier.Span.ToSpan(); //Create method file (name/path) and return type do not match as required
        }

        private static List<string> ReturnTypeCandidates(this MethodDeclarationSyntax node)
        {
            List<string> returnTypeNames = new List<string>();

            var type = node.ReturnType;
            if (type is QualifiedNameSyntax)
                type = ((QualifiedNameSyntax)type).Right;

            string returnType = type.ToString();

            returnTypeNames.Add(returnType);

            //Handle the case where the method returns a generic type.
            //Then check the constraints of the generic type, if any of them matches the file name, then that is also acceptable
            if (node.ConstraintClauses.Count != 0)
            {
                foreach (var constraintClause in node.ConstraintClauses)
                {
                    string constraintString = constraintClause.ToString();  //where T : XXX, YYY
                    string[] split = constraintString.Split(':');
                    if (split.Length != 2)
                        continue;

                    string target = split[0].Trim().Split(' ').Last().Trim();  //Split with space to get rid of where

                    if (target != returnType)
                        continue;

                    returnTypeNames.AddRange(split[1].Split(',').Select(x => x.Trim()));    
                }
            }


            return returnTypeNames;
        }
    }
}







