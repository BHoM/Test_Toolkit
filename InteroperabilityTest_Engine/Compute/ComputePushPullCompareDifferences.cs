using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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

        public static TestResult ComputePushPullCompareDifferences(List<TestResult> setResults, List<TestResult> refResults)
        {
            TestResult fullResult = new TestResult { Description = "PushPullCompare difference" };

            List<TestResult> refResultsCopy = new List<TestResult>(refResults);

            foreach (TestResult result in setResults)
            {
                TestResult refRes = refResultsCopy.Where(x => x.ID == result.ID).FirstOrDefault();

                if (refRes == null)
                {
                    fullResult.Information.Add(NoReferenceFound(refRes));
                }
                else
                {
                    refResultsCopy.Remove(refRes);
                    fullResult.Information.Add(CompareResultSets(result, refRes));
                }
            }

            foreach (TestResult refObjRes in refResultsCopy)
            {
                fullResult.Information.Add(new TestResult
                {
                    Description = $"PushPullCompare difference: {refObjRes.ID}",
                    ID = refObjRes.ID,
                    Status = oM.Test.TestStatus.Warning,
                    Message = "Set only found in reference data. No comparison available from run."

                });
            }

            fullResult.Status = fullResult.Information.MostSevereStatus();

            if (fullResult.Status == oM.Test.TestStatus.Error)
                fullResult.Message = "Differences have been introduced, made worse or have changed in at least one of the result sets.";
            else if (fullResult.Status == oM.Test.TestStatus.Pass)
                fullResult.Message = "No new differences have been introduced.";
            else
                fullResult.Message = "Some differences have either been removed or probably improved. The change is probably for the better but needs to be validated.";

            return fullResult;

        }

        /***************************************************/
        /**** Private Methods                           ****/
        /***************************************************/

        private static TestResult CompareResultSets(TestResult resultSet, TestResult refRes)
        {
            TestResult diffResult = new TestResult() { Description = $"PushPullCompare difference: {resultSet.ID}", ID = resultSet.ID };

            List<TestResult> refObejctResults = refRes.Information.OfType<TestResult>().ToList();

            foreach (TestResult objRes in resultSet.Information.OfType<TestResult>())
            {
                TestResult refObjRes = refObejctResults.Where(x => x.ID == objRes.ID).FirstOrDefault();

                if (refRes == null)
                {
                    diffResult.Information.Add(NoReferenceFound(refObjRes));
                }
                else
                {
                    refObejctResults.Remove(refObjRes);
                    diffResult.Information.Add(CompareObjectResults(objRes, refObjRes));
                }

            }

            foreach (TestResult refObjRes in refObejctResults)
            {
                diffResult.Information.Add(new TestResult
                {
                    Description = $"PushPullCompare object difference: {refObjRes.ID}",
                    ID = refObjRes.ID,
                    Status = oM.Test.TestStatus.Warning,
                    Message = "Object only found in reference data. No comparison available from run."

                });
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

        private static TestResult CompareObjectResults(TestResult result, TestResult refResult)
        {
            TestResult diffResult = new TestResult() { Description = $"PushPullCompare object difference: {result.ID}", ID = result.ID };

            //Check if same type, i.e. equal/exception/difference
            if (result.Status == refResult.Status)
            {
                if (result.Status == oM.Test.TestStatus.Warning)
                {
                    //If of type difference, check the inner results
                    List<PushPullObjectComparison> referenceComparisons = refResult.Information.OfType<PushPullObjectComparison>().ToList();

                    foreach (PushPullObjectComparison comp in result.Information.OfType<PushPullObjectComparison>())
                    {
                        PushPullObjectComparison reference = referenceComparisons.Where(x => x.PropertyId == comp.PropertyId).FirstOrDefault();

                        diffResult.Information.Add(Compare(comp, reference));

                        if (reference != null)
                            referenceComparisons.Remove(reference);
                    }

                    foreach (PushPullObjectComparison comp in referenceComparisons)
                    {
                        diffResult.Information.Add(new ComparisonDifference
                        {
                            Message = "Difference only found in reference.",
                            ReferenceValue = comp.ReturnedItem,
                            Property = comp.PropertyId,
                            Status = oM.Test.TestStatus.Warning
                        });
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
            //Result types are different
            else
            {
                //Reference is equal, current is not
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
                        foreach (PushPullObjectComparison comp in result.Information.OfType<PushPullObjectComparison>())
                        {
                            diffResult.Information.Add(new ComparisonDifference
                            {
                                Message = "Difference introduced.",
                                ReferenceValue = comp.ReturnedItem,
                                Property = comp.PropertyId,
                                Status = oM.Test.TestStatus.Warning
                            });
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
                        foreach (PushPullObjectComparison comp in result.Information.OfType<PushPullObjectComparison>())
                        {
                            diffResult.Information.Add(new ComparisonDifference
                            {
                                Message = "Difference now showing after previous crash.",
                                ReferenceValue = comp.ReturnedItem,
                                Property = comp.PropertyId,
                                Status = oM.Test.TestStatus.Warning
                            });
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
                        diffResult.Message = "Reference data is showing a crash while new data is showing a full pass!";
                        diffResult.Status = oM.Test.TestStatus.Warning;

                        //Differences have now been cleared out. Registered as improvement
                        foreach (PushPullObjectComparison comp in refResult.Information.OfType<PushPullObjectComparison>())
                        {
                            diffResult.Information.Add(new ComparisonDifference
                            {
                                Message = "Difference only found in reference.",
                                ReferenceValue = comp.ReturnedItem,
                                Property = comp.PropertyId,
                                Status = oM.Test.TestStatus.Warning
                            });
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

        private static ComparisonDifference Compare(PushPullObjectComparison result, PushPullObjectComparison reference)
        {
            if (reference == null)
            {
                //No avilable reference
                return new ComparisonDifference
                {
                    Message = "A difference has been introduced.",
                    Status = oM.Test.TestStatus.Error,
                    Property = result.PropertyId,
                    RunValue = result.ReturnedItem
                };
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

        private static TestResult NoReferenceFound(TestResult resultSet)
        {
            return new TestResult
            {
                Description = "PushPullCompare difference",
                Message = $"No reference results could be found for {resultSet.ID}",
                ID = resultSet.ID,
                Status = oM.Test.TestStatus.Error
            };
        }

        /***************************************************/

    }
}
