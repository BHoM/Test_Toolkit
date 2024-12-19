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

using System;
using System.Linq;
using System.Collections.Generic;
using System.ComponentModel;
using BH.oM.Base.Attributes;
using BH.oM.Base;
using BH.oM.Data.Library;
using System.IO;
using System.Reflection;
using BH.Engine.Serialiser;

namespace BH.Engine.UnitTest
{
    public static partial class Compute
    {
        /***************************************************/
        /**** Public Methods                            ****/
        /***************************************************/


        [Description("Stores out the List of UnitTests as approperiately grouped Datasets in the .ci/Dataset corresponding to the project and class names with a file names matching the method names.")]
        [Input("unitTests", "UnitTests to store in .ci folder. UnitTests will be merged based on Method and grouped by MethodName and declaring type.")]
        [Input("repoFolder", "Folder link to the folder corresponding to the repo containing the method(s) being tested by the UnitTests.")]
        [Input("sourceLink", "Link to the script or process used to generate the UnitTest. Important to be able to easily update the test in case of a change required from code updates.")]
        [Input("author", "Author of the UnitTests. If nothing is provided, the currently logged in windows username will be used.")]
        [Input("confidence", "Confidence of the UnitTests. Should generally relate to the number of potential usecases and edge cases that the test data for the UnitTest is covering.")]
        [Input("checkAssemblyFolder", "If true, checks that the provided repo folder contains an assembly matching the assembly of the unit test. For general case this can be left to the default value of true.")]
        [Input("replacePreExisting", "If true, replaces any pre-existing dataset in the folder, if false, merges the content of the provided Dataset with the one in the folder.")]
        [Input("activate", "Toggle to push dataset to file.")]
        [Output("success", "Returns true if sucessfully able to write the dataset to file.")]
        public static bool StoreUnitTests(List<BH.oM.Test.UnitTests.UnitTest> unitTests, string repoFolder, string sourceLink = "", string author = "", Confidence confidence = Confidence.Undefined, bool checkAssemblyFolder = true, bool replacePreExisting = false, bool activate = false)
        {
            bool success = true;

            foreach (Dataset dataset in Create.UnitTestDataSet(unitTests, sourceLink, author, confidence))
            {
                success &= StoreUnitTest(dataset, repoFolder, checkAssemblyFolder, replacePreExisting, activate);
            }

            return success;
        }

        /***************************************************/

        [Description("Stores out the Provided Dataset in the .ci/Dataset corresponding to the project and class name with a file name matching the method name.")]
        [Input("dataset", "Dataset to store in .ci folder. Should contain only UnitTest information.")]
        [Input("repoFolder", "Folder link to the folder corresponding to the repo containing the method(s) being tested by the UnitTest(s) in the Dataset.")]
        [Input("checkAssemblyFolder", "If true, checks that the provided repo folder contains an assembly matching the assembly of the unit test. For general case this can be left to the default value of true.")]
        [Input("replacePreExisting", "If true, replaces any pre-existing dataset in the folder, if false, merges the content of the provided Dataset with the one in the folder.")]
        [Input("activate", "Toggle to push dataset to file.")]
        [Output("success", "Returns true if sucessfully able to write the dataset to file.")]
        public static bool StoreUnitTest(Dataset dataset, string repoFolder, bool checkAssemblyFolder = true, bool replacePreExisting = false, bool activate = false)
        {
            string repoBaseFolder;
            if (!CheckValidUnitTestDataSet(dataset))
                return false;

            string assemblyName, className, methodName;
            GetMethodStrings(dataset, out assemblyName, out className, out methodName);

            bool multiEngineProject;
            if (!CheckValidRepoFolderString(repoFolder, checkAssemblyFolder, assemblyName, out repoBaseFolder, out multiEngineProject))
                return false;

            methodName += ".json";
            string fullPathName;

            if (multiEngineProject)
                fullPathName = Path.Combine(repoBaseFolder, ".ci", "Datasets", assemblyName, className, methodName);
            else
                fullPathName = Path.Combine(repoBaseFolder, ".ci", "Datasets", className, methodName);


            if (!Directory.Exists(Path.GetDirectoryName(fullPathName)))
                Directory.CreateDirectory(Path.GetDirectoryName(fullPathName));

            if (File.Exists(fullPathName))
            {
                if (!replacePreExisting)
                {
                    Base.Compute.RecordWarning($"File {fullPathName} already exists. Dataset will be merged with pre-exisiting one, and Unit tests added to it. To replace it, toggle {nameof(replacePreExisting)} to true.");
                    Dataset existingSet = Query.DatasetFromFile(fullPathName);
                    if (existingSet == null)
                        return false;

                    dataset = MergeTestDataSets(existingSet, dataset);

                    if (dataset == null)    //Safeguard in case something went wrong during merge
                        return false;

                    if (activate)
                        File.Delete(fullPathName);  //Remove pre-existing as data has been scraped already and new one will be added containing both new and old data.
                }
                else
                {
                    if (activate)
                    {
                        Engine.Base.Compute.RecordWarning($"File {fullPathName} is being replaced.");
                        File.Delete(fullPathName);
                    }
                    else
                        Engine.Base.Compute.RecordWarning($"File {fullPathName} will be replaced.");
                }
            }

            if (!activate)
                return false;

            try
            {
                File.WriteAllText(fullPathName, dataset.ToJson());
            }
            catch (Exception e)
            {
                string message = "Failed to write to file. Exception message given:";

                do
                {
                    message += Environment.NewLine + e.Message;
                    e = e.InnerException;
                } while (e != null);

                Base.Compute.RecordError(message);
                return false;
            }


            return true;
        }

