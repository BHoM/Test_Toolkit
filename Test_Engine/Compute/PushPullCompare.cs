///*
// * This file is part of the Buildings and Habitats object Model (BHoM)
// * Copyright (c) 2015 - 2019, the respective contributors. All rights reserved.
// *
// * Each contributor holds copyright over their respective contributions.
// * The project versioning (Git) records all such contribution source information.
// *                                           
// *                                                                              
// * The BHoM is free software: you can redistribute it and/or modify         
// * it under the terms of the GNU Lesser General Public License as published by  
// * the Free Software Foundation, either version 3.0 of the License, or          
// * (at your option) any later version.                                          
// *                                                                              
// * The BHoM is distributed in the hope that it will be useful,              
// * but WITHOUT ANY WARRANTY; without even the implied warranty of               
// * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the                 
// * GNU Lesser General Public License for more details.                          
// *                                                                            
// * You should have received a copy of the GNU Lesser General Public License     
// * along with this code. If not, see <https://www.gnu.org/licenses/lgpl-3.0.html>.      
// */

//using System;
//using System.Linq;
//using System.Collections.Generic;
//using System.ComponentModel;
//using BH.oM.Reflection.Attributes;
//using BH.oM.Base;
//using BH.oM.Data.Requests;
//using BH.oM.Data.Collections;

//using BH.oM.Test.Results;
//using BH.Adapter;


//namespace BH.Engine.Test
//{
//    public static partial class Compute
//    {
//        /***************************************************/
//        /**** Public Methods                            ****/
//        /***************************************************/

//        [Description("")]
//        [Input("", "")]
//        [Output("", "")]
//        public static List<PushPullSetDiffing> PushPullCompare(BHoMAdapter adapter, Type type, string config = "", bool active = false)
//        {

//            if (!active)
//                return new List<PushPullSetDiffing>();

//            //Get testdata
//            var testSets = Query.TestDataOfType(type);
//            List<string> testSetNames = testSets.Item1;
//            List<List<IBHoMObject>> testData = testSets.Item2;

//            //Set up comparer and request
//            FilterRequest request = new FilterRequest { Type = type };
//            AdapterIdComparer comparer = new AdapterIdComparer(adapter.AdapterId);

//            //List for storing output
//            List<PushPullSetDiffing> results = new List<PushPullSetDiffing>();

//            for (int i = 0; i < testSetNames.Count; i++)
//            {
//                results.Add(RunOneSet(adapter, testSetNames[i], testData[i], request, comparer));
//            }


//            return results;
//        }

//        /***************************************************/
//        /**** Private Methods                           ****/
//        /***************************************************/

//        private static PushPullSetDiffing RunOneSet(BHoMAdapter adapter, string setName, List<IBHoMObject> objects, IRequest request, IEqualityComparer<IBHoMObject> comparer)
//        {

//            PushPullSetDiffing result = new PushPullSetDiffing { Name = setName };

//            List<IBHoMObject> pushedObjects;
//            List<IBHoMObject> pulledObjects;

//            //Push objects
//            try
//            {
//                Engine.Reflection.Compute.ClearCurrentEvents();
//                pushedObjects = adapter.Push(objects).Cast<IBHoMObject>().ToList();
//                result.PushSuccess = pushedObjects.Count == objects.Count;
//            }
//            catch (Exception e)
//            {
//                result.PushMessages.Add(e.Message);
//                result.PushSuccess = false;
//                return result;
//            }
//            finally
//            {
//                result.PushMessages.AddRange(Engine.Reflection.Query.CurrentEvents().Select(x => x.Message));
//            }

//            //Pull objects
//            try
//            {
//                Engine.Reflection.Compute.ClearCurrentEvents();
//                pulledObjects = adapter.Pull(request).Cast<IBHoMObject>().ToList();
//            }
//            catch (Exception e)
//            {
//                result.PullMessages.Add(e.Message);
//                result.PullSuccess = false;
//                return result;
//            }
//            finally
//            {
//                result.PullMessages.AddRange(Engine.Reflection.Query.CurrentEvents().Select(x => x.Message));
//            }

//            //Compare pushed and pulled objects
//            VennDiagram<IBHoMObject> diagram = Engine.Data.Create.VennDiagram(pushedObjects, pulledObjects, comparer);

//            //Check that all pushed objects found a match in pulled obejcts
//            result.PullSuccess = diagram.OnlySet1.Count == 0;

//            foreach (Tuple<IBHoMObject, IBHoMObject> pair in diagram.Intersection)
//            {
//                var equalityResult = Engine.Testing.Query.IsEqual(pair.Item1, pair.Item2);
//                DiffingResult diff = new DiffingResult { Name = pair.Item1.Name, IsEqual = equalityResult.Item1 };

//                for (int i = 0; i < equalityResult.Item2.Count; i++)
//                {
//                    diff.Differences.Add(new PropertyDifference { Name = equalityResult.Item2[i], FirstItemValue = equalityResult.Item3[i], SecondItemValue = equalityResult.Item4[i] });
//                }

//                result.DiffingResults.Add(diff);
//            }

//            return result;
//        }

//        /***************************************************/

//    }
//}
