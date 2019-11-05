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

using BH.Engine.Reflection;
using BH.oM.Test;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

using System.IO;
using BH.oM.Test.Attributes;

namespace BH.Engine.Test
{
    public static partial class Compute
    {
        public static ComplianceResult RunChecks(this SyntaxNode node)
        {
            string path = node.SyntaxTree.FilePath;
            if (Path.GetFileName(path) == "AssemblyInfo.cs")
                return Create.ComplianceResult(ResultStatus.Pass);

            Type type = node.GetType();
            IEnumerable<MethodInfo> checks = Assembly.GetCallingAssembly().DefinedTypes
                .Where(t => t.IsClass && t.Name == "Query" && t.Namespace == "BH.Engine.Test.Checks")
                .SelectMany(t => t.DeclaredMethods)
                .Where(method => method.IsPublic && method.ReturnType == typeof(Span) && method.GetParameters()[0].ParameterType.IsAssignableFrom(type));

            ComplianceResult finalResult = Create.ComplianceResult(ResultStatus.Pass);
            foreach(MethodInfo method in checks)
            {
                if (!method.GetCustomAttributes<ConditionAttribute>().All(condition => condition.IPasses(node)))
                    continue;

                Func<object[], object> fn = method.ToFunc();
                Span result = fn(new object[] { node }) as Span;
                if (result != null)
                {
                    string message = method.GetCustomAttribute<MessageAttribute>()?.Message??"";
                    ErrorLevel errLevel = method.GetCustomAttribute<ErrorLevelAttribute>()?.Level??ErrorLevel.Error;
                    finalResult = finalResult.Merge(
                        Create.ComplianceResult(
                            errLevel == ErrorLevel.Error ? ResultStatus.CriticalFail : ResultStatus.Fail,
                            new List<Error> {
                                Create.Error(message, Create.Location(path, result.ToLineSpan(node.SyntaxTree.GetRoot().ToFullString())), errLevel, method.Name)
                            })
                        );
                }
            }
            return finalResult;
        }
    }
}
