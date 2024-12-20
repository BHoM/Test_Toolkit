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
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using BH.oM.Test.CodeCompliance;
using System.IO;
using BH.oM.Data.Library;

using BH.oM.Test;
using BH.oM.Test.Results;

namespace BH.Engine.Test.CodeCompliance.DynamicChecks
{
    public static partial class Query
    {
        public static TestResult IsValidDataset(this string filePath)
        {
            if (!File.Exists(filePath))
                return Create.TestResult(TestStatus.Pass);

            string documentationLink = "IsValidDataset";
            //Read the dataset
            StreamReader sr = new StreamReader(filePath);
            string json = sr.ReadToEnd();
            sr.Close();

            if (json == null)
                return Create.TestResult(TestStatus.Pass);

            //Check if the dataset deserialises to a dataset object
            Dataset ds = BH.Engine.Serialiser.Convert.FromJson(json) as Dataset;

            if(ds == null)
            {
                //Dataset did not deserialise successfully
                return Create.TestResult(TestStatus.Error,
                    new List<Error>() { Create.Error($"Dataset file did not deserialise into a BH.oM.Data.Library.Dataset object successfully. For more information see {Base.Query.DocumentationURL("DevOps/Code%20Compliance%20and%20CI/Compliance%20Checks/IsValidDataset")}",
                        Create.Location(filePath, Create.LineSpan(1, 1)),
                        documentationLink,
                        TestStatus.Error,
                        "Dataset deserialisation error"
                    ) });
            }

            if(ds.SourceInformation == null)
            {
                //Source information is not set
                return Create.TestResult(TestStatus.Warning,
                    new List<Error>() { Create.Error($"Dataset file does not contain any source information. For more information see {Base.Query.DocumentationURL("DevOps/Code%20Compliance%20and%20CI/Compliance%20Checks/IsValidDataset")}",
                        Create.Location(filePath, Create.LineSpan(1, 1)),
                        documentationLink,
                        TestStatus.Warning,
                        "Dataset source error"
                    ) });
            }

            DatasetSource dss = new DatasetSource();
            dss.Message = "Title: " + ds.SourceInformation.Title + System.Environment.NewLine +
                        "Author: " + ds.SourceInformation.Author + System.Environment.NewLine +
                        "Confidence: " + ds.SourceInformation.Confidence.ToString() + System.Environment.NewLine +
                        "Version: " + ds.SourceInformation.Version + System.Environment.NewLine +
                        "Source Link: " + ds.SourceInformation.SourceLink + System.Environment.NewLine +
                        "Publisher: " + ds.SourceInformation.Publisher + System.Environment.NewLine +
                        "Language: " + ds.SourceInformation.Language + System.Environment.NewLine +
                        "Schema: " + ds.SourceInformation.Schema + System.Environment.NewLine +
                        "Location: " + ds.SourceInformation.Location + System.Environment.NewLine +
                        "Copyright: " + ds.SourceInformation.Copyright + System.Environment.NewLine +
                        "Contributors: " + ds.SourceInformation.Contributors + System.Environment.NewLine +
                        "Item Reference: " + ds.SourceInformation.ItemReference + System.Environment.NewLine;

            dss.Status = TestStatus.Pass;

            TestResult result = new TestResult() { Status = TestStatus.Pass };
            result.Information.Add(dss);
            List<Error> errors = new List<Error>();

            if (ds.SourceInformation.Author == null || ds.SourceInformation.Author == "")
            {
                //Source information does not contain an author
                errors.Add(Create.Error($"Dataset file does not contain an author in the source information. For more information see {Base.Query.DocumentationURL("DevOps/Code%20Compliance%20and%20CI/Compliance%20Checks/IsValidDataset")}",
                        Create.Location(filePath, Create.LineSpan(1, 1)),
                        documentationLink,
                        TestStatus.Warning,
                        "Dataset source author error"
                    ));

                result.Status = TestStatus.Warning;
            }

            if (ds.SourceInformation.Title == null || ds.SourceInformation.Title == "")
            {
                //Source information does not contain an author
                errors.Add(Create.Error($"Dataset file does not contain a title in the source information. For more information see {Base.Query.DocumentationURL("DevOps/Code%20Compliance%20and%20CI/Compliance%20Checks/IsValidDataset")}",
                        Create.Location(filePath, Create.LineSpan(1, 1)),
                        documentationLink,
                        TestStatus.Warning,
                        "Dataset source title error"
                    ));

                result.Status = TestStatus.Warning;
            }

            if (ds.SourceInformation.Confidence == Confidence.Undefined)
            {
                //Source confidence has not been set
                errors.Add(Create.Error($"Dataset confidence level has not been correctly set. For more information see {Base.Query.DocumentationURL("DevOps/Code%20Compliance%20and%20CI/Compliance%20Checks/IsValidDataset")}",
                        Create.Location(filePath, Create.LineSpan(1, 1)),
                        documentationLink,
                        TestStatus.Error,
                        "Dataset confidence error"
                    ));

                result.Status = TestStatus.Error;
            }

            if (errors.Count > 0)
                result.Information.AddRange(errors.Select(x => x as ITestInformation));

            return result;
        }
    }
}





