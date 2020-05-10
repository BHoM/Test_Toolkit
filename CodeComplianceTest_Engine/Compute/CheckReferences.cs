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

using BH.oM.Test.Attributes;

namespace BH.Engine.Test.CodeCompliance
{
    public static partial class Compute
    {
        public static ComplianceResult CheckReferences(this string csProjFilePath)
        {
            if (Path.GetExtension(csProjFilePath) != ".csproj")
                return Create.ComplianceResult(ResultStatus.Pass);

            ComplianceResult finalResult = Create.ComplianceResult(ResultStatus.Pass);

            string documentationLink = "Project-References-and-Build-Paths";

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
                    "File_Adapter",
                    "Geometry",
                    "Graphics",
                    "Humans",
                    "Library",
                    "LifeCycleAssessment",
                    "Matter",
                    "MEP",
                    "Physical",
                    "Planning",
                    "Programming",
                    "Quantities",
                    "Reflection",
                    "Results",
                    "Serialiser",
                    "Spatial",
                    "Structure",
                };

            List<string> adapterCore = new List<string>
            {
                "Adapter_Engine",
                "Adapter_oM",
                "File_Adapter",
                "StructureModules_AdapterModules", //Renamed this way due to the logic below where we strip _Adapter and then readd it for the check of Adapter projects so this is valid
            };

            List<string> uiCore = new List<string>()
            {
                "BHoM_UI",
                "UI_Engine",
                "UI_oM",
            };

            List<string> localisationToolkit = new List<string>()
            {
                "Units_Engine",
            };

            foreach(XElement x in referenceElements)
            {
                if (x == null) continue;

                string include = x.Attribute("Include").Value;

                if (include.Contains("Version") || include.Contains("Culture") || include.Contains("processorArchitecture"))
                {
                    finalResult = finalResult.Merge(Create.ComplianceResult(ResultStatus.CriticalFail, new List<Error> { Create.Error("Project references for BHoM DLLs should not include Version, Culture, or Processor Architecture", Create.Location(csProjFilePath, Create.LineSpan(1, 1)), documentationLink) }));
                    continue; //Difficult to check rest of reference due to string parsing if this bit is wrong
                }

                string reference = include.Replace("_oM", "").Replace("_Engine", "").Replace("_Adapter", "");
                string hintPathEnding = include.Split('_').Last();

                if (x.Element(msbuild + "HintPath") == null)
                {
                    finalResult = finalResult.Merge(Create.ComplianceResult(ResultStatus.CriticalFail, new List<Error> { Create.Error($"HintPath for reference to '{reference}' must be set", Create.Location(csProjFilePath, Create.LineSpan(1, 1)), documentationLink) }));
                    continue;
                }

                string referenceHintPath = x.Element(msbuild + "HintPath").Value;

                string hintPath = "";
                string referenceError = "";
                bool shouldBeProjectReference = false;

                if (coreProjects.IndexOf(reference) != -1)
                {
                    if ((csProjFilePath.Contains("\\BHoM\\") && (hintPathEnding == "oM" || hintPathEnding == "BHoM")) || (csProjFilePath.Contains("\\BHoM_Engine\\") && (hintPathEnding == "Engine" || hintPathEnding == "BHoM_Engine")))
                        shouldBeProjectReference = true;

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
                else if (adapterCore.IndexOf(reference + "_" + hintPathEnding) != -1)
                {
                    string hintPathFolder = "BHoM_Adapter";
                    if (reference == "StructureModules")
                        reference = "Structure";

                    if (csProjFilePath.Contains("\\BHoM_Adapter\\"))
                        shouldBeProjectReference = true;

                    hintPath = "..\\..\\" + hintPathFolder + "\\Build\\" + reference + "_" + hintPathEnding + ".dll";
                    referenceError = reference;
                }
                else if(uiCore.IndexOf(reference + "_" + hintPathEnding) != -1)
                {
                    if (csProjFilePath.Contains("\\BHoM_UI\\"))
                        shouldBeProjectReference = true;

                    string hintPathFolder = "BHoM_UI";
                    hintPath = "..\\..\\" + hintPathFolder + "\\Build\\" + reference + "_" + hintPathEnding + ".dll";
                    referenceError = reference;
                }
                else if(localisationToolkit.IndexOf(reference + "_" + hintPathEnding) != -1)
                {
                    if (csProjFilePath.Contains("\\Localisation_Toolkit\\"))
                        shouldBeProjectReference = true;

                    string hintPathFolder = "Localisation_Toolkit";
                    hintPath = "..\\..\\" + hintPathFolder + "\\Build\\" + reference + "_" + hintPathEnding + ".dll";
                    referenceError = reference;
                }
                else if(!referenceHintPath.Contains("packages"))
                {
                    string hintPathFolder = reference + "_Toolkit";

                    if (csProjFilePath.Contains("\\" + hintPathFolder + "\\"))
                        shouldBeProjectReference = true;

                    hintPath = "..\\..\\" + hintPathFolder + "\\Build\\" + reference + "_" + hintPathEnding + ".dll";
                    referenceError = reference + "_" + hintPathEnding;
                }

                if (referenceHintPath != hintPath)
                {
                    if (shouldBeProjectReference)
                        finalResult = finalResult.Merge(Create.ComplianceResult(ResultStatus.CriticalFail, new List<Error> { Create.Error($"Project references for '{referenceError}' should be set as a project reference rather than as a DLL reference", Create.Location(csProjFilePath, Create.LineSpan(1, 1)), documentationLink) }));
                    else
                        finalResult = finalResult.Merge(Create.ComplianceResult(ResultStatus.CriticalFail, new List<Error> { Create.Error($"Project references for '{referenceError}' should be set to '{hintPath}'", Create.Location(csProjFilePath, Create.LineSpan(1, 1)), documentationLink) }));
                }

                if(x.Element(msbuild + "Private") == null || x.Element(msbuild + "Private").Value.ToString().ToLower() != "false")
                    finalResult = finalResult.Merge(Create.ComplianceResult(ResultStatus.CriticalFail, new List<Error> { Create.Error($"Project references for '{x.Attribute("Include").Value}' should be set to NOT copy local", Create.Location(csProjFilePath, Create.LineSpan(1, 1)), documentationLink) }));
            }

            foreach(XElement xe in projDefinition.Element(msbuild + "Project").Elements(msbuild + "PropertyGroup"))
            {
                if (xe.Element(msbuild + "OutputPath") == null) continue;

                if (xe.Element(msbuild + "OutputPath").Value != "..\\Build\\")
                    finalResult = finalResult.Merge(Create.ComplianceResult(ResultStatus.CriticalFail, new List<Error> { Create.Error($"Output path for all build configurations should be set to '..\\Build\\'", Create.Location(csProjFilePath, Create.LineSpan(1, 1)), documentationLink) }));
            }

            return finalResult;
        }
    }
}

