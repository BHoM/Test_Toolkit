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

using System;
using System.Linq;
using System.Collections.Generic;
using System.ComponentModel;
using BH.oM.Reflection.Attributes;
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
        [Input("replacePreExisting", "If true, replaces any pre-existing dataset in the folder.")]
        [Input("activate", "Toggle to push dataset to file.")]
        [Output("success", "Returns true if sucessfully able to write the dataset to file.")]
        public static bool StoreUnitTests(List<BH.oM.Test.UnitTests.UnitTest> unitTests, string repoFolder, string sourceLink, string author = "", bool replacePreExisting = false, bool activate = false)
        {
            if (!activate)
                return false;

            bool success = true;

            foreach (Dataset dataset in Create.UnitTestDataSet(unitTests, sourceLink, author))
            {
                success &= StoreUnitTest(dataset, repoFolder, replacePreExisting, activate);
            }

            return success;
        }

        /***************************************************/

        [Description("Stores out the Provided Dataset in the .ci/Dataset corresponding to the project and class name with a file name matching the method name.")]
        [Input("dataset", "Dataset to store in .ci folder. Should contain only UnitTest information.")]
        [Input("repoFolder", "Folder link to the folder corresponding to the repo containing the method(s) being tested by the UnitTest(s) in the Dataset.")]
        [Input("replacePreExisting", "If true, replaces any pre-existing dataset in the folder.")]
        [Input("activate", "Toggle to push dataset to file.")]
        [Output("success", "Returns true if sucessfully able to write the dataset to file.")]
        public static bool StoreUnitTest(Dataset dataset, string repoFolder, bool replacePreExisting = false, bool activate = false)
        {
            if (!activate)
                return false;

            string repoBaseFolder;
            if (!CheckValidUnitTestDataSet(dataset) || !CheckValidRepoFolderString(repoFolder, out repoBaseFolder))
                return false;

            string fullPathName = Path.Combine(repoBaseFolder, GetRelativeFilePath(dataset));

            if (!Directory.Exists(Path.GetDirectoryName(fullPathName)))
                Directory.CreateDirectory(Path.GetDirectoryName(fullPathName));

            if (File.Exists(fullPathName))
            {
                if (!replacePreExisting)
                {
                    Engine.Reflection.Compute.RecordError($"File {fullPathName} already exists. To replace it, toggle {nameof(replacePreExisting)} to true.");
                    return false;
                }
                else
                {
                    Engine.Reflection.Compute.RecordNote($"File {fullPathName} is being replaced.");
                    File.Delete(fullPathName);
                }
            }

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

                Reflection.Compute.RecordError(message);
                return false;
            }


            return true;
        }

        /***************************************************/
        /**** Private Methods                           ****/
        /***************************************************/

        private static string GetRelativeFilePath(Dataset dataset)
        {
            MethodBase method = (dataset.Data.First() as BH.oM.Test.UnitTests.UnitTest).Method;

            string className = method.DeclaringType.Name;
            string assemblyName = method.DeclaringType.Assembly.FullName.Split(',').First();

            string methodName = (method is ConstructorInfo) ? method.DeclaringType.Name : method.Name;
            if (Reflection.Query.IsInterfaceMethod(method))
                methodName = methodName.Substring(1);

            methodName += ".json";

            return Path.Combine(".ci", "Datasets", assemblyName, className, methodName);
        }

        /***************************************************/

        private static bool CheckValidUnitTestDataSet(Dataset dataset)
        {
            if (dataset == null)
            {
                Engine.Reflection.Compute.RecordError("Provided Dataset is null.");
                return false;
            }

            if (dataset.Data == null || dataset.Data.Count == 0)
            {
                Engine.Reflection.Compute.RecordError("Provided Dataset does not contain any data.");
                return false;
            }

            if (dataset.Data.Any(x => !(x is BH.oM.Test.UnitTests.UnitTest)))
            {
                Engine.Reflection.Compute.RecordError("Provided Dataset does not contain UnitTest information.");
                return false;
            }

            if (dataset.Data.GroupBy(x => (x as BH.oM.Test.UnitTests.UnitTest).Method.DeclaringType).Count() != 1)
            {
                Engine.Reflection.Compute.RecordError("All provided UnitTests in the Dataset needs to belong to the same declaring type.");
                return false;
            }

            if (dataset.SourceInformation == null)
            {
                Engine.Reflection.Compute.RecordError("Dataset does not contain any source information.");
                return false;
            }

            if (string.IsNullOrEmpty(dataset.SourceInformation.SourceLink) || !dataset.SourceInformation.SourceLink.StartsWith("https://"))
            {
                Engine.Reflection.Compute.RecordError("Dataset source does not contain a valid source link. Should be a link starting with 'https://'.");
                return false;
            }

            if (string.IsNullOrEmpty(dataset.SourceInformation.Author))
            {
                Engine.Reflection.Compute.RecordError("Dataset source does not contain an Author.");
                return false;
            }

            return true;
        }

        /***************************************************/

        private static bool CheckValidRepoFolderString(this string repoFolder, out string repoBaseFolder)
        {
            repoBaseFolder = "";

            if (string.IsNullOrWhiteSpace(repoFolder))
            {
                Engine.Reflection.Compute.RecordError("Provided repofolder is null or empty. Please provide a folder targeting the github repo corresponding to the UnitTest Dataset being pushed.");
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
                Reflection.Compute.RecordError("Provided repofolder does not exist.");
                return false;
            }

            if (!Directory.GetFiles(repoBaseFolder, "*.sln").Any())
            {
                Reflection.Compute.RecordError("Provided folder is not correctly targeting a github repository folder.");
                return false;
            }

            return true;
        }

        /***************************************************/
    }
}
