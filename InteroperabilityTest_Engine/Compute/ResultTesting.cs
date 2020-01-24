/*
 * This file is part of the Buildings and Habitats object Model (BHoM)
 * Copyright (c) 2015 - 2019, the respective contributors. All rights reserved.
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
using System.Reflection;
using BH.oM.Reflection.Attributes;
using BH.oM.Base;
using BH.oM.Data.Requests;
using BH.oM.Data.Collections;
using BH.oM.Structure.Loads;
using BH.oM.Structure.Results;
using BH.oM.Structure.Elements;

using BH.oM.Test.Results;
using BH.Adapter;
using BH.oM.Structure.Requests;
using BH.oM.Geometry;
using BH.Engine.Geometry;
using BH.oM.Adapter.Commands;

namespace BH.Engine.Test.Interoperability
{
    public static partial class Compute
    {

        /***************************************************/
        /**** Public Methods                            ****/
        /***************************************************/

        [Description("Compares the results extraced from an adapter to a handcalculation of checkingMethod")]
        [Input("adapter", "The instance of the adapter to test for")]
        [Input("objects", "The type of object to test. This will use test sets in the Dataset library")]
        [Input("active", "Toggles whether to run the test")]
        [Output("analysisResults", "Diffing results outlining any differences found between the pushed and pulled objects. Also contains any error or warning messages returned by the adapter in the process")]
        public static CustomObject CheckAnalysisResults(BHoMAdapter adapter, IEnumerable<IBHoMObject> objects, MethodInfo checkingMethod = null, bool active = false)
        {
            if (active == false)
            {
                return new CustomObject();
            }

            CustomObject results = new CustomObject();

            //1. Filter out non+load objects

            IEnumerable<ILoad> loads = objects.Where(x => x is ILoad).Cast<ILoad>();
            IEnumerable<IBHoMObject> nonLoads = objects.Where(x => !(x is ILoad));

            //Clear everything in the analysisprogram before pushing?

            //2. Push non+load objects        

            List<IBHoMObject> pushedObjects = new List<IBHoMObject>();

            pushedObjects = adapter.Push(nonLoads).Cast<IBHoMObject>().ToList();

            results.CustomData["PushObjectsSuccess"] = pushedObjects.Count == nonLoads.Count();

            //3. Re+assign pushed elements to loads
            int method = 1; int reactions = 1;
            if (method != reactions)
            {
                foreach (ILoad load in loads)
                {
                    ReassignObjectsToLoad(load as dynamic, pushedObjects);
                }
            }
            // Push loads

            results.CustomData["PushObjectsSuccess"] = adapter.Push(loads).Count() == loads.Count();

            // Run model
            
            AnalyseLoadCases commandAnalyse = new AnalyseLoadCases() {LoadCases = nonLoads.OfType<ICase>() };
            
            results.CustomData["AnalysisRunSuccess"] = adapter.Execute(commandAnalyse).Item2;

            //Invoke checking method

            //      if(checkingMethod != null)
            //      checkingMethod.Invoke(null, new object[] { adapter, objects, loads });
            if (false)
            results.CustomData["TestResultsF&M"] = CheckShearForceAndMoments(adapter, nonLoads, loads); // rename based on what method ran?

            if (false)
            results.CustomData["TestResultsAxial"] = CheckAxialForce(adapter, nonLoads, loads);

            if (false)
            results.CustomData["TestResultsTorsion"] = CheckTorsion(adapter, nonLoads, loads);

            results.CustomData["TestResultsReactions"] = CheckReactionForces(adapter, nonLoads, loads);


            return results;
            //Close adapter after sending results back?            
            //Close commandClose = new Close();
            //adapter.Execute(commandClose);

        }

        /***************************************************/
        /**** Private Methods                           ****/
        /***************************************************/

        private static Load<T> ReassignObjectsToLoad<T>(Load<T> load, IEnumerable<IBHoMObject> objects) where T : IBHoMObject
        {
            IEnumerable<T> filteredObjects = objects.OfType<T>().Cast<T>();
            for (int i = 0; i < load.Objects.Elements.Count; i++)
            {
                load.Objects.Elements[i] = filteredObjects.Single(x => x.BHoM_Guid == load.Objects.Elements[i].BHoM_Guid);
            }
            return load;
        }

        /****************************************************/

        private static ILoad ReassignObjectsToLoad<T>(ILoad load, IEnumerable<IBHoMObject> objects) where T : IBHoMObject
        {
            return load;
        }

        /***************************************************/

        public static CustomObject CheckShearForceAndMoments(BHoMAdapter adapter, IEnumerable<IBHoMObject> objects, IEnumerable<ILoad> loads)
        {
            //Pull Results from robot
            BarResultRequest request = new BarResultRequest() { Cases = new List<object>() {objects.Single((x => x is Loadcase)) } };            
            List<BarForce> pulledResults = new List<BarForce>();

            pulledResults = adapter.Pull(request).Cast<BarForce>().ToList();
            bool pullSuccess = pulledResults.Count > 0;

            //Handcalc variables
            //W
            IEnumerable<BarUniformlyDistributedLoad> uniformlyDistributedLoads = loads.Where(x => (x is BarUniformlyDistributedLoad)).Cast<BarUniformlyDistributedLoad>();
            double yForce = uniformlyDistributedLoads.ElementAt(0).Force.Y;
            double zForce = uniformlyDistributedLoads.ElementAt(0).Force.Z;            

            //L
            IEnumerable<Bar> bar = objects.Where(x => (x is Bar)).Cast<Bar>(); // pull this instead?
            double barLength = bar.ElementAt(0).EndNode.Position.Distance(bar.ElementAt(0).StartNode.Position);

            //Result
            List<double> FYDiff = new List<double>();
            List<double> FZDiff = new List<double>();
            List<double> MYDiff = new List<double>();
            List<double> MZDiff = new List<double>();

            foreach (BarForce result in pulledResults)
            {
                double x = result.Position * barLength;

                double HandCalcFY = yForce * (x - 3 * barLength / 8);
                double HandCalcFZ = zForce * (x - 3 * barLength / 8);
                double HandCalcMY = zForce * ((Math.Pow(x, 2) / 2) - 3 * barLength * x / 8);
                double HandCalcMZ = yForce * ((Math.Pow(x, 2) / 2) - 3 * barLength * x / 8);

                double FY = result.FY;
                double FZ = result.FZ;
                double MY = result.MY;
                double MZ = result.MZ;

                if (FY == 0 && HandCalcFY == 0)
                {
                    FYDiff.Add(0);
                }
                else
                {
                    FYDiff.Add(FY / HandCalcFY - 1);
                }
                if (FZ == 0 && HandCalcFZ == 0)
                {
                    FZDiff.Add(0);
                }
                else
                {
                    FZDiff.Add(FZ / HandCalcFZ - 1);
                }
                if (MY == 0 && HandCalcMY == 0)
                {
                    MYDiff.Add(0);
                }
                else
                {
                    MYDiff.Add(MY / HandCalcMY - 1);
                }
                if (MZ == 0 && HandCalcMZ == 0)
                {
                    MZDiff.Add(0);
                }
                else
                {
                    MZDiff.Add(MZ / HandCalcMZ - 1);
                }
            }

            CustomObject results = new CustomObject();

            results.CustomData["PullSuccess"] = pullSuccess;
            results.CustomData["FYDifference"] = FYDiff;
            results.CustomData["FZDifference"] = FZDiff;
            results.CustomData["MYDifference"] = MYDiff;
            results.CustomData["MZDifference"] = MZDiff;

            return results;

        }

        /***************************************************/

        public static CustomObject CheckAxialForce(BHoMAdapter adapter, IEnumerable<IBHoMObject> objects, IEnumerable<ILoad> loads)
        {
            // Pull
            BarResultRequest request = new BarResultRequest() { Cases = new List<object>() { objects.Single((x => x is Loadcase)) } };            
            List<BarForce> pulledResults = new List<BarForce>();
            pulledResults = adapter.Pull(request).Cast<BarForce>().ToList();
            bool pullSuccess = pulledResults.Count > 0;

            //Handcalc variables
            //L
            IEnumerable<Bar> bar = objects.Where(x => (x is Bar)).Cast<Bar>();
            double L = bar.ElementAt(0).EndNode.Position.Distance(bar.ElementAt(0).StartNode.Position);

            //W 
            double W = loads.Where(x => (x is BarUniformlyDistributedLoad)).Cast<BarUniformlyDistributedLoad>().ElementAt(0).Force.Z;

            List<double> FXDiff = new List<double>();

            foreach (BarForce result in pulledResults)
            {
                double x = result.Position * L;
                double test = result.FX;
                double handCalc = W * (x - L);

                if (test == 0 && handCalc == 0)
                {
                    FXDiff.Add(0);
                }
                else
                {
                    FXDiff.Add(test / handCalc - 1);
                }
            }

            CustomObject results = new CustomObject();
            results.CustomData["PullSuccess"] = pullSuccess;
            results.CustomData["FXDifference"] = FXDiff;

            return results;

        }

        /***************************************************/

        public static CustomObject CheckTorsion(BHoMAdapter adapter, IEnumerable<IBHoMObject> objects, IEnumerable<ILoad> loads)
        {
            // Pull
            BarResultRequest request = new BarResultRequest() { Cases = new List<object>() { objects.Single((x => x is Loadcase)) } };
            List<BarForce> pulledResults = new List<BarForce>();
            pulledResults = adapter.Pull(request).Cast<BarForce>().ToList();
            bool pullSuccess = pulledResults.Count > 0;

            //Handcalc
            //L
            IEnumerable<Bar> bar = objects.Where(x => (x is Bar)).Cast<Bar>();
            double L = bar.ElementAt(0).EndNode.Position.Distance(bar.ElementAt(0).StartNode.Position);

            //T
            double T = loads.Where(x => (x is BarUniformlyDistributedLoad)).Cast<BarUniformlyDistributedLoad>().ElementAt(0).Moment.X;

            List<double> MXDiff = new List<double>();

            foreach (BarForce result in pulledResults)
            {
                double x = result.Position * L;
                double test = result.MX;
                double handCalc = T * (x - L);
                if (test == 0 && handCalc == 0)
                {
                    MXDiff.Add(0);
                }
                else
                {
                    MXDiff.Add(test / handCalc - 1);
                }
            }

            CustomObject results = new CustomObject();
            results.CustomData["PullSuccess"] = pullSuccess;
            results.CustomData["MXDifference"] = MXDiff;

            return results;

        }

        /***************************************************/

        public static CustomObject CheckReactionForces(BHoMAdapter adapter, IEnumerable<IBHoMObject> objects, IEnumerable<ILoad> loads)
        {
            NodeResultRequest request = new NodeResultRequest() { Cases = new List<object>() { objects.Single((x => x is Loadcase)) } };
            List<NodeReaction> pulledResults = new List<NodeReaction>();

            pulledResults = adapter.Pull(request).Cast<NodeReaction>().ToList();
            bool pullSuccess = pulledResults.Count > 0;

            double Load = loads.Where(x => (x is PointLoad)).Cast<PointLoad>().ElementAt(0).Force.Z;

            double nodeReactions = 0;

            foreach (NodeReaction result in pulledResults)
            {
                nodeReactions += result.FZ;
            }

            double comparision = nodeReactions / Load - 1;

            CustomObject results = new CustomObject();
            results.CustomData["PullSuccess"] = pullSuccess;
            results.CustomData["ReactionDifference"] = comparision;

            return results;            

        }
    }
}
