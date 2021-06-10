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
using BH.oM.Test.Interoperability.Results;
using BH.oM.UnitTest.Results;
using BH.Engine.Test;

namespace BH.Engine.Test.Interoperability
{
    public static partial class Compute
    {
        /***************************************************/
        /**** Public Methods                            ****/
        /***************************************************/

        [Description("Compares two results sets against each other. DOes a deep comparison of inner Test information and returns a TestResult containing the details of the comparison.\n" + 
                     "Method assumes the two TestResults to be matching, that is be pointing at the same reference set/Object ID.")]
        [Input("result", "Result from a test just run to be compared to a reference result.")]
        [Input("refResult", "Reference result to compare to.")]
        [Output("result", "A test result containing information about the equality of the two input TestResults.")]
        public static TestResult Compare(TestResult result, TestResult refResult)
        {
            if (result == null)
                return null;

            if (refResult == null)
                return result.NoReferenceFound();

            TestResult diffResult = new TestResult() { Description = $"PushPullCompare difference: {result.ID}", ID = result.ID };

            //Check if same type, i.e. Pass/Warning/Error
            if (result.Status == refResult.Status)
            {
                if (result.Status == oM.Test.TestStatus.Warning)
                {
                    //If of type Warning, check the inner results
                    List<oM.Test.ITestInformation> referenceInformation = refResult.NonEventMessageInformation();

                    foreach (oM.Test.ITestInformation innerInformation in result.NonEventMessageInformation())
                    {
                        //Find matching reference data
                        oM.Test.ITestInformation reference = referenceInformation.GetSameIdInformation(innerInformation);

                        //Compare the inner test information
                        oM.Test.ITestInformation comparisonResult = ICompare(innerInformation, reference);
                        if (comparisonResult != null)
                            diffResult.Information.Add(comparisonResult);

                        if (reference != null)
                            referenceInformation.Remove(reference);
                    }

                    foreach (oM.Test.ITestInformation refInfo in referenceInformation)
                    {
                        diffResult.Information.Add(refInfo.IOnlyReferenceFound());
                    }
                }
                else
                {
                    //Else, the results are the same
                    diffResult.Message = "No change in the result status. Was and still is: " + result.Status + ". No changes in differences found from reference data.";
                    diffResult.Status = oM.Test.TestStatus.Pass;
                    return diffResult;
                }
            }
            //Result Status are different
            else
            {
                //Reference is pass, current is not
                if (refResult.Status == oM.Test.TestStatus.Pass)
                {
                    if (result.Status == oM.Test.TestStatus.Error)
                    {
                        //System is now crashing, was working before
                        diffResult.Message = "An error has been introduced that was previously fully passing!";
                        diffResult.Status = oM.Test.TestStatus.Error;
                        return diffResult;
                    }
                    else if (result.Status == oM.Test.TestStatus.Warning)
                    {
                        //New differences previously not present have been introduced
                        foreach (oM.Test.ITestInformation info in result.NonEventMessageInformation())
                        {
                            diffResult.Information.Add(info.INoReferenceFound());
                        }
                        diffResult.Message = "Object previously showing up as a pass is now showing warnings!";
                        diffResult.Status = oM.Test.TestStatus.Error;
                        return diffResult;
                    }
                }
                //Reference is exception, current is not
                else if (refResult.Status == oM.Test.TestStatus.Error)
                {
                    //Was previously crashing, is now returning equal
                    if (result.Status == oM.Test.TestStatus.Pass)
                    {
                        diffResult.Message = "Reference data is showing an error while new data is showing a full pass!";
                        diffResult.Status = oM.Test.TestStatus.Pass;
                        return diffResult;
                    }
                    else if (result.Status == oM.Test.TestStatus.Warning)
                    {
                        //Diffrences are now being flagged up that were not flagged before. Reason being the reference run contained crashes.
                        //Differences still to be logged to be investigated, but this should generally be seen as an improvement
                        //New differences previously not present have been introduced
                        foreach (oM.Test.ITestInformation info in result.NonEventMessageInformation())
                        {
                            diffResult.Information.Add(info.INewResultAfterCrashFix());
                        }
                        diffResult.Message = "A crash has been fixed. New data shows difference that now needs to be validated!";
                        diffResult.Status = oM.Test.TestStatus.Warning;
                        return diffResult;
                    }
                }
                else if (refResult.Status == oM.Test.TestStatus.Warning)
                {
                    if (result.Status == oM.Test.TestStatus.Pass)
                    {
                        diffResult.Message = "Reference data is showing warnings while new data is showing a full pass!";
                        diffResult.Status = oM.Test.TestStatus.Pass;

                        //Differences have now been cleared out. Registered as improvement
                        foreach (oM.Test.ITestInformation refInfo in refResult.NonEventMessageInformation())
                        {
                            diffResult.Information.Add(refInfo.IOnlyReferenceFound());
                        }
                    }
                    else if (result.Status == oM.Test.TestStatus.Error)
                    {
                        //System is now crashing, was working before
                        diffResult.Message = "An error has been introduced that was previously only showing warnings!";
                        diffResult.Status = oM.Test.TestStatus.Error;
                        return diffResult;
                    }
                }
            }


            diffResult.Status = diffResult.Information.MostSevereStatus();

            if (diffResult.Status == oM.Test.TestStatus.Error)
                diffResult.Message = "Differences have been introduced, made worse or have changed.";
            else if (diffResult.Status == oM.Test.TestStatus.Pass)
                diffResult.Message = "No new differences have been introduced.";
            else
                diffResult.Message = "Some differences have either been removed or probably improved. The change is probably for the better but needs to be validated.";

            return diffResult;
        }

