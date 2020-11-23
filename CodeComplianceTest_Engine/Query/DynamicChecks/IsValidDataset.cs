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

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using BH.oM.Test.CodeCompliance;
using System.IO;
using BH.oM.Data.Library;
using BH.Engine.Library;

namespace BH.Engine.Test.CodeCompliance.DynamicChecks
{
    public static partial class Query
    {
        public static ComplianceResult IsValidDataset(string filePath)
        {
            string documentationLink = "https://github.com/BHoM/documentation/wiki/IsValidDataset";
            //Read the dataset
            StreamReader sr = new StreamReader(filePath);
            string json = sr.ReadToEnd();
            sr.Close();

            if (json == null)
                return Create.ComplianceResult(ResultStatus.Pass);

            //Check if the dataset deserialises to a dataset object
            Dataset ds = BH.Engine.Serialiser.Convert.FromJson(json) as Dataset;

            if(ds == null)
            {
                //Dataset did not deserialise successfully
                return Create.ComplianceResult(ResultStatus.CriticalFail,
                    new List<Error>() { Create.Error("Dataset file did not deserialise into a BH.oM.Data.Library.Dataset object successfully.",
                        Create.Location(filePath, Create.LineSpan(1, 1)),
                        documentationLink,
                        ErrorLevel.Error,
                        "Dataset deserialisation error"
                    ) });
            }

            if(ds.SourceInformation == null)
            {
                //Source information is not set
                return Create.ComplianceResult(ResultStatus.Fail,
                    new List<Error>() { Create.Error("Dataset file does not contain any source information.",
                        Create.Location(filePath, Create.LineSpan(1, 1)),
                        documentationLink,
                        ErrorLevel.Warning,
                        "Dataset source error"
                    ) });
            }

            if (ds.SourceInformation.Author == null || ds.SourceInformation.Author == "")
            {
                //Source information does not contain an author
                return Create.ComplianceResult(ResultStatus.Fail,
                    new List<Error>() { Create.Error("Dataset file does not contain an author in the source information.",
                        Create.Location(filePath, Create.LineSpan(1, 1)),
                        documentationLink,
                        ErrorLevel.Warning,
                        "Dataset source author error"
                    ) });
            }

            if (ds.SourceInformation.Title == null || ds.SourceInformation.Title == "")
            {
                //Source information does not contain an author
                return Create.ComplianceResult(ResultStatus.Fail,
                    new List<Error>() { Create.Error("Dataset file does not contain a title in the source information.",
                        Create.Location(filePath, Create.LineSpan(1, 1)),
                        documentationLink,
                        ErrorLevel.Warning,
                        "Dataset source title error"
                    ) });
            }

            return Create.ComplianceResult(ResultStatus.Pass); //All is good
        }
    }
}
