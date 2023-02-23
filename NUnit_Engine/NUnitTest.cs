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

using NUnit.Framework;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Xml;
using System.Xml.Linq;

namespace BH.oM.Test.NUnit
{
    public class NUnitTest
    {
        [OneTimeSetUp]
        public void LoadReferencedAssemblies()
        {
            // Get the referenced assemblies of the Test Project.
            var testProjectDir = Directory.GetParent(AppDomain.CurrentDomain.BaseDirectory).Parent.Parent.Parent.FullName;
            var files = Directory.GetFiles(testProjectDir, "*.csproj");

            string projectFullPath = files.First();
            XmlDocument projDefinition = new XmlDocument();
            projDefinition.Load(projectFullPath);

            var referencedAssembliesNames = projDefinition.GetElementsByTagName("HintPath");
            List<(string, int)> assembliesToCheck = new List<(string, int)>();
            foreach (XmlNode reference in referencedAssembliesNames)
            {
                string assemblyToLoadPath = Path.GetFileNameWithoutExtension(reference.InnerText);
                assembliesToCheck.Add((assemblyToLoadPath, 0));
            }

            // Queue the referenced assemblies that must be loaded into a deque.
            // If they were already loaded by the CLI, this does not load them again.
            // If some error occurs, put them at end of the deque and try again later.
            // Max tries must not exceed initial length of collection.
            // This is useful to report errors in loading; it can be tested by setting some assemblies' Copy Local to false.
            List<string> assembliesCouldNotLoad = new List<string>();
            int totalCount = assembliesToCheck.Count;
            while (assembliesToCheck.Any())
            {
                (string, int) assemblyTuple = assembliesToCheck.First();
                assembliesToCheck.RemoveAt(0);

                if (assemblyTuple.Item2 < totalCount)
                {
                    try
                    {
                        Assembly.Load(assemblyTuple.Item1);
                    }
                    catch
                    {
                        // Could not load. Might be because of missing dependencies that are already enqueued. Try later.
                        assembliesToCheck.Add((assemblyTuple.Item1, assemblyTuple.Item2 + 1));
                    }
                }
                else
                {
                    assembliesCouldNotLoad.Add(assemblyTuple.Item1);
                }
            }

            // Report errors in loading.
            if (assembliesCouldNotLoad.Any())
                throw new FileLoadException($"Could not load some assemblies that were added to the {Path.GetFileNameWithoutExtension(projectFullPath)} project dependencies. " +
                    $"\nProblematic assemblies: {string.Join(", ", assembliesCouldNotLoad)}." +
                    $"\nMake sure that Copy Local is set to true for them in the test project {Path.GetFileNameWithoutExtension(projectFullPath)}." +
                    $"\nAlternatively, this could be because some of their dependencies were not referenced in the project.");
        }
    }
}
