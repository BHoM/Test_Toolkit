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


namespace BH.Engine.Test.Interoperability
{
    public static partial class Query
    {
        /***************************************************/
        /**** Public Methods                            ****/
        /***************************************************/

        [Description("Collects all PushPullObjectComparison for the list of TestResults and gives back a summary in terms of how many times each property shows up in the result set.")]
        [Input("pushPullCompareResults", "List of TestResults containing PushPullObjectComparison to evaluate.")]
        [Input("onlyLastProperty", "Only group by the last property key. This is, only the name of the final property failing, excluding any initial property.\n" +
               "As an example this would be StartNode.Position vs Position for the position of the start Node of a Bar.")]
        [Input("ignoreListIndex", "Ignores the list index position of a Property. if true the method will return Nodes rather than for example Nodes[4] for list properties.")]
        [Input("ignoreProperties", "Any properties containing any of the strings in this list will be omitted from the summary.")]
        [MultiOutput(0, "propertyType", "The type of property evaluated.")]
        [MultiOutput(1, "occurrences", "Number of occurrences of the difference showing up.")]
        [MultiOutput(2, "averageDifference", "Average difference between the pushed and pulled item. Difference given as a ratio of the difference between input and output divided by the input. Only available properties with a number type.")]
        [MultiOutput(3, "maximumDifference", "Maximum difference between pushed and pulled item. Difference given as a ratio of the difference between input and output divided by the input. Only available for numerical properties.")]
        [MultiOutput(4, "maxDiffPushedValue", "Value of the pushed item for the maximum difference of the property. Only available for numerical properties.")]
        [MultiOutput(5, "maxDiffPulledValue", "Value of the pulled item for the maximum difference of the property. Only available for numerical properties.")]
        public static Output<List<string>, List<int>, List<double>, List<double>, List<object>, List<object>> DifferenceSummary(this List<TestResult> pushPullCompareResults, bool onlyLastProperty = false, bool ignoreListIndex = false, List<string> ignoreProperties = null)
        {
            List<PushPullObjectComparison> differences = pushPullCompareResults.SelectMany(x => x.TestInformationOfType<PushPullObjectComparison>(true)).ToList();
            differences = differences.Where(x => KeepResult(x, ignoreProperties)).ToList();

            List<string> propertyType = new List<string>();
            List<int> occurrences = new List<int>();
            List<double> averageDifference = new List<double>();
            List<double> maximumDifference = new List<double>();
            List<object> maximumDifferencePushedValue = new List<object>();
            List<object> maximumDifferencePulledValue = new List<object>();

            foreach (var group in differences.GroupBy(x => PropertyGroupingIDKey(x.PropertyID, onlyLastProperty, ignoreListIndex)))
            {
                string prop = group.Key;
                int count = group.Count();

                List<double> diffVals = group.Select(x => PushPullValueDifference(x)).ToList();
                propertyType.Add(prop);
                occurrences.Add(count);
                averageDifference.Add(diffVals.Average());
                maximumDifference.Add(diffVals.Max());

                var orderedGroup = group.OrderByDescending(x => PushPullValueDifference(x));
                maximumDifferencePushedValue.Add(orderedGroup.First().PushedItem);
                maximumDifferencePulledValue.Add(orderedGroup.First().ReturnedItem);

            }

            return new Output<List<string>, List<int>, List<double>, List<double>, List<object>, List<object>>
            {
                Item1 = propertyType,
                Item2 = occurrences,
                Item3 = averageDifference,
                Item4 = maximumDifference,
                Item5 = maximumDifferencePushedValue,
                Item6 = maximumDifferencePulledValue
            };
        }

        /***************************************************/
        /**** Private Methods                           ****/
        /***************************************************/

        private static double PushPullValueDifference(PushPullObjectComparison diffObj)
        {
            double input, output;

            if (!double.TryParse(diffObj.PushedItem.ToString(), out input) || !double.TryParse(diffObj.ReturnedItem.ToString(), out output))
                return 0;

            if (input == 0 && output == 0)
                return 0;

            if (input == 0)
                return Math.Abs(input - output) / output;

            return Math.Abs(input - output) / input;
        }

        /***************************************************/

        private static bool KeepResult(PushPullObjectComparison diffrences, List<string> propertiesToIgnore)
        {
            if (propertiesToIgnore == null || propertiesToIgnore.Count == 0)
                return true;

            foreach (string prop in propertiesToIgnore)
            {
                if (diffrences.PropertyID.Contains(prop))
                    return false;
            }

            return true;
        }

        /***************************************************/
    }
}
