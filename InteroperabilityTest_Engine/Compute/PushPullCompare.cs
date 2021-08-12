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
using BH.oM.Data.Requests;
using BH.oM.Data.Collections;
using BH.oM.Reflection;

using BH.oM.Test.Results;
using BH.Adapter;
using BH.oM.Adapter.Commands;
using BH.oM.Diffing;
using BH.Engine.Base;
using BH.oM.Reflection.Debugging;
using BH.oM.Test.Interoperability;
using BH.oM.Test.Interoperability.Results;
using BH.oM.Test.Interoperability.Settings;
using BH.oM.Adapter;


namespace BH.Engine.Test.Interoperability
{
    public static partial class Compute
    {
        /***************************************************/
        /**** Public Methods                            ****/
        /***************************************************/

        public static List<TestResult> PushPullCompare(InteropabilityTestSettings settings)
        {
            //Check settings for null
            if (settings == null)
            {
                Reflection.Compute.RecordError("Null InteropabilityTestSettings provided.");
                return null;
            }

            //Create the adapter
            IBHoMAdapter adapter = Adapter.Create.BHoMAdapter(settings.AdapterType, settings.AdapterConstructorArguments);

            //Check adapter created properly
            if (adapter == null)
            {
                Reflection.Compute.RecordError("Failed to construct the adapter");
                return null;
            }

            //Check types provided properly
            if (settings.TestTypes == null || settings.TestTypes.Count == 0)
            {
                Reflection.Compute.RecordError("No test types provided.");
                return null;
            }

            //Check config provided properly
            if (settings.PushPullConfig == null)
            {
                Reflection.Compute.RecordError("No PushPullComapreConfig provided");
                return null;
            }

            //Run through the PushPullCompare testing and return all the results
            List<TestResult> results = new List<TestResult>();
            foreach (Type type in settings.TestTypes)
            {
                results.AddRange(PushPullCompare(adapter, type, settings.PushPullConfig, true));
            }

            return results;
        }

        /*************************************/

        [Description("Tests Pushing objects of a specific type, then pulling them back and comparing the objects. Returns Results outlining if the objects pulled are identical to the pushed ones, and if not, what properties are different between the two.")]
        [Input("adapter", "The instance of the adapter to test for.")]
        [Input("type", "The type of object to test. This will use test sets in the Dataset library.")]
        [Input("config", "Config for the test. Controls whether the adapter should be reset between runs and what comparer to use.")]
        [Input("active", "Toggles whether to run the test.")]
        [Output("testResult", "Diffing results outlining any differences found between the pushed and pulled objects.")]
        public static List<TestResult> PushPullCompare(IBHoMAdapter adapter, Type type, PushPullCompareConfig config = null, bool active = false)
        {
            if (!active)
                return new List<TestResult>();

            config = config ?? new PushPullCompareConfig();

            //Get testdata
            var testSets = Query.TestDataOfType(type);

            if (testSets == null || testSets.Item1 == null || testSets.Item2 == null)
            {
                Reflection.Compute.RecordError("Failed to extract testdata");
                return new List<TestResult>();
            }

            List<string> testSetNames = testSets.Item1;
            List<List<IBHoMObject>> testData = testSets.Item2;

            //Set up comparer and request
            FilterRequest request = new FilterRequest { Type = type };
            IEqualityComparer<IBHoMObject> comparer = config.Comparer(adapter.AdapterIdFragmentType);

            //List for storing output

            List<TestResult> results = new List<TestResult>();


            for (int i = 0; i < testSetNames.Count; i++)
            {
                results.Add(RunOneSet(adapter, testSetNames[i], testData[i], request, comparer, config.ResetModelBetweenPushes));
            }

            return results;
        }

        /***************************************************/

