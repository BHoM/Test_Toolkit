/*
 * This file is part of the Buildings and Habitats object Model (BHoM)
 * Copyright (c) 2015 - 2021, the respective contributors. All rights reserved.
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

using BH.oM.XML.CSProject;
using BH.Adapter.XML;
using BH.oM.Adapter;
using BH.oM.Adapters.XML;
using BH.oM.Data.Requests;

using BH.oM.Test;
using BH.oM.Test.Results;

namespace BH.Engine.Test.CodeCompliance
{
    public static partial class Compute
    {
        public static TestResult CheckReferences(this string csProjFilePath)
        {
            if (Path.GetExtension(csProjFilePath) != ".csproj")
                return Create.TestResult(TestStatus.Pass);

            TestResult finalResult = Create.TestResult(TestStatus.Pass);
            string documentationLink = "Project-References-and-Build-Paths";

            List<string> fileLines = ReadCSProjFile(csProjFilePath);

            FileSettings fileSettings = new FileSettings()
            {
                Directory = Path.GetDirectoryName(csProjFilePath),
                FileName = Path.GetFileName(csProjFilePath),
            };

            XMLAdapter adapter = new XMLAdapter(fileSettings);
            XMLConfig config = new XMLConfig();
            config.Schema = oM.Adapters.XML.Enums.Schema.CSProject;

            List<Project> xmlData = adapter.Pull(new FilterRequest(), PullType.AdapterDefault, config).Select(x => x as Project).ToList();

            if (xmlData.Count <= 0)
                return finalResult;

            Project csProject = xmlData[0];

            //Check the output path
            finalResult = CheckOutputPath(csProject, new List<string>(fileLines), finalResult, csProjFilePath, documentationLink);
            finalResult = CheckReferences(csProject, new List<string>(fileLines), finalResult, csProjFilePath, documentationLink);

            return finalResult;
        }

        private static List<string> ReadCSProjFile(string fileName)
        {
            StreamReader sr = new StreamReader(fileName);

            List<string> lines = new List<string>();

            string line = "";
            while ((line = sr.ReadLine()) != null)
                lines.Add(line);

            sr.Close();

            return lines;
        }

        private static TestResult CheckOutputPath(this Project csProject, List<string> fileLines, TestResult finalResult, string csProjFilePath, string documentationLink)
        {
            int foundCounter = 1; //Start at one because the index from the list is not the line of the file (0 index + 1 to get the line number)

            foreach (PropertyGroup group in csProject.PropertyGroups)
            {
                if (group.OutputPath != null)
                {
                    if (group.OutputPath != "..\\Build\\")
                    {
                        int index = fileLines.IndexOf(fileLines.Where(x => x.Contains(group.OutputPath)).FirstOrDefault());
                        if (index == -1)
                            index = 1;
                        else
                        {
                            index += foundCounter;
                            foundCounter++; //For the second search, we remove the line so the index is one away from where it should be
                            fileLines.RemoveAt(index); //So we don't have to find it again
                        }

                        finalResult = finalResult.Merge(Create.TestResult(TestStatus.Error, new List<Error> { Create.Error($"Output path for all build configurations should be set to '..\\Build\\'", Create.Location(csProjFilePath, Create.LineSpan(index, index)), documentationLink) }));
                    }
                }
            }

            return finalResult;
        }

        private static TestResult CheckReferences(this Project csProject, List<string> fileLines, TestResult finalResult, string csProjFilePath, string documentationLink)
        {
            foreach(ItemGroup group in csProject.ItemGroups)
            {
                if (group.References == null)
                    continue;

                foreach(Reference reference in group.References)
                {
                    string includeName = reference.IncludeName.ToLower();
                    string refName = reference.IncludeName.Split(',')[0];

                    if (includeName != "bhom" && !includeName.Contains("_om") && !includeName.Contains("_engine") && !includeName.Contains("_adapter") && !includeName.Contains("_ui"))
                        continue; //Not a BHoM DLL so no point worrying

                    if(includeName.Contains("culture") || includeName.Contains("version") || includeName.Contains("processorarchitecture"))
                    {
                        int index = fileLines.IndexOf(fileLines.Where(x => x.Contains(reference.IncludeName)).FirstOrDefault());
                        if (index == -1)
                            index = 1;
                        else
                            index += 1; //To account for 0 indexing of the list

                        finalResult = finalResult.Merge(Create.TestResult(TestStatus.Error, new List<Error> { Create.Error("Project references for BHoM DLLs should not include Version, Culture, or Processor Architecture", Create.Location(csProjFilePath, Create.LineSpan(index, index)), documentationLink) }));
                    }

                    string hintPath = @"C:\ProgramData\BHoM\Assemblies\" + refName + ".dll";

                    if (reference.HintPath == null)
                    {
                        int index = fileLines.IndexOf(fileLines.Where(x => x.Contains(reference.IncludeName)).FirstOrDefault());
                        if (index == -1)
                            index = 1;
                        else
                            index += 1;

                        finalResult = finalResult.Merge(Create.TestResult(TestStatus.Error, new List<Error> { Create.Error($"Project reference for '{refName}' should be set to '{hintPath}'", Create.Location(csProjFilePath, Create.LineSpan(index, index)), documentationLink) }));
                    }
                    else if (reference.HintPath != hintPath)
                    {
                        int index = fileLines.IndexOf(fileLines.Where(x => x.Contains(reference.HintPath)).FirstOrDefault());
                        if (index == -1)
                            index = 1;
                        else
                            index += 1;
                        
                        finalResult = finalResult.Merge(Create.TestResult(TestStatus.Error, new List<Error> { Create.Error($"Project reference for '{refName}' should be set to '{hintPath}'", Create.Location(csProjFilePath, Create.LineSpan(index, index)), documentationLink) }));
                    }

                    if(reference.IsCopyLocal == null || reference.IsCopyLocal.ToLower() != "false")
                    {
                        int index = fileLines.IndexOf(fileLines.Where(x => x.Contains("<Reference Include=") && x.Contains(reference.IncludeName)).FirstOrDefault());
                        if (index == -1)
                            index = 1;
                        else
                        {
                            while (true)
                            {
                                if (fileLines[index + 1].Contains("<Private>"))
                                {
                                    index += 2; //To account for the 0 index list
                                    break;
                                }
                                else if (fileLines[index + 1].Contains("</Reference>"))
                                {
                                    index = fileLines.IndexOf(fileLines.Where(x => x.Contains("<Reference Include=") && x.Contains(reference.IncludeName)).FirstOrDefault());
                                    index += 1;
                                    break;
                                }

                                index++;
                            }
                        }

                        finalResult = finalResult.Merge(Create.TestResult(TestStatus.Error, new List<Error> { Create.Error($"Project reference for '{refName}' should be set to not copy local", Create.Location(csProjFilePath, Create.LineSpan(index, index)), documentationLink) }));
                    }
                }
            }

            return finalResult;
        }
    }
}


