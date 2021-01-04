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

namespace BH.Engine.Test.Interoperability
{
    public static partial class Compute
    {
        /***************************************************/
        /**** Public Methods                            ****/
        /***************************************************/

        [Description("Compares two sets of InputOutputComparisons and sorts out any differences between the two sets.")]
        [Input("results", "The results from a test just run.")]
        [Input("referenceResults", "The reference set of results to compare two.")]
        [Output("diffings", "The differences and similarities between the two sets.")]
        public static List<InputOutputComparisonDiffing> ComputeInputOutputComparisonDiffings(List<InputOutputComparison> results, List<InputOutputComparison> referenceResults)
        {

            List<InputOutputComparisonDiffing> comparisonResults = new List<InputOutputComparisonDiffing>();

            //Group the reference results by result set
            Dictionary<string, List<InputOutputComparison>> referenceResultsDict = referenceResults.GroupBy(x => x.ResultCase.ToString()).ToDictionary(g => g.Key, g => g.ToList());

            foreach (InputOutputComparison comparison in results)
            {
                List<InputOutputComparison> referenceResultsList;
                //Try to get the results from the corresponding set out
                if (referenceResultsDict.TryGetValue(comparison.ResultCase.ToString(), out referenceResultsList))
                {

                    //Find with matching ID
                    var match = referenceResultsList.Select((x, i) => new { res = x, index = i }).FirstOrDefault(x => x.res.ObjectId.Equals(comparison.ObjectId));

                    if (match == null || match.res == null || match.index == -1)
                    {
                        //No object matches found, matching the hash -> input data has changed
                        comparisonResults.Add(CreateComparison(comparison, null, InputOutputComparisonDiffingType.InputDataChanged));
                        continue;
                    }

                    referenceResultsList.RemoveAt(match.index);
                    InputOutputComparison reference = match.res;
                    //Check if comparison is fully equal equal
                    if (IsDispatchEqual(comparison, reference))
                    {
                        if (comparison.Results.Any())
                        {
                            for (int i = 0; i < comparison.Results.Count; i++)
                            {
                                comparisonResults.Add(CreateComparison(comparison.Results[i], reference.Results[i], InputOutputComparisonDiffingType.Equal));
                            }
                        }
                        else
                            comparisonResults.Add(CreateComparison(comparison, match.res, InputOutputComparisonDiffingType.Equal));
                    }

                    //If not fully equal, compare the two comparisons
                    comparisonResults.AddRange(Compare(comparison, reference));

                }
                else
                {
                    //No results found in the same reference set
                    if (comparison.Results.Any())
                    {
                        for (int i = 0; i < comparison.Results.Count; i++)
                        {
                            comparisonResults.Add(CreateComparison(comparison.Results[i], null, InputOutputComparisonDiffingType.NoAvailableReferenceSet));
                        }
                    }
                    else
                        comparisonResults.Add(CreateComparison(comparison, null, InputOutputComparisonDiffingType.NoAvailableReferenceSet));

                }
            }

            //Results only found in reference
            foreach (KeyValuePair<string, List<InputOutputComparison>> kvp in referenceResultsDict)
            {
                foreach (InputOutputComparison refResult in kvp.Value)
                {
                    if (refResult.Results.Count > 0)
                    {
                        foreach (InputOutputDifference refDifferenece in refResult.Results)
                        {
                            comparisonResults.Add(new InputOutputComparisonDiffing(refDifferenece.ObjectId, refDifferenece.ResultCase, refDifferenece.PropertyId, refDifferenece.ObjectType, InputOutputComparisonDiffingType.DataOnlyAvailableInReference, refDifferenece.InputItem, "", refDifferenece.ReturnedItem));
                        }
                    }
                    else
                    {
                        comparisonResults.Add(new InputOutputComparisonDiffing(refResult.ObjectId, refResult.ResultCase, "", refResult.ObjectType, InputOutputComparisonDiffingType.DataOnlyAvailableInReference, "", "", ""));
                    }
                }
            }

            return comparisonResults;

        }

        /***************************************************/
        /**** Private Methods                           ****/
        /***************************************************/