        /***************************************************/

        [Description("Compares two rPushPullObjectComparison. Atempts to work out if a numerical difference is an improvement or not, comparing against the pushed data.\n" +
                     "Method assumes the two PushPullObjectComparison to be matching, that is be pointing at the same reference Object and PropertyID.")]
        [Input("result", "Result from a test just run to be compared to a reference result.")]
        [Input("reference", "Reference result to compare to.")]
        [Output("result", "A ComparisonDifference containing information about the equality of the two input PushPullObjectComparison.")]
        public static ComparisonDifference Compare(PushPullObjectComparison result, PushPullObjectComparison reference)
        {
            if (result == null)
                return null;

            if (reference == null)
            {
                //No avilable reference
                return result.NoReferenceFound();
            }
            else if (result.PushedItem != reference.PushedItem)
            {
                //Different data has been pushed
                return new ComparisonDifference
                {
                    Message = "The input data has changed",
                    Status = oM.Test.TestStatus.Error,
                    Property = result.PropertyId,
                    RunValue = result.PushedItem,
                    ReferenceValue = reference.PushedItem
                };
            }
            else if (result.ReturnedItem == reference.ReturnedItem)
            {
                //Identical return as the reference
                return new ComparisonDifference
                {
                    Message = "Difference is unchanged",
                    Status = oM.Test.TestStatus.Pass,
                    Property = result.PropertyId,
                    RunValue = result.ReturnedItem,
                    ReferenceValue = reference.ReturnedItem
                };
            }
            else
            {
                //Something is different than before

                //Try extract numerical information to try to deduce if it is an improvement or not
                double d, d1, d2;
                if (double.TryParse(result.PushedItem, out d) && double.TryParse(result.ReturnedItem, out d1) && double.TryParse(reference.ReturnedItem, out d2))
                {
                    double diffVal1 = Math.Abs(d - d1);
                    double diffVal2 = Math.Abs(d - d2);

                    if (diffVal1 == diffVal2)
                    {
                        //Equal
                        //Identical return as the reference
                        return new ComparisonDifference
                        {
                            Message = "Difference is unchanged",
                            Status = oM.Test.TestStatus.Pass,
                            Property = result.PropertyId,
                            RunValue = result.ReturnedItem,
                            ReferenceValue = reference.ReturnedItem
                        };
                    }
                    else if (diffVal1 < diffVal2)
                    {
                        //Improvement
                        return new ComparisonDifference
                        {
                            Message = "Return data is different from the reference, but is a probable improvement.",
                            Status = oM.Test.TestStatus.Warning,
                            Property = result.PropertyId,
                            RunValue = result.ReturnedItem,
                            ReferenceValue = reference.ReturnedItem
                        };
                    }
                    else
                    {
                        //Worsened
                        return new ComparisonDifference
                        {
                            Message = "Data is different from the reference, worse than reference.",
                            Status = oM.Test.TestStatus.Error,
                            Property = result.PropertyId,
                            RunValue = result.ReturnedItem,
                            ReferenceValue = reference.ReturnedItem
                        };
                    }
                }
                else
                {
                    //Can not extract numerical information, report back that there is a difference
                    return new ComparisonDifference
                    {
                        Message = "Data is different from the reference",
                        Status = oM.Test.TestStatus.Error,
                        Property = result.PropertyId,
                        RunValue = result.ReturnedItem,
                        ReferenceValue = reference.ReturnedItem
                    };
                }
            }
        }


        /***************************************************/
        /**** Public Methods - Interface                ****/
        /***************************************************/

        public static oM.Test.ITestInformation ICompare(oM.Test.ITestInformation result, oM.Test.ITestInformation refResult)
        {
            if (refResult == null)
                return result.INoReferenceFound();

            return Compare(result as dynamic, refResult as dynamic);
        }

        /***************************************************/
        /**** Private Methods - Fallback                ****/
        /***************************************************/

        private static oM.Test.ITestInformation Compare(oM.Test.ITestInformation result, oM.Test.ITestInformation refResult)
        {
            return null;
        }

        /***************************************************/
        /**** Private Methods                           ****/
        /***************************************************/

        private static oM.Test.ITestInformation GetSameIdInformation(this IEnumerable<oM.Test.ITestInformation> infoList, oM.Test.ITestInformation toFind)
        {
            return infoList.FirstOrDefault(x => x.IHasMatchingIds(toFind));
        }

        /***************************************************/
    }
}
