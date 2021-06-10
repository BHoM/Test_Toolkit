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
using BH.oM.Test.Results;
using BH.oM.UnitTest.Results;
using BH.oM.Test.Interoperability.Results;
using BH.oM.Reflection;
using BH.Engine.Test;
using BH.oM.Test;

namespace BH.Engine.Test.Interoperability
{
    public static partial class Query
    {
        /***************************************************/
        /**** Public Methods                            ****/
        /***************************************************/

        [Description("Finds all properties reported and dispatches to output corresponding to error, warning and passes.")]
        [Input("pushPullCompareDiffingResults", "Test result from diffing on a PushPullCOmpare result compared to a reference set.")]
        [Input("onlyLastProperty", "Only group by the last property key. This is, only the name of the final property failing, excluding any initial property.\n" +
               "As an example this would be StartNode.Position vs Position for the Positional point of the start Node of a Bar.")]
        [Input("ignoreListIndex", "Igonores the list index position of a Property. if true the will return Nodes rather than for example Nodes[4] for list properties.")]
        [MultiOutput(0, "errors", "The properties that show up as errors in the test.")]
        [MultiOutput(1, "warnings", "The properties that show up as warnings in the test.")]
        [MultiOutput(2, "passes", "The properties that show up as passes in the test.")]
        public static Output<List<string>, List<string>, List<string>> ExceptionProperties(this TestResult pushPullCompareDiffingResults, bool onlyLastProperty = false, bool ignoreListIndex = false)
        {
            if (pushPullCompareDiffingResults == null)
                return new Output<List<string>, List<string>, List<string>>();

            List<ComparisonDifference> diffrences = pushPullCompareDiffingResults.TestInformationOfType<ComparisonDifference>(true);

            List<string> errors = new List<string>();
            List<string> warnings = new List<string>();
            List<string> passes = new List<string>();

            foreach (var group in diffrences.GroupBy(x => PropertyGroupingIdKey(x.Property, onlyLastProperty, ignoreListIndex)))
            {
                TestStatus status = group.MostSevereStatus();

                switch (status)
                {
                    case TestStatus.Error:
                        errors.Add(group.Key);
                        break;
                    case TestStatus.Pass:
                        passes.Add(group.Key);
                        break;
                    case TestStatus.Warning:
                        warnings.Add(group.Key);
                        break;
                }
            }

            return new Output<List<string>, List<string>, List<string>>() { Item1 = errors, Item2 = warnings, Item3 = passes };
        }

        /***************************************************/
        /**** Private Methods                           ****/
        /***************************************************/

        private static string ExceptionPropertiesGroupingKey(string propertyId, bool onlyLastProperty)
        {
            if (onlyLastProperty)
            {
                return propertyId.Split('.').Last();
            }
            else
                return propertyId;
        }

        /***************************************************/
    }
}
