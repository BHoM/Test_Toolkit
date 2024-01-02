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
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

using System.IO;

using System.Xml.Linq;

using BH.oM.Test.CodeCompliance.Attributes;

using BH.oM.Test;
using BH.oM.Test.Results;

namespace BH.Engine.Test.CodeCompliance
{
    public static partial class Compute
    {
        public static TestResult CheckAssemblyInfo(this string assemblyInfoPath, string assemblyDescriptionOrg)
        {
            if (!assemblyInfoPath.EndsWith("AssemblyInfo.cs"))
                return Create.TestResult(TestStatus.Pass);

            if (!File.Exists(assemblyInfoPath))
                return Create.TestResult(TestStatus.Pass);

            TestResult finalResult = Create.TestResult(TestStatus.Pass);
            string documentationLink = "AssemblyInfo-compliance";

            List<string> fileLines = ReadFileContents(assemblyInfoPath);

            finalResult = CheckAssemblyVersion(fileLines, finalResult, assemblyInfoPath, documentationLink);
            finalResult = CheckAssemblyFileVersion(fileLines, finalResult, assemblyInfoPath, documentationLink);
            finalResult = CheckAssemblyDescription(fileLines, finalResult, assemblyInfoPath, documentationLink, assemblyDescriptionOrg);

            return finalResult;
        }

        private static TestResult CheckAssemblyVersion(this List<string> fileLines, TestResult finalResult, string assemblyInfoPath, string documentationLink)
        {
            string currentAssemblyVersion = Query.FullCurrentAssemblyVersion();

[assembly: AssemblyVersion("7.0.0.0")]

            string foundLine = fileLines.Where(x => x.StartsWith(searchLine)).FirstOrDefault();

            if (string.IsNullOrEmpty(foundLine))
                return finalResult.Merge(Create.TestResult(TestStatus.Error, new List<Error> { Create.Error($"Assembly Version should be set to {currentAssemblyVersion}", Create.Location(assemblyInfoPath, Create.LineSpan(1, 1)), documentationLink) }));
            else
            {
                string allowedLine = $"{searchLine}{Query.FullCurrentAssemblyVersion()}\")]";
                int line = fileLines.IndexOf(foundLine) + 1;
                if (!foundLine.Contains(allowedLine))
                    return finalResult.Merge(Create.TestResult(TestStatus.Error, new List<Error> { Create.Error($"Assembly Version should be set to {currentAssemblyVersion}", Create.Location(assemblyInfoPath, Create.LineSpan(line, line)), documentationLink) }));
            }

            return finalResult;
        }

        private static TestResult CheckAssemblyFileVersion(this List<string> fileLines, TestResult finalResult, string assemblyInfoPath, string documentationLink)
        {
            string currentAssemblyVersion = Query.FullCurrentAssemblyFileVersion();

[assembly: AssemblyFileVersion("7.1.0.0")]

            string foundLine = fileLines.Where(x => x.StartsWith(searchLine)).FirstOrDefault();

            if (string.IsNullOrEmpty(foundLine))
                return finalResult.Merge(Create.TestResult(TestStatus.Error, new List<Error> { Create.Error($"Assembly File Version should be set to {currentAssemblyVersion}", Create.Location(assemblyInfoPath, Create.LineSpan(1, 1)), documentationLink) }));
            else
            {
                string allowedLine = $"{searchLine}{Query.FullCurrentAssemblyFileVersion()}\")]";
                int line = fileLines.IndexOf(foundLine) + 1;
                if (!foundLine.Contains(allowedLine))
                    return finalResult.Merge(Create.TestResult(TestStatus.Error, new List<Error> { Create.Error($"Assembly File Version should be set to {currentAssemblyVersion}", Create.Location(assemblyInfoPath, Create.LineSpan(line, line)), documentationLink) }));
            }

            return finalResult;
        }

        private static TestResult CheckAssemblyDescription(this List<string> fileLines, TestResult finalResult, string assemblyInfoPath, string documentationLink, string descriptionUrl)
        {
[assembly: AssemblyDescription("https://github.com/BHoM/Test_Toolkit")]

            string foundLine = fileLines.Where(x => x.StartsWith(searchLine)).FirstOrDefault();

            if (string.IsNullOrEmpty(foundLine))
                return finalResult.Merge(Create.TestResult(TestStatus.Error, new List<Error> { Create.Error($"Assembly Description should contain the URL to the GitHub organisation which owns the repository", Create.Location(assemblyInfoPath, Create.LineSpan(1, 1)), documentationLink) }));
            else
            {
                string allowedLine = $"{searchLine}{descriptionUrl}\")]";
                int line = fileLines.IndexOf(foundLine) + 1;
                if (!foundLine.Contains(allowedLine))
                    return finalResult.Merge(Create.TestResult(TestStatus.Error, new List<Error> { Create.Error($"Assembly Description should contain the URL to the GitHub organisation which owns the repository", Create.Location(assemblyInfoPath, Create.LineSpan(line, line)), documentationLink) }));
            }

            return finalResult;
        }
    }
}





