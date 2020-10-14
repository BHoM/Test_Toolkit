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
using System.Linq;
using System.Collections.Generic;
using System.ComponentModel;
using BH.oM.Reflection.Attributes;
using BH.oM.Base;
using BH.oM.Test.Results;

namespace BH.Engine.Test.Interoperability
{ 
    public static partial class Query
    {
        /***************************************************/
        /**** Public Methods                            ****/
        /***************************************************/

        [Description("Generates a summary of the InputOutputComparison, stating worst cases and how many occurences of each type of result that exists in the testset.")]
        [Input("results", "The set of results to evaluate.")]
        [Input("ignoreProperties", "Optional properties to ignore from the summary. Only applicable for results of type difference.")]
        [Output("summary", "The summary of the evaluated results")]
        public static List<InputOutputComparisonSummary> TestSummary(List<InputOutputComparison> results, List<string> ignoreProperties = null)
        {
            ignoreProperties = ignoreProperties ?? new List<string>();

            List<InputOutputComparisonSummary> summaries = new List<InputOutputComparisonSummary>();

            foreach (var group in results.GroupBy(x => new { obj = x.ObjectType, res = x.ResultType }))
            {
                if (group.Key.res == InputOutputComparisonType.Difference)
                {
                    foreach (var differenceGroup in group.SelectMany(x => x.Results).GroupBy(x => x.PropertyId))
                    {
                        if (!ignoreProperties.Any(x => differenceGroup.Key.Contains(x)))
                            summaries.Add(Summarise(differenceGroup));
                    }
                }
                else
                {
                    string objectIds = group.Select(x => x.ObjectId.ToString()).Distinct().Aggregate((x, y) => x + ", " + y);
                    string resultCases = group.Select(x => x.ResultCase.ToString()).Distinct().Aggregate((x, y) => x + ", " + y);
                    summaries.Add(new InputOutputComparisonSummary(objectIds, resultCases, group.Key.obj, group.Key.res, "", 0, group.Count(), 0, 0, 0, 0));
                }

            }

            summaries.Sort();

            return summaries;
        }

        /***************************************************/
        /**** Private Methods                            ****/
        /***************************************************/

        private static InputOutputComparisonSummary Summarise(IEnumerable<InputOutputDifference> differences)
        {

            double max = 0;
            double maxItem1 = 0;
            double maxItem2 = 0;
            double avg = 0;
            double count = 0;

            foreach (InputOutputDifference diff in differences)
            {
                double item1, item2;
                if (double.TryParse(diff.InputItem, out item1) && double.TryParse(diff.ReturnedItem, out item2))
                {
                    double valDiff;
                    if ((item1 == 0 && item2 == 0) || (item1 + item2) / Math.Max(Math.Abs(item1), Math.Abs(item2)) < 0.000001)
                        valDiff = 0;
                    else
                        valDiff = Math.Abs((item1 - item2) / ((item1 + item2) / 2));

                    if (valDiff > max)
                    {
                        max = valDiff;
                        maxItem1 = item1;
                        maxItem2 = item2;
                    }
                    avg += valDiff;
                    count++;
                }
            }

            avg = avg / (Math.Max(1.0, count));
            int nb = differences.Count();

            string objectIds = differences.Select(x => x.ObjectId.ToString()).Distinct().Aggregate((x, y) => x + ", " + y);
            string resultCases = differences.Select(x => x.ResultCase.ToString()).Distinct().Aggregate((x, y) => x + ", " + y);
            return new InputOutputComparisonSummary(objectIds, resultCases, differences.First().ObjectType, InputOutputComparisonType.Difference, differences.First().PropertyId, 0, nb, avg, max, maxItem1, maxItem2);

        }

        /***************************************************/
    }
}
