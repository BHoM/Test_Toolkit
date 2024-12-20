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

using BH.Engine.Base;
using BH.oM.Test.CodeCompliance;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

using System.IO;
using BH.oM.Test.CodeCompliance.Attributes;

using BH.oM.Test;
using BH.oM.Test.Results;

namespace BH.Engine.Test.CodeCompliance
{
    public static partial class Compute
    {
        public static TestResult Check(this MethodInfo method, SyntaxNode node, string checkType = null)
        {
            TestResult finalResult = Create.TestResult(TestStatus.Pass);
            if (method == null || node == null)
                return finalResult;

            string path = node.SyntaxTree.FilePath;
            if (Path.GetFileName(path) == "AssemblyInfo.cs")
                return finalResult;

            Type type = node.GetType();
                    
            if (method.GetParameters()[0].ParameterType.IsAssignableFrom(type) &&
                    !(typeof(MemberDeclarationSyntax).IsAssignableFrom(node.GetType())
                    && ((MemberDeclarationSyntax)node).IsDeprecated()) &&
                    method.GetCustomAttributes<ConditionAttribute>().All(condition => condition.IPasses(node)) &&
                    (checkType != null && method.GetCustomAttribute<ComplianceTypeAttribute>()?.ComplianceType == checkType))
            {
                Func<object[], object> fn = method.ToFunc();
                Span result = fn(new object[] { node }) as Span;
                if (result != null)
                {
                    string message = method.GetCustomAttribute<MessageAttribute>()?.Message ?? "";
                    string documentation = method.GetCustomAttribute<MessageAttribute>()?.DocumentationLink ?? "";

                    TestStatus errLevel = method.GetCustomAttribute<ErrorLevelAttribute>()?.Level ?? TestStatus.Error;
                    finalResult = finalResult.Merge(Create.TestResult(
                        errLevel == TestStatus.Error ? TestStatus.Error : TestStatus.Warning,
                        new List<Error> {
                Create.Error(message, Create.Location(path, result.ToLineSpan(node.SyntaxTree.GetRoot().ToFullString())), documentation, errLevel, method.Name)
                        }));
                }
            }

            return finalResult.Merge(method.Check(node.ChildNodes(), checkType));
        }

        public static TestResult Check(this MethodInfo method, IEnumerable<SyntaxNode> nodes, string checkType = null)
        {
            TestResult finalResult = Create.TestResult(TestStatus.Pass);
            if (method == null || nodes == null)
                return finalResult;

            foreach(var node in nodes)
            {
                finalResult = finalResult.Merge(method.Check(node, checkType));
            }
            return finalResult;
        }
    }
}






