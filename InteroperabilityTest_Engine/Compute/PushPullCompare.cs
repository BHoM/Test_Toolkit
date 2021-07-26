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


namespace BH.Engine.Test.Interoperability
{
    public static partial class Compute
    {
        /***************************************************/
        /**** Public Methods                            ****/
        /***************************************************/

        [Description("Tests Pushing objects of a specific type, then pulling them back and comparing the objects. Returns Results outlining if the objects pulled are identical to the pushed ones, and if not, what properties are different between the two")]
        [Input("adapter", "The instance of the adapter to test for")]
        [Input("type", "The type of object to test. This will use test sets in the Dataset library")]
        [Input("config", "Config for the test. Controls whether the adapter should be reset between runs and what comparer to use.")]
        [Input("active", "Toggles whether to run the test")]
        [MultiOutput(0, "diffingResults", "Diffing results outlining any differences found between the pushed and pulled objects.")]
        [MultiOutput(1, "adapterLog", "A list of any error or warning messages returned by the adapter in the process, as a dictionary sorted by name of the set.")]
        public static Output<List<InputOutputComparison>, Dictionary<string, List<Event>>> PushPullCompare(BHoMAdapter adapter, Type type, PushPullCompareConfig config = null, bool active = false)
        {
            if (!active)
                return new Output<List<InputOutputComparison>, Dictionary<string, List<Event>>>();

            config = config ?? new PushPullCompareConfig();

            //Get testdata
            var testSets = Query.TestDataOfType(type);

            if (testSets == null || testSets.Item1 == null || testSets.Item2 == null)
            {
                Reflection.Compute.RecordError("Failed to extract testdata");
                return new Output<List<InputOutputComparison>, Dictionary<string, List<Event>>>();
            }

            List<string> testSetNames = testSets.Item1;
            List<List<IBHoMObject>> testData = testSets.Item2;

            //Set up comparer and request
            FilterRequest request = new FilterRequest { Type = type };
            IEqualityComparer<IBHoMObject> comparer = config.Comparer(adapter.AdapterIdFragmentType);

            //List for storing output

            List<InputOutputComparison> results = new List<InputOutputComparison>();
            Dictionary<string, List<Event>> events = new Dictionary<string, List<Event>>();

            for (int i = 0; i < testSetNames.Count; i++)
            {
                List<InputOutputComparison> tempResults;
                List<Event> tempEvents;
                if (!RunOneSet(adapter, testSetNames[i], testData[i], request, comparer, config.ResetModelBetweenPushes, out tempResults, out tempEvents))
                    Engine.Reflection.Compute.RecordWarning("Failed to run set " + testSetNames[i] + ". Please check the event log!");

                results.AddRange(tempResults);
                events[testSetNames[i]] = tempEvents;
            }

            return new Output<List<InputOutputComparison>, Dictionary<string, List<Event>>>() { Item1 = results, Item2 = events };
        }

        /***************************************************/

        [Description("Tests Pushing objects, then pulling them back and comparing the objects. Returns Results outlining if the objects pulled are identical to the pushed ones, and if not, what properties are different between the two")]
        [Input("adapter", "The instance of the adapter to test for")]
        [Input("testObjects", "The list of object to test.")]
        [Input("setName", "The name of the testset to obejcts belongs to.")]
        [Input("enforcedType", "If null, the testObjects will be grouped by type and a filter request of this type will be used to pull the obejcts back.\n" +
                               "If set, the objects will be checked if they can be assigned to the provided type. If not, the execution will stop, if yes, the obejcts will be pulled using a filter request with the provided type.")]
        [Input("config", "Config for the test. Controls whether the adapter should be reset between runs and what comparer to use.")]
        [Input("active", "Toggles whether to run the test")]
        [MultiOutput(0, "diffingResults", "Diffing results outlining any differences found between the pushed and pulled objects.")]
        [MultiOutput(1, "adapterLog", "A list of any error or warning messages returned by the adapter in the process.")]
        public static Output<List<InputOutputComparison>, List<Event>> PushPullCompare(BHoMAdapter adapter, List<IBHoMObject> testObjects, string setName = "", Type enforcedType = null, PushPullCompareConfig config = null, bool activate = false)
        {
            if (!activate)
                return new Output<List<InputOutputComparison>, List<Event>>();

            if (enforcedType != null && testObjects.Any(x => !enforcedType.IsAssignableFrom(x.GetType())))
            {
                Reflection.Compute.RecordError("The testObjects is not matching the enforced type and are not a subtype of the enforced type.");
                return new Output<List<InputOutputComparison>, List<Event>>();
            }

            config = config ?? new PushPullCompareConfig();

            IEqualityComparer<IBHoMObject> comparer = config.Comparer(adapter.AdapterIdFragmentType);

            List<InputOutputComparison> results = new List<InputOutputComparison>();
            List<Event> events = new List<Event>();

            if (enforcedType == null)
            {
                foreach (var group in testObjects.GroupBy(x => x.GetType()))
                {
                    FilterRequest request = new FilterRequest { Type = group.Key };
                    List<InputOutputComparison> tempResults;
                    List<Event> tempEvents;
                    RunOneSet(adapter, setName, group.ToList(), request, comparer, config.ResetModelBetweenPushes, out tempResults, out tempEvents);

                    results.AddRange(tempResults);
                    events.AddRange(tempEvents);
                }
            }
            else
            {
                FilterRequest request = new FilterRequest { Type = enforcedType };
                RunOneSet(adapter, setName, testObjects, request, comparer, config.ResetModelBetweenPushes, out results, out events);
            }

            return new Output<List<InputOutputComparison>, List<Event>>() { Item1 = results, Item2 = events };
        }