        /***************************************************/
        /**** Private Methods                           ****/
        /***************************************************/

        private static void GetMethodStrings(Dataset dataset, out string assemblyName, out string className, out string methodName)
        {
            MethodBase method = (dataset.Data.First() as BH.oM.Test.UnitTests.UnitTest).Method;

            className = method.DeclaringType.Name;
            assemblyName = method.DeclaringType.Assembly.FullName.Split(',').First();

            methodName = (method is ConstructorInfo) ? method.DeclaringType.Name : method.Name;
            if (Base.Query.IsInterfaceMethod(method))
                methodName = methodName.Substring(1);
        }

        /***************************************************/

        private static bool CheckValidUnitTestDataSet(Dataset dataset)
        {
            if (dataset == null)
            {
                Engine.Base.Compute.RecordError("Provided Dataset is null.");
                return false;
            }

            if (dataset.Data == null || dataset.Data.Count == 0)
            {
                Engine.Base.Compute.RecordError("Provided Dataset does not contain any data.");
                return false;
            }

            if (dataset.Data.Any(x => !(x is BH.oM.Test.UnitTests.UnitTest)))
            {
                Engine.Base.Compute.RecordError("Provided Dataset does not contain UnitTest information.");
                return false;
            }

            if (dataset.Data.GroupBy(x => (x as BH.oM.Test.UnitTests.UnitTest).Method.DeclaringType).Count() != 1)
            {
                Engine.Base.Compute.RecordError("All provided UnitTests in the Dataset needs to belong to the same declaring type.");
                return false;
            }

            if (dataset.SourceInformation == null)
            {
                Engine.Base.Compute.RecordError("Dataset does not contain any source information.");
                return false;
            }

            if (!string.IsNullOrEmpty(dataset.SourceInformation.SourceLink) && !dataset.SourceInformation.SourceLink.StartsWith("https://"))
            {
                Engine.Base.Compute.RecordError("Dataset source does not contain a valid source link. Should be a link starting with 'https://'.");
                return false;
            }

            if (string.IsNullOrEmpty(dataset.SourceInformation.Author))
            {
                Engine.Base.Compute.RecordError("Dataset source does not contain an Author.");
                return false;
            }

            return true;
        }

        /***************************************************/

        private static bool CheckValidRepoFolderString(this string repoFolder, bool checkAssemblyFolder, string assemblyName, out string repoBaseFolder, out bool multiEngineProject)
        {
            repoBaseFolder = "";

            if (string.IsNullOrWhiteSpace(repoFolder))
            {
                Engine.Base.Compute.RecordError("Provided repofolder is null or empty. Please provide a folder targeting the github repo corresponding to the UnitTest Dataset being pushed.");
            }
            
            string[] array = Path.GetFullPath(repoFolder).Split('\\');

            int i = 0;
            while (i < array.Length && array[i] != ".ci")
            {
                repoBaseFolder += array[i] + "\\";
                i++;
            }

            if (!Directory.Exists(repoBaseFolder))
            {
                Base.Compute.RecordError("Provided repofolder does not exist.");
                multiEngineProject = false;
                return false;
            }

            if (!Directory.GetFiles(repoBaseFolder, "*.sln").Any())
            {
                Base.Compute.RecordError("Provided folder is not correctly targeting a github repository folder.");
                multiEngineProject = false;
                return false;
            }

            if (checkAssemblyFolder && !Directory.GetDirectories(repoBaseFolder, assemblyName).Any())
            {
                Base.Compute.RecordError($"Provided folder is not targeting a folder containing the project {assemblyName}. Please ensure the folder is the correct solution folder for the UnitTests evaluated.");
                multiEngineProject = false;
                return false;
            }

            multiEngineProject = Directory.GetDirectories(repoBaseFolder, "*_Engine").Count() > 1;

            return true;
        }

        /***************************************************/
    }
}




