/*
 * This file is part of the Buildings and Habitats object Model (BHoM)
 * Copyright (c) 2015 - 2023, the respective contributors. All rights reserved.
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
        public static TestResult CheckProjectFile(this string csProjFilePath, string assemblyDescriptionOrg = null)
        {
            if ((Path.GetExtension(csProjFilePath) != ".csproj"))
                return Create.TestResult(TestStatus.Pass);

            if (!File.Exists(csProjFilePath))
                return Create.TestResult(TestStatus.Pass);

            TestResult finalResult = Create.TestResult(TestStatus.Pass);
            string documentationLink = "Project-References-and-Build-Paths";

            List<string> fileLines = ReadFileContents(csProjFilePath);

            ProjectFile csProject = GetProjectFile(fileLines);

            if (csProject == null)
                return finalResult;

            finalResult = CheckNETTarget(csProject, new List<string>(fileLines), finalResult, csProjFilePath, documentationLink);
            finalResult = CheckOutputPath(csProject, new List<string>(fileLines), finalResult, csProjFilePath, documentationLink);
            finalResult = CheckReferences(csProject, new List<string>(fileLines), finalResult, csProjFilePath, documentationLink);
            finalResult = CheckPostBuild(csProject, new List<string>(fileLines), finalResult, csProjFilePath, documentationLink);

            if (csProject.IsOldStyle)
            {
                finalResult = finalResult.Merge(Create.TestResult(TestStatus.Warning, new List<Error> { Create.Error($"CSProject files should be in the new format as used by core BHoM projects. Upgrading the file is possible for .Net Framework 4.7.2 projects as well. Please speak to a member of the DevOps team for assistance with this.", Create.Location(csProjFilePath, Create.LineSpan(1, 1)), documentationLink, TestStatus.Warning) }));
            }
            else
            {
                finalResult = CheckAssemblyVersion(csProject, new List<string>(fileLines), finalResult, csProjFilePath, documentationLink);
                finalResult = CheckAssembyFileVersion(csProject, new List<string>(fileLines), finalResult, csProjFilePath, documentationLink);

                if (!string.IsNullOrEmpty(assemblyDescriptionOrg))
                    finalResult = CheckAssemblyDescription(csProject, new List<string>(fileLines), finalResult, csProjFilePath, documentationLink, assemblyDescriptionOrg);
            }

            return finalResult;
        }

        private static List<string> ReadFileContents(string fileName)
        {
            StreamReader sr = new StreamReader(fileName);

            List<string> lines = new List<string>();

            string line = "";
            while ((line = sr.ReadLine()) != null)
                lines.Add(line);

            sr.Close();

            return lines;
        }

        private static TestResult CheckNETTarget(this ProjectFile csProject, List<string> fileLines, TestResult finalResult, string csProjFilePath, string documentationLink)
        {
            List<string> acceptableNETTargets = new List<string> { "v4.7.2", "net472", "net480", "net481", "netstandard2.0", "net5.0", "net6.0" };
            bool atLeastOneCorrect = false;

            foreach(string target in csProject.TargetNETVersions)
            {
                if (!string.IsNullOrEmpty(target) && !acceptableNETTargets.Contains(target))
                {
                    string fullXMLText = $"<TargetFramework>{target}</TargetFramework>";
                    int lineNumber = fileLines.IndexOf(fileLines.Where(x => x.Contains(fullXMLText)).FirstOrDefault()) + 1; //+1 because index is 0 based but line numbers start at 1 for the spans
                    finalResult = finalResult.Merge(Create.TestResult(TestStatus.Warning, new List<Error> { Create.Error($"Target frameworks for BHoM projects should either be .Net Framework 4.7.2, .Net Framework 4.8, .Net Framework 4.8.1, .Net Standard 2.0, .Net 5.0, or .Net 6.0.", Create.Location(csProjFilePath, Create.LineSpan(lineNumber, lineNumber)), documentationLink, TestStatus.Warning) }));
                }
                else
                    atLeastOneCorrect = true;
            }

            if(!atLeastOneCorrect)
            {
                finalResult = finalResult.Merge(Create.TestResult(TestStatus.Error, new List<Error> { Create.Error($"At least one of the Target frameworks for BHoM projects must either be .Net Framework 4.7.2, .Net Standard 2.0, or .Net 5.0.", Create.Location(csProjFilePath, Create.LineSpan(1, 1)), documentationLink) }));
            }

            return finalResult;
        }

        private static TestResult CheckOutputPath(this ProjectFile csProject, List<string> fileLines, TestResult finalResult, string csProjFilePath, string documentationLink)
        {
            List<string> acceptableOutputPaths = new List<string>() { "..\\Build\\" };

            foreach(string outputPath in csProject.OutputPaths)
            {
                if(!string.IsNullOrEmpty(outputPath) && !acceptableOutputPaths.Contains(outputPath))
                {
                    string fullXMLText = $"<OutputPath>{outputPath}</OutputPath>";
                    int lineNumber = fileLines.IndexOf(fileLines.Where(x => x.Contains(fullXMLText)).FirstOrDefault()) + 1; //+1 because index is 0 based but line numbers start at 1 for the spans
                    finalResult = finalResult.Merge(Create.TestResult(TestStatus.Error, new List<Error> { Create.Error($"Output path for all build configurations should be set to '..\\Build\\'", Create.Location(csProjFilePath, Create.LineSpan(lineNumber, lineNumber)), documentationLink) }));
                }
            }

            return finalResult;
        }

        private static TestResult CheckReferences(this ProjectFile csProject, List<string> fileLines, TestResult finalResult, string csProjFilePath, string documentationLink)
        {
            foreach(AssemblyReference reference in csProject.References)
            {
                string includeName = reference.Name.ToLower();
                if (includeName != "bhom" && !includeName.Contains("_om") && !includeName.Contains("_engine") && !includeName.Contains("_adapter") && !includeName.Contains("_ui"))
                    continue; //Not a BHoM DLL so no point worrying

                string includeNameXMLStart = $"<Reference Include=\"{reference.Name}";
                int lineNumber = -1;
                if(!string.IsNullOrEmpty(reference.Version) || !string.IsNullOrEmpty(reference.Culture) || !string.IsNullOrEmpty(reference.ProcessorArchitecture))
                {
                    lineNumber = fileLines.IndexOf(fileLines.Where(x => x.Contains(includeNameXMLStart)).FirstOrDefault()) + 1; //+1 because index is 0 based but line numbers start at 1 for the spans
                    finalResult = finalResult.Merge(Create.TestResult(TestStatus.Error, new List<Error> { Create.Error("Project references for BHoM DLLs should not include Version, Culture, or Processor Architecture", Create.Location(csProjFilePath, Create.LineSpan(lineNumber, lineNumber)), documentationLink) }));
                }

                string hintPath = @"C:\ProgramData\BHoM\Assemblies\" + reference.Name + ".dll";
                string hintPathXML = $"<HintPath>{reference.HintPath}</HintPath>";
                if(reference.HintPath != hintPath)
                {
                    lineNumber = fileLines.IndexOf(fileLines.Where(x => x.Contains(hintPathXML)).FirstOrDefault()) + 1; //+1 because index is 0 based but line numbers start at 1 for the spans
                    finalResult = finalResult.Merge(Create.TestResult(TestStatus.Error, new List<Error> { Create.Error($"Project reference for '{reference.Name}' should be set to '{hintPath}'", Create.Location(csProjFilePath, Create.LineSpan(lineNumber, lineNumber)), documentationLink) }));
                }

                if(reference.CopyLocal)
                {
                    lineNumber = fileLines.IndexOf(fileLines.Where(x => x.Contains(hintPathXML)).FirstOrDefault()) + 1; //+1 because index is 0 based but line numbers start at 1 for the spans - searching from hintPathXML to then make sure we get the right line number related to this copy local issue
                    while (!fileLines[lineNumber].Contains("<Private>true</Private>"))
                    {
                        lineNumber++;
                        if (lineNumber >= fileLines.Count)
                        {
                            lineNumber = fileLines.IndexOf(fileLines.Where(x => x.Contains(hintPathXML)).FirstOrDefault()) + 1; //return to this line as the copy local isn't set at all
                            break; //To avoid infinite loop
                        }
                    }

                    finalResult = finalResult.Merge(Create.TestResult(TestStatus.Error, new List<Error> { Create.Error($"Project reference for '{reference.Name}' should be set to not copy local", Create.Location(csProjFilePath, Create.LineSpan(lineNumber, lineNumber)), documentationLink) }));
                }

                if(reference.SpecificVersion)
                {
                    lineNumber = fileLines.IndexOf(fileLines.Where(x => x.Contains(hintPathXML)).FirstOrDefault()) + 1; //+1 because index is 0 based but line numbers start at 1 for the spans - searching from hintPathXML to then make sure we get the right line number related to this copy local issue
                    while (!fileLines[lineNumber].Contains("<SpecificVersion>true</SpecificVersion>"))
                    {
                        lineNumber++;
                        if (lineNumber >= fileLines.Count)
                        {
                            lineNumber = fileLines.IndexOf(fileLines.Where(x => x.Contains(hintPathXML)).FirstOrDefault()) + 1; //return to this line as the specific version isn't set at all
                            break; //To avoid infinite loop
                        }
                    }

                    finalResult = finalResult.Merge(Create.TestResult(TestStatus.Error, new List<Error> { Create.Error($"Project reference for '{reference.Name}' should set specific version as false", Create.Location(csProjFilePath, Create.LineSpan(lineNumber, lineNumber)), documentationLink) }));
                }
            }

            return finalResult;
        }

        private static TestResult CheckAssemblyVersion(this ProjectFile csProject, List<string> fileLines, TestResult finalResult, string csProjFilePath, string documentationLink)
        {
            string currentAssemblyVersion = Query.FullCurrentAssemblyVersion();
            if (csProject.AssemblyVersion.ToLower() != currentAssemblyVersion.ToLower())
            {
                string fullXMLText = $"<AssemblyVersion>{csProject.AssemblyVersion}</AssemblyVersion>";
                int lineNumber = fileLines.IndexOf(fileLines.Where(x => x.Contains(fullXMLText)).FirstOrDefault()) + 1; //+1 because index is 0 based but line numbers start at 1 for the spans
                return finalResult.Merge(Create.TestResult(TestStatus.Error, new List<Error> { Create.Error($"Assembly Version should be set to {currentAssemblyVersion}", Create.Location(csProjFilePath, Create.LineSpan(lineNumber, lineNumber)), documentationLink) }));
            }

            return finalResult;
        }

        private static TestResult CheckAssembyFileVersion(this ProjectFile csProject, List<string> fileLines, TestResult finalResult, string csProjFilePath, string documentationLink)
        {
            string currentlyAssemblyFileVersion = Query.FullCurrentAssemblyFileVersion();
            if (csProject.AssemblyFileVersion.ToLower() != currentlyAssemblyFileVersion.ToLower())
            {
                string fullXMLText = $"<FileVersion>{csProject.AssemblyFileVersion}</FileVersion>";
                int lineNumber = fileLines.IndexOf(fileLines.Where(x => x.Contains(fullXMLText)).FirstOrDefault()) + 1; //+1 because index is 0 based but line numbers start at 1 for the spans
                return finalResult.Merge(Create.TestResult(TestStatus.Error, new List<Error> { Create.Error($"Assembly File Version should be set to {currentlyAssemblyFileVersion}", Create.Location(csProjFilePath, Create.LineSpan(lineNumber, lineNumber)), documentationLink) }));
            }

            return finalResult;
        }

        private static TestResult CheckAssemblyDescription(this ProjectFile csProject, List<string> fileLines, TestResult finalResult, string csProjFilePath, string documentationLink, string descriptionUrl)
        {
            if (csProject.AssemblyDescription.ToLower() != descriptionUrl.ToLower())
            {
                string fullXMLText = $"<Description>{csProject.AssemblyDescription}</Description>";
                int lineNumber = fileLines.IndexOf(fileLines.Where(x => x.Contains(fullXMLText)).FirstOrDefault()) + 1; //+1 because index is 0 based but line numbers start at 1 for the spans
                return finalResult.Merge(Create.TestResult(TestStatus.Error, new List<Error> { Create.Error($"Assembly Description should contain the URL to the GitHub organisation which owns the repository", Create.Location(csProjFilePath, Create.LineSpan(lineNumber, lineNumber)), documentationLink) }));
            }

            return finalResult;
        }

        private static TestResult CheckPostBuild(this ProjectFile csProject, List<string> fileLines, TestResult finalResult, string csProjFilePath, string documentationLink)
        {
            string postBuildShouldContain = "";
            string searchLine = "";
            if (csProject.IsOldStyle)
            {
                postBuildShouldContain = "xcopy \"$(TargetDir)$(TargetFileName)\" \"C:\\ProgramData\\BHoM\\Assemblies\" /Y";
                searchLine = "<PostBuildEvent";
            }
            else
            {
                postBuildShouldContain = "&quot;$(TargetDir)$(TargetFileName)&quot; &quot;C:\\ProgramData\\BHoM\\Assemblies&quot; /Y";
                searchLine = "<Exec Command=\"xcopy";
            }

            if(!csProject.PostBuildEvent.Any(x => x.Contains(postBuildShouldContain)))
            {
                postBuildShouldContain = "xcopy \"$(TargetDir)$(TargetFileName)\"  \"C:\\ProgramData\\BHoM\\Assemblies\" /Y"; //Check again with a double spacing
                if (!csProject.PostBuildEvent.Any(x => x.Contains(postBuildShouldContain)))
                {
                    postBuildShouldContain = "&quot;$(TargetDir)$(TargetFileName)&quot;  &quot;C:\\ProgramData\\BHoM\\Assemblies&quot; /Y"; //Check again with a double spacing
                    if (!csProject.PostBuildEvent.Any(x => x.Contains(postBuildShouldContain)))
                    {
                        int lineNumber = fileLines.IndexOf(fileLines.Where(x => x.Contains(searchLine)).FirstOrDefault()) + 1; //+1 because index is 0 based but line numbers start at 1 for the spans
                        return finalResult.Merge(Create.TestResult(TestStatus.Warning, new List<Error> { Create.Error($"Post Build event should be correctly set to copy the compiled DLL to the BHoM Assemblies folder", Create.Location(csProjFilePath, Create.LineSpan(lineNumber, lineNumber)), documentationLink, TestStatus.Warning) }));
                    }
                }
            }

            return finalResult;
        }

        private static ProjectFile GetProjectFile(List<string> fileLines)
        {
            ProjectFile projectFile = new ProjectFile();

            for(int x = 0; x < fileLines.Count; x++)
            {
                if (fileLines[x].Contains("<AssemblyVersion"))
                    projectFile.AssemblyVersion = fileLines[x].Split('>')[1].Split('<')[0];
                else if (fileLines[x].Contains("<FileVersion"))
                    projectFile.AssemblyFileVersion = fileLines[x].Split('>')[1].Split('<')[0];
                else if (fileLines[x].Contains("<Description"))
                    projectFile.AssemblyDescription = fileLines[x].Split('>')[1].Split('<')[0];
                else if (fileLines[x].Contains("<TargetFramework"))
                    projectFile.TargetNETVersions.Add(fileLines[x].Split('>')[1].Split('<')[0]);
                else if (fileLines[x].Contains("<OutputPath>"))
                    projectFile.OutputPaths.Add(fileLines[x].Split('>')[1].Split('<')[0]);
                else if (fileLines[x].Contains("<Compile Include="))
                    projectFile.IsOldStyle = true; //New Style has <Compile Exclude instead
                else if (fileLines[x].Contains("<Exec Command=\"xcopy"))
                    projectFile.PostBuildEvent = new List<string> { fileLines[x] };
                else if (fileLines[x].Contains("<PostBuildEvent"))
                {
                    List<string> postBuildLines = new List<string>();
                    while(!fileLines[x].Contains("</PostBuildEvent") && !fileLines[x].EndsWith("/>"))
                    {
                        postBuildLines.Add(fileLines[x]);
                        x++;
                    }
                    projectFile.PostBuildEvent = postBuildLines;
                }                
                else if (fileLines[x].Contains("<Reference"))
                {
                    List<string> referenceLines = new List<string>();
                    while (!fileLines[x].Contains("</Reference") && !fileLines[x].EndsWith("/>"))
                    {
                        referenceLines.Add(fileLines[x]);
                        x++;
                    }

                    if (referenceLines.Count == 0)
                        continue; //Not a BHoM reference, maybe a system reference, but not enough data to compile

                    AssemblyReference assemblyReference = new AssemblyReference();

                    string referenceNameLine = referenceLines[0].Split('\"')[1];
                    string[] splitData = referenceNameLine.Split(',');
                    if (splitData.Length > 0)
                        assemblyReference.Name = splitData[0];

                    string refName = assemblyReference.Name.ToLower();

                    if (!refName.StartsWith("bhom") && !refName.Contains("_om") && !refName.Contains("_engine") && !refName.Contains("_adapter") && !refName.Contains("_ui"))
                        continue; //Not a BHoM DLL so no point worrying

                    if (splitData.Length > 1)
                        assemblyReference.Version = splitData[1];
                    if (splitData.Length > 2)
                        assemblyReference.Culture = splitData[2];
                    if (splitData.Length > 3)
                        assemblyReference.ProcessorArchitecture = splitData[3];

                    string hintPathLine = referenceLines.Where(y => y.Contains("<HintPath>")).FirstOrDefault();
                    string privateLine = referenceLines.Where(y => y.Contains("<Private>")).FirstOrDefault();
                    string specificVersionLine = referenceLines.Where(y => y.Contains("<SpecificVersion>")).FirstOrDefault();

                    if (hintPathLine != null)
                        assemblyReference.HintPath = hintPathLine.Split('>')[1].Split('<')[0];

                    if (privateLine != null)
                        assemblyReference.CopyLocal = System.Convert.ToBoolean(privateLine.Split('>')[1].Split('<')[0]);

                    if (specificVersionLine != null)
                        assemblyReference.SpecificVersion = System.Convert.ToBoolean(specificVersionLine.Split('>')[1].Split('<')[0]);

                    projectFile.References.Add(assemblyReference);
                }
            }

            return projectFile;
        }
    }
}