        /***************************************************/
        /**** Private Methods                           ****/
        /***************************************************/

        private static bool RunOneSet(BHoMAdapter adapter, string setName, List<IBHoMObject> objects, IRequest request, IEqualityComparer<IBHoMObject> comparer, bool resetModelBetweenRuns, out List<InputOutputComparison> results, out List<Event> events)
        {
            bool success = true;

            results = new List<InputOutputComparison>();
            events = new List<Event>();

            //Calcualte timestamp
            double timestep = DateTime.UtcNow.ToOADate();

            //CalculateObjectHashes
            objects = objects.Select(x => x.ShallowClone()).ToList();
            objects.ForEach(x => x.CustomData["PushPull_Hash"] = x.Hash());

            //Start up an empty model
            if(resetModelBetweenRuns)
                adapter.Execute(new NewModel());

            List<IBHoMObject> pushedObjects = new List<IBHoMObject>();
            List<IBHoMObject> pulledObjects = new List<IBHoMObject>();

            BH.oM.Base.ComparisonConfig config = new BH.oM.Base.ComparisonConfig();
            config.PropertyExceptions.Add("CustomData");
            config.PropertyExceptions.Add("Fragments");

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
                results = TestFailedResults(setName, objects, InputOutputComparisonType.Exception, timestep);
                return false;
            }
            finally
            {
                events.AddRange(Engine.Reflection.Query.CurrentEvents());
            }

            if (pushedObjects.Count != objects.Count)
            {
                results.AddRange(TestFailedResults(setName, objects.Where(x => !pushedObjects.Any(y => x.Name == y.Name)), InputOutputComparisonType.Exception, timestep));
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
                results = TestFailedResults(setName, objects, InputOutputComparisonType.Exception, timestep);
                return false;
            }
            finally
            {
                events.AddRange(Engine.Reflection.Query.CurrentEvents());
            }

            //Compare pushed and pulled objects
            VennDiagram<IBHoMObject> diagram = Engine.Data.Create.VennDiagram(pushedObjects, pulledObjects, comparer);

            //Check that all pushed objects found a match in pulled obejcts
            //result.PullSuccess = diagram.OnlySet1.Count == 0;

            foreach (Tuple<IBHoMObject, IBHoMObject> pair in diagram.Intersection)
            {
                var equalityResult = Engine.Test.Query.IsEqual(pair.Item1, pair.Item2, config);

                InputOutputComparisonType type = equalityResult.Item1 ? InputOutputComparisonType.Equal : InputOutputComparisonType.Difference;
                List<InputOutputDifference> differences = new List<InputOutputDifference>();

                string objectId = pair.Item1.CustomData["PushPull_Hash"].ToString();
                Type objectType = pair.Item1.GetType();

                for (int i = 0; i < equalityResult.Item2.Count; i++)
                {
                    differences.Add(new InputOutputDifference(objectId, setName, equalityResult.Item2[i], timestep, objectType, equalityResult.Item3[i], equalityResult.Item4[i]));
                }

                results.Add(new InputOutputComparison(objectId, setName, timestep, objectType, type, differences));
            }

            results.AddRange(TestFailedResults(setName, diagram.OnlySet1, InputOutputComparisonType.Exception, timestep));

            //Close the model
            if(resetModelBetweenRuns)
                adapter.Execute(new Close { SaveBeforeClose = false });

            //Clear events
            Engine.Reflection.Compute.ClearCurrentEvents();

            return success;
        }

        /***************************************************/

        private static List<InputOutputComparison> TestFailedResults(string setName, IEnumerable<IBHoMObject> objects, InputOutputComparisonType type, double timeStep)
        {
            return objects.Select(x => new InputOutputComparison(x.CustomData["PushPull_Hash"].ToString(), setName, timeStep, x.GetType(), type, new List<InputOutputDifference>())).ToList();
            //return objects.Select(x => new InputOutputDifference(x.CustomData["PushPull_Hash"].ToString(), setName, x.GetType().ToString(), timeStep, x.GetType(), type, x.ToString(), null)).ToList();
        }

    }
}