        [Description("Tests Pushing objects, then pulling them back and comparing the objects. Returns Results outlining if the objects pulled are identical to the pushed ones, and if not, what properties are different between the two.")]
        [Input("adapter", "The instance of the adapter to test for.")]
        [Input("testObjects", "The list of object to test.")]
        [Input("setName", "The name of the testset to obejcts belongs to.")]
        [Input("enforcedType", "If null, the testObjects will be grouped by type and a filter request of this type will be used to pull the obejcts back.\n" +
                               "If set, the objects will be checked if they can be assigned to the provided type. If not, the execution will stop, if yes, the obejcts will be pulled using a filter request with the provided type.")]
        [Input("config", "Config for the test. Controls whether the adapter should be reset between runs and what comparer to use.")]
        [Input("active", "Toggles whether to run the test.")]
        [Output("testResult", "Diffing results outlining any differences found between the pushed and pulled objects.")]
        public static List<TestResult> PushPullCompare(IBHoMAdapter adapter, List<IBHoMObject> testObjects, string setName = "", Type enforcedType = null, PushPullCompareConfig config = null, bool active = false)
        {
            if (!active)
                return new List<TestResult>();

            if (enforcedType != null && testObjects.Any(x => !enforcedType.IsAssignableFrom(x.GetType())))
            {
                Reflection.Compute.RecordError("The testObjects is not matching the enforced type and are not a subtype of the enforced type.");
                return new List<TestResult>();
            }

            config = config ?? new PushPullCompareConfig();

            IEqualityComparer<IBHoMObject> comparer = config.Comparer(adapter.AdapterIdFragmentType);

            List<TestResult> testResults = new List<TestResult>();

            if (enforcedType == null)
            {
                foreach (var group in testObjects.GroupBy(x => x.GetType()))
                {
                    FilterRequest request = new FilterRequest { Type = group.Key };

                    testResults.Add(RunOneSet(adapter, setName, group.ToList(), request, comparer, config.ResetModelBetweenPushes));
                }
            }
            else
            {
                FilterRequest request = new FilterRequest { Type = enforcedType };
                testResults.Add(RunOneSet(adapter, setName, testObjects, request, comparer, config.ResetModelBetweenPushes));
            }
            return testResults;
        }

        /***************************************************/
        /**** Private Methods                           ****/
        /***************************************************/

        private static TestResult RunOneSet(IBHoMAdapter adapter, string setName, List<IBHoMObject> objects, IRequest request, IEqualityComparer<IBHoMObject> comparer, bool resetModelBetweenRuns)
        {
            if (adapter == null)
            {
                return new TestResult { Description = "PushPullCompare", Message = "Adapter is null, could not preform test", Status = oM.Test.TestStatus.Error };
            }

            TestResult setResult = new TestResult { Description = $"PushPullCompare: Adapter: {adapter.GetType().Name}, test set: {setName}.", ID = adapter.GetType().Name + ":" + setName };

            bool success = true;

            //CalculateObjectHashes
            objects = objects.SetHashFragment();

            //Start up an empty model
            if (resetModelBetweenRuns)
                adapter.Execute(new NewModel());

            List<IBHoMObject> pushedObjects = new List<IBHoMObject>();
            List<IBHoMObject> pulledObjects = new List<IBHoMObject>();

            BH.oM.Base.ComparisonConfig config = new BH.oM.Base.ComparisonConfig();
            config.PropertyExceptions.Add("CustomData");
            config.TypeExceptions.Add(typeof(HashFragment));

            //Push objects
            try
            {
                Engine.Reflection.Compute.ClearCurrentEvents();
                pushedObjects = adapter.Push(objects).Cast<IBHoMObject>().ToList();
                success &= pushedObjects.Count == objects.Count;
            }
            catch (Exception e)
            {
                Engine.Reflection.Compute.RecordError(e.Message);
                setResult.Message = "The adapter crashed trying to push the objects.";
                setResult.Status = oM.Test.TestStatus.Error;
                return setResult;
            }
            finally
            {
                setResult.Information.AddRange(Engine.Reflection.Query.CurrentEvents().Select(x => x.ToEventMessage()));
            }

            if (pushedObjects.Count != objects.Count)
            {
                setResult.Information.AddRange(objects.Where(x => !pushedObjects.Any(y => x.Name == y.Name)).Select(x => TestFailedResult(x, "Object could not be pushed.")));
            }

            //Pull objects
            try
            {
                Engine.Reflection.Compute.ClearCurrentEvents();
                pulledObjects = adapter.Pull(request).Cast<IBHoMObject>().ToList();
                success &= pulledObjects.Count == pushedObjects.Count;
            }
            catch (Exception e)
            {
                Engine.Reflection.Compute.RecordError(e.Message);
                setResult.Message = "The adapter crashed trying to pull the objects.";
                setResult.Status = oM.Test.TestStatus.Error;
                return setResult;
            }
            finally
            {
                setResult.Information.AddRange(Engine.Reflection.Query.CurrentEvents().Select(x => x.ToEventMessage()));
            }

            //Compare pushed and pulled objects
            VennDiagram<IBHoMObject> diagram = Engine.Data.Create.VennDiagram(pushedObjects, pulledObjects, comparer);


            foreach (Tuple<IBHoMObject, IBHoMObject> pair in diagram.Intersection)
            {
                setResult.Information.Add(ComparePushedAndPulledObject(pair.Item1, pair.Item2, config));
            }

            setResult.Information.AddRange(diagram.OnlySet1.Select(x => TestFailedResult(x, "Unable to compare the object.")));

            //Close the model
            if (resetModelBetweenRuns)
                adapter.Execute(new Close { SaveBeforeClose = false });

            //Clear events
            Engine.Reflection.Compute.ClearCurrentEvents();

            setResult.Status = setResult.Information.OfType<TestResult>().MostSevereStatus();

            if (setResult.Status == oM.Test.TestStatus.Error)
                setResult.Message = "Errors where raised trying to run the test";
            else if (setResult.Status == oM.Test.TestStatus.Pass)
                setResult.Message = "All objects were successfully pushed, pulled and compared without any differences.";
            else
                setResult.Message = "All objects were successfully pushed and pulled, but some differences were found.";

            return setResult;
        }

