/*
 * This file is part of the Buildings and Habitats object Model (BHoM)
 * Copyright (c) 2015 - 2024, the respective contributors. All rights reserved.
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

        [Description("Compares two results sets against each other. Does a deep comparison of inner Test information and returns a TestResult containing the details of the comparison.\n" + 
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
                    List<oM.Test.ITestInformation> referenceInformationNoMatch = referenceInformation.ToList();

                    foreach (oM.Test.ITestInformation innerInformation in result.NonEventMessageInformation())
                    {
                        //Find matching reference data
                        oM.Test.ITestInformation reference = referenceInformation.IGetSameIDInformation(innerInformation);

                        //Compare the inner test information
                        oM.Test.ITestInformation comparisonResult = ICompare(innerInformation, reference);
                        if (comparisonResult != null)
                            diffResult.Information.Add(comparisonResult);

                        if (reference != null)
                            referenceInformationNoMatch.Remove(reference);
                    }

                    foreach (oM.Test.ITestInformation refInfo in referenceInformationNoMatch)
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
            else
            {
                //Result Status are different
                if (refResult.Status == oM.Test.TestStatus.Pass)
                {
                    //Reference is pass, current is not
                    if (result.Status == oM.Test.TestStatus.Error)
                    {
                        //System is now crashing, was working before
                        diffResult.Message = "An error has been introduced that did not exist previously.";
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
                        diffResult.Message = "Warnings have been introduced with these changes.";
                        diffResult.Message += Environment.NewLine + diffResult.ExceptionProperties(true, true, oM.Test.TestStatus.Warning);
                        diffResult.Status = oM.Test.TestStatus.Error;
                        return diffResult;
                    }
                }
                else if (refResult.Status == oM.Test.TestStatus.Error)
                {
                    //Reference is Error, current is not
                    if (result.Status == oM.Test.TestStatus.Pass)
                    {
                        //Was previously crashing, is now returning Pass
                        diffResult.Message = "This test was failing and is now passing under test conditions with the latest changes.";
                        diffResult.Status = oM.Test.TestStatus.Pass;
                        return diffResult;
                    }
                    else if (result.Status == oM.Test.TestStatus.Warning)
                    {
                        //Differences are now being flagged up that were not flagged before. Reason being the reference run contained crashes.
                        //Differences still to be logged to be investigated, but this should generally be seen as an improvement
                        //New differences previously not present have been introduced
                        foreach (oM.Test.ITestInformation info in result.NonEventMessageInformation())
                        {
                            diffResult.Information.Add(info.INewResultAfterCrashFix());
                        }
                        diffResult.Message = "A crash has been fixed. New data shows differences that need to be validated.";
                        diffResult.Message += Environment.NewLine + diffResult.ExceptionProperties(true, true, oM.Test.TestStatus.Warning);
                        diffResult.Status = oM.Test.TestStatus.Warning;
                        return diffResult;
                    }
                }
                else if (refResult.Status == oM.Test.TestStatus.Warning)
                {
                    if (result.Status == oM.Test.TestStatus.Pass)
                    {
                        diffResult.Message = "This test previously had warnings which have now been resolved in the latest test.";
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
                        diffResult.Message = "An error has been introduced where previously there were only warnings.";
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

            diffResult.Message += Environment.NewLine + diffResult.ExceptionProperties(true, true, oM.Test.TestStatus.Warning);

            return diffResult;
        }

        /***************************************************/

        [Description("Compares two PushPullObjectComparison. Attempts to work out if a numerical difference is an improvement or not, comparing against the pushed data.\n" +
                     "Method assumes the two PushPullObjectComparison to be matching, that is be pointing at the same reference Object and PropertyID.")]
        [Input("result", "An inner PushPullObjectComparison from a test just run to be compared to a reference result.")]
        [Input("reference", "Reference result to compare to.")]
        [Output("result", "A ComparisonDifference containing information about the equality of the two input PushPullObjectComparison.")]
        public static ComparisonDifference Compare(PushPullObjectComparison result, PushPullObjectComparison reference)
        {
            if (result == null)
                return null;

            if (reference == null)
            {
                //No available reference
                return result.NoReferenceFound();
            }
            else if (result.PropertyID != reference.PropertyID)
            {
                if (result.PropertyID.Contains(reference.PropertyID))
                {
                    //Results property ID is a subpart of the reference. This mean the new result is closer to the original pushed object
                    return new ComparisonDifference
                    {
                        Message = "Run item is showing a difference of an inner property of an object that was previously showing difference on a higher level object. This is likely an improvement, but needs to be validated!",
                        Status = oM.Test.TestStatus.Warning,
                        Property = result.PropertyID,
                        RunValue = result.ReturnedItem,
                        ReferenceValue = reference.PropertyID
                    };
                }
                else if (reference.PropertyID.Contains(result.PropertyID))
                {
                    //Opposite of the above. The reference is showing a difference on a inner more property than the just run data.
                    //This generally should mean a worsening of the convert situation.
                    return new ComparisonDifference
                    {
                        Message = "Run item is showing a difference on an outermore property compared to the reference. This possibly means a convert has been made worse.",
                        Status = oM.Test.TestStatus.Error,
                        Property = result.PropertyID,
                        RunValue = result.ReturnedItem,
                        ReferenceValue = reference.PropertyID
                    };
                }
                else
                {
                    return new ComparisonDifference
                    {
                        Message = "Unable to compare results as properties are different.",
                        Status = oM.Test.TestStatus.Error,
                        Property = result.PropertyID,
                        RunValue = result.ReturnedItem,
                        ReferenceValue = reference.PropertyID
                    };
                }
            }
            else if (result.PushedItem != reference.PushedItem)
            {
                //Different data has been pushed
                return new ComparisonDifference
                {
                    Message = "The input data has changed",
                    Status = oM.Test.TestStatus.Error,
                    Property = result.PropertyID,
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
                    Property = result.PropertyID,
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
                            Property = result.PropertyID,
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
                            Property = result.PropertyID,
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
                            Property = result.PropertyID,
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
                        Property = result.PropertyID,
                        RunValue = result.ReturnedItem,
                        ReferenceValue = reference.ReturnedItem
                    };
                }
            }
        }


        /***************************************************/
        /**** Public Methods - Interface                ****/
        /***************************************************/

        [Description("Compares two TestInformation results against each other by dispatching to a comparison matching the type of the two obejcts. Types between the result and reference need to be an exact match.")]
        [Input("result", "Result from a test just run to be compared to a reference result.")]
        [Input("refResult", "Reference result to compare to. Needs to be of the same type as the provided result.")]
        [Output("result", "A test result containing information about the equality of the two input TestInformations. Returned type will depend on provided type.")]
        public static oM.Test.ITestInformation ICompare(oM.Test.ITestInformation result, oM.Test.ITestInformation refResult)
        {
            if (result == null)
                return null;

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

        private static oM.Test.ITestInformation IGetSameIDInformation(this IEnumerable<oM.Test.ITestInformation> infoList, oM.Test.ITestInformation toFind)
        {
            return GetSameIDInformation(infoList as dynamic, toFind as dynamic);
        }

        /***************************************************/

        private static oM.Test.ITestInformation GetSameIDInformation(this IEnumerable<oM.Test.ITestInformation> infoList, oM.Test.ITestInformation toFind)
        {
            return infoList.FirstOrDefault(x => x.IHasMatchingIDs(toFind));
        }

        /***************************************************/

        private static oM.Test.ITestInformation GetSameIDInformation(this IEnumerable<oM.Test.ITestInformation> infoList, PushPullObjectComparison toFind)
        {
            //Find exact match
            oM.Test.ITestInformation found = infoList.FirstOrDefault(x => x.IHasMatchingIDs(toFind));

            //If no exact match found, look for partial match
            if (found == null)
            {
                found = infoList.OfType<PushPullObjectComparison>().FirstOrDefault(x => x.PropertyID.Contains(toFind.PropertyID) || toFind.PropertyID.Contains(x.PropertyID));
            }
            return found;
        }

        /***************************************************/

    }
}



