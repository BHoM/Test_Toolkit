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
using BH.oM.Reflection;
using BH.oM.Test.Results;

namespace BH.Engine.Test.Interoperability
{
    public static partial class Compute
    {
        /***************************************************/
        /**** Public Methods                            ****/
        /***************************************************/

        [Description("Dispatches results based on whether they exist in the reference set or not.")]
        [Input("results", "The list of result to check against a reference.")]
        [Input("referenceResults", "The list of reference results to check against.")]
        [MultiOutput(0, "matches", "Results existing in both the result input and in the reference.")]
        [MultiOutput(1, "onlyInresults", "Results that only exists in the results input.")]
        [MultiOutput(2, "onlyInReference", "Results that only exists in the reference.")]
        public static Output<List<InputOutputComparison>, List<InputOutputComparison>, List<InputOutputComparison>> DispatchResultDifferences(List<InputOutputComparison> results, List<InputOutputComparison> referenceResults)
        {
            List<InputOutputComparison> matches = new List<InputOutputComparison>();
            List<InputOutputComparison> onlyInInput = new List<InputOutputComparison>();

            //Group by result case to increace performance
            Dictionary<string, List<InputOutputComparison>> referenceResultsDict = referenceResults.GroupBy(x => x.ResultCase.ToString()).ToDictionary(g => g.Key, g => g.ToList());

            foreach (InputOutputComparison result in results)
            {
                List<InputOutputComparison> referenceResultsList;
                //Try to get the results from the corresponding set out
                if (referenceResultsDict.TryGetValue(result.ResultCase.ToString(), out referenceResultsList))
                {
                    //Try find a full match
                    var match = referenceResultsList.Select((x, i) => new { res = x, index = i }).FirstOrDefault(x => IsDispatchEqual(result, x.res));

                    if (match == null)
                    {
                        onlyInInput.Add(result);
                    }
                    else
                    {
                        //Add match and remove from reference
                        matches.Add(result);
                        referenceResultsList.RemoveAt(match.index);
                    }
                }
                else
                {
                    onlyInInput.Add(result);
                }
            }

            return new Output<List<InputOutputComparison>, List<InputOutputComparison>, List<InputOutputComparison>>() { Item1 = matches, Item2 = onlyInInput, Item3 = referenceResultsDict.Values.SelectMany(x => x).ToList() };
        }

        /***************************************************/
        /**** Private Methods                           ****/
        /***************************************************/

        private static bool IsDispatchEqual(InputOutputComparison result, InputOutputComparison reference)
        {
            try
            {
                if (result == null || reference == null)
                    return false;

                if (!result.ResultCase.Equals(reference.ResultCase))
                    return false;

                if (!result.ObjectId.Equals(reference.ObjectId))
                    return false;

                if (result.ResultType != reference.ResultType)
                    return false;

                if (result.ObjectType != reference.ObjectType)
                    return false;

                if (result.Results.Count != reference.Results.Count)
                    return false;

                for (int i = 0; i < result.Results.Count; i++)
                {
                    if (!IsDispatchEqual(result.Results[i], reference.Results[i]))
                        return false;
                }

                return true;
            }
            catch
            {
                return false;
            }
        }

        /***************************************************/

        private static bool IsDispatchEqual(InputOutputDifference result, InputOutputDifference reference)
        {
            try
            {
                if (result == null || reference == null)
                    return false;

                if (!result.ResultCase.Equals(reference.ResultCase))
                    return false;

                if (!result.ObjectId.Equals(reference.ObjectId))
                    return false;

                if (result.PropertyId != reference.PropertyId)
                    return false;

                if (result.InputItem != reference.InputItem)
                    return false;

                if (result.ReturnedItem != reference.ReturnedItem)
                    return false;

                return true;
            }
            catch
            {
                return false;
            }
        }

        /***************************************************/
    }
}

