/*
 * This file is part of the Buildings and Habitats object Model (BHoM)
 * Copyright (c) 2015 - 2026, the respective contributors. All rights reserved.
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
using BH.oM.Test.UnitTests;
using System.Reflection;

namespace BH.Engine.UnitTest
{ 
    public static partial class Create
    {
        /***************************************************/
        /**** Public Methods                            ****/
        /***************************************************/

        [Description("Merges a lists of UnitTests and groups them based on their MethodName and declaring type, and produces one dataset per group. Created Dataset set up to be valid with .ci checks of unit tests.")]
        [Input("unitTests", "UnitsTests to make part of the Dataset. UnitTests will be merged based on Method and grouped by MethodName and declaring type.")]
        [Input("sourceLink", "Link to the script or process used to generate the UnitTest. Important to be able to easily update the test in case of a change required from code updates.")]
        [Input("author", "Author of the UnitTests. If nothing is provided, the currently logged in windows username will be used.")]
        [Input("confidence", "Confidence of the UnitTests. Should generally relate to the number of potential usecases and edge cases that the test data for the UnitTest is covering.")]
        [Output("datasets", "The created Datasets containing the provided UnitTests.")]
        public static List<Dataset> UnitTestDataSet(List<BH.oM.Test.UnitTests.UnitTest> unitTests, string sourceLink = "", string author = "", Confidence confidence = Confidence.Undefined)
        {
            if (unitTests == null || unitTests.Count == 0)
                return new List<Dataset>();

            if (!string.IsNullOrEmpty(sourceLink) && !sourceLink.StartsWith("https://"))
            {
                Engine.Base.Compute.RecordError("Please provide a valid source link to the script used to generate the UnitTests. Link needs to be a valid weblink starting with 'https://'.");
                return new List<Dataset>();
            }

            if (string.IsNullOrWhiteSpace(author))
            {
                author = Environment.UserName;
                Engine.Base.Compute.RecordNote($"No author provided. Using current username: {author}");
            }

            //Merge unittests of the same method
            List<BH.oM.Test.UnitTests.UnitTest> mergedTests = Compute.MergeUnitTests(unitTests);

            if (mergedTests.Count == 0)
            {
                Engine.Base.Compute.RecordError($"No valid {nameof(BH.oM.Test.UnitTests.UnitTest)}s provided. No Dataset created.");
                return new List<Dataset>();
            }

            List<Dataset> datasets = new List<Dataset>();
            foreach (var group in mergedTests.GroupBy(x => new { Name = x.Method.NonInterfaceName(), T = x.Method.DeclaringType }))
            {
                Source source = new Source() { SourceLink = sourceLink, Title = group.Key.Name, Author = author, Confidence = confidence };
                datasets.Add(Engine.Data.Create.Dataset(group.Cast<IBHoMObject>().ToList(), source, group.Key.Name));
            }

            return datasets;
        }

        /***************************************************/
        /**** Private Methods                           ****/
        /***************************************************/

        private static string NonInterfaceName(this MethodBase method)
        {
            string name = (method is ConstructorInfo) ? method.DeclaringType.Name : method.Name;
            if (Base.Query.IsInterfaceMethod(method))
                name = name.Substring(1);
            return name;
        }

        /***************************************************/
    }
}





