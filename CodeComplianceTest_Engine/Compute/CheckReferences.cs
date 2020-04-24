/*
 * This file is part of the Buildings and Habitats object Model (BHoM)
 * Copyright (c) 2015 - 2020, the respective contributors. All rights reserved.
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

using System.Xml.Linq;

namespace BH.Engine.Test.CodeCompliance
{
    public static partial class Compute
    {
        public static ComplianceResult CheckReferences(this string csProjFilePath)
        {
            if (Path.GetExtension(csProjFilePath) != ".csproj")
                return Create.ComplianceResult(ResultStatus.Pass);

            ComplianceResult finalResult = Create.ComplianceResult(ResultStatus.Pass);

            XNamespace msbuild = "http://schemas.microsoft.com/developer/msbuild/2003";
            XDocument projDefinition = XDocument.Load(csProjFilePath);
            List<XElement> referenceElements = projDefinition
                .Element(msbuild + "Project")
                .Elements(msbuild + "ItemGroup")
                .Elements(msbuild + "Reference")
                .Select(refElem => (refElem.Attribute("Include") != null && (refElem.Attribute("Include").Value.Contains("_oM") || refElem.Attribute("Include").Value.Contains("_Engine") || refElem.Attribute("Include").Value.Contains("_Adapter")) ? refElem : null))
                .ToList();

            List<string> coreProjects = new List<string> {
                    "Acoustic",
                    "Analytical",
                    "Architecture",
                    "BHoM",
                    "BHoM_Adapter",
                    "Common",
                    "Data",
                    "Diffing",
                    "Dimensional",
                    "Environment",
                    "Geometry",
                    "Graphics",
                    "Humans",
                    "Library",
                    "Matter",
                    "MEP",
                    "Physical",
                    "Planning",
                    "Quantities",
                    "Reflection",
                    "Serialiser",
                    "Spatial",
                    "Structure",
                };

            foreach(XElement x in referenceElements)
            {
                if (x == null) continue;

                string include = x.Attribute("Include").Value;

                if (include.Contains("Version") || include.Contains("Culture") || include.Contains("processorArchitecture"))
                {
                    finalResult = finalResult.Merge(Create.ComplianceResult(ResultStatus.CriticalFail, new List<Error> { Create.Error("Project references for BHoM DLLs should not include Version, Culture, or Processor Architecture", Create.Location(csProjFilePath, Create.LineSpan(1, 1))) }));
                    continue; //Difficult to check rest of reference due to string parsing if this bit is wrong
                }

                string reference = include.Replace("_oM", "").Replace("_Engine", "").Replace("_Adapter", "");
                string hintPathEnding = include.Split('_').Last();

                if (x.Element(msbuild + "HintPath") == null)
                {
                    finalResult = finalResult.Merge(Create.ComplianceResult(ResultStatus.CriticalFail, new List<Error> { Create.Error($"HintPath for reference to '{reference}' must be set", Create.Location(csProjFilePath, Create.LineSpan(1, 1))) }));
                    continue;
                }

                string referenceHintPath = x.Element(msbuild + "HintPath").Value;

                string hintPath = "";
                string referenceError = "";

                if (coreProjects.IndexOf(reference) != -1)
                {
                    string hintPathFolder = "";
                    if (hintPathEnding == "oM" || hintPathEnding == "BHoM") hintPathFolder = "BHoM";
                    else if (hintPathEnding == "Engine" || hintPathEnding == "BHoM_Engine") hintPathFolder = "BHoM_Engine";
                    else if (hintPathEnding == "Adapter" || hintPathEnding == "BHoM_Adapter") hintPathFolder = "BHoM_Adapter";

                    if ((reference + "_" + hintPathEnding) == "BHoM")
                    {
                        hintPath = "..\\..\\" + hintPathFolder + "\\Build\\" + reference + ".dll";
                        referenceError = reference;
                    }
                    else
                    {
                        hintPath = "..\\..\\" + hintPathFolder + "\\Build\\" + reference + "_" + hintPathEnding + ".dll";
                        referenceError = reference + "_" + hintPathEnding;
                    }
                }
                else if(!referenceHintPath.Contains("packages"))
                {
                    string hintPathFolder = reference + "_Toolkit";
                    hintPath = "..\\..\\" + hintPathFolder + "\\Build\\" + reference + "_" + hintPathEnding + ".dll";
                    referenceError = reference + "_" + hintPathEnding;
                }

                if (referenceHintPath != hintPath)
                    finalResult = finalResult.Merge(Create.ComplianceResult(ResultStatus.CriticalFail, new List<Error> { Create.Error($"Project references for '{referenceError}' should be set to '{hintPath}'", Create.Location(csProjFilePath, Create.LineSpan(1, 1))) }));

                if(x.Element(msbuild + "Private") == null || x.Element(msbuild + "Private").Value.ToString().ToLower() != "false")
                    finalResult = finalResult.Merge(Create.ComplianceResult(ResultStatus.CriticalFail, new List<Error> { Create.Error($"Project references for '{x.Attribute("Include").Value}' should be set to NOT copy local", Create.Location(csProjFilePath, Create.LineSpan(1, 1))) }));
            }

            foreach(XElement xe in projDefinition.Element(msbuild + "Project").Elements(msbuild + "PropertyGroup"))
            {
                if (xe.Element(msbuild + "OutputPath") == null || xe.Element(msbuild + "OutputPath").Value != "..\\Build\\")
                    finalResult = finalResult.Merge(Create.ComplianceResult(ResultStatus.CriticalFail, new List<Error> { Create.Error($"Output path for all build configurations should be set to '..\\Build\\'", Create.Location(csProjFilePath, Create.LineSpan(1, 1))) }));
            }

            return finalResult;
        }
    }
}