        /***************************************************/

        private static TestResult TestResultFromPushedObject(IBHoMObject obj)
        {
            return new TestResult
            {
                Description = $"Test object of type {obj.GetType().Name} with name {obj.Name}",
                ID = obj.Hash(null, true),
            };
        }

        /***************************************************/

        private static TestResult TestFailedResult(IBHoMObject obj, string message = "")
        {
            TestResult result = TestResultFromPushedObject(obj);
            result.Message = message;
            result.Status = oM.Test.TestStatus.Error;
            return result;
        }

        /***************************************************/

        private static TestResult ComparePushedAndPulledObject(IBHoMObject pushedObj, IBHoMObject pulledObj, ComparisonConfig config)
        {
            TestResult result = TestResultFromPushedObject(pushedObj);

            try
            {
                var equalityResult = Engine.Test.Query.IsEqual(pushedObj, pulledObj, config);

                Type type = pushedObj.GetType();

                for (int i = 0; i < equalityResult.Item2.Count; i++)
                {
                    result.Information.Add(new PushPullObjectComparison
                    {
                        Message = $"Difference found in {type.Name}. {equalityResult.Item2[i]} was {equalityResult.Item3[i]} on the pushed item and {equalityResult.Item4[i]} on the pulled item.",
                        ObjectType = type,
                        PropertyID = equalityResult.Item2[i],
                        PushedItem = equalityResult.Item3[i],
                        ReturnedItem = equalityResult.Item4[i],
                        Status = oM.Test.TestStatus.Warning
                    });
                }
            }
            catch (Exception e)
            {
                result.Status = oM.Test.TestStatus.Error;
                result.Message = "Failed to run the comparison of the Pushed and Pulled object." + Environment.NewLine;
                result.Message += $"Exception thrown: {e.Message}";
                return result;
            }

            if (result.Information.Count == 0)
            {
                result.Status = oM.Test.TestStatus.Pass;
                result.Message = "No differences found between the pushed and pulled object";
            }
            else
            {
                result.Status = oM.Test.TestStatus.Warning;
                result.Message = "Differences found between the pushed and the pulled object.";
            }

            return result;
        }
        
        /***************************************************/

    }
}

