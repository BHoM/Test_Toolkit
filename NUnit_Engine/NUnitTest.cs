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
using NUnitTestStatus = NUnit.Framework.Interfaces.TestStatus;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Xml;
using System.Xml.Linq;
using BH.Engine.Base;

namespace BH.oM.Test.NUnit
{
    public abstract class NUnitTest
    {

        [OneTimeSetUp]
        [Description("Loads all assemblies referenced by the derived Test class' project, when in a Test Explorer context. " +
            "This is required to make sure that otherwise lazy-loaded assemblies are loaded upfront, " +
            "in order to avoid runtime errors when using dynamic mechanisms like RunExtensionMethod().")]
        public void LoadReferencedAssemblies()
        {
            // If the tests are being run from a process that is based in ProgramData (e.g. BHoMBot),
            // this method does not apply, because we assume that all the available assemblies are pre-loaded by such process. Return.
            if (AppDomain.CurrentDomain.BaseDirectory.EndsWith(@"ProgramData\BHoM\Assemblies\"))
                return;

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

            var projectReferences = projDefinition.GetElementsByTagName("ProjectReference");
            foreach (XmlNode reference in projectReferences)
            {
                string projectToLoadName = string.Concat(reference.OuterXml.Split('\\').FirstOrDefault(p => p.Contains(".csproj"))
                    .Replace(".csproj", "")
                    .ToCharArray().Where(c => (char.IsLetterOrDigit(c)
                                  || char.IsWhiteSpace(c)
                                  || c == '_')));

                assembliesToCheck.Add((projectToLoadName, 0));
            }

            // Try loading explicitly referenced assemblies.
            HashSet<string> loadedRefAssemblies = LoadReferencedAssemblies(this.GetType().Assembly.FullName);

            assembliesToCheck = assembliesToCheck.Where(a => !loadedRefAssemblies.Contains(a.Item1)).ToList();
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

        [TearDown]
        [Description("Reports any message recorded by the BH.Engine.Base.Compute.Record* methods to the NUnit context." +
            "If the test failed, the reported messages are prefixed with their category (e.g. `Error: some message`)." +
            "If the test passed, only the message content is reported (e.g. `some message`).")]
        public void LogRecordedEvents()
        {
            var events = BH.Engine.Base.Query.CurrentEvents();
            if (events.Any())
            {
                foreach (var ev in events)
                {
                    if (TestContext.CurrentContext.Result.Outcome.Status == NUnitTestStatus.Failed)
                    {
                        // Only state the type of event (Error/Warning/Note) in case of failure.
                        string message = $"Recorded {ev.Type}: {ev.Message}";
                        TestContext.Out.Write(message); // Log to NUnit
                        Console.WriteLine(message); // Output to Test Explorer
                    }
                    else
                    {
                        // Do not state the type of event (Error/Warning/Note) in case of pass.
                        TestContext.Out.Write(ev.Message); // Log to NUnit
                        Console.WriteLine(ev.Message); // Output to Test Explorer
                    }
                }
            }

            BH.Engine.Base.Compute.ClearCurrentEvents();
        }


        [Description("Makes sure that the assemblies referenced by the input assembly are loaded in memory." +
   "Required to ensure that all dependencies (including dynamically-loaded assemblies) are present for the test run, regardless of the executing environment.")]
        private static HashSet<string> LoadReferencedAssemblies(string dllname)
        {
            var loadedAssemblies = new HashSet<string>();
            AppDomain.CurrentDomain.GetAssemblies().ToList().ForEach(a => loadedAssemblies.Add(a.GetName().Name));

            List<(string, int)> assembliesToCheck = new List<(string, int)>();

            assembliesToCheck.Add((dllname, 0));

            // Queue the referenced assemblies that must be loaded into a deque.
            // If they were already loaded by the CLI, this does not load them again.
            // If some error occurs, put them at end of the deque and try again later.
            // Max tries must not exceed initial length of collection.
            // This is useful to report errors in loading; it can be tested by setting some assemblies' Copy Local to false.
            HashSet<string> assembliesCouldNotLoad = new HashSet<string>();
            int totalCount = assembliesToCheck.Count;
            while (assembliesToCheck.Any())
            {
                (string, int) assemblyTuple = assembliesToCheck.First();
                assembliesToCheck.RemoveAt(0);

                if (assemblyTuple.Item2 < totalCount)
                {
                    try
                    {
                        Assembly loadedAssembly = Assembly.Load(assemblyTuple.Item1);
                        loadedAssemblies.Add(loadedAssembly.GetName().Name);

                        var refAssemblies = loadedAssembly.GetReferencedAssemblies().ToList();
                        refAssemblies.Where(a => a.Name.Contains("_oM") || a.Name.Contains("_Engine") || a.Name.Contains("_Adapter") || a.Name.Contains("BHoM"))
                            .ToList()
                            .ForEach(a => assembliesToCheck.Add((a.Name, 0)));
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

            // For any assembly that could not be loaded, try using LoadFrom from the BHoM ProgramData folder.
            string BHoMAssembliesFolder = Path.Combine(System.Environment.GetFolderPath(System.Environment.SpecialFolder.CommonApplicationData), "BHoM", "Assemblies");
            foreach (var assemblyName in new List<string>(assembliesCouldNotLoad))
            {
                try
                {
                    Assembly.LoadFrom(Path.Combine(BHoMAssembliesFolder, $"{assemblyName}.dll"));
                    assembliesCouldNotLoad.Remove(assemblyName);
                }
                catch (Exception e)
                {
                }
            }

            // Report errors in loading.
            if (assembliesCouldNotLoad.Any())
                throw new FileLoadException($"Could not load some assemblies: {string.Join(", ", assembliesCouldNotLoad)}");

            return loadedAssemblies;
        }
    }

}