        private static List<InputOutputComparisonDiffing> Compare(InputOutputComparison comparison, InputOutputComparison reference)
        {
            List<InputOutputComparisonDiffing> comparisonResults = new List<InputOutputComparisonDiffing>();

            //Check if same type, i.e. equal/exception/difference
            if (comparison.ResultType == reference.ResultType)
            {
                if (comparison.ResultType == InputOutputComparisonType.Difference)
                {
                    //If of type difference, check the inner results
                    Dictionary<string, InputOutputDifference> referenceDiffrences = reference.Results.ToDictionary(x => x.PropertyId);

                    foreach (InputOutputDifference diffrence in comparison.Results)
                    {
                        InputOutputDifference refDiff;
                        if (referenceDiffrences.TryGetValue(diffrence.PropertyId, out refDiff))
                        {
                            comparisonResults.Add(Compare(diffrence, refDiff));
                            referenceDiffrences.Remove(diffrence.PropertyId);
                        }
                        else
                            comparisonResults.Add(CreateComparison(diffrence, null, InputOutputComparisonDiffingType.IntroducedDifference));
                    }

                    foreach (var kvp in referenceDiffrences)
                    {
                        //Add any leftover values only available in the reference set
                        comparisonResults.Add(new InputOutputComparisonDiffing(kvp.Value.ObjectId, kvp.Value.ResultCase, kvp.Value.PropertyId, kvp.Value.ObjectType, InputOutputComparisonDiffingType.RemovedDifference, kvp.Value.InputItem, null, kvp.Value.ReturnedItem));
                    }
                }
                else
                {
                    //Else, the results are the same (This part should never be reached, but filtered out beforehand)
                    comparisonResults.Add(CreateComparison(comparison, reference, InputOutputComparisonDiffingType.Equal));
                }
            }
            //Result types are different
            else
            {
                //Reference is equal, current is not
                if (reference.ResultType == InputOutputComparisonType.Equal)
                {
                    if (comparison.ResultType == InputOutputComparisonType.Exception)
                    {
                        //System is now crashing, was working before
                        comparisonResults.Add(CreateComparison(comparison, reference, InputOutputComparisonDiffingType.IntroducedException));
                    }
                    else if (comparison.ResultType == InputOutputComparisonType.Difference)
                    {
                        //New differences previously not present have been introduced
                        foreach (InputOutputDifference difference in comparison.Results)
                        {
                            comparisonResults.Add(CreateComparison(difference, null, InputOutputComparisonDiffingType.IntroducedDifference));
                        }
                    }
                }
                //Reference is exception, current is not
                else if (reference.ResultType == InputOutputComparisonType.Exception)
                {
                    //Was previously crashing, is now returning equal
                    if (comparison.ResultType == InputOutputComparisonType.Equal)
                    {
                        comparisonResults.Add(CreateComparison(comparison, reference, InputOutputComparisonDiffingType.Improvement));
                    }
                    else if (comparison.ResultType == InputOutputComparisonType.Difference)
                    {
                        //Diffrences are now being flagged up that were not flagged before. Reason being the reference run contained crashes.
                        //Differences still to be logged to be investigated, but this should generally be seen as an improvement
                        foreach (InputOutputDifference difference in comparison.Results)
                        {
                            comparisonResults.Add(CreateComparison(difference, null, InputOutputComparisonDiffingType.RemovedException));
                        }
                    }
                }
                else if (reference.ResultType == InputOutputComparisonType.Difference)
                {
                    if (comparison.ResultType == InputOutputComparisonType.Equal)
                    {
                        //Differences have now been cleared out. Registered as improvement
                        foreach (InputOutputDifference difference in reference.Results)
                        {
                            //Add any leftover values only available in the reference set
                            comparisonResults.Add(new InputOutputComparisonDiffing(difference.ObjectId, difference.ResultCase, difference.PropertyId, difference.ObjectType, InputOutputComparisonDiffingType.RemovedDifference, difference.InputItem, null, difference.ReturnedItem));
                        }
                    }
                    else if (comparison.ResultType == InputOutputComparisonType.Exception)
                    {
                        //System is now crashing, was working before
                        comparisonResults.Add(CreateComparison(comparison, reference, InputOutputComparisonDiffingType.IntroducedException));
                    }
                }
            }

            return comparisonResults;
        }

        /***************************************************/

        private static InputOutputComparisonDiffing Compare(InputOutputDifference result, InputOutputDifference reference)
        {
            if (reference == null)
            {
                //No reference found
                return CreateComparison(result, reference, InputOutputComparisonDiffingType.IntroducedDifference);
            }
            else if (result.InputItem != reference.InputItem)
            {
                return new InputOutputComparisonDiffing(result.ObjectId, result.ResultCase, result.PropertyId, result.ObjectType, InputOutputComparisonDiffingType.InputDataChanged, null, result.InputItem, reference.InputItem);
            }
            else if (result.ReturnedItem == reference.ReturnedItem)
            {
                return CreateComparison(result, reference, InputOutputComparisonDiffingType.Equal);
            }
            else
            {
                //Something is different than before

                //Try extract numerical information to try to deduce if it is an improvement or not
                double d, d1, d2;
                if (double.TryParse(result.InputItem, out d) && double.TryParse(result.ReturnedItem, out d1) && double.TryParse(reference.ReturnedItem, out d2))
                {
                    double diffVal1 = Math.Abs(d - d1);
                    double diffVal2 = Math.Abs(d - d2);

                    if (diffVal1 == diffVal2)
                    {
                        //Equal
                        return CreateComparison(result, reference, InputOutputComparisonDiffingType.Equal);
                    }
                    else if (diffVal1 < diffVal2)
                    {
                        //Improvement
                        return CreateComparison(result, reference, InputOutputComparisonDiffingType.Improvement);
                    }
                    else
                    {
                        //Worsened
                        return CreateComparison(result, reference, InputOutputComparisonDiffingType.Deterioration);
                    }
                }
                else
                {
                    //Can not extract numerical information, report back that there is a difference
                    return CreateComparison(result, reference, InputOutputComparisonDiffingType.Difference);
                }
            }
        }

        /***************************************************/

        private static InputOutputComparisonDiffing CreateComparison(InputOutputDifference result, InputOutputDifference reference, InputOutputComparisonDiffingType type)
        {
            return new InputOutputComparisonDiffing(result.ObjectId, result.ResultCase, result.PropertyId, result.ObjectType, type, result.InputItem, result.ReturnedItem, reference?.ReturnedItem ?? "");
        }

        /***************************************************/

        private static InputOutputComparisonDiffing CreateComparison(InputOutputComparison result, InputOutputComparison reference, InputOutputComparisonDiffingType type)
        {
            return new InputOutputComparisonDiffing(result.ObjectId, result.ResultCase, result.ObjectType.Name, result.ObjectType, type, "", "", "");
        }

        /***************************************************/
    }
}

